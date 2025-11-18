using Stovetop.Commands;

namespace Stovetop.stovetop;

public class StovetopLogger
{
    public bool Verbose { get; set; } = StovetopCore.RunVerbose;
    public bool Silent { get; set; } = StovetopCore.RunSilent;

    public void Info(string message) => Write(message, ConsoleColor.Gray, "[STOVE]");

    public void Success(string message) => Write(message, ConsoleColor.Green, "[OK]");

    public void Warn(string message) => Write(message, ConsoleColor.Yellow, "[WARN]");

    public void Error(string message) => Write(message, ConsoleColor.Red, "[ERROR]");

    public void Debug(string message)
    {
        if (Verbose)
            Write(message, ConsoleColor.Cyan, "[DEBUG]");
    }

    private void Write(string message, ConsoleColor color, string prefix, bool overrideSilent = false)
    {
        if (!overrideSilent && Silent)
            return;
        Console.ForegroundColor = color;
        Console.Write(prefix);
        Console.ResetColor();
        Console.WriteLine($" {message}");
    }
}
