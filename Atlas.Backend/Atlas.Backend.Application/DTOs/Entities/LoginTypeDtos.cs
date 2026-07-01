using FluentValidation;

namespace Atlas.Backend.Application.DTOs.Entities;

public class LoginTypeCreateDto
{
    public string? Name { get; set; }
    public bool? Mfa { get; set; }
    public string? Protocol { get; set; }
}

public class LoginTypeCreateDtoValidator : AbstractValidator<LoginTypeCreateDto>
{
    public LoginTypeCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("The Name field is required.");

        RuleFor(x => x.Mfa)
            .NotNull().WithMessage("The Mfa field is required.");
    }
}

public class LoginTypeReadDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required bool Mfa { get; set; }
    public string? Protocol { get; set; }
}
