using Stovetop.Commands;
using Stovetop.stovetop;
using Wasabi;

namespace Stovetop.commands.user;

/// <summary>
/// Executes Wasabi scripts with access to Stovetop variables and configuration.
/// </summary>
public class ScriptCommand
{
    public static void Run()
    {
        string? scriptPath = CommandRegistry.GetPositionalArgument("script", 1);

        if (string.IsNullOrEmpty(scriptPath))
        {
            StovetopCore.StovetopLogger?.Error("No script file specified");
            StovetopCore.StovetopLogger?.Info("Usage: stove script <path-to-script.wasabi>");
            return;
        }

        ExecuteScript(scriptPath);
    }

    /// <summary>
    /// Executes a Wasabi script with Stovetop context (variables, project info, etc.)
    /// </summary>
    /// <param name="scriptPath">Path to the .wasabi script file</param>
    public static void ExecuteScript(string scriptPath)
    {
        if (!File.Exists(scriptPath))
        {
            StovetopCore.StovetopLogger?.Error($"Script not found: {scriptPath}");
            return;
        }

        try
        {
            // Build variable dictionary from Stovetop config
            var variables = new Dictionary<string, string>();

            // Add user-defined variables from config
            if (StovetopCore.StovetopConfig?.Stovetop.Variables != null)
            {
                foreach (var (key, value) in StovetopCore.StovetopConfig.Stovetop.Variables)
                {
                    variables[key] = value;
                }
            }

            // Add built-in Stovetop variables
            variables["PROJECT"] = StovetopCore.StovetopConfig?.Project ?? "";
            variables["VERSION"] = StovetopCore.StovetopConfig?.Version ?? "";
            variables["RUNTIME"] = StovetopCore.StovetopConfig?.Stovetop.Runtime.Type ?? "";
            variables["RUNTIME_VERSION"] = StovetopCore.StovetopConfig?.Stovetop.Runtime.Version ?? "";

            // Add current working directory
            variables["CWD"] = Directory.GetCurrentDirectory();
            variables["SCRIPT_DIR"] = Path.GetDirectoryName(Path.GetFullPath(scriptPath)) ?? "";

            if (StovetopCore.RunVerbose)
            {
                StovetopCore.StovetopLogger?.Debug($"Executing Wasabi script: {scriptPath}");
                StovetopCore.StovetopLogger?.Debug($"Available variables: {string.Join(", ", variables.Keys)}");
            }

            // Execute the Wasabi script
            WasabiInterpreter.Execute(scriptPath, variables);

            if (StovetopCore.RunVerbose)
                StovetopCore.StovetopLogger?.Success("Script execution complete");
        }
        catch (Exception ex)
        {
            StovetopCore.StovetopLogger?.Error($"Script execution failed: {ex.Message}");
            if (StovetopCore.RunVerbose)
                StovetopCore.StovetopLogger?.Debug($"Stack trace: {ex.StackTrace}");
        }
    }
}