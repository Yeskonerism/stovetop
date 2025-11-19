using Wasabi.core;
using Wasabi.utils;

namespace Wasabi.commands;

public class SetCommand : IWasabiCommand
{
    public string CommandName => "set";
    public bool CanHandle(string line)
    {
        return line.StartsWith("set");
    }

    public void Execute(string line, WasabiContext context)
    {
        string key = "";
        string value = "";
        
        if (line.StartsWith("set"))
        {
            key = line.Split(" ")[1];
            value = line.Substring(line.IndexOf(key) + key.Length + 1);
        }
        
        context._variables.Add(key, (value.StartsWith("\"") || value.StartsWith(" '")) ? StringUtils.ExtractQuotedString(value) : value);
    }
}