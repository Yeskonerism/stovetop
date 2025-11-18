namespace Stovetop.stovetop.config;

public class CommandsConfig
{
    public string Build { get; set; } = "";
    public string Run { get; set; } = "";
    public string? Executable { get; set; }
    public string? Test { get; set; }
    public string? Clean { get; set; }
    public string? Deploy { get; set; }
}