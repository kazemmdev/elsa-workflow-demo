using ElsaWorkflow.Data.Entities;
using ElsaWorkflow.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ElsaWorkflow.Seeding;

public sealed class DatabaseSeeder
{
    private readonly RoleManager<ApplicationRole>  _roleManager;
    private readonly UserManager<ApplicationUser>  _userManager;
    private readonly SeedingOptions _opts;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IOptions<SeedingOptions> opts,
        ILogger<DatabaseSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _opts        = opts.Value;
        _logger      = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedRolesAsync();
        await SeedAdminUserAsync();
    }

    private async Task SeedRolesAsync()
    {
        var descriptions = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [Roles.Admin]    = "Full system access including user management and configuration.",
            [Roles.Manager]  = "Can approve workflows and view team activity.",
            [Roles.Finance]  = "Access to financial workflows and reports.",
            [Roles.Employee] = "Standard employee access; can submit and track own requests.",
        };

        foreach (var roleName in Roles.All)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogDebug("Role '{Role}' already exists – skipping.", roleName);
                continue;
            }

            var role = new ApplicationRole(roleName)
            {
                Description = descriptions.GetValueOrDefault(roleName),
            };

            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
                _logger.LogInformation("Created role '{Role}'.", roleName);
            else
                LogIdentityErrors($"create role '{roleName}'", result);
        }
    }

    private async Task SeedAdminUserAsync()
    {
        var email = _opts.AdminEmail;

        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            _logger.LogDebug("Admin user '{Email}' already exists – skipping.", email);
            return;
        }

        var admin = new ApplicationUser
        {
            UserName  = _opts.AdminUserName,
            Email     = email,
            FirstName = "System",
            LastName  = "Administrator",
            IsActive  = true,
            EmailConfirmed = true,
        };

        var createResult = await _userManager.CreateAsync(admin, _opts.AdminPassword);
        if (!createResult.Succeeded)
        {
            LogIdentityErrors($"create admin user '{email}'", createResult);
            return;
        }

        var roleResult = await _userManager.AddToRoleAsync(admin, Roles.Admin);
        if (roleResult.Succeeded)
            _logger.LogInformation(
                "Seeded admin user '{Email}' and assigned role '{Role}'.",
                email, Roles.Admin);
        else
            LogIdentityErrors($"assign Admin role to '{email}'", roleResult);
    }

    private void LogIdentityErrors(string action, IdentityResult result)
    {
        var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
        _logger.LogError("Failed to {Action}. Errors: {Errors}", action, errors);
    }
}

public sealed class SeedingOptions
{
    public const string SectionName = "Seeding";

    public string AdminUserName { get; set; } = "admin";
    public string AdminEmail    { get; set; } = "admin@example.com";
    public string AdminPassword { get; set; } = "Admin@12345!";
}
