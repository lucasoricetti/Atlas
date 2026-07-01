using FluentValidation;

namespace Atlas.Backend.Application.DTOs.Entities;

public class SettingCreateDto
{
    public string? Name { get; set; }
    public List<string>? Links { get; set; }
    public string? Description { get; set; }
}

public class SettingCreateDtoValidator : AbstractValidator<SettingCreateDto>
{
    public SettingCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("The Name field is required.");

        RuleFor(x => x.Links)
            .NotEmpty().WithMessage("At least one Link is required.");

        RuleForEach(x => x.Links)
            .NotEmpty().WithMessage("A Link cannot be empty.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Each Link must be a valid absolute URL.");
    }
}

public class SettingReadDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required List<string> Links { get; set; }
    public string? Description { get; set; }
}