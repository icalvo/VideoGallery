using System.Diagnostics.CodeAnalysis;

namespace VideoGallery.CommandLine.Utils.Parsing;

public record ParseStatus<T, TSeq>
{
    public ParseStatus(T value, TSeq[] args, string? errorMessage = null)
    {
        _value = value;
        HasValue = true;
        Args = args;
        ErrorMessage = errorMessage;
    }
    public ParseStatus(TSeq[] args, string? errorMessage = null)
    {
        _value = default;
        HasValue = false;
        Args = args;
        ErrorMessage = errorMessage;
    }

    public bool IsError => ErrorMessage != null;

    [MemberNotNullWhen(true, "_value")]
    public bool HasValue { get; init; }
    
    private readonly T? _value;

    public T Value
    {
        get => HasValue ? _value : throw new ArgumentException();
        init
        {
            HasValue = true;
            _value = value;
        }
    }

    public TSeq[] Args { get; init; }
    public string? ErrorMessage { get; init; }
}

public record ParseStatus<T> : ParseStatus<T, string>
{
    public ParseStatus(string[] Args, string? ErrorMessage = null)
        : base(Args, ErrorMessage)
    {
    }

    public ParseStatus(T Value, string[] Args, string? ErrorMessage = null) : base(Value, Args, ErrorMessage)
    {
    }
}