using Stovetop.Commands.Config;
using Stovetop.stovetop;

namespace Stovetop.Commands;

public class CommandParser
{
    public static void ParseCommands(string[] args, bool ignoreConfig = false)
    {
        if (args.Length > 0 && args[0] != "init" && !ignoreConfig)
        {
            StovetopCore.LoadConfig();
        }

        if (args.Length == 0)
        {
            var helpCommand = CommandRegistry.GetCommand("help");
            helpCommand?.Command?.Invoke();
            return;
        }

        if (args.Length > 0)
        {
            // Only check the first argument (the command) - this will change later to support multiple command calls
            string commandName = args[0].ToLower();

            foreach(var command in CommandRegistry.Commands)
            {
                if (command.Aliases != null && (command.Name == commandName || command.Aliases.Contains(commandName)))
                {
                    if (command.Name != null && SupportsBackupFlag(command.Name))
                        ExecuteWithOptionalBackup(command);
                    else
                        command.Command?.Invoke();
                    return;
                }
            }

            // If we get here, command not found
            StovetopCore.StovetopLogger?.Error($"Unknown command: {commandName}");
        }
    }

    // parse command specific arguments from config
    public static string[] ParseArguments(string? argString)
    {
        List<string> args = new List<string>();
        string currentToken = "";
        bool inQuotes = false;

        if (argString != null)
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
    
    // Helper method to execute run/build commands with optional backup config
    private static void ExecuteWithOptionalBackup(StovetopCommand command)
    {
        // Check if backup flag is set (--backup)
        if (command.Name != null && CommandRegistry.GetPositionalArgument(command.Name, 1) == "--backup")
        {
            string? positionalArgument = CommandRegistry.GetPositionalArgument(command.Name, 2);

            // Handle missing backup ID
            if (string.IsNullOrEmpty(positionalArgument))
            {
                StovetopCore.StovetopLogger?.Error("Please specify a backup ID or 'latest' (e.g., stove run --backup latest)");
                return;
            }

            // Resolve "latest" to actual backup ID
            string? backupId = positionalArgument == "latest" ? BackupCommand.GetLatestBackup() : positionalArgument;

            // Validate backup exists
            if (!BackupCommand.BackupExists(backupId!))
            {
                StovetopCore.StovetopLogger?.Error($"Backup '{backupId}' does not exist");
                return;
            }

            // Validate backup root is initialized
            if (string.IsNullOrEmpty(StovetopCore.StovetopBackupRoot))
            {
                StovetopCore.StovetopLogger?.Error("Backup directory not found");
                return;
            }

            // Build path to back up config
            string pathToBackupConfig = Path.Combine(StovetopCore.StovetopBackupRoot, $"{backupId}-stovetop-backup.json");
            string? originalConfigPath = StovetopCore.StovetopConfigPath;

            try
            {
                // Temporarily swap to back up config
                StovetopCore.StovetopConfigPath = pathToBackupConfig;
                StovetopCore.StovetopLogger?.Info($"Using backup config: {backupId}");
                StovetopCore.LoadConfig();
                StovetopCore.StovetopRuntime = StovetopCore.StovetopConfig?.Runtime;

                // Execute the command
                command.Command?.Invoke();
            }
            finally
            {
                // Always restore original config path and reload
                StovetopCore.StovetopConfigPath = originalConfigPath;
                StovetopCore.LoadConfig();
            }
        }
        else
        {
            // No backup flag, execute normally
            command.Command?.Invoke();
        }
    }

    private static bool SupportsBackupFlag(string commandName)
    {
        return commandName == "run" || commandName == "build";
    }
}