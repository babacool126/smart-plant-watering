using MQTTnet;

public sealed class MqttService : IAsyncDisposable
{
    private readonly IMqttClient _mqttClient;
    private readonly MqttClientOptions _options;

    public MqttService(string host, int port)
    {
        var factory = new MqttClientFactory();
        _mqttClient = factory.CreateMqttClient();

        _options = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .Build();
    }

    public async Task ConnectAsync()
    {
        await _mqttClient.ConnectAsync(_options);
    }

    public async Task SubscribeAsync(
        string topic,
        Func<string, string, Task> messageHandler)
    {
        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            string receivedTopic = e.ApplicationMessage.Topic;
            string payload = e.ApplicationMessage.ConvertPayloadToString();

            await messageHandler(receivedTopic, payload);
        };

        await _mqttClient.SubscribeAsync(topic);
    }

    public async Task PublishAsync(string topic, string payload)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .Build();

        await _mqttClient.PublishAsync(message);
    }

    public async ValueTask DisposeAsync()
    {
        if (_mqttClient.IsConnected)
        {
            await _mqttClient.DisconnectAsync();
        }

        _mqttClient.Dispose();
    }
}
