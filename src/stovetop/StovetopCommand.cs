namespace Stovetop.stovetop;

public record StovetopCommand
{
    public required string Name;
    public string? Description;
    public string? Usage;
    public string? Category;

    public string[]? Aliases;

    public required Action Command;
}
