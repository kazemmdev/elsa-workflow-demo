using Elsa.Extensions;
using Elsa.Identity.Contracts;
using Elsa.Identity.Entities;
using ElsaWorkflow.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace ElsaWorkflow.Infrastructure;

/// <summary>
/// Validates Elsa login credentials against ASP.NET Identity (password hashes, lockout, active flag).
/// </summary>
public sealed class AspNetIdentityUserCredentialsValidator : IUserCredentialsValidator
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly IUserProvider _userProvider;

    public AspNetIdentityUserCredentialsValidator(
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signIn,
        IUserProvider userProvider)
    {
        _users         = users;
        _signIn        = signIn;
        _userProvider  = userProvider;
    }

    public async ValueTask<User?> ValidateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var appUser = await _users.FindByNameAsync(username);
        if (appUser is null || !appUser.IsActive)
            return null;

        var result = await _signIn.CheckPasswordSignInAsync(
            appUser, password, lockoutOnFailure: true);

        if (!result.Succeeded)
            return null;

        appUser.LastLoginAt = DateTimeOffset.UtcNow;
        await _users.UpdateAsync(appUser);

        return await _userProvider.FindByNameAsync(username, cancellationToken);
    }
}
