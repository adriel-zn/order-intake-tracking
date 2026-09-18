using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrderIntake.Domain.Orders;

namespace OrderIntake.Api.Validation;

public sealed class DomainValidationFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not DomainValidationException exception) return;
        context.Result = new BadRequestObjectResult(new ValidationProblemDetails(
            new Dictionary<string, string[]> { ["order"] = new[] { exception.Message } })
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        });
        context.ExceptionHandled = true;
    }
}
