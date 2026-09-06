namespace SerialMqttGateway.Models;

public class WateringEvent
{
    public int WateringEventId { get; set; }

    public int PlantId { get; set; }

    public DateTime StartedAt { get; set; }

    public int DurationSeconds { get; set; }

    public string Reason { get; set; } = string.Empty;

    public Plant Plant { get; set; } = null!;
}
