using MQTTnet;
using System.Text.Json;

namespace SmartPlantApp.Services;

public class MqttService
{
    private readonly IMqttClient _mqttClient;

    public event Action<int>? MoistureReceived;
    public event Action<double>? TemperatureReceived;

    public MqttService()
    {
        var factory = new MqttClientFactory();
        _mqttClient = factory.CreateMqttClient();

        _mqttClient.ApplicationMessageReceivedAsync += e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = e.ApplicationMessage.ConvertPayloadToString();

            Console.WriteLine($"MQTT message received on {topic}: {payload}");

            try
            {
                using var document = JsonDocument.Parse(payload);
                var value = document.RootElement.GetProperty("value");

                if (topic == "plant/sensors/moisture")
                {
                    MoistureReceived?.Invoke(value.GetInt32());
                }
                else if (topic == "plant/sensors/temperature")
                {
                    TemperatureReceived?.Invoke(value.GetDouble());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to process MQTT message: {ex.Message}");
            }

            return Task.CompletedTask;
        };
    }

    public async Task ConnectAsync()
    {
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("100.78.255.58", 1883)
            .WithClientId("smart-plant-maui")
            .Build();

        var result = await _mqttClient.ConnectAsync(options);

        Console.WriteLine($"MQTT connection result: {result.ResultCode}");

        var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(f =>
            {
                f.WithTopic("plant/sensors/moisture");
            })
            .WithTopicFilter(f =>
            {
                f.WithTopic("plant/sensors/temperature");
            })
            .Build();

        await _mqttClient.SubscribeAsync(subscribeOptions);

        Console.WriteLine("Subscribed to moisture and temperature topics");
    }
}
