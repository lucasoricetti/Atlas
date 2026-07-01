using FluentValidation;
using Atlas.Backend.Core.Enums;

namespace Atlas.Backend.Application.DTOs.Entities;

public class VirtualMachineCreateDto
{
    public string? Name { get; set; }
    public VirtualMachineType? Type { get; set; }
    public string? Ip { get; set; }
    public string? Cluster { get; set; }
    public string? Role { get; set; }
}

public class VirtualMachineCreateDtoValidator : AbstractValidator<VirtualMachineCreateDto>
{
    public VirtualMachineCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("The Name field is required.");

        RuleFor(x => x.Type)
            .NotNull().WithMessage("The Type field is required.");

        RuleFor(x => x.Ip)
            .Matches(@"^(\d{1,3}\.){3}\d{1,3}$").WithMessage("The Ip field must be a valid IP address.");
    }
}

public class VirtualMachineReadDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required VirtualMachineType Type { get; set; }
    public string? Ip { get; set; }
    public string? Cluster { get; set; }
    public string? Role { get; set; }
}
