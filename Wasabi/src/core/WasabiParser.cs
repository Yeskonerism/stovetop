namespace Wasabi.core;

/// <summary>
/// Handles parsing of Wasabi script files and lines.
/// </summary>
public static class WasabiParser
{
    /// <summary>
    /// Reads and parses a Wasabi script file.
    /// </summary>
    /// <param name="scriptPath">Path to the script file</param>
    /// <returns>Array of parsed lines, or null if file doesn't exist</returns>
    public static string[]? ParseFile(string scriptPath)
    {
        if (!File.Exists(scriptPath))
        {
            Console.WriteLine($"[WASABI] Script not found: {scriptPath}");
            return null;
        }
        
        return File.ReadAllLines(scriptPath);
    }
    
    /// <summary>
    /// Checks if a line should be skipped (empty or comment).
    /// </summary>
    /// <param name="line">The line to check</param>
    /// <returns>True if the line should be skipped</returns>
    public static bool ShouldSkipLine(string line)
    {
        return string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#");
    }
    
    /// <summary>
    /// Normalizes a line by trimming whitespace.
    /// </summary>
    /// <param name="line">The line to normalize</param>
    /// <returns>The normalized line</returns>
    public static string NormalizeLine(string line)
    {
        return line.Trim();
    }
}

