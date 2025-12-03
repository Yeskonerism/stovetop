using System.Diagnostics;
using System.Text;
using Stovetop.ConfigParser;
using Stovetop.Commands;
using Stovetop.stovetop.handlers;
using static System.Environment;

namespace Stovetop.stovetop;

public static class StovetopCore
{
    public static string? StovetopRoot;
    public static string? StovetopConfigRoot;
    public static string? StovetopConfigPath;
    public static bool StovetopConfigExists;
    public static ConfigModel? StovetopConfig;
    public static StovetopLogger? StovetopLogger;
    public static string? StovetopRuntime;

    public static string? StovetopBackupRoot;
    public static string? StovetopScriptRoot;

    public static bool RunSilent;
    public static bool RunVerbose;
    public static bool RunHookless;

    public static void Initialize(bool ignoreConfig = false)
    {
        StovetopRoot = Directory.GetCurrentDirectory();
        StovetopConfigRoot = Path.Combine(StovetopRoot, ".stove");
        StovetopConfigPath = Path.Combine(StovetopConfigRoot, "stovetop.stove");

        StovetopBackupRoot = Path.Combine(StovetopConfigRoot, "cache/backups");
        StovetopScriptRoot = Path.Combine(StovetopConfigRoot, "scripts");

        RunSilent = CommandRegistry.CurrentArgs != null && (
            CommandRegistry.CurrentArgs.Contains("-s")
            || CommandRegistry.CurrentArgs.Contains("--silent")
        );
        RunVerbose = CommandRegistry.CurrentArgs != null && (
            CommandRegistry.CurrentArgs.Contains("-v")
            || CommandRegistry.CurrentArgs.Contains("--verbose")
        );
        RunHookless = CommandRegistry.CurrentArgs != null && (
            CommandRegistry.CurrentArgs.Contains("--no-hooks")
            || CommandRegistry.CurrentArgs.Contains("-nh")
            || CommandRegistry.CurrentArgs.Contains("--hookless")
        );

        SetupLogger();

        if (!ignoreConfig)
        {
            if (VerifyConfig())
            {
                LoadConfig();
                StovetopRuntime = StovetopConfig?.Runtime;
            }
            else
            {
                StovetopLogger?.Error("No config found");
            }
        }
    }

    public static bool VerifyConfig(bool ignoreConfig = false)
    {
        StovetopConfigExists = File.Exists(StovetopConfigPath);

        if (!ignoreConfig)
            StovetopLogger?.Info(
                StovetopConfigExists ? "Main Config Verified" : "Main Config Not Verified"
            );

        return StovetopConfigExists;
    }

    public static void LoadConfig()
    {
        if (StovetopConfigPath != null)
        {
            StovetopConfig = StovetopConfigParser.ParseFile(StovetopConfigPath);
        }
    }

    public static void SaveConfig()
    {
        if (StovetopConfig == null || StovetopConfigPath == null)
            return;

        var sb = new StringBuilder();

        // Write project info
        sb.AppendLine($"// Stovetop configuration file");
        sb.AppendLine($"project(\"{StovetopConfig.Project}\")");
        sb.AppendLine($"version({StovetopConfig.Version})");
        sb.AppendLine();

        // Write runtime
        sb.AppendLine($"runtime({StovetopConfig.Runtime})");
        sb.AppendLine();

        // Write variables
        if (StovetopConfig.Variables.Count > 0)
        {
            sb.AppendLine("// Variables");
            foreach (var variable in StovetopConfig.Variables)
            {
                sb.AppendLine($"var {variable.Key} = {variable.Value}");
            }
            sb.AppendLine();
        }

        // Write commands
        sb.AppendLine("// Commands");
        foreach (var command in StovetopConfig.Commands)
        {
            sb.AppendLine($"{command.Key}_command({command.Value})");
        }
        sb.AppendLine();

        // Write aliases
        if (StovetopConfig.Aliases.Count > 0)
        {
            sb.AppendLine("// Aliases");
            foreach (var alias in StovetopConfig.Aliases)
            {
                sb.AppendLine($"alias(\"{alias.Key}\", \"{alias.Value}\")");
            }
        }

        File.WriteAllText(StovetopConfigPath, sb.ToString());
        StovetopLogger?.Success("Configuration saved successfully.");
    }

    public static void SetupLogger()
    {
        StovetopLogger = new StovetopLogger();
        StovetopLogger.Info("Logger initialized");
    }

    public static void CreateDefaultStructure()
    {
        if (StovetopConfigRoot != null)
        {
            Directory.CreateDirectory(StovetopConfigRoot);

            foreach (
                var subDirectory in new[]
                {
                    "profiles",
                    "cache",
                    "cache/backups",
                    "scripts/user",
                    "scripts/hooks",
                }
            )
                Directory.CreateDirectory(Path.Combine(StovetopConfigRoot, subDirectory));
        }
    }

    public static bool VerifyRuntime()
    {
        // TODO | Runtime verification with "which/where" command and stdout + stderr redirect and reading
        ProcessStartInfo startInfo = new()
        {
            Arguments = StovetopRuntime,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        if (OSVersion.Platform == PlatformID.Unix)
            startInfo.FileName = "which";
        else if (OSVersion.Platform == PlatformID.Win32NT)
            startInfo.FileName = "where";

        using (Process? process = Process.Start(startInfo))
        {
            process?.WaitForExit();
            if (process != null && process.ExitCode != 0)
            {
                StovetopLogger?.Error($"Runtime '{StovetopRuntime}' not found.");
                return false;
            }
        }

        return true;
    }
}
