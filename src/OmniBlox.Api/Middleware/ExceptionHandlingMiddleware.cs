using System.Net;
using System.Text.Json;
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
        var (statusCode, message) = ex switch
        {
            UnauthorizedException => (HttpStatusCode.Unauthorized, ex.Message),
            NotFoundException => (HttpStatusCode.NotFound, ex.Message),
            ConflictException => (HttpStatusCode.Conflict, ex.Message),
            FluentValidation.ValidationException validationEx
                => (HttpStatusCode.BadRequest, validationEx.Errors.First().ErrorMessage),
            _ => (HttpStatusCode.InternalServerError, "An error occurred.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(new
        {
            error = message,
            statusCode = (int)statusCode
        });

        await context.Response.WriteAsync(result);
    }
}
