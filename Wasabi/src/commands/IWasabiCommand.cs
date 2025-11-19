using Wasabi.core;

namespace Wasabi.commands;

/// <summary>
/// Interface for all Wasabi commands.
/// Implement this interface to create custom Wasabi commands.
/// </summary>
public interface IWasabiCommand
{
    /// <summary>
    /// The command name (e.g., "log.info", "shell", "var.set")
    /// </summary>
    string CommandName { get; }
    
    /// <summary>
    /// Checks if this command can handle the given line.
    /// </summary>
    /// <param name="line">The line to check</param>
    /// <returns>True if this command can handle the line</returns>
    bool CanHandle(string line);
    
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="line">The full line to execute</param>
    /// <param name="context">The current Wasabi execution context</param>
    void Execute(string line, WasabiContext context);
}

