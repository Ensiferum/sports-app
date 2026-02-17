namespace SportsAggregator.Domain.Results;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Failure,
    Unexpected
}

public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure);

public sealed class Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        _error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value when result is failure.");

    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error when result is success.");

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Error error) => new(error);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value) : onFailure(Error);
    }
}
