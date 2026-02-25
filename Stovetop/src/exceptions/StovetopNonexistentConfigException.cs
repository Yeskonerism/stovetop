namespace Stovetop.Exceptions;

public class StovetopNonexistentConfigException : Exception
{
    public StovetopNonexistentConfigException() : base("Config does not exist.") { }
}