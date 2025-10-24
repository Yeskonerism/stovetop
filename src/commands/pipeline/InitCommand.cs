using System.Text.Json;
using Stovetop.stovetop;

namespace Stovetop.Commands.Pipeline;

public static class InitCommand
{
    public static void Run(string runtime)
    {
        StovetopCore.CreateDefaultStructure();
        
        StovetopCore.STOVETOP_CONFIG = new StovetopConfig
        {
            WorkingDirectory = Directory.GetCurrentDirectory()
        };

        StovetopCore.STOVETOP_CONFIG.Project = Ask("[STOVE] Enter project name");
        
        string runtimeAsked = string.IsNullOrEmpty(runtime) ? Ask("[STOVE] Enter project runtime", "dotnet") : runtime;
        StovetopCore.STOVETOP_CONFIG.Runtime = runtimeAsked;
        
        StovetopCore.STOVETOP_CONFIG.RunCommand = Ask("[STOVE] Enter run command", "run --");
        StovetopCore.STOVETOP_CONFIG.BuildCommand = Ask("[STOVE] Enter build command", "build");

        StovetopCore.STOVETOP_CONFIG.Aliases["r"] = "run";
        StovetopCore.STOVETOP_CONFIG.Aliases["b"] = "build";

        if (StovetopCore.STOVETOP_CONFIG_EXISTS)
        {
            Console.Write("[STOVE] Config already exists. Overwrite? [y/N] -> ");
            var overwrite = (Console.ReadLine() ?? "").Trim().ToLower();
            if (overwrite != "y")
            {
                StovetopCore.STOVETOP_LOGGER.Warn("Aborted: existing configuration preserved.");
                return;
            }

            // create a backup version if overwriting stove config file
            StovetopBackup.CreateBackup();
        }

        Console.Write(
            (StovetopCore.STOVETOP_CONFIG.Project != "")
                ? $"[STOVE] Save config for {StovetopCore.STOVETOP_CONFIG.Project}? [Y/n] -> "
                : "[STOVE] Save this stove config? [Y/n] -> "
        );

        var confirm = (Console.ReadLine() ?? "").Trim().ToLower();
        if (string.IsNullOrEmpty(confirm) || confirm == "y")
        {
            StovetopCore.SaveConfig();
            StovetopCore.STOVETOP_LOGGER.Info($"Config saved to {StovetopCore.STOVETOP_CONFIG_PATH}");
            StovetopCore.STOVETOP_LOGGER.Success("Stovetop ready! Use 'stove run' to test your setup.");
        }
        else
        {
            StovetopCore.STOVETOP_LOGGER.Warn("Configuration process aborted.");
        }
    }

    // Helper for consistent input handling
    private static string Ask(string prompt, string defaultValue = "")
    {
        Console.Write(defaultValue != "" ? $"{prompt} (default: {defaultValue}) -> " : $"{prompt} -> ");
        var input = Console.ReadLine()?.Trim();
        return string.IsNullOrEmpty(input) ? defaultValue : input;
    }
}
