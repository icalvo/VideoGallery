using Microsoft.Extensions.Logging;
using NuGet.Common;
using LogLevel = NuGet.Common.LogLevel;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace VideoGallery.Library;

internal class LoggerProxy : NuGet.Common.ILogger
{
    private readonly Microsoft.Extensions.Logging.ILogger _loggerImplementation;

    public LoggerProxy(Microsoft.Extensions.Logging.ILogger logger)
    {
        _loggerImplementation = logger;
    }

    public void LogDebug(string data)
    {
        _loggerImplementation.LogTrace("{Message}", data);
    }

    public void LogVerbose(string data)
    {
        _loggerImplementation.LogDebug("{Message}", data);
    }

    public void LogInformation(string data)
    {
        _loggerImplementation.LogInformation("{Message}", data);
    }

    public void LogMinimal(string data)
    {
        _loggerImplementation.LogInformation("Minimal: {Message}", data);
    }

    public void LogWarning(string data)
    {
        _loggerImplementation.LogWarning("{Message}", data);
    }

    public void LogError(string data)
    {
        _loggerImplementation.LogError("{Message}", data);
    }

    public void LogInformationSummary(string data)
    {
        _loggerImplementation.LogInformation("Summary: {Message}", data);
    }

    public void Log(LogLevel level, string data)
    {
        _loggerImplementation.Log(Convert(level), "{Message}", data);
    }

    public Task LogAsync(LogLevel level, string data)
    {
        Log(level, data);
        return Task.CompletedTask;
    }

    private static MsLogLevel Convert(LogLevel level)
    {
        var newLevel = level switch
        {
            LogLevel.Debug => MsLogLevel.Trace,
            LogLevel.Verbose => MsLogLevel.Debug,
            LogLevel.Information => MsLogLevel.Information,
            LogLevel.Minimal => MsLogLevel.Information,
            LogLevel.Warning => MsLogLevel.Warning,
            LogLevel.Error => MsLogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
        return newLevel;
    }

    public void Log(ILogMessage message)
    {
        _loggerImplementation.Log(Convert(message.Level), new EventId((int)message.Code, message.Code.ToString()), "{Message}", message.Message);
    }

    public Task LogAsync(ILogMessage message)
    {
        Log(message);
        return Task.CompletedTask;
    }
}