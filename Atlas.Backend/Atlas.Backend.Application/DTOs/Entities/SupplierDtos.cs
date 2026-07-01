using FluentValidation;

namespace Atlas.Backend.Application.DTOs.Entities;

public class SupplierCreateDto
{
    public string? Name { get; set; }
}

public class SupplierCreateDtoValidator : AbstractValidator<SupplierCreateDto>
{
    public SupplierCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("The Name field is required.");
    }
}

public class SupplierReadDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
}
