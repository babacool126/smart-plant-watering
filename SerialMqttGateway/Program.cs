using MQTTnet;

var mqttFactory = new MqttClientFactory();

using var mqttClient = mqttFactory.CreateMqttClient();

var mqttClientOptions = new MqttClientOptionsBuilder()
    .WithTcpServer("node-01.lab.thomaslab.nl", 1883)
    .Build();

Console.WriteLine("Verbinden met MQTT broker...");

await mqttClient.ConnectAsync(mqttClientOptions);

Console.WriteLine("Verbonden.");

var message = new MqttApplicationMessageBuilder()
    .WithTopic("plant/test")
    .WithPayload("hello from C#")
    .Build();

await mqttClient.PublishAsync(message);

Console.WriteLine("Bericht gepubliceerd.");

await mqttClient.DisconnectAsync();