namespace Stovetop.Exceptions;

public class StovetopUninitialisedException : Exception
{
    public StovetopUninitialisedException() : base("Stovetop has not been initialised.") { }
}