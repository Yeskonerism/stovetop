using Wasabi.commands;

namespace Wasabi.core;

/// <summary>
/// Executes Wasabi commands using registered command handlers.
/// </summary>
public class WasabiExecutor
{
    private readonly List<IWasabiCommand> _commands;
    
    public WasabiExecutor()
    {
        _commands = new List<IWasabiCommand>();
        RegisterDefaultCommands();
    }
    
    /// <summary>
    /// Registers all default Wasabi commands.
    /// </summary>
    private void RegisterDefaultCommands()
    {
        _commands.Add(new LogCommand());
        _commands.Add(new ShellCommand());
        _commands.Add(new SetCommand());
    }
    
    /// <summary>
    /// Registers a custom command.
    /// </summary>
    /// <param name="command">The command to register</param>
    public void RegisterCommand(IWasabiCommand command)
    {
        _commands.Add(command);
    }
    
    /// <summary>
    /// Executes a single line of Wasabi code.
    /// </summary>
    /// <param name="line">The line to execute</param>
    /// <param name="context">The execution context</param>
    public void ExecuteLine(string line, WasabiContext context)
    {
        // Substitute variables in the line
        line = context.SubstituteVariables(line);
        
        // Find and execute the appropriate command
        foreach (var command in _commands)
        {
            if (command.CanHandle(line))
            {
                command.Execute(line, context);
                return;
            }
        }
        
        // No command found
        Console.WriteLine($"[WASABI] Unknown command: {line}");
    }
}

