namespace Domain.Shared;

public class Result
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; private set; }
    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
    public static Result Success() => new Result(true, null);
    public static Result Failure(Error error) => new Result(false, error);
}

public class Result<T> : Result
{
    public T? Value { get; private set; }
    protected Result(bool isSuccess, Error? error, T? value) : base(isSuccess, error)
    {
        Value = value;
    }
    public static Result<T> Success(T value) => new Result<T>(true, null, value);
    public static new Result<T> Failure(Error error) => new Result<T>(false, error, default);
}