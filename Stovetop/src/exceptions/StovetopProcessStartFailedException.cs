namespace Stovetop.Exceptions;

public class StovetopProcessStartFailedException : Exception
{
    public StovetopProcessStartFailedException() : base("Failed to start process.") { }
}

