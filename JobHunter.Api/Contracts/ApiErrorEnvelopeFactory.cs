using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JobHunter.Api.Contracts;

public sealed record ValidationFieldError(string Field, string Message);

public static class ApiErrorEnvelopeFactory
{
    public static ApiResponse<object> CreateFromModelState(ModelStateDictionary modelState)
    {
        var fieldErrors = modelState
            .Where(entry => entry.Value is not null)
            .SelectMany(entry => entry.Value!.Errors.Select(error => new ValidationFieldError(
                string.IsNullOrWhiteSpace(entry.Key) ? "general" : entry.Key,
                string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Validation failed." : error.ErrorMessage)))
            .ToList();

        object message = fieldErrors.Count > 0
            ? fieldErrors
            : "Validation failed.";

        return ApiResponse<object>.ErrorResponse(
            StatusCodes.Status400BadRequest,
            ResolveError(StatusCodes.Status400BadRequest),
            message);
    }

    public static ApiResponse<object> CreateFromException(Exception exception, int statusCode)
    {
        if (exception is ValidationException validationException)
        {
            return CreateFromValidationException(validationException);
        }

        object message = statusCode == StatusCodes.Status500InternalServerError
            ? "An unexpected server error occurred."
            : exception.Message;

        return ApiResponse<object>.ErrorResponse(statusCode, ResolveError(statusCode), message);
    }

    public static ApiResponse<object> CreateFromResultPayload(int statusCode, object? payload)
    {
        if (payload is ValidationProblemDetails validationProblem)
        {
            var fieldErrors = validationProblem.Errors
                .SelectMany(entry => entry.Value.Select(error => new ValidationFieldError(
                    string.IsNullOrWhiteSpace(entry.Key) ? "general" : entry.Key,
                    string.IsNullOrWhiteSpace(error) ? "Validation failed." : error)))
                .ToList();

            object message = fieldErrors.Count > 0
                ? fieldErrors
                : "Validation failed.";

            return ApiResponse<object>.ErrorResponse(statusCode, ResolveError(statusCode), message);
        }

        if (payload is ProblemDetails problemDetails)
        {
            var message = problemDetails.Detail ?? problemDetails.Title ?? GetReasonPhrase(statusCode);
            return ApiResponse<object>.ErrorResponse(statusCode, ResolveError(statusCode), message);
        }

        if (payload is string messageText && !string.IsNullOrWhiteSpace(messageText))
        {
            return ApiResponse<object>.ErrorResponse(statusCode, ResolveError(statusCode), messageText);
        }

        var fallbackMessage = payload ?? GetReasonPhrase(statusCode);
        return ApiResponse<object>.ErrorResponse(statusCode, ResolveError(statusCode), fallbackMessage);
    }

    public static string ResolveError(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Exception occurs...",
            StatusCodes.Status403Forbidden => "Permission denied...",
            StatusCodes.Status500InternalServerError => "Internal server error...",
            _ => GetReasonPhrase(statusCode)
        };
    }

    private static ApiResponse<object> CreateFromValidationException(ValidationException exception)
    {
        var memberNames = exception.ValidationResult?.MemberNames?.ToList() ?? [];
        var fieldErrors = memberNames.Count > 0
            ? memberNames.Select(memberName => new ValidationFieldError(
                string.IsNullOrWhiteSpace(memberName) ? "general" : memberName,
                string.IsNullOrWhiteSpace(exception.ValidationResult?.ErrorMessage)
                    ? exception.Message
                    : exception.ValidationResult!.ErrorMessage!)).ToList()
            : [new ValidationFieldError("general", exception.ValidationResult?.ErrorMessage ?? exception.Message)];

        return ApiResponse<object>.ErrorResponse(
            StatusCodes.Status400BadRequest,
            ResolveError(StatusCodes.Status400BadRequest),
            fieldErrors);
    }

    private static string GetReasonPhrase(int statusCode)
    {
        return Enum.IsDefined(typeof(HttpStatusCode), statusCode)
            ? ((HttpStatusCode)statusCode).ToString().Replace('_', ' ')
            : "Request failed.";
    }
}
