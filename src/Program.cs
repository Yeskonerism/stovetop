using Stovetop.Commands;
using Stovetop.stovetop;

namespace Stovetop;

public class Program
{
    public static void Main(string[] args)
    {
        CommandRegistry.PassArguments(args);
        
        bool ignoreConfig = args.Contains("init") || args.Contains("i") || args.Contains("help") || args.Contains("h");
        
        StovetopCore.Initialize(ignoreConfig);
        CommandParser.ParseCommands(args, ignoreConfig);
    }
}