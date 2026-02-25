namespace Stovetop.stovetop;

public record StovetopCommand
{
    public enum CommandCatagory{
        Pipeline,
        Config,
        User
    }
    
    public required string Name;
    public string? Description;
    public string? Usage;
    public CommandCatagory? Category;

    public string[]? Aliases;

    public required Action Command;
}
