using Wasabi.core;

namespace Wasabi;

/// <summary>
/// Main entry point for the Wasabi scripting language interpreter.
/// Orchestrates parsing and execution of Wasabi scripts.
/// </summary>
public static class WasabiInterpreter
{
    /// <summary>
    /// Executes a Wasabi script file.
    /// </summary>
    /// <param name="scriptPath">Path to the Wasabi script file</param>
    /// <param name="variables">Optional dictionary of variables to make available in the script</param>
    public static void Execute(string scriptPath, Dictionary<string, string>? variables = null)
    {
        // Parse the script file
        var lines = WasabiParser.ParseFile(scriptPath);
        if (lines == null)
            return;

        // Create execution context with variables
        var context = new WasabiContext(variables ?? new Dictionary<string, string>());

        // Create executor with registered commands
        var executor = new WasabiExecutor();

        // Execute each line
        foreach (var rawLine in lines)
        {
            var line = WasabiParser.NormalizeLine(rawLine);

            // Skip empty lines and comments
            if (WasabiParser.ShouldSkipLine(line))
                continue;

            // Execute the line
            executor.ExecuteLine(line, context);
        }
    }
}