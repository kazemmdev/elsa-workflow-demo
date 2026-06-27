using Microsoft.AspNetCore.Identity;

namespace ElsaWorkflow.Data.Entities;

public class ApplicationRole : IdentityRole
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }

    public string? Description { get; set; }
}