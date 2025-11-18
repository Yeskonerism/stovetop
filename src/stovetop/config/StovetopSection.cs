namespace Stovetop.stovetop.config;

public class StovetopSection
{
    public Dictionary<string, string> Variables { get; set; } = new();
    public RuntimeConfig Runtime { get; set; } = new();
    public CommandsConfig Commands { get; set; } = new();
    public Dictionary<string, string> Aliases { get; set; } = new();
    public HooksConfig? Hooks { get; set; }
    public Dictionary<string, ProfileConfig>? Profiles { get; set; }
}