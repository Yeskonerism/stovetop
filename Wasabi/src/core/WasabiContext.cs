namespace Wasabi.core;

public class WasabiContext
{
    public Dictionary<string, string> _variables;
    
    public WasabiContext(Dictionary<string, string> variables)
    {
        _variables = variables;
    }
    
    public string SubstituteVariables(string input)
    {
        foreach (var (key, value) in _variables)
        {
            input = input.Replace($"${{{key}}}", value);
        }
        return input;
    }
}