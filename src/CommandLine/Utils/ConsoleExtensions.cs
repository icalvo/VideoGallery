namespace VideoGallery.CommandLine.Utils;

public static class ConsoleExtensions
{
    public static IDisposable Color(ConsoleColor fg, ConsoleColor bg) => new ColorDisposable(fg, bg);
    
    private class ColorDisposable : IDisposable
    {
        private readonly ConsoleColor _formerFgColor;
        private readonly ConsoleColor _formerBgColor;

        public ColorDisposable(ConsoleColor fg, ConsoleColor bg)
        {
            _formerFgColor = Console.ForegroundColor;
            _formerBgColor = Console.BackgroundColor;
            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
        }

        public void Dispose()
        {
            Console.ForegroundColor = _formerFgColor;
            Console.BackgroundColor = _formerBgColor;
        }
    }
}