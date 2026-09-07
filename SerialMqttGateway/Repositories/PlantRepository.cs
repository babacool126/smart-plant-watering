using Microsoft.EntityFrameworkCore;
using SerialMqttGateway.Data;
using SerialMqttGateway.Models;

namespace SerialMqttGateway.Repositories;

public class PlantRepository : IPlantRepository
{
    private readonly PlantDbContext _dbContext;

    public PlantRepository(PlantDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddSensorReadingAsync(
        SensorReading reading,
        CancellationToken cancellationToken = default)
    {
        _dbContext.SensorReadings.Add(reading);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddWateringEventAsync(
        WateringEvent wateringEvent,
        CancellationToken cancellationToken = default)
    {
        _dbContext.WateringEvents.Add(wateringEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<SensorReading>> GetRecentSensorReadingsAsync(
        int plantId,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SensorReadings
            .Where(reading => reading.PlantId == plantId)
            .OrderByDescending(reading => reading.MeasuredAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
