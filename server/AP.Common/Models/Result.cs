namespace AP.Common.Models;

public class Result
{
    private readonly List<string> errors;

    internal Result(bool succeeded, List<string> errors)
    {
        Succeeded = succeeded;
        this.errors = errors;
    }

    public static Result Success
        => new(true, []);

    public bool Succeeded { get; }

    public List<string> Errors
        => Succeeded
            ? []
            : errors;

    public static Result Failure(List<string> errors)
        => new(false, errors);

    public static Result Failure(string error)
        => new(false, [error]);
}

public class Result<TData> : Result
{
    private readonly TData data;

    private Result(bool succeeded, TData data, List<string> errors)
        : base(succeeded, errors)
        => this.data = data;

    public TData Data => data;

    public static Result<TData> SuccessWith(TData data)
        => new(true, data, []);

    public static new Result<TData> Failure(string error)
        => new(false, default!, [error]);

    public static new Result<TData> Failure(List<string> errors)
        => new(false, default!, errors);

    public static Result<TData> FailureWith(TData data)
        => new(false, data, []);

    public static Result<TData> FailureWith(TData data, List<string> errors)
        => new(false, data, errors);
}