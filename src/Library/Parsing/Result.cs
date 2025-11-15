namespace VideoGallery.Library.Parsing;

public abstract record Result<T>
{
    public static Result<T> Success(T value) => new Success<T>(value);
    public static Result<T> Failure(string error) => new Failure<T>(error);

    public abstract TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure);
    public T GetOrThrow<TException>(Func<string, TException> onFailure) where TException : Exception =>
        Match(e => e, e => throw onFailure(e));

public abstract void Match(Action<T> onSuccess, Action<string> onFailure);

    public bool IsSuccess => this is Success<T>;
    public bool IsFailure => this is Failure<T>;

    public Result<TResult> Map<TResult>(Func<T, TResult> mapper) => 
        Match(
            value => Result<TResult>.Success(mapper(value)),
            Result<TResult>.Failure);

    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder) =>
        Match(
            binder,
            Result<TResult>.Failure);
}