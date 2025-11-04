using Stovetop.Commands;
using Stovetop.stovetop;

namespace Stovetop;

public class Program
{
    public static void Main(string[] args)
    {
        bool ignoreConfig = args.Contains("init") || args.Contains("help");
        
        StovetopCore.Initialize(ignoreConfig);
        
        CommandRegistry.PassArguments(args);
        
        CommandParser.ParseCommands(args, ignoreConfig);
    }
}