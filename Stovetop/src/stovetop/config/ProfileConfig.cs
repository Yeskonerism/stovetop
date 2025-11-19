namespace Stovetop.stovetop.config;

public class ProfileConfig
{
    public CommandsConfig Commands { get; set; } = new();
    public Dictionary<string, string>? Variables { get; set; }
}