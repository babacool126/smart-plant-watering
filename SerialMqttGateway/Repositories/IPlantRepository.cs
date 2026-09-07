using SerialMqttGateway.Models;

namespace SerialMqttGateway.Repositories;

public interface IPlantRepository
{
    Task AddSensorReadingAsync(
        SensorReading reading,
        CancellationToken cancellationToken = default);

    Task AddWateringEventAsync(
        WateringEvent wateringEvent,
        CancellationToken cancellationToken = default);

    Task<List<SensorReading>> GetRecentSensorReadingsAsync(
        int plantId,
        int limit = 20,
        CancellationToken cancellationToken = default);
}
