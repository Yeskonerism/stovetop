using System.Reflection;
using Stovetop.ConfigParser;

namespace Stovetop.stovetop.handlers;

public class StovetopConfigTemplater
{
    // Map of runtime names to template file names
    private static readonly Dictionary<string, string> RuntimeTemplateMap = new()
    {
        { "dotnet", "dotnet.stove" },
        { "python", "python.stove" },
        { "npm", "npm.stove" },
        { "node", "npm.stove" },
        { "gcc", "c.stove" },
        { "g++", "c.stove" },
        { "clang", "c.stove" },
        { "cc", "c.stove" },
        { "rustc", "rust.stove" },
        { "empty", "empty.stove" },
    };

    /// <summary>
    /// Loads a template configuration from embedded resources based on runtime name.
    /// Returns a ConfigModel with pre-filled values from the template.
    /// </summary>
    /// <param name="runtime">The runtime name (e.g., "dotnet", "python", "gcc")</param>
    /// <returns>A ConfigModel loaded from the template, or a default config if template not found</returns>
    public static ConfigModel LoadTemplate(string runtime)
    {
        // Normalize runtime name to lowercase
        string normalizedRuntime = runtime.ToLower();

        // Get template file name from map, or try using runtime name directly
        string templateFileName = RuntimeTemplateMap.TryGetValue(normalizedRuntime, out var mapped)
            ? mapped
            : $"{normalizedRuntime}.stove";

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

            // Read the .stove content from the stream
            using StreamReader reader = new StreamReader(stream);
            string stoveContent = reader.ReadToEnd();

            // Parse the .stove content into a ConfigModel object
            ConfigModel? config = StovetopConfigParser.Parse(stoveContent);

            if (config == null)
            {
                StovetopCore.StovetopLogger?.Warn(
                    $"Failed to parse template '{templateFileName}'. Using default configuration."
                );
                return CreateDefaultTemplate(runtime);
            }

            // Override the runtime type with the user's actual choice
            // This ensures that if they chose "clang" but loaded "c.stove",
            // the config will have "clang" not "gcc"
            config.Runtime = normalizedRuntime;

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
    private static ConfigModel CreateDefaultTemplate(string runtime)
    {
        return new ConfigModel
        {
            Project = "",
            Version = "0.0.1",
            Runtime = runtime,
            RuntimeVersion = "",
            Variables = new Dictionary<string, string>(),
            Commands = new Dictionary<string, string> { { "build", "" }, { "run", "" } },
            Aliases = new Dictionary<string, string>(),
        };
    }
}
