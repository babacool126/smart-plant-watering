using System.IO.Ports;
using System.Text.Json;

using var serialPort = new SerialPort("COM3", 9600);

await using var mqttService =
    new MqttService("node-01.lab.thomaslab.nl", 1883);

await mqttService.ConnectAsync();

await mqttService.SubscribeAsync(
    "plant/pump/command",
    async (topic, payload) =>
    {
        Console.WriteLine($"MQTT ontvangen: {topic} -> {payload}");

        using JsonDocument document = JsonDocument.Parse(payload);

        string? action = document.RootElement
            .GetProperty("action")
            .GetString();

        if (action == "on")
        {
            serialPort.WriteLine("PUMP_ON");
            Console.WriteLine("PUMP_ON naar Arduino gestuurd");
        }
        else if (action == "off")
        {
            serialPort.WriteLine("PUMP_OFF");
            Console.WriteLine("PUMP_OFF naar Arduino gestuurd");
        }

        await Task.CompletedTask;
    });

serialPort.Open();

Console.WriteLine("Gateway gestart.");
Console.WriteLine("Luistert naar MQTT topic: plant/pump/command");

while (true)
{
    string line = serialPort.ReadLine().Trim();

    Console.WriteLine($"Arduino: {line}");

    if (line.StartsWith("Bodemvocht raw:"))
    {
        string valueText = line
            .Split(':', 2)[1]
            .Trim()
            .Split(' ', 2)[0];

        if (int.TryParse(valueText, out int moisture))
        {
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
            string payload = JsonSerializer.Serialize(new
            {
                value = humidity,
                unit = "percent"
            });

            await mqttService.PublishAsync(
                "plant/sensors/humidity",
                payload);
        }
    }
}