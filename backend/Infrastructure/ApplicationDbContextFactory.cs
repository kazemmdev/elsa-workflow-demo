using ElsaWorkflow.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ElsaWorkflow.Infrastructure;

public sealed class ApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connStr = Environment.GetEnvironmentVariable("DESIGN_TIME_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=elsa_workflow;Username=postgres;Password=postgres";

        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connStr, npg => npg.MigrationsHistoryTable("__app_migrations_history"))
            .Options;

        return new ApplicationDbContext(opts);
    }
}
