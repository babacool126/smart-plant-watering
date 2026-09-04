using System.IO.Ports;
using MQTTnet;

using var serialPort = new SerialPort("COM3", 9600);

var mqttFactory = new MqttClientFactory();
using var mqttClient = mqttFactory.CreateMqttClient();

mqttClient.ApplicationMessageReceivedAsync += e =>
{
    string topic = e.ApplicationMessage.Topic;
    string payload = e.ApplicationMessage.ConvertPayloadToString();

    Console.WriteLine($"MQTT ontvangen: {topic} -> {payload}");

    if (topic == "plant/pump/command")
    {
        if (payload == "ON")
        {
            serialPort.WriteLine("PUMP_ON");
            Console.WriteLine("PUMP_ON naar Arduino gestuurd");
        }
        else if (payload == "OFF")
        {
            serialPort.WriteLine("PUMP_OFF");
            Console.WriteLine("PUMP_OFF naar Arduino gestuurd");
        }
    }

    return Task.CompletedTask;
};

var mqttClientOptions = new MqttClientOptionsBuilder()
    .WithTcpServer("node-01.lab.thomaslab.nl", 1883)
    .Build();

await mqttClient.ConnectAsync(mqttClientOptions);

await mqttClient.SubscribeAsync("plant/pump/command");

serialPort.Open();

Console.WriteLine("Gateway gestart.");
Console.WriteLine("Luistert naar MQTT topic: plant/pump/command");

while (true)
{
    string line = serialPort.ReadLine().Trim();

    Console.WriteLine($"Arduino: {line}");

    var message = new MqttApplicationMessageBuilder()
        .WithTopic("plant/serial")
        .WithPayload(line)
        .Build();

    await mqttClient.PublishAsync(message);
}