using System.ComponentModel.DataAnnotations;

namespace ElsaWorkflow.Controllers;

public sealed record LoginRequest(
    [Required] string UserName,
    [Required] string Password);

public sealed record LoginResponse(
    string AccessToken,
    int ExpiresInSeconds,
    string UserId,
    string UserName,
    IEnumerable<string> Roles);
