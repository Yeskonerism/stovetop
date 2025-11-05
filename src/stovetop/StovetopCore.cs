using System.Text.Json;
using System.Text.Json.Serialization;

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
}
