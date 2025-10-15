using Stovetop.Commands;

namespace Stovetop;

public class Program
{
    public static void Main(string[] args)
    {
        SubcommandParser(args);
    }

    public static void SubcommandParser(string[] args)
    {
        if (args.Length > 0)
        {
            if (args[0] == "init")
            {
                if(args.Length > 1)
                    InitCommand.Run(args[1]);
                else
                    Console.WriteLine("[STOVE] Error: runtime not specified");
            }
            else if(args[0] == "run")
                RunCommand.Run();
            else if(args[0] == "build")
                RunCommand.Build();
        }
    }
}