namespace Stovetop.Exceptions;

public class StovetopNonexistentRuntimeException : Exception
{
    public StovetopNonexistentRuntimeException() : base("Runtime does not exist.") { }
}