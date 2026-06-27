using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ElsaWorkflow.Data.Entities;
using ElsaWorkflow.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ElsaWorkflow.Security;

public sealed class JwtTokenService
{
    private readonly JwtOptions _opts;
    private readonly UserManager<ApplicationUser> _users;

    public JwtTokenService(IOptions<JwtOptions> opts, UserManager<ApplicationUser> users)
    {
        _opts  = opts.Value;
        _users = users;
    }

    public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
    {
        var roles  = await _users.GetRolesAsync(user);
        var claims = BuildClaims(user, roles);

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:             string.IsNullOrWhiteSpace(_opts.Issuer)   ? null : _opts.Issuer,
            audience:           string.IsNullOrWhiteSpace(_opts.Audience) ? null : _opts.Audience,
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            DateTime.UtcNow.Add(_opts.AccessTokenLifetime),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    private static List<Claim> BuildClaims(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),

            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name,           user.UserName ?? user.Id),

            new(AppClaims.IsActive,    user.IsActive.ToString().ToLowerInvariant()),
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        return claims;
    }
}
