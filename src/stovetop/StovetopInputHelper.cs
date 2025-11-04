namespace Stovetop.stovetop;

public class StovetopInputHelper
{
    public static string Ask(string prompt, string defaultValue = "")
    {
        Console.Write(
            defaultValue != "" 
                ? $"{prompt} (default: {defaultValue}) -> " 
                : $"{prompt} -> ");
        var input = Console.ReadLine()?.Trim();
        return string.IsNullOrEmpty(input) ? defaultValue : input;
    }

    public static bool Confirm(string prompt, bool defaultYes = true)
    {
        string defaultHint = defaultYes ? "[Y/n]" : "[y/N]";
        Console.Write($"{prompt} {defaultHint} -> ");
        
        var input = Console.ReadLine()?.Trim().ToLower();
        
        if (string.IsNullOrEmpty(input))
            return defaultYes;
        
        return input == "y" || input == "yes";
    }
}