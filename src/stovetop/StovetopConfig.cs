using System.Text.Json;
using System.Text.Json.Serialization;

namespace Stovetop.Config;

public class StovetopConfig
{
    [JsonPropertyName("project")] public string Project { get; set; } = "";
    [JsonPropertyName("runtime")] public string Runtime { get; set; } = "";
    [JsonPropertyName("runCommand")] public string RunCommand { get; set; } = "";
    [JsonPropertyName("buildCommand")] public string BuildCommand { get; set; } = "";
}