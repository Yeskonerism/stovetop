using Stovetop.stovetop;
using Stovetop.stovetop.config;

namespace Stovetop.Commands.Config;

public class ConfigCommand
{
    public static void Run()
    {
        if (!StovetopCore.VerifyConfig(true))
        {
            StovetopCore.StovetopLogger?.Error("No config found");
            return;
        }

        var config = StovetopCore.StovetopConfig!;

        string? subcommand = CommandRegistry.GetSubcommand("config", "view");

        switch (subcommand?.ToLower())
        {
            case "view" or "v":
                ViewConfig(config);
                break;
            default:
                ViewConfig(config);
                break;
        }
    }

    private static void ViewConfig(StovetopConfig config)
    {
        var flags = new Dictionary<string[], Action>
        {
            { ["--name", "-n"], () => PrintValue(("Name", config.Project)) },
            { ["--version", "-v"], () => PrintValue(("Version", config.Version)) },
            { ["--runtime", "-rt"], () => PrintValue(("Runtime", config.Stovetop.Runtime.Type)) },
            {
                ["--run", "-r", "-rc", "--run-command"],
                () => PrintValue(("Run Command", config.Stovetop.Commands.Run))
            },
            {
                ["--executable", "-e", "-exec"],
                () => PrintValue(("Executable", config.Stovetop.Commands.Executable ?? ""))
            },
            {
                ["--build", "-b", "-bc", "--build-command"],
                () => PrintValue(("Build Command", config.Stovetop.Commands.Build))
            },
            { ["--aliases", "-a"], () => PrintAliases(config.Stovetop.Aliases) },
            { ["--variables", "--vars"], () => PrintVariables(config.Stovetop.Variables) },
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
        if (!foundFlags)
            PrintAll(config);
    }

    private static void PrintAll(StovetopConfig config)
    {
        // print title
        Console.WriteLine("Stovetop Configuration\n");

        // print project information
        PrintSection(
            "Project Information",
            new[] { ("Name", config.Project), ("Version", config.Version) }
        );

        // print runtime information
        var runtimeInfo = new List<(string key, string value)>
        {
            ("Type", config.Stovetop.Runtime.Type),
        };

        if (!string.IsNullOrEmpty(config.Stovetop.Runtime.Version))
        {
            runtimeInfo.Add(("Version", config.Stovetop.Runtime.Version));
        }

        PrintSection("Runtime", runtimeInfo.ToArray());

        // print commands
        var commandsInfo = new List<(string key, string value)>
        {
            ("Build", config.Stovetop.Commands.Build),
            ("Run", config.Stovetop.Commands.Run),
        };

        if (!string.IsNullOrEmpty(config.Stovetop.Commands.Executable))
        {
            commandsInfo.Add(("Executable", config.Stovetop.Commands.Executable));
        }

        if (!string.IsNullOrEmpty(config.Stovetop.Commands.Test))
        {
            commandsInfo.Add(("Test", config.Stovetop.Commands.Test));
        }

        if (!string.IsNullOrEmpty(config.Stovetop.Commands.Clean))
        {
            commandsInfo.Add(("Clean", config.Stovetop.Commands.Clean));
        }

        if (!string.IsNullOrEmpty(config.Stovetop.Commands.Deploy))
        {
            commandsInfo.Add(("Deploy", config.Stovetop.Commands.Deploy));
        }

        PrintSection("Commands", commandsInfo.ToArray());

        // print variables
        if (config.Stovetop.Variables.Count > 0)
        {
            PrintVariables(config.Stovetop.Variables);
        }

        // print aliases
        if (config.Stovetop.Aliases.Count > 0)
        {
            PrintAliases(config.Stovetop.Aliases);
        }
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

    private static void PrintVariables(Dictionary<string, string> variables)
    {
        Console.WriteLine("Variables: ");
        if (variables.Count == 0)
        {
            Console.WriteLine("\tNo variables found");
            return;
        }
        foreach (var variable in variables)
        {
            Console.WriteLine($"\t{variable.Key}: {variable.Value}");
        }
        Console.WriteLine();
    }
}
