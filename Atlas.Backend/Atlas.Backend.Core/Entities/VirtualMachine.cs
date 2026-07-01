using Atlas.Backend.Core.Enums;

namespace Atlas.Backend.Core.Entities;

public class VirtualMachine
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required VirtualMachineType Type { get; set; }
    public string? Ip { get; set; }
    public string? Cluster { get; set; }
    public string? Role { get; set; }
}
