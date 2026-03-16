using JobHunter.Api.Attributes;
using JobHunter.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JobHunter.Api.Filters;

public sealed class ApiResponseEnvelopeFilter : IAsyncResultFilter
{
    private const string DefaultSuccessMessage = "CALL API SUCCESS";

    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (ShouldBypass(context))
        {
            return next();
        }

        var apiMessage = ResolveApiMessage(context);

        context.Result = context.Result switch
        {
            ObjectResult objectResult => WrapObjectResult(objectResult, apiMessage),
            StatusCodeResult statusCodeResult => WrapStatusCodeResult(statusCodeResult, apiMessage),
            EmptyResult => WrapStatusCodeResult(new StatusCodeResult(context.HttpContext.Response.StatusCode), apiMessage),
            ContentResult contentResult => WrapContentResult(contentResult, apiMessage),
            _ => context.Result
        };

        return next();
    }

    private static bool ShouldBypass(ResultExecutingContext context)
    {
        var path = context.HttpContext.Request.Path;
        if (path.StartsWithSegments("/swagger") || path.StartsWithSegments("/v3/api-docs"))
        {
            return true;
        }

        return context.Result is FileResult;
    }

    private static string ResolveApiMessage(ResultExecutingContext context)
    {
        var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
        var attribute = actionDescriptor?.MethodInfo.GetCustomAttributes(typeof(ApiMessageAttribute), true)
            .OfType<ApiMessageAttribute>()
            .FirstOrDefault();

        return attribute?.Value ?? DefaultSuccessMessage;
    }

    private static ObjectResult WrapObjectResult(ObjectResult result, string apiMessage)
    {
        if (result.Value is IApiResponse)
        {
            return result;
        }

        var statusCode = result.StatusCode ?? StatusCodes.Status200OK;
        if (statusCode >= StatusCodes.Status400BadRequest)
        {
            return new ObjectResult(ApiErrorEnvelopeFactory.CreateFromResultPayload(statusCode, result.Value))
            {
                StatusCode = statusCode
            };
        }

        return new ObjectResult(ApiResponse<object>.Success(statusCode, result.Value, apiMessage))
        {
            StatusCode = statusCode
        };
    }

    private static ObjectResult WrapStatusCodeResult(StatusCodeResult result, string apiMessage)
    {
        if (result.StatusCode >= StatusCodes.Status400BadRequest)
        {
            return new ObjectResult(ApiErrorEnvelopeFactory.CreateFromResultPayload(result.StatusCode, null))
            {
                StatusCode = result.StatusCode
            };
        }

        return new ObjectResult(ApiResponse<object>.Success<object>(result.StatusCode, null, apiMessage))
        {
            StatusCode = result.StatusCode
        };
    }

    private static ObjectResult WrapContentResult(ContentResult result, string apiMessage)
    {
        var statusCode = result.StatusCode ?? StatusCodes.Status200OK;
        if (statusCode >= StatusCodes.Status400BadRequest)
        {
            return new ObjectResult(ApiErrorEnvelopeFactory.CreateFromResultPayload(statusCode, result.Content))
            {
                StatusCode = statusCode
            };
        }

        return new ObjectResult(ApiResponse<object>.Success(statusCode, result.Content, apiMessage))
        {
            StatusCode = statusCode
        };
    }

}
