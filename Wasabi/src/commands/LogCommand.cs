using Wasabi.core;
using Wasabi.runtime;
using Wasabi.utils;

namespace Wasabi.commands;

/// <summary>
/// Handles all log.* commands (log.info, log.warn, log.error, etc.)
/// </summary>
public class LogCommand : IWasabiCommand
{
    public string CommandName => "log";
    
    public bool CanHandle(string line)
    {
        return line.StartsWith("log.");
    }
    
    public void Execute(string line, WasabiContext context)
    {
        var message = StringUtils.ExtractQuotedString(line);
        
        if(context._variables.ContainsKey("silent"))
        {
            if (context._variables["silent"] == "true")
                return;
        }
        
        if (line.StartsWith("log.info"))
        {
            WasabiLogger.Info(message);
        }
        else if (line.StartsWith("log.warn"))
        {
            WasabiLogger.Warn(message);
        }
        else if (line.StartsWith("log.error"))
        {
            WasabiLogger.Error(message);
        }
        else if (line.StartsWith("log.debug"))
        {
            WasabiLogger.Debug(message);
        }
        else if (line.StartsWith("log.success"))
        {
            WasabiLogger.Success(message);
        }
        else if (line.StartsWith("log.raw"))
        {
            WasabiLogger.Raw(message);
        }
        else
        {
            WasabiLogger.Error($"Unknown log command: {line}");
        }
    }
}

