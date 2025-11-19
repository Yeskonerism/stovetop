using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Stovetop.Commands;
using Stovetop.stovetop.config;
using Stovetop.stovetop.handlers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
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

    public static bool RunSilent;
    public static bool RunVerbose;
    public static bool RunHookless;

    public static void Initialize(bool ignoreConfig = false)
    {
        StovetopRoot = Directory.GetCurrentDirectory();
        StovetopConfigRoot = Path.Combine(StovetopRoot, ".stove");
        StovetopConfigPath = Path.Combine(StovetopConfigRoot, "stovetop.config.yaml");

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
                StovetopRuntime = StovetopConfig?.Stovetop.Runtime.Type;
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
            string yaml = File.ReadAllText(StovetopConfigPath);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            StovetopConfig = deserializer.Deserialize<StovetopConfig>(yaml);
        }
    }

    public static void SaveConfig()
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        string yaml = serializer.Serialize(StovetopConfig);

        if (StovetopConfigPath != null)
            File.WriteAllText(StovetopConfigPath, yaml);

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
