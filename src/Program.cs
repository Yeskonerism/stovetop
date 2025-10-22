using Stovetop.Commands;
using Stovetop.stovetop;

namespace Stovetop;

public class Program
{
    public static void Main(string[] args)
    {
        StovetopCore.Initialize();
        
        CommandParser.ParseCommands(args);
    }
}