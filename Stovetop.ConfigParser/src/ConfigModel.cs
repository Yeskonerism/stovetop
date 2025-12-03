namespace Stovetop.ConfigParser;

/// <summary>
/// Represents the parsed configuration from a .stove file
/// </summary>
public class ConfigModel
{
    public string Project { get; set; } = "";
    public string Version { get; set; } = "";
    public string Runtime { get; set; } = "";
    public string? RuntimeVersion { get; set; }

    public Dictionary<string, string> Variables { get; set; } = new();
    public Dictionary<string, string> Commands { get; set; } = new();
    public Dictionary<string, string> Aliases { get; set; } = new();
    public Dictionary<string, string> Hooks { get; set; } = new();
    public Dictionary<string, string> Scripts { get; set; } = new();

    /// <summary>
    /// Resolves variable references in a string value
    /// </summary>
    public string ResolveVariables(string value)
    {
        foreach (var variable in Variables)
        {
            value = value.Replace($"{{{variable.Key}}}", variable.Value);
        }
        return value;
    }
}