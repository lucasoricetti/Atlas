namespace Atlas.Backend.Core.Entities;

public class Setting
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required List<string> Links { get; set; } = [];
    public string? Description { get; set; }
}