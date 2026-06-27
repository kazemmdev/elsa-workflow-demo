using System.Text;
using ElsaWorkflow.Data;
using ElsaWorkflow.Data.Entities;
using ElsaWorkflow.Infrastructure;
using ElsaWorkflow.Security;
using ElsaWorkflow.Seeding;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ElsaWorkflow.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseNpgsql(connectionString, npg =>
            {
                npg.MigrationsHistoryTable("__app_migrations_history");
                npg.EnableRetryOnFailure(maxRetryCount: 5);
            }));

        return services;
    }

    public static IServiceCollection AddAppIdentity(this IServiceCollection services)
    {
        services
            .AddIdentityCore<ApplicationUser>(opts =>
            {
                opts.Password.RequireDigit           = true;
                opts.Password.RequireLowercase       = true;
                opts.Password.RequireUppercase       = true;
                opts.Password.RequireNonAlphanumeric = true;
                opts.Password.RequiredLength         = 8;
                opts.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
                opts.Lockout.MaxFailedAccessAttempts = 5;
                opts.Lockout.AllowedForNewUsers      = true;
                opts.User.RequireUniqueEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddSignInManager<SignInManager<ApplicationUser>>();

        return services;
    }

    public static IServiceCollection AddAppJwtTokenService(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
 
        services.AddScoped<JwtTokenService>();
        return services;
    }

    public static IServiceCollection AddDatabaseSeeder(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<SeedingOptions>()
            .Bind(configuration.GetSection(SeedingOptions.SectionName));

        services.AddScoped<DatabaseSeeder>();

        return services;
    }

    public static IServiceCollection AddElsaIdentityBridge(this IServiceCollection services)
    {
        services.AddScoped<Elsa.Identity.Contracts.IUserProvider, AspNetIdentityUserProvider>();
        services.AddScoped<Elsa.Identity.Contracts.IUserCredentialsValidator, AspNetIdentityUserCredentialsValidator>();

        return services;
    }
}
