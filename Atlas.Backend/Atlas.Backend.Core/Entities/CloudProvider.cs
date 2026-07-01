using Atlas.Backend.Core.Enums;

namespace Atlas.Backend.Core.Entities;

public class CloudProvider
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required CloudProviderType Type { get; set; }
    public string? PortalUrl { get; set; }
    public string? Account { get; set; }
}
