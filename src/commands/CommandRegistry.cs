using Stovetop.Commands.Config;
using Stovetop.Commands.Pipeline;
using Stovetop.stovetop;

namespace Stovetop.Commands;

public class CommandRegistry
{
    public static List<StovetopCommand> Commands { get; private set; }
    public static string[]? CurrentArgs;

    static CommandRegistry()
    {
        // initialise command list
        Commands = new List<StovetopCommand>();

        // register all commands
        RegisterCommand(
            "init",
            "Initialize a new project",
            "stove init (runtime)",
            "pipeline",
            InitCommand.Run,
            ["i"]
        );
        RegisterCommand(
            "run",
            "Run the project",
            "stove run (--backup [backup])",
            "pipeline",
            RunCommand.Run,
            ["r"]
        );
        RegisterCommand(
            "build",
            "Build the project",
            "stove build (--backup [backup])",
            "pipeline",
            BuildCommand.Run,
            ["b", "bld"]
        );
        RegisterCommand(
            "backup",
            "Create a backup of the current config",
            "stove backup (list | revert [backup])",
            "config",
            BackupCommand.Run,
            ["bak", "bkp"]
        );
        RegisterCommand(
            "help",
            "Show this help message",
            "stove help (command)",
            "user",
            HelpCommand.Run,
            ["h"]
        );
        RegisterCommand(
            "config",
            "View and edit the current config",
            "stove config (view|edit)",
            "config",
            ConfigCommand.Run,
            ["cfg"]
        );
    }

    public static void RegisterCommand(
        string name,
        string desc,
        string usage,
        string category,
        Action command,
        string[]? aliases = null
    )
    {
        string[] aliasVerification = aliases ?? Array.Empty<string>();

        Commands.Add(
            new StovetopCommand
            {
                Name = name,
                Description = desc,
                Usage = usage,
                Category = category,
                Command = command,
                Aliases = aliasVerification,
            }
        );
    }

    public static void PassArguments(string[] args)
    {
        CurrentArgs = args;
    }

    // command getting and fetching
    public static StovetopCommand? GetCommand(string? name)
    {
        return GetCommandByNameOrAlias(name!);
    }

    private static StovetopCommand? GetCommandByNameOrAlias(string name)
    {
        foreach (var command in Commands)
        {
            if (MatchesCommand(command, name))
                return command;
        }

        return null;
    }

    private static bool MatchesCommand(StovetopCommand command, string name)
    {
        return command.Name != null
            && command.Aliases != null
            && (
                command.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                || command.Aliases.Any(a => a.Equals(name, StringComparison.OrdinalIgnoreCase))
            );
    }

    public static int GetCommandIndex(string commandName)
    {
        if (CurrentArgs != null)
            for (int i = 0; i < CurrentArgs.Length; i++)
            {
                // check for both exact name match or alias (GetCommand already handles aliases)
                if (IsCommandAtIndex(i, commandName))
                    return i;
            }

        return -1; // Not found
    }

    private static bool IsCommandAtIndex(int index, string commandName)
    {
        var command = GetCommand(CurrentArgs?[index]);
        return command?.Name == commandName;
    }

    public static string? GetPositionalArgument(string commandName, int position)
    {
        int commandIndex = GetCommandIndex(commandName);

        if (commandIndex == -1)
            return null;

        int targetIndex = commandIndex + position;

        if (CurrentArgs != null && targetIndex >= CurrentArgs.Length)
            return null;

        return CurrentArgs?[targetIndex];
    }

    public static string? GetSubcommand(string commandName, string? defaultValue = null)
    {
        int commandIndex = GetCommandIndex(commandName);

        if (commandIndex >= 0 && CurrentArgs != null && CurrentArgs.Length > commandIndex + 1)
            return CurrentArgs[commandIndex + 1];

        return defaultValue;
    }
}
