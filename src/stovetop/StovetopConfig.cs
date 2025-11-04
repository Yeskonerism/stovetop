using System.Text.Json;
using System.Text.Json.Serialization;

namespace Stovetop.stovetop;

public class StovetopConfig
{
    [JsonPropertyName("project")] public string Project { get; set; } = "";
    [JsonPropertyName("workingDirectory")] public string WorkingDirectory { get; set; } = "";
    [JsonPropertyName("runtime")] public string Runtime { get; set; } = "";
    [JsonPropertyName("runCommand")] public string RunCommand { get; set; } = "";
    [JsonPropertyName("buildCommand")] public string BuildCommand { get; set; } = "";
    
    /*
     * The idea:
     *
     *  "aliases": {
     *       "test": "dotnet test --verbosity normal",
     *       "clean": "rm -rf bin obj",
     *       "deploy": "dotnet publish -c Release && scp -r ./bin/Release user@server:/app",
     *       "db-migrate": "dotnet ef database update"
     *   }
     *
     * My general idea for aliases is that they will run custom shell commands specific to the project, as opposed to just renamed commands.
     * This is because my CommandRegistry class already supports aliases, these never need to be changed either (though, could possibly be
     * able to adjust them in a global stove config file)
     */
    [JsonPropertyName("aliases")] public Dictionary<string, string> Aliases { get; set; } = new();
    
    public static StovetopConfig Load(string file = ".stove/stovetop.json")
    {
        if (!File.Exists(file))
            throw new Exception("[STOVE] Error: stovetop.json not found\nTry running:\n\tstove init");
        
        string text = File.ReadAllText(file);
        StovetopConfig? config = JsonSerializer.Deserialize<StovetopConfig>(text);

        if (config == null || string.IsNullOrEmpty(config.Runtime) || string.IsNullOrEmpty(config.RunCommand))
            throw new Exception("[STOVE] Invalid stovetop.json: missing runtime or run command");

        config.Aliases ??= new Dictionary<string, string>();
        
        return config;
    }
}