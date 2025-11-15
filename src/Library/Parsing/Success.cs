namespace VideoGallery.Library.Parsing;

public sealed record Success<T>(T Value) : Result<T>
{
    public override TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        => onSuccess(Value);

    public override void Match(Action<T> onSuccess, Action<string> onFailure)
        => onSuccess(Value);
}