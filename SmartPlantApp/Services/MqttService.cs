using MQTTnet;
using System.Text.Json;

namespace SmartPlantApp.Services;

public class MqttService
{
    private readonly IMqttClient _mqttClient;

    public event Action<int>? MoistureReceived;
    public event Action<double>? TemperatureReceived;
    public event Action<string>? HistoryReceived;

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
                if (topic == "plant/history/response")
                {
                    HistoryReceived?.Invoke(payload);
                    return Task.CompletedTask;
                }

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
            .WithTopicFilter(f =>
            {
                f.WithTopic("plant/history/response");
            })
            .Build();

        await _mqttClient.SubscribeAsync(subscribeOptions);

        Console.WriteLine(
            "Subscribed to moisture, temperature and history topics");
    }

    public async Task RequestHistoryAsync()
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic("plant/history/request")
            .WithPayload("{}")
            .Build();

        await _mqttClient.PublishAsync(message);

        Console.WriteLine("History request published.");
    }
}
