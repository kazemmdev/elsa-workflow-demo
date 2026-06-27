using Microsoft.AspNetCore.Identity;

namespace ElsaWorkflow.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTimeOffset CreatedAt    { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
}