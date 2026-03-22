namespace FlowFi.Domain.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    private Result(bool isSuccess, T? value, string? error, string? errorCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result<T> Success(T value) => new(true, value, null, null);
    public static Result<T> Failure(string error, string errorCode = "ERROR") => new(false, default, error, errorCode);

    public static Result<T> NotFound(string resource) =>
        Failure($"{resource} not found", "NOT_FOUND");

    public static Result<T> Unauthorized(string message = "Authentication required") =>
        Failure(message, "UNAUTHORIZED");

    public static Result<T> Forbidden(string message = "Access denied") =>
        Failure(message, "FORBIDDEN");

    public static Result<T> Conflict(string message) =>
        Failure(message, "CONFLICT");
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    private Result(bool isSuccess, string? error, string? errorCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string error, string errorCode = "ERROR") => new(false, error, errorCode);
    public static Result NotFound(string resource) => Failure($"{resource} not found", "NOT_FOUND");
    public static Result Unauthorized(string msg = "Authentication required") => Failure(msg, "UNAUTHORIZED");
}
