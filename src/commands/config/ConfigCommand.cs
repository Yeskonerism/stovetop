using Stovetop.stovetop;

namespace Stovetop.Commands.Config;

public class ConfigCommand
{
    public static void Run()
    {
        string? subcommand = CommandRegistry.GetSubcommand("config", "view");

        switch (subcommand?.ToLower())
        {
            case "view" or "v":
                ViewConfig();
                break;
            case "edit" or "e":
                EditConfig();
                break;
            default:
                ViewConfig();
                break;
        }
    }

    private static void ViewConfig()
    {
        if (!StovetopCore.VerifyConfig(true))
        {
            StovetopCore.StovetopLogger?.Error("No config found");
            return;
        }

        // TODO | Allow for backup viewing as well (stove config view --backup [backup])
        var config = StovetopCore.StovetopConfig!;

        var flags = new Dictionary<string[], Action>
        {
            { ["--name", "-n"], () => PrintValue(("Name", config.Project)) },
            {
                ["--working-directory", "-wd"],
                () => PrintValue(("Working Directory", config.WorkingDirectory))
            },
            { ["--runtime", "-r"], () => PrintValue(("Runtime", config.Runtime)) },
            {
                ["--run", "-rc", "--run-command"],
                () => PrintValue(("Run Command", config.RunCommand))
            },
            {
                ["--build", "-bc", "--build-command"],
                () => PrintValue(("Build Command", config.BuildCommand))
            },
            { ["--aliases", "-a"], () => PrintAliases(config.Aliases) },
        };

        bool foundFlags = false;
        
        // search for flags
        foreach (var flag in flags)
        {
            if (CommandRegistry.CurrentArgs != null)
            {
                foreach (var arg in CommandRegistry.CurrentArgs)
                {
                    if (flag.Key.Contains(arg))
                    {
                        foundFlags = true;
                        flag.Value.Invoke();
                    }
                }
            }
        }

        // if no flags are found, print all
        if(!foundFlags)
            PrintAll(config);
    }

    private static void PrintAll(StovetopConfig config)
    {
        // print title
        Console.WriteLine("Stovetop Configuration\n");

        // print project information
        PrintSection(
            "Project Information",
            new[] { ("Name", config.Project), ("Working Directory", config.WorkingDirectory) }
        );

        // print runtime information
        PrintSection(
            "Runtime Information",
            new[]
            {
                ("Runtime", config.Runtime),
                ("Run Command", config.RunCommand),
                ("Build Command", config.BuildCommand),
            }
        );

        // print aliases
        PrintAliases(config.Aliases);
    }

    private static void PrintValue((string key, string value) item)
    {
        Console.WriteLine($"{item.key}: {item.value}");
    }

    private static void PrintSection(string title, (string key, string value)[] items)
    {
        Console.WriteLine($"{title}: ");
        foreach (var item in items)
        {
            Console.WriteLine($"\t{item.key}: {item.value}");
        }
        Console.WriteLine();
    }

    private static void PrintAliases(Dictionary<string, string> aliases)
    {
        Console.WriteLine("Aliases: ");
        if (aliases.Count == 0)
        {
            Console.WriteLine("\tNo aliases found");
            return;
        }
        foreach (var alias in aliases)
        {
            Console.WriteLine($"\t{alias.Key}: {alias.Value}");
        }
        Console.WriteLine();
    }

    private static void EditConfig()
    {
        // TODO | Implement config editing
        // this will get positional arguments to switch config editing mode
        // possible FOR FUTURE read values from file

        /* examples
         * stove config edit (cli editing mode)
         * stove config edit aliases
         * stove config edit aliases --add test "dotnet test --verbosity normal"
         * stove config edit runtime
         * stove config edit runtime --set dotnet
         * stove config edit build-command
         * stove config edit run-command
        */

        // 1. determine config edit mode

        // 2. determine config edit action
        // 3. execute action
    }
}
