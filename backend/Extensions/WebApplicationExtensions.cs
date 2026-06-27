using ElsaWorkflow.Data;
using ElsaWorkflow.Seeding;
using Microsoft.EntityFrameworkCore;

namespace ElsaWorkflow.Extensions;

public static class WebApplicationExtensions
{
    public static async Task<WebApplication> MigrateAndSeedDatabaseAsync(this WebApplication app)
    {
        using var scope  = app.Services.CreateScope();
        var       sp     = scope.ServiceProvider;
        var       logger = sp.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            logger.LogInformation("Applying ApplicationDbContext migrations…");
            var db = sp.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();
            logger.LogInformation("Migrations applied.");

            logger.LogInformation("Running database seeder…");
            var seeder = sp.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync();
            logger.LogInformation("Seeding complete.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "A fatal error occurred while migrating or seeding the database.");
            throw;
        }

        return app;
    }

    public static void MigrateAndSeedDatabase(this WebApplication app) => app.MigrateAndSeedDatabaseAsync().GetAwaiter().GetResult();
}
