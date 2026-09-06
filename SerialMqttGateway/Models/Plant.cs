namespace SerialMqttGateway.Models;

public class Plant
{
    public int PlantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int MoistureThreshold { get; set; }
}
