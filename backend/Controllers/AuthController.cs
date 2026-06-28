using Elsa.Extensions;
using Elsa.Identity.Contracts;
using Elsa.Identity.Options;
using ElsaWorkflow.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ElsaWorkflow.Controllers;

public sealed class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly IUserProvider _elsaUserProvider;
    private readonly IAccessTokenIssuer _tokenIssuer;
    private readonly IdentityTokenOptions _tokenOpts;


    public AuthController(
        UserManager<ApplicationUser>   users,
        SignInManager<ApplicationUser> signIn,
        IUserProvider                  elsaUserProvider,
        IAccessTokenIssuer             tokenIssuer,
        IOptions<IdentityTokenOptions> tokenOpts)
    {
        _users            = users;
        _signIn           = signIn;
        _elsaUserProvider = elsaUserProvider;
        _tokenIssuer      = tokenIssuer;
        _tokenOpts        = tokenOpts.Value;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _users.FindByNameAsync(request.UserName);
        if (user is null)
            return Unauthorized(new { message = "Invalid credentials." });

        if (!user.IsActive)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "Account is deactivated." });

        var result = await _signIn.CheckPasswordSignInAsync(
            user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    new { message = "Account is temporarily locked. Try again later." });

            return Unauthorized(new { message = "Invalid credentials." });
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _users.UpdateAsync(user);

        var elsaUser = await _elsaUserProvider.FindByNameAsync(request.UserName, cancellationToken);
        if (elsaUser is null)
            return Unauthorized(new { message = "Invalid credentials." });

        var tokens = await _tokenIssuer.IssueTokensAsync(elsaUser, cancellationToken);
        var roles  = await _users.GetRolesAsync(user);

        return Ok(new LoginResponse(
            AccessToken:     tokens.AccessToken,
            ExpiresInSeconds: (int)_tokenOpts.AccessTokenLifetime.TotalSeconds,
            UserId:          user.Id,
            UserName:        user.UserName ?? user.Id,
            Roles:           roles));
    }

}