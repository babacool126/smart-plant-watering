using MQTTnet;

namespace SmartPlantApp.Services;

public class MqttService
{
    private readonly IMqttClient _mqttClient;

    public MqttService()
    {
        var factory = new MqttClientFactory();
        _mqttClient = factory.CreateMqttClient();

        _mqttClient.ApplicationMessageReceivedAsync += e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = e.ApplicationMessage.ConvertPayloadToString();

            Console.WriteLine($"MQTT message received on {topic}: {payload}");

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
            .Build();

        await _mqttClient.SubscribeAsync(subscribeOptions);

        Console.WriteLine("Subscribed to plant/sensors/moisture");

    }
}
