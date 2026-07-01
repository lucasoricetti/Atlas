using FluentValidation;
using Atlas.Backend.Core.Enums;

namespace Atlas.Backend.Application.DTOs.Entities;

public class CloudProviderCreateDto
{
    public string? Name { get; set; }
    public CloudProviderType? Type { get; set; }
    public string? PortalUrl { get; set; }
    public string? Account { get; set; }
}

public class CloudProviderCreateDtoValidator : AbstractValidator<CloudProviderCreateDto>
{
    public CloudProviderCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("The Name field is required.");

        RuleFor(x => x.Type)
            .NotNull().WithMessage("The Type field is required.");

        RuleFor(x => x.PortalUrl)
            .Must(x => x == null || Uri.TryCreate(x, UriKind.Absolute, out _))
            .WithMessage("The PortalUrl field must be a valid absolute URL.");
    }
}

public class CloudProviderReadDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required CloudProviderType Type { get; set; }
    public string? PortalUrl { get; set; }
    public string? Account { get; set; }
}
