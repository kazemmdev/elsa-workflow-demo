namespace ElsaWorkflow.Identity;

public static class Roles
{
    public const string Admin    = "Admin";
    public const string Manager  = "Manager";
    public const string Finance  = "Finance";
    public const string Employee = "Employee";

    public static readonly IReadOnlyList<string> All = [Admin, Manager, Finance, Employee];
}
