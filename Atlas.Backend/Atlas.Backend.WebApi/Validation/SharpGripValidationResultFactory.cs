using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace Atlas.Backend.WebApi.Validation;

public class SharpGripValidationResultFactory : IFluentValidationAutoValidationResultFactory
{
    public Task<IActionResult?> CreateActionResult(
        ActionExecutingContext context,
        ValidationProblemDetails validationProblemDetails,
        IDictionary<IValidationContext, ValidationResult> validationResults)
    {
        validationProblemDetails.Status = StatusCodes.Status400BadRequest;
        validationProblemDetails.Title = "Bad Request";
        validationProblemDetails.Detail = "Uno o più errori di validazione si sono verificati.";
        validationProblemDetails.Instance = context.HttpContext.Request.Path;

        IActionResult result = new BadRequestObjectResult(validationProblemDetails);
        return Task.FromResult<IActionResult?>(result);
    }
}