using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Stovetop.stovetop;

//
// StovetopCore.cs
// A publicly accessible class containing all relevant Stovetop
// information (such as the config path)
//
// Oliver Hughes
// 22-oct-25 
//
public static class StovetopCore
{
    public static string STOVETOP_ROOT;
    public static string STOVETOP_CONFIG_ROOT;
    public static string STOVETOP_CONFIG_PATH;
    public static bool STOVETOP_CONFIG_EXISTS;
    public static StovetopConfig STOVETOP_CONFIG;
    public static StovetopLogger STOVETOP_LOGGER;
    public static string STOVETOP_RUNTIME;
    
    public static void Initialize()
    {
        STOVETOP_ROOT = Directory.GetCurrentDirectory();
        STOVETOP_CONFIG_ROOT = Path.Combine(STOVETOP_ROOT, ".stove");
        STOVETOP_CONFIG_PATH =  Path.Combine(STOVETOP_CONFIG_ROOT, "stovetop.json");

        SetupLogger();
        VerifyConfig();

        if (STOVETOP_CONFIG_EXISTS)
        {
            LoadConfig();
            STOVETOP_RUNTIME = STOVETOP_CONFIG.Runtime;
        }
    }

    public static bool VerifyConfig(bool backup = false)
    {
        STOVETOP_CONFIG_EXISTS = File.Exists(STOVETOP_CONFIG_PATH);
        if(!backup)
            STOVETOP_LOGGER.Info(STOVETOP_CONFIG_EXISTS ? "Main Config Verified" : "Main Config Not Verified");
        else
            STOVETOP_LOGGER.Info(STOVETOP_CONFIG_EXISTS ? "Backup Verified" : "Backup Not Verified");
        
        return STOVETOP_CONFIG_EXISTS;
    }

    public static void LoadConfig()
    {
        string json = File.ReadAllText(STOVETOP_CONFIG_PATH);
        STOVETOP_CONFIG = JsonSerializer.Deserialize<StovetopConfig>(json);
    }

    public static void SaveConfig()
    {
        string json = JsonSerializer.Serialize(STOVETOP_CONFIG, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        });
        File.WriteAllText(STOVETOP_CONFIG_PATH, json);
        STOVETOP_LOGGER.Success("Configuration saved successfully.");
    }

    public static void SetupLogger()
    {
        STOVETOP_LOGGER = new StovetopLogger();
        STOVETOP_LOGGER.Info("Logger initialized");
    }

    public static void CreateDefaultStructure()
    {
        Directory.CreateDirectory(STOVETOP_CONFIG_ROOT);
            
        Directory.CreateDirectory(STOVETOP_CONFIG_ROOT);
        foreach (var subdir in new[] { "profiles", "cache", "cache/backups" })
            Directory.CreateDirectory(Path.Combine(STOVETOP_CONFIG_ROOT, subdir));
        
        StovetopHookHandler.CreateDefaultHookScripts();
    }
}