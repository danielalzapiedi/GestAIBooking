using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Middleware;

public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException vex)
        {
            logger.LogWarning(vex, "Validation error");

            var details = new ValidationProblemDetails(
                vex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation error"
            };

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(details);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error");

            var details = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server error",
                Detail = "Ocurrió un error inesperado."
            };

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(details);
        }
    }
}

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionMiddleware>();
}
