using System.Text.Json;
using System.Text.Json.Serialization;

namespace Stovetop.stovetop;

public class StovetopConfig
{
    public enum PrintMode
    {
        All,
        Profiles,
        Aliases,
        Hooks,
        Commands
    }
    
    [JsonPropertyName("project")] public string Project { get; set; } = "";
    [JsonPropertyName("workingDirectory")] public string WorkingDirectory { get; set; } = "";
    [JsonPropertyName("runtime")] public string Runtime { get; set; } = "";
    [JsonPropertyName("runCommand")] public string RunCommand { get; set; } = "";
    [JsonPropertyName("buildCommand")] public string BuildCommand { get; set; } = "";
    [JsonPropertyName("aliases")] public Dictionary<string, string> Aliases { get; set; } = new();
    [JsonPropertyName("profiles")] public string[] Profiles { get; set; } =  Array.Empty<string>();

    // hooks
    [JsonPropertyName("preRun")] public string PreRun { get; set; } = $"sh {StovetopCore.STOVETOP_CONFIG_ROOT}/scripts/preRunHook.sh";
    [JsonPropertyName("postRun")] public string PostRun { get; set; } = $"sh {StovetopCore.STOVETOP_CONFIG_ROOT}/scripts/postRunHook.sh";

    public static StovetopConfig Load(string file = ".stove/stovetop.json")
    {
        if (!File.Exists(file))
            throw new Exception("[STOVE] Error: stovetop.json not found\nTry running:\n\tstove init");
        
        string text = File.ReadAllText(file);
        StovetopConfig? config = JsonSerializer.Deserialize<StovetopConfig>(text);

        if (config == null || string.IsNullOrEmpty(config.Runtime) || string.IsNullOrEmpty(config.RunCommand))
            throw new Exception("[STOVE] Invalid stovetop.json: missing runtime or run command");

        config.Aliases ??= new Dictionary<string, string>();
        config.Profiles ??= Array.Empty<string>();
        config.PreRun ??= "";
        config.PostRun ??= "";
        
        return config;
    }
}