namespace Stovetop.stovetop;

public class StovetopCommand
{
    public string? Name;
    public string? Description;
    public string? Usage;
    public string? Category;
    
    public string[]? Aliases;
    
    public Action? Command;
}