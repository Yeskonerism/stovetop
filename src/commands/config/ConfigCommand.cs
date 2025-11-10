using Stovetop.stovetop;
using Stovetop.stovetop.handlers;

namespace Stovetop.Commands.Config;

public class ConfigCommand
{
    private static bool _hasChanges;

    public static void Run()
    {
        if (!StovetopCore.VerifyConfig(true))
        {
            StovetopCore.StovetopLogger?.Error("No config found");
            return;
        }

        var config = StovetopCore.StovetopConfig!;

        var flags = new Dictionary<string[], Action>
        {
            { ["--name", "-n"], () => PrintValue(("Name", config.Project)) },
            {
                ["--working-directory", "-wd"],
                () => PrintValue(("Working Directory", config.WorkingDirectory))
            },
            { ["--runtime", "-rt"], () => PrintValue(("Runtime", config.Runtime)) },
            {
                ["--run", "-r", "-rc", "--run-command"],
                () => PrintValue(("Run Command", config.RunCommand))
            },
            {
                ["--build", "-b", "-bc", "--build-command"],
                () => PrintValue(("Build Command", config.BuildCommand))
            },
            { ["--aliases", "-a"], () => PrintAliases(config.Aliases) },
        };

        string? subcommand = CommandRegistry.GetSubcommand("config", "view");

        switch (subcommand?.ToLower())
        {
            case "view" or "v":
                ViewConfig(config, flags);
                break;
            case "edit" or "e":
                EditConfig(config);
                break;
            default:
                ViewConfig(config, flags);
                break;
        }
    }

    private static void ViewConfig(StovetopConfig config, Dictionary<string[], Action> flags)
    {
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

    private static void EditConfig(StovetopConfig config)
    {
        bool looping = true;

        // print title
        Console.WriteLine("Stovetop Configuration Editor V.0.5\n");

        // initialise a temporary config
        // this is what will get all the changes applied to
        StovetopConfig tempConfig = config.Clone();

        var editModes = new Dictionary<string[], Action>
        {
            {
                ["runtime", "rt"],
                () => tempConfig.Runtime = EditValue("runtime", tempConfig.Runtime)
            },
            {
                ["run", "r", "rc", "run-command"],
                () => tempConfig.RunCommand = EditValue("run command", tempConfig.RunCommand)
            },
            {
                ["build", "b", "bc", "build-command"],
                () => tempConfig.BuildCommand = EditValue("build command", tempConfig.BuildCommand)
            },
            {
                ["name", "n"],
                () => tempConfig.Project = EditValue("project name", tempConfig.Project)
            },
            {
                ["aliases", "a"],
                () =>
                    tempConfig.Aliases = EditAliases(
                        new Dictionary<string, string>(tempConfig.Aliases)
                    )
            },
            {
                ["exit", "quit", "q", "e"],
                () =>
                    looping = ExitConfigEditor(
                        tempConfig,
                        _hasChanges
                            && StovetopInputHandler.Confirm("Would you like to save your changes?")
                    )
            },
            { ["save"], () => SaveConfig(tempConfig) },
            { ["view", "list", "v", "ls"], () => PrintAll(tempConfig) },
        };

        PrintHelpMenu(editModes);

        while (looping)
        {
            _hasChanges = !config.Equals(tempConfig);

            string editMode = StovetopInputHandler.Ask("What would you like to do?");

            foreach (var mode in editModes)
            {
                if (mode.Key.Contains(editMode))
                {
                    mode.Value.Invoke();
                }
            }

            if (editMode == "h" || editMode == "help")
                PrintHelpMenu(editModes);
        }
    }

    private static string EditValue(string prompt, string defaultValue)
    {
        string returnValue = StovetopInputHandler.Ask($"\tEnter new {prompt}", defaultValue);

        if (!string.IsNullOrEmpty(returnValue))
            FeedbackOnEdit(prompt, defaultValue, returnValue);

        return (!string.IsNullOrEmpty(returnValue)) ? returnValue : defaultValue;
    }

    private static void FeedbackOnEdit(string key, string originalValue, string newValue)
    {
        if (originalValue != newValue)
            StovetopCore.StovetopLogger?.Info(
                $"'{key}' has been edited from {originalValue} to {newValue}"
            );
        else
            StovetopCore.StovetopLogger?.Info($"'{key}' has not been changed");
    }

    private static Dictionary<string, string> EditAliases(Dictionary<string, string> configAliases)
    {
        Dictionary<string, string> aliases = configAliases;

        while (true)
        {
            string mode = StovetopInputHandler.Ask("(Aliases) What would you like to do?");

            string aliasName = "";
            string aliasValue = "";

            switch (mode)
            {
                case "add" or "a":
                    aliases.Add(
                        aliasName = StovetopInputHandler.Ask("\tEnter alias name"),
                        aliasValue = StovetopInputHandler.Ask("\tEnter alias command")
                    );

                    StovetopCore.StovetopLogger?.Info($"Alias {aliasName} added with value {aliasValue}.");
                    break;
                case "set" or "s":
                    aliases[aliasName = StovetopInputHandler.Ask("\tEnter alias name")] =
                        (aliasValue = StovetopInputHandler.Ask("\tEnter alias command"));

                    StovetopCore.StovetopLogger?.Info($"Alias '{aliasName}' set to '{aliasValue}'");
                    break;
                case "remove" or "rm" or "del":
                    aliases.Remove(aliasName = StovetopInputHandler.Ask("\tEnter alias name"));

                    StovetopCore.StovetopLogger?.Info($"Alias '{aliasName}' removed");
                    break;
                case "view" or "list" or "ls" or "v":
                    PrintAliases(aliases);
                    break;
                case "e" or "exit" or "quit" or "q":
                    if (
                        StovetopInputHandler.Confirm(
                            "Are you sure you want to exit? All changes will be lost.",
                            false
                        )
                    )
                        return configAliases;
                    else
                        break;
                case "save":
                    return aliases;
                default:
                    Console.WriteLine("Invalid mode:\n\tadd\n\tset\n\tremove\n\tview");
                    break;
            }
        }
    }

    private static void PrintHelpMenu(Dictionary<string[], Action> editModes)
    {
        // help menu
        Console.WriteLine("Help Menu:");

        foreach (var mode in editModes)
        {
            Console.WriteLine($"\t{mode.Key[0]}");
        }

        Console.WriteLine("\tsave\n\texit\n\thelp\n\tview");
    }

    private static void SaveConfig(StovetopConfig config)
    {
        if (_hasChanges)
        {
            StovetopCore.StovetopConfig = config;
            StovetopCore.SaveConfig();
        }
        else
            StovetopCore.StovetopLogger?.Info("There are no changes to save.");
    }

    private static bool ExitConfigEditor(
        StovetopConfig tempConfig,
        bool save = false
    )
    {
        if (!_hasChanges) return false;
        if (!save)
        {
            if (
                !StovetopInputHandler.Confirm(
                    "Are you sure you want to exit? All changes will be lost.",
                    false
                )
            )
                return true;
        }
        else
        {
            SaveConfig(tempConfig);
        }

        return false;
    }
}
