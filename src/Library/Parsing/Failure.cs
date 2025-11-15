namespace VideoGallery.Library.Parsing;

public sealed record Failure<T>(string Error) : Result<T>
{
    public override TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        => onFailure(Error);

    public override void Match(Action<T> onSuccess, Action<string> onFailure)
        => onFailure(Error);
}