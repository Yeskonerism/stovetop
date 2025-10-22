using System.Text.Json;
using Stovetop.stovetop;

namespace Stovetop.Commands.Pipeline;

public static class InitCommand
{
    public static void Run(string runtime)
    {
        string runtimeAsked;

        if (string.IsNullOrEmpty(runtime))
            runtimeAsked = Ask("[STOVE] Enter project runtime", "dotnet");
        else
            runtimeAsked = runtime;
        
        StovetopCore.STOVETOP_CONFIG = new StovetopConfig
        {
            Runtime = runtimeAsked,
            WorkingDirectory = Directory.GetCurrentDirectory()
        };

        StovetopCore.STOVETOP_CONFIG.Project = Ask("[STOVE] Enter project name");
        StovetopCore.STOVETOP_CONFIG.RunCommand = Ask("[STOVE] Enter run command", "run --");
        StovetopCore.STOVETOP_CONFIG.BuildCommand = Ask("[STOVE] Enter build command", "build");

        StovetopCore.STOVETOP_CONFIG.Aliases["r"] = "run";
        StovetopCore.STOVETOP_CONFIG.Aliases["b"] = "build";

        if (Directory.Exists(StovetopCore.STOVETOP_CONFIG_ROOT))
        {
            Console.Write("[STOVE] Config already exists. Overwrite? [y/N] -> ");
            var overwrite = (Console.ReadLine() ?? "").Trim().ToLower();
            if (overwrite != "y")
            {
                StovetopCore.STOVETOP_LOGGER.Warn("Aborted: existing configuration preserved.");
                return;
            }

            string backupPath = Path.Combine(StovetopCore.STOVETOP_CONFIG_ROOT, $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            if (File.Exists(StovetopCore.STOVETOP_CONFIG_PATH))
                File.Copy(StovetopCore.STOVETOP_CONFIG_PATH, backupPath, true);
        }

        Directory.CreateDirectory(StovetopCore.STOVETOP_CONFIG_ROOT);
        foreach (var subdir in new[] { "profiles", "cache" })
            Directory.CreateDirectory(Path.Combine(StovetopCore.STOVETOP_CONFIG_ROOT, subdir));

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
