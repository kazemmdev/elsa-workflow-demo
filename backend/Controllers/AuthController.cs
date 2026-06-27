using ElsaWorkflow.Data.Entities;
using ElsaWorkflow.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ElsaWorkflow.Controllers;

public sealed class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly JwtTokenService _jwt;
    private readonly JwtOptions _jwtOpts;


    public AuthController(
        UserManager<ApplicationUser>   users,
        SignInManager<ApplicationUser> signIn,
        JwtTokenService                jwt,
        Microsoft.Extensions.Options.IOptions<JwtOptions> jwtOpts)
    {
        _users   = users;
        _signIn  = signIn;
        _jwt     = jwt;
        _jwtOpts = jwtOpts.Value;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // 1. Look up user
        var user = await _users.FindByNameAsync(request.UserName);
        if (user is null)
            return Unauthorized(new { message = "Invalid credentials." });

        // 2. Check soft-delete flag before touching SignInManager
        if (!user.IsActive)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "Account is deactivated." });

        // 3. Validate password (respects lockout)
        var result = await _signIn.CheckPasswordSignInAsync(
            user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    new { message = "Account is temporarily locked. Try again later." });

            return Unauthorized(new { message = "Invalid credentials." });
        }

        // 4. Stamp last-login and issue token
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _users.UpdateAsync(user);

        var accessToken = await _jwt.GenerateAccessTokenAsync(user);
        var roles       = await _users.GetRolesAsync(user);

        return Ok(new LoginResponse(
            AccessToken:     accessToken,
            ExpiresInSeconds: (int)_jwtOpts.AccessTokenLifetime.TotalSeconds,
            UserId:          user.Id,
            UserName:        user.UserName ?? user.Id,
            Roles:           roles));
    }

}