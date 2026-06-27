using Elsa.Identity.Contracts;
using Elsa.Identity.Entities;
using Elsa.Identity.Models;
using ElsaWorkflow.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace ElsaWorkflow.Infrastructure;

/// <summary>
/// Maps ASP.NET Identity users into Elsa's identity model for login and token issuance.
/// </summary>
public sealed class AspNetIdentityUserProvider : IUserProvider
{
    private readonly UserManager<ApplicationUser> _users;

    public AspNetIdentityUserProvider(UserManager<ApplicationUser> users) => _users = users;

    public async Task<User?> FindAsync(UserFilter filter, CancellationToken ct = default)
    {
        ApplicationUser? appUser = null;

        if (!string.IsNullOrWhiteSpace(filter.Name))
            appUser = await _users.FindByNameAsync(filter.Name);
        else if (!string.IsNullOrWhiteSpace(filter.Id))
            appUser = await _users.FindByIdAsync(filter.Id);

        if (appUser is null || !appUser.IsActive)
            return null;

        var roles = await _users.GetRolesAsync(appUser);

        return new User
        {
            Id    = appUser.Id,
            Name  = appUser.UserName ?? appUser.Id,
            Roles = roles.ToList(),
        };
    }
}
