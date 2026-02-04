namespace FIXIT.Domain.Abstractions;

public class Result<T>
{
    private Result(bool IsSuccess,T Value, Error error)
    {
        if (IsSuccess && error != Error.None || !IsSuccess && error == Error.None)
            throw new ArgumentException("A result cannot be successful and contain an error");

        this.IsSuccess = IsSuccess;
        this.Error = error;
        this.Value = Value;
    }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    public T Value { get; set; }
    public static Result<T> Success(T Value) => new(true,Value ,Error.None);
    public static Result<T> Failure(Error error) => new(false,default!, error);
}
