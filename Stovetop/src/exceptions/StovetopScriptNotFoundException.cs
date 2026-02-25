namespace Stovetop.Exceptions;

public class StovetopScriptNotFoundException : Exception
{
    public StovetopScriptNotFoundException(string scriptName) : base($"Script '{scriptName}' not found.") { }
}

