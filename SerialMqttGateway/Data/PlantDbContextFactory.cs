using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SerialMqttGateway.Data;

public class PlantDbContextFactory
    : IDesignTimeDbContextFactory<PlantDbContext>
{
    public PlantDbContext CreateDbContext(string[] args)
    {
        string connectionString =
            Environment.GetEnvironmentVariable("PLANT_DB_CONNECTION")
            ?? throw new InvalidOperationException(
                "Environment variable PLANT_DB_CONNECTION is niet ingesteld.");

        var options = new DbContextOptionsBuilder<PlantDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new PlantDbContext(options);
    }
}
