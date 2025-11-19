namespace Wasabi.utils;

public static class StringUtils
{
    /// <summary>
    /// Extracts a string enclosed in quotes (single or double) from a line.
    /// </summary>
    /// <param name="line">The line containing the quoted string</param>
    /// <returns>The extracted string without quotes, or empty string if not found</returns>
    public static string ExtractQuotedString(string line)
    {
        string character = "";
        
        if (line.Contains("'"))
            character = "'";
        else if (line.Contains("\""))
            character = "\"";
        
        if (string.IsNullOrEmpty(character))
            return "";
        
        var start = line.IndexOf(character, StringComparison.Ordinal) + 1;
        var end = line.LastIndexOf(character, StringComparison.Ordinal);
        return start > 0 && end > start ? line[start..end] : "";
    }
}

