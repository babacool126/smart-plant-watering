namespace SerialMqttGateway.Models;

public class SensorReading
{
    public int SensorReadingId { get; set; }

    public int PlantId { get; set; }

    public int Moisture { get; set; }

    public double Temperature { get; set; }

    public double Humidity { get; set; }

    public DateTime MeasuredAt { get; set; }

    public Plant Plant { get; set; } = null!;
}
