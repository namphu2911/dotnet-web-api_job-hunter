namespace JobHunter.Api.Contracts;

public interface IApiResponse
{
    int StatusCode { get; }
}

public sealed class ApiResponse<T> : IApiResponse
{
    public int StatusCode { get; init; }

    public string? Error { get; init; }

    public object? Message { get; init; }

    public T? Data { get; init; }

    public static ApiResponse<TData> Success<TData>(int statusCode, TData? data, object? message)
    {
        return new ApiResponse<TData>
        {
            StatusCode = statusCode,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<object> ErrorResponse(int statusCode, string? error, object? message)
    {
        return new ApiResponse<object>
        {
            StatusCode = statusCode,
            Error = error,
            Message = message,
            Data = null
        };
    }
}
