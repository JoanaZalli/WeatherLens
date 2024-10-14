namespace WeatherLens.Application.Common.Models;

public class Result
{
    internal Result(bool succeeded, IEnumerable<string> errors, string? successMessage = null)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
        SuccessMessage = successMessage;
    }

    public bool Succeeded { get; init; }
    public string? SuccessMessage { get; init; }
    public string[] Errors { get; init; }

    public static Result Success()
    {
        return new Result(true, Array.Empty<string>());
    }
    public static Result Success(string message)
    {
        return new Result(true, Array.Empty<string>(), message);
    }
    public static Result Failure(IEnumerable<string> errors)
    {
        return new Result(false, errors);
    }
}

public class Result<T>
{
    public T? Value { get; }
    public string? Error { get; }
    public bool IsSuccess => Error == null;

    public Result(T value)
    {
        Value = value;
    }

    public Result(string error)
    {
        Error = error;
    }
}

