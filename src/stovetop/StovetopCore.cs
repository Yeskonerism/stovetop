using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Stovetop.stovetop.handlers;
using static System.Environment;

namespace Stovetop.stovetop;

public static class StovetopCore
{
    public static string? StovetopRoot;
    public static string? StovetopConfigRoot;
    public static string? StovetopConfigPath;
    public static bool StovetopConfigExists;
    public static StovetopConfig? StovetopConfig;
    public static StovetopLogger? StovetopLogger;
    public static string? StovetopRuntime;

    public static string? StovetopBackupRoot;
    public static string? StovetopScriptRoot;

    public static void Initialize(bool ignoreConfig = false)
    {
        StovetopRoot = Directory.GetCurrentDirectory();
        StovetopConfigRoot = Path.Combine(StovetopRoot, ".stove");
        StovetopConfigPath = Path.Combine(StovetopConfigRoot, "stovetop.json");

        StovetopBackupRoot = Path.Combine(StovetopConfigRoot, "cache/backups");
        StovetopScriptRoot = Path.Combine(StovetopConfigRoot, "scripts");

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
            string json = File.ReadAllText(StovetopConfigPath);
            StovetopConfig = JsonSerializer.Deserialize<StovetopConfig>(json);
        }
    }

    public static void SaveConfig()
    {
        string json = JsonSerializer.Serialize(
            StovetopConfig,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            }
        );
        if (StovetopConfigPath != null)
            File.WriteAllText(StovetopConfigPath, json);
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

        StovetopHookHandler.CreateDefaultHookScripts();
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
