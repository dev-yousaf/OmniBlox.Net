using System.Net;
using System.Text.Json;
using FluentValidation;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
#if DEBUG
        Console.Error.WriteLine($"[Exception] {ex.GetType().Name}: {ex.Message}");
        Console.Error.WriteLine(ex.StackTrace);
#endif

        context.Response.ContentType = "application/json";

        switch (ex)
        {
            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var errors = validationEx.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    message = e.ErrorMessage,
                    attemptedValue = e.AttemptedValue?.ToString()
                });
                var result = JsonSerializer.Serialize(new
                {
                    message = "Validation failed",
                    statusCode = (int)HttpStatusCode.BadRequest,
                    errors
                });
                await context.Response.WriteAsync(result);
                return;

            case UnauthorizedException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;

            case NotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;

            case ConflictException:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var body = JsonSerializer.Serialize(new
        {
            message = ex switch
            {
                UnauthorizedException => ex.Message,
                NotFoundException => ex.Message,
                ConflictException => ex.Message,
                _ => "An error occurred."
            },
            statusCode = context.Response.StatusCode
        });

        if (ex is not ValidationException)
            await context.Response.WriteAsync(body);
    }
}
