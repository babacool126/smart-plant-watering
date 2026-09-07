using MQTTnet;

namespace SmartPlantApp.Services;

public class MqttService
{
    private readonly IMqttClient _mqttClient;

    public MqttService()
    {
        var factory = new MqttClientFactory();
        _mqttClient = factory.CreateMqttClient();
    }

    public async Task ConnectAsync()
    {
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("100.78.255.58", 1883)
            .WithClientId("smart-plant-maui")
            .Build();

        var result = await _mqttClient.ConnectAsync(options);

        Console.WriteLine($"MQTT connection result: {result.ResultCode}");
    }
}
