namespace Atlas.Backend.Core.Entities;

public class LoginType
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required bool Mfa { get; set; }
    public string? Protocol { get; set; }
}
