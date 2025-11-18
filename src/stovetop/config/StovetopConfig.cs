using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Stovetop.stovetop.config;

// TODO | Environment variables
public class StovetopConfig
{
    public string Project { get; set; } = "";
    public string Version { get; set; } = "0.0.1";
    public StovetopSection Stovetop { get; set; } = new();

    public static StovetopConfig Load(string file = ".stove/stovetop.config.yaml")
    {
        if (!File.Exists(file))
            throw new Exception(
                "[STOVE] Error: stovetop.config.yaml not found\nTry running:\n\tstove init"
            );

        string yaml = File.ReadAllText(file);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        StovetopConfig? config = deserializer.Deserialize<StovetopConfig>(yaml);

        if (
            config == null
            || string.IsNullOrEmpty(config.Stovetop.Runtime.Type)
            || (string.IsNullOrEmpty(config.Stovetop.Commands.Run) && string.IsNullOrEmpty(config.Stovetop.Commands.Executable))
        )
            throw new Exception(
                "[STOVE] Invalid stovetop.config.yaml: missing runtime type, run command, or executable"
            );

        return config;
    }

    public StovetopConfig Clone()
    {
        StovetopConfig clone = new()
        {
            Project = StovetopCore.StovetopConfig!.Project,
            Version = StovetopCore.StovetopConfig.Version,
            Stovetop = new StovetopSection
            {
                Variables = new Dictionary<string, string>(StovetopCore.StovetopConfig.Stovetop.Variables),
                Runtime = new RuntimeConfig
                {
                    Type = StovetopCore.StovetopConfig.Stovetop.Runtime.Type,
                    Version = StovetopCore.StovetopConfig.Stovetop.Runtime.Version
                },
                Commands = new CommandsConfig
                {
                    Build = StovetopCore.StovetopConfig.Stovetop.Commands.Build,
                    Run = StovetopCore.StovetopConfig.Stovetop.Commands.Run,
                    Executable = StovetopCore.StovetopConfig.Stovetop.Commands.Executable,
                    Test = StovetopCore.StovetopConfig.Stovetop.Commands.Test,
                    Clean = StovetopCore.StovetopConfig.Stovetop.Commands.Clean,
                    Deploy = StovetopCore.StovetopConfig.Stovetop.Commands.Deploy
                },
                Aliases = new Dictionary<string, string>(StovetopCore.StovetopConfig.Stovetop.Aliases),
                Hooks = StovetopCore.StovetopConfig.Stovetop.Hooks,
                Profiles = StovetopCore.StovetopConfig.Stovetop.Profiles
            }
        };

        return clone;
    }

    public override bool Equals(object? obj)
    {
        return obj is StovetopConfig config
            && Project == config.Project
            && Version == config.Version
            && Stovetop.Equals(config.Stovetop);
    }

    protected bool Equals(StovetopConfig other)
    {
        return Project == other.Project
            && Version == other.Version
            && Stovetop.Equals(other.Stovetop);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Project, Version, Stovetop);
    }
}
