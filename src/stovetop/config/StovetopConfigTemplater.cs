using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Stovetop.stovetop.config;

public class StovetopConfigTemplater
{
    // Map of runtime names to template file names
    private static readonly Dictionary<string, string> RuntimeTemplateMap = new()
    {
        { "dotnet", "dotnet.yaml" },
        { "python", "python.yaml" },
        { "npm", "npm.yaml" },
        { "node", "npm.yaml" },
        { "gcc", "c.yaml" },
        { "g++", "c.yaml" },
        { "clang", "c.yaml" },
        { "cc", "c.yaml" }
    };

    /// <summary>
    /// Loads a template configuration from embedded resources based on runtime name.
    /// Returns a StovetopConfig with pre-filled values from the template.
    /// </summary>
    /// <param name="runtime">The runtime name (e.g., "dotnet", "python", "gcc")</param>
    /// <returns>A StovetopConfig loaded from the template, or a default config if template not found</returns>
    public static StovetopConfig LoadTemplate(string runtime)
    {
        // Normalize runtime name to lowercase
        string normalizedRuntime = runtime.ToLower();

        // Get template file name from map, or try using runtime name directly
        string templateFileName = RuntimeTemplateMap.ContainsKey(normalizedRuntime)
            ? RuntimeTemplateMap[normalizedRuntime]
            : $"{normalizedRuntime}.yaml";

        try
        {
            // Get the assembly containing the embedded resources
            var assembly = Assembly.GetExecutingAssembly();

            // Build the resource name (namespace path to the template)
            string resourceName = $"Stovetop.src.stovetop.templates.{templateFileName}";

            // Try to load the embedded resource
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                StovetopCore.StovetopLogger?.Warn(
                    $"Template '{templateFileName}' not found. Using default configuration."
                );
                return CreateDefaultTemplate(runtime);
            }

            // Read the YAML content from the stream
            using StreamReader reader = new StreamReader(stream);
            string yaml = reader.ReadToEnd();

            // Deserialize the YAML into a StovetopConfig object
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            StovetopConfig? config = deserializer.Deserialize<StovetopConfig>(yaml);

            if (config == null)
            {
                StovetopCore.StovetopLogger?.Warn(
                    $"Failed to parse template '{templateFileName}'. Using default configuration."
                );
                return CreateDefaultTemplate(runtime);
            }

            return config;
        }
        catch (Exception ex)
        {
            StovetopCore.StovetopLogger?.Error(
                $"Error loading template for '{runtime}': {ex.Message}"
            );
            return CreateDefaultTemplate(runtime);
        }
    }

    /// <summary>
    /// Creates a minimal default template when no template file is found.
    /// </summary>
    private static StovetopConfig CreateDefaultTemplate(string runtime)
    {
        return new StovetopConfig
        {
            Project = "",
            Version = "0.0.1",
            Stovetop = new StovetopSection
            {
                Runtime = new RuntimeConfig { Type = runtime, Version = "" },
                Commands = new CommandsConfig
                {
                    Build = "",
                    Run = "",
                    Executable = null,
                    Test = null,
                    Clean = null,
                    Deploy = null
                },
                Variables = new Dictionary<string, string>(),
                Aliases = new Dictionary<string, string>(),
                Hooks = null,
                Profiles = null
            }
        };
    }
}
