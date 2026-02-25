namespace Stovetop.Exceptions;

public class StovetopNotRootDirectoryException : Exception
{
    public StovetopNotRootDirectoryException() : base("Stovetop must be run from the root directory of your project.") { }
}