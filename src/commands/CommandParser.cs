using Stovetop.commands.config;
using Stovetop.Commands.Pipeline;
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
                StovetopCore.STOVETOP_LOGGER.Info($"Alias '{args[0]}' resolved");
            }
        }

        if (args.Length > 0)
        {
            switch (args[0])
            {
                case "init":
                    string passedRuntime = (args.Length > 1) ? args[1] : null;
                    
                    Pipeline.InitCommand.Run(passedRuntime);
                    break;
                case "run":
                    // TODO | Backup running 
                    string passedConfig;
                    
                    if (args.Length > 1)
                    {
                        if (args[1] == "--backup")
                        {
                            if (args.Length > 2)
                            {
                                if(args[2] == "latest")
                                    passedConfig = StovetopBackup.GetLatestBackup();
                                else
                                {
                                    passedConfig = args[2];
                                }

                                string pathToBackups = Path.Combine(Path.Combine(StovetopCore.STOVETOP_CONFIG_ROOT, "cache"), "backups");
                                string pathToBackupConfig = Path.Combine(pathToBackups, passedConfig+"-stovetop-backup.json");
                                
                                StovetopCore.STOVETOP_CONFIG_PATH = pathToBackupConfig;

                                if (StovetopCore.VerifyConfig(true))
                                {
                                    StovetopCore.LoadConfig();
                                    Pipeline.RunCommand.Run();
                                }
                                else
                                {
                                    StovetopCore.STOVETOP_LOGGER.Error("Given backup ID is nonexistent");
                                }
                            } 
                        }
                    } else
                        Pipeline.RunCommand.Run();
                    break;
                case "build":
                    Pipeline.BuildCommand.Run();
                    break;
                case "backup":
                    if (args.Length > 1)
                    {
                        switch (args[1])
                        {
                            case "ls" or "view" or "list":
                                StovetopBackup.ViewBackups();
                                break;
                            case "revert":
                                if (args.Length > 2)
                                {
                                    if (args[2] == "--latest")
                                        Config.RevertCommand.Run(StovetopBackup.GetLatestBackup());
                                    else if (!string.IsNullOrEmpty(args[2]))
                                        Config.RevertCommand.Run(args[2]);
                                }
                                else
                                {
                                    StovetopCore.STOVETOP_LOGGER.Error("Please specify a backup ID (e.g. [date]-stovetop-backup.jsom");
                                }
                                break;
                        }
                    }
                    else
                    {
                        BackupCommand.Run();
                    } 

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