using Wasabi.core;
using Wasabi.runtime;
using Wasabi.utils;

namespace Wasabi.commands;

/// <summary>
/// Handles shell command execution.
/// </summary>
public class ShellCommand : IWasabiCommand
{
    public string CommandName => "shell";
    
    public bool CanHandle(string line)
    {
        return line.StartsWith("shell");
    }
    
    public void Execute(string line, WasabiContext context)
    {
        var command = StringUtils.ExtractQuotedString(line);
        
        if (string.IsNullOrEmpty(command))
        {
            WasabiLogger.Error("Shell command cannot be empty");
            return;
        }
        
        WasabiShell.Execute(command);
    }
}

