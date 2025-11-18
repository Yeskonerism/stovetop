using Stovetop.stovetop;
using Stovetop.stovetop.config;
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

        

        string? subcommand = CommandRegistry.GetSubcommand("config", "view");

        switch (subcommand?.ToLower())
        {
            case "view" or "v":
                ViewConfig(config);
                break;
            case "edit" or "e":
                EditConfig(config);
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

    private static void EditConfig(StovetopConfig config)
    {
        _hasChanges = false;
        
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
                () => tempConfig.Stovetop.Runtime.Type = EditValue("runtime type", tempConfig.Stovetop.Runtime.Type)
            },
            {
                ["run", "r", "rc", "run-command"],
                () => tempConfig.Stovetop.Commands.Run = EditValue("run command", tempConfig.Stovetop.Commands.Run)
            },
            {
                ["build", "b", "bc", "build-command"],
                () => tempConfig.Stovetop.Commands.Build = EditValue("build command", tempConfig.Stovetop.Commands.Build)
            },
            {
                ["name", "n"],
                () => tempConfig.Project = EditValue("project name", tempConfig.Project)
            },
            {
                ["version", "ver"],
                () => tempConfig.Version = EditValue("version", tempConfig.Version)
            },
            {
                ["aliases", "a"],
                () =>
                    tempConfig.Stovetop.Aliases = EditAliases(
                        new Dictionary<string, string>(tempConfig.Stovetop.Aliases)
                    )
            },
            {
                ["variables", "vars"],
                () =>
                    tempConfig.Stovetop.Variables = EditVariables(
                        new Dictionary<string, string>(tempConfig.Stovetop.Variables)
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

    private static Dictionary<string, string> EditVariables(Dictionary<string, string> configVariables)
    {
        Dictionary<string, string> variables = configVariables;

        while (true)
        {
            string mode = StovetopInputHandler.Ask("(Variables) What would you like to do?");

            string varName = "";
            string varValue = "";

            switch (mode)
            {
                case "add" or "a":
                    variables.Add(
                        varName = StovetopInputHandler.Ask("\tEnter variable name"),
                        varValue = StovetopInputHandler.Ask("\tEnter variable value")
                    );

                    StovetopCore.StovetopLogger?.Info($"Variable {varName} added with value {varValue}.");
                    break;
                case "set" or "s":
                    variables[varName = StovetopInputHandler.Ask("\tEnter variable name")] =
                        (varValue = StovetopInputHandler.Ask("\tEnter variable value"));

                    StovetopCore.StovetopLogger?.Info($"Variable '{varName}' set to '{varValue}'");
                    break;
                case "remove" or "rm" or "del":
                    variables.Remove(varName = StovetopInputHandler.Ask("\tEnter variable name"));

                    StovetopCore.StovetopLogger?.Info($"Variable '{varName}' removed");
                    break;
                case "view" or "list" or "ls" or "v":
                    PrintVariables(variables);
                    break;
                case "e" or "exit" or "quit" or "q":
                    if (
                        StovetopInputHandler.Confirm(
                            "Are you sure you want to exit? All changes will be lost.",
                            false
                        )
                    )
                        return configVariables;
                    else
                        break;
                case "save":
                    return variables;
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
