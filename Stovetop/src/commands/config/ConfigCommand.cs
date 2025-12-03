using Stovetop.ConfigParser;
using Stovetop.stovetop;

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

    private static void ViewConfig(ConfigModel config)
    {
        var flags = new Dictionary<string[], Action>
        {
            { ["--name", "-n"], () => PrintValue(("Name", config.Project)) },
            { ["--version", "-v"], () => PrintValue(("Version", config.Version)) },
            { ["--runtime", "-rt"], () => PrintValue(("Runtime", config.Runtime)) },
            {
                ["--run", "-r", "-rc", "--run-command"],
                () => PrintValue(("Run Command", config.Commands.GetValueOrDefault("run", "")))
            },
            {
                ["--executable", "-e", "-exec"],
                () => PrintValue(("Executable", config.Commands.GetValueOrDefault("executable", "")))
            },
            {
                ["--build", "-b", "-bc", "--build-command"],
                () => PrintValue(("Build Command", config.Commands.GetValueOrDefault("build", "")))
            },
            { ["--aliases", "-a"], () => PrintAliases(config.Aliases) },
            { ["--variables", "--vars"], () => PrintVariables(config.Variables) },
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

    private static void PrintAll(ConfigModel config)
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
            ("Type", config.Runtime),
        };

        if (!string.IsNullOrEmpty(config.RuntimeVersion))
        {
            runtimeInfo.Add(("Version", config.RuntimeVersion));
        }

        PrintSection("Runtime", runtimeInfo.ToArray());

        // print commands
        var commandsInfo = new List<(string key, string value)>();

        if (config.Commands.TryGetValue("build", out var buildCmd))
            commandsInfo.Add(("Build", buildCmd));
        if (config.Commands.TryGetValue("run", out var runCmd))
            commandsInfo.Add(("Run", runCmd));
        if (config.Commands.TryGetValue("executable", out var execCmd))
            commandsInfo.Add(("Executable", execCmd));
        if (config.Commands.TryGetValue("test", out var testCmd))
            commandsInfo.Add(("Test", testCmd));
        if (config.Commands.TryGetValue("clean", out var cleanCmd))
            commandsInfo.Add(("Clean", cleanCmd));
        if (config.Commands.TryGetValue("deploy", out var deployCmd))
            commandsInfo.Add(("Deploy", deployCmd));

        if (commandsInfo.Count > 0)
            PrintSection("Commands", commandsInfo.ToArray());

        // print variables
        if (config.Variables.Count > 0)
        {
            PrintVariables(config.Variables);
        }

        // print aliases
        if (config.Aliases.Count > 0)
        {
            PrintAliases(config.Aliases);
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
