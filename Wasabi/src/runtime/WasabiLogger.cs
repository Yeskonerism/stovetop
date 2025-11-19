namespace Wasabi.runtime;

/// <summary>
/// Provides logging functionality for Wasabi scripts.
/// Handles log.info, log.warn, log.error, etc.
/// </summary>
public static class WasabiLogger
{
    public static void Info(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }
    
    public static void Warn(string message)
    {
        Console.WriteLine($"[WARN] {message}");
    }
    
    public static void Error(string message)
    {
        Console.WriteLine($"[ERROR] {message}");
    }
    
    public static void Debug(string message)
    {
        Console.WriteLine($"[DEBUG] {message}");
    }
    
    public static void Success(string message)
    {
        Console.WriteLine($"[SUCCESS] {message}");
    }
    
    public static void Raw(string message)
    {
        Console.WriteLine($"{message}");
    }
}

