using System.IO.Ports;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using SerialMqttGateway.Data;
using SerialMqttGateway.Repositories;
using SerialMqttGateway.Models;

var connectionString =
    Environment.GetEnvironmentVariable("PLANT_DB_CONNECTION")
    ?? throw new InvalidOperationException(
        "Environment variable PLANT_DB_CONNECTION is niet ingesteld.");

var dbOptions = new DbContextOptionsBuilder<PlantDbContext>()
    .UseNpgsql(connectionString)
    .Options;

await using var dbContext = new PlantDbContext(dbOptions);

var repository = new PlantRepository(dbContext);

var plant = await dbContext.Plants.FindAsync(1);

if (plant is null)
{
    throw new InvalidOperationException("Plant met ID 1 niet gevonden.");
}


using var serialPort = new SerialPort("COM3", 9600);
serialPort.Open();

await using var mqttService =
    new MqttService("node-01.lab.thomaslab.nl", 1883);

await mqttService.ConnectAsync();

var pumpSemaphore = new SemaphoreSlim(1, 1);
var dbSemaphore = new SemaphoreSlim(1, 1);

var sensorState = new SensorState();

DateTime lastWateringTime = DateTime.MinValue;
DateTime? manualWateringStartedAt = null;

var wateringCooldown = TimeSpan.FromMinutes(1);

var wateringTask = Task.Run(async () =>
{
    while (true)
    {
        await Task.Delay(TimeSpan.FromSeconds(10));

        if (sensorState.Moisture is int moisture)
        {
            Console.WriteLine(
                $"Automatische check: moisture={moisture}, threshold={plant.MoistureThreshold}");

            if (moisture > plant.MoistureThreshold)
            {
                Console.WriteLine("Plant is te droog.");

                if (DateTime.UtcNow - lastWateringTime < wateringCooldown)
                {
                    Console.WriteLine("Cooldown actief, nog niet opnieuw bewateren.");
                    continue;
                }

                await pumpSemaphore.WaitAsync();

                try
                {
                    Console.WriteLine("Automatische bewatering gestart.");

                    serialPort.WriteLine("PUMP_ON");

                    await Task.Delay(TimeSpan.FromSeconds(5));

                    serialPort.WriteLine("PUMP_OFF");

                    lastWateringTime = DateTime.UtcNow;

                    await dbSemaphore.WaitAsync();

                    try
                    {
                        await repository.AddWateringEventAsync(
                            new WateringEvent
                            {
                                PlantId = plant.PlantId,
                                StartedAt = lastWateringTime.AddSeconds(-5),
                                DurationSeconds = 5,
                                Reason = "Automatic moisture threshold"
                            });

                        Console.WriteLine("Waterbeurt opgeslagen in database.");
                    }
                    finally
                    {
                        dbSemaphore.Release();
                    }

                    Console.WriteLine("Automatische bewatering gestopt.");
                }
                finally
                {
                    pumpSemaphore.Release();
                }
            }
            else
            {
                Console.WriteLine("Plant is vochtig genoeg.");
            }
        }
    }
});

// Subscribe asynchronously to MQTT pump commands
await mqttService.SubscribeAsync(
    "plant/pump/command",
    async (topic, payload) =>
    {
        Console.WriteLine($"MQTT ontvangen: {topic} -> {payload}");

        using JsonDocument document = JsonDocument.Parse(payload);

        string? action = document.RootElement
            .GetProperty("action")
            .GetString();

        await pumpSemaphore.WaitAsync();

        try
        {
            if (action == "on")
            {
                serialPort.WriteLine("PUMP_ON");

                manualWateringStartedAt ??= DateTime.UtcNow;

                Console.WriteLine("PUMP_ON naar Arduino gestuurd");
            }
            else if (action == "off")
            {
                serialPort.WriteLine("PUMP_OFF");

                if (manualWateringStartedAt is DateTime startedAt)
                {
                    DateTime stoppedAt = DateTime.UtcNow;

                    int durationSeconds = Math.Max(
                        1,
                        (int)Math.Round((stoppedAt - startedAt).TotalSeconds));

                    await dbSemaphore.WaitAsync();

                    try
                    {
                        await repository.AddWateringEventAsync(
                            new WateringEvent
                            {
                                PlantId = plant.PlantId,
                                StartedAt = startedAt,
                                DurationSeconds = durationSeconds,
                                Reason = "Manual MQTT command"
                            });

                        Console.WriteLine(
                            $"Handmatige waterbeurt opgeslagen: duration={durationSeconds}s");
                    }
                    finally
                    {
                        dbSemaphore.Release();
                    }

                    manualWateringStartedAt = null;
                }

                Console.WriteLine("PUMP_OFF naar Arduino gestuurd");
            }
        }
        finally
        {
            pumpSemaphore.Release();
        }

    });



Console.WriteLine("Gateway gestart.");
Console.WriteLine("Luistert naar MQTT topic: plant/pump/command");

// Thread-safe channel between the serial reader and MQTT publisher
var sensorChannel = Channel.CreateUnbounded<string>();

// Consume serial messages from the channel and publish sensor data to MQTT
var mqttPublisherTask = Task.Run(async () =>
{
    await foreach (string line in sensorChannel.Reader.ReadAllAsync())
    {
        if (line.StartsWith("Bodemvocht raw:"))
        {
            string valueText = line
                .Split(':', 2)[1]
                .Trim()
                .Split(' ', 2)[0];

            if (int.TryParse(valueText, out int moisture))
            {
                sensorState.Moisture = moisture;

                string payload = JsonSerializer.Serialize(new
                {
                    value = moisture,
                    unit = "raw"
                });

                await mqttService.PublishAsync(
                    "plant/sensors/moisture",
                    payload);
            }
        }
        else if (line.StartsWith("Temperatuur:"))
        {
            string valueText = line
                .Split(':', 2)[1]
                .Trim()
                .Split(' ', 2)[0];

            if (double.TryParse(
                valueText,
                System.Globalization.CultureInfo.InvariantCulture,
                out double temperature))
            {
                sensorState.Temperature = temperature;
                string payload = JsonSerializer.Serialize(new
                {
                    value = temperature,
                    unit = "celsius"
                });

                await mqttService.PublishAsync(
                    "plant/sensors/temperature",
                    payload);
            }
        }
        else if (line.StartsWith("Luchtvochtigheid:"))
        {
            string valueText = line
                .Split(':', 2)[1]
                .Trim()
                .TrimEnd('%');

            if (double.TryParse(
                valueText,
                System.Globalization.CultureInfo.InvariantCulture,
                out double humidity))
            {
                sensorState.Humidity = humidity;

                string payload = JsonSerializer.Serialize(new
                {
                    value = humidity,
                    unit = "percent"
                });

                await mqttService.PublishAsync(
                    "plant/sensors/humidity",
                    payload);

                if (sensorState.Moisture is int currentMoisture &&
                    sensorState.Temperature is double currentTemperature &&
                    sensorState.Humidity is double currentHumidity)
                {
                    await dbSemaphore.WaitAsync();

                    try
                    {
                        await repository.AddSensorReadingAsync(
                            new SensorReading
                            {
                                PlantId = plant.PlantId,
                                Moisture = currentMoisture,
                                Temperature = currentTemperature,
                                Humidity = currentHumidity,
                                MeasuredAt = DateTime.UtcNow
                            });

                        Console.WriteLine(
                            $"Meting opgeslagen: moisture={currentMoisture}, " +
                            $"temperature={currentTemperature}, humidity={currentHumidity}");
                    }
                    finally
                    {
                        dbSemaphore.Release();
                    }
                }
            }
        }
    }
});

while (true)
{
    string line = serialPort.ReadLine().Trim();

    Console.WriteLine($"Arduino: {line}");

    await sensorChannel.Writer.WriteAsync(line);
}

