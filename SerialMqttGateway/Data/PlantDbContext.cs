using Microsoft.EntityFrameworkCore;
using SerialMqttGateway.Models;

namespace SerialMqttGateway.Data;

public class PlantDbContext : DbContext
{
    public PlantDbContext(DbContextOptions<PlantDbContext> options)
        : base(options)
    {
    }

    public DbSet<Plant> Plants { get; set; }
    public DbSet<SensorReading> SensorReadings { get; set; }
    public DbSet<WateringEvent> WateringEvents { get; set; }
}
