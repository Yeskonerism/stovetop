namespace Stovetop.ConfigParser;

public class ConfigModel
{
    public string Project { get; set; } = "";
    public string Version { get; set; } = "";
    public string Runtime { get; set; } = "";
    public Dictionary<string, string> Variables { get; set; } = new();
    public Dictionary<string, string> Commands { get; set; } = new();
}