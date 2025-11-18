using System.Text.RegularExpressions;

namespace Stovetop.stovetop.handlers;

public static class StovetopVariableHandler
{
    /// <summary>
    /// Substitutes variables in a command string using ${VAR} or ${VAR:-default} syntax
    /// </summary>
    /// <param name="command">The command string with variables</param>
    /// <param name="variables">Dictionary of variable names and values</param>
    /// <returns>Command string with variables substituted</returns>
    public static string SubstituteVariables(string command, Dictionary<string, string>? variables)
    {
        if (string.IsNullOrEmpty(command) || variables == null || variables.Count == 0)
            return command;

        // Pattern matches ${VAR} or ${VAR:-default}
        string pattern = @"\$\{([A-Za-z_][A-Za-z0-9_]*)(:-([^}]*))?\}";
        
        return Regex.Replace(command, pattern, match =>
        {
            string varName = match.Groups[1].Value;
            string? defaultValue = match.Groups[3].Success ? match.Groups[3].Value : null;
            
            // Check if variable exists in dictionary
            if (variables.TryGetValue(varName, out string? value) && !string.IsNullOrEmpty(value))
            {
                return value;
            }
            
            // Check environment variables
            string? envValue = Environment.GetEnvironmentVariable(varName);
            if (!string.IsNullOrEmpty(envValue))
            {
                return envValue;
            }
            
            // Use default value if provided
            if (defaultValue != null)
            {
                return defaultValue;
            }
            
            // If no value found and no default, keep the original
            StovetopCore.StovetopLogger?.Warn($"Variable '{varName}' not found, keeping original");
            return match.Value;
        });
    }
    
    /// <summary>
    /// Substitutes variables in a command using the current config's variables
    /// </summary>
    public static string SubstituteVariables(string command)
    {
        return SubstituteVariables(command, StovetopCore.StovetopConfig?.Stovetop.Variables);
    }
}

