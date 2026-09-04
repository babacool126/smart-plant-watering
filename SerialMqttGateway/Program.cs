using System.IO.Ports;
using MQTTnet;

using var serialPort = new SerialPort("COM3", 9600);

var mqttFactory = new MqttClientFactory();
using var mqttClient = mqttFactory.CreateMqttClient();

var mqttClientOptions = new MqttClientOptionsBuilder()
    .WithTcpServer("node-01.lab.thomaslab.nl", 1883)
    .Build();

await mqttClient.ConnectAsync(mqttClientOptions);

serialPort.Open();

Console.WriteLine("Gateway gestart.");

while (true)
{
    string line = serialPort.ReadLine();

    Console.WriteLine($"Arduino: {line}");

    var message = new MqttApplicationMessageBuilder()
        .WithTopic("plant/serial")
        .WithPayload(line)
        .Build();

    await mqttClient.PublishAsync(message);
}