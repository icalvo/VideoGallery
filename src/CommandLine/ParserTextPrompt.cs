using System.Globalization;
using Spectre.Console;

namespace VideoGallery.CommandLine;

public class ParserTextPrompt<T> : IPrompt<T>
{
    private readonly Func<string, (T, string?)> _parser;
    private readonly TextPrompt<string> _promptImplementation;

    public ParserTextPrompt(string promptText, Func<string, (T, string?)> parser)
    {
        _parser = parser;
        _promptImplementation = new TextPrompt<string>(promptText)
        {
            Validator = s =>
            {
                var (_, msg) = _parser(s);
                return msg is null
                    ? ValidationResult.Success()
                    : ValidationResult.Error(msg);
            }
        };
    }

    public bool AllowEmpty
    {
        get => _promptImplementation.AllowEmpty;
        set => _promptImplementation.AllowEmpty = value;
    }

    public Style? PromptStyle
    {
        get => _promptImplementation.PromptStyle;
        set => _promptImplementation.PromptStyle = value;
    }

    public CultureInfo? Culture
    {
        get => _promptImplementation.Culture;
        set => _promptImplementation.Culture = value;
    }

    public bool IsSecret
    {
        get => _promptImplementation.IsSecret;
        set => _promptImplementation.IsSecret = value;
    }

    public char? Mask
    {
        get => _promptImplementation.Mask;
        set => _promptImplementation.Mask = value;
    }

    public bool ShowDefaultValue
    {
        get => _promptImplementation.ShowDefaultValue;
        set => _promptImplementation.ShowDefaultValue = value;
    }

    public Style? DefaultValueStyle
    {
        get => _promptImplementation.DefaultValueStyle;
        set => _promptImplementation.DefaultValueStyle = value;
    }

    public T Show(IAnsiConsole console)
    {
        var result = _promptImplementation.Show(console);
        return _parser(result).Item1;
    }

    public async Task<T> ShowAsync(IAnsiConsole console, CancellationToken cancellationToken)
    {
        var result = await _promptImplementation.ShowAsync(console, cancellationToken);
        return _parser(result).Item1;
    }
}