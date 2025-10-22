using Stovetop.stovetop;

namespace Stovetop.Commands;

//
// CommandParser.cs
// The class that works to parse the given arguments and commands within
// Stovetop's config file
//
// Oliver Hughes
// 22-oct-25
//
public class CommandParser
{
    public static void ParseCommands(string[] args)
    {
        if (args.Length > 0 && args[0] != "init")
        {
            var config = StovetopConfig.Load();

            // check for possible aliases
            if (args.Length > 0 && config.Aliases != null && config.Aliases.ContainsKey(args[0]))
            {
                args[0] = config.Aliases[args[0]]; // replace alias with real command
                Console.WriteLine($"[STOVE] Alias '{args[0]}' resolved");
            }
        }

        if (args.Length > 0)
        {
            switch (args[0])
            {
                case "init":
                    string passedRuntime = null;
                    
                    // check if runtime passed in, if not, remain null
                    // (this gets handled in InitCommand.cs)
                    if (args.Length > 1)
                        passedRuntime = args[1];
                    
                    Pipeline.InitCommand.Run(passedRuntime);
                    break;
                case "run":
                    Pipeline.RunCommand.Run();
                    break;
                case "build":
                    Pipeline.BuildCommand.Run();
                    break;
            }
        }
    }

    // parse command specific arguments from config
    public static string[] ParseArguments(string argString)
    {
        List<string> args = new List<string>();
        string currentToken = "";
        bool inQuotes = false;

        foreach (char c in argString)
        {
            if (c == '"' || c == '\'')
            {
                inQuotes = !inQuotes; // toggle
            }
            else if (c == ' ' && !inQuotes)
            {
                if (!string.IsNullOrEmpty(currentToken))
                {
                    args.Add(currentToken);
                    currentToken = "";
                }
            }
            else
            {
                currentToken += c;
            }
        }

        if (!string.IsNullOrEmpty(currentToken))
            args.Add(currentToken);
        
        return args.ToArray();
    }
}