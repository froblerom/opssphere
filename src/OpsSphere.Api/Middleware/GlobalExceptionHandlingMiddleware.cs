using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OpsSphere.Api.Common;
using OpsSphere.Application.Common.Exceptions;
using OpsSphere.Application.Common.Interfaces;

namespace OpsSphere.Api.Middleware;

public sealed class GlobalExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items[CorrelationIdMiddleware.ItemsKey] as string ?? string.Empty;

        // Log safely — no passwords, tokens, secrets, or connection strings
        LogException(exception, correlationId, context);

        if (context.Response.HasStarted)
        {
            return;
        }

        var (statusCode, errorCode, message, details) = MapException(exception);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var envelope = new ApiErrorEnvelope(new ApiError(errorCode, message, details, correlationId));
        await context.Response.WriteAsync(JsonSerializer.Serialize(envelope, JsonOptions));
    }

    private void LogException(Exception exception, string correlationId, HttpContext context)
    {
        var userId = context.User?.FindFirst("sub")?.Value
            ?? context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        // Log internal exception details for diagnosis — never returned to callers
        logger.LogError(
            exception,
            "Unhandled exception. ExceptionType={ExceptionType} CorrelationId={CorrelationId} Path={Path} Method={Method} UserId={UserId}",
            exception.GetType().Name,
            correlationId,
            context.Request.Path,
            context.Request.Method,
            userId);
    }

    private static (int statusCode, string code, string message, IReadOnlyList<ApiErrorDetail>? details) MapException(
        Exception exception)
    {
        return exception switch
        {
            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                ErrorCodes.ValidationError,
                "The request contains validation errors.",
                ve.Failures.Select(f => new ApiErrorDetail(f.Field, f.Message)).ToList()),

            BusinessRuleException bre => (
                StatusCodes.Status400BadRequest,
                ErrorCodes.BusinessRuleViolation,
                bre.Message,
                null),

            NotFoundException nfe => (
                StatusCodes.Status404NotFound,
                ErrorCodes.NotFound,
                nfe.Message,
                null),

            ConflictException ce => (
                StatusCodes.Status409Conflict,
                ErrorCodes.Conflict,
                ce.Message,
                null),

            OperationCanceledException => (
                StatusCodes.Status400BadRequest,
                ErrorCodes.UnexpectedError,
                "The request was cancelled.",
                null),

            _ => (
                StatusCodes.Status500InternalServerError,
                ErrorCodes.UnexpectedError,
                "An unexpected error occurred. Please try again or contact support.",
                null)
        };
    }
}
