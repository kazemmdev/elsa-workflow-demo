using System.ComponentModel.DataAnnotations;

namespace ElsaWorkflow.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required, MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    public string? Issuer { get; set; } 

    public string? Audience { get; set; }

    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(15);

    public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(7);
}
