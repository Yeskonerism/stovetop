using System.Text.Json;
using Stovetop.Commands.Config;
using Stovetop.stovetop;

namespace Stovetop.Commands.Pipeline;

public static class InitCommand
{
    public static void Run()
    {
        StovetopCore.CreateDefaultStructure();
        
        StovetopCore.StovetopConfig = new StovetopConfig
        {
            WorkingDirectory = Directory.GetCurrentDirectory()
        };

        StovetopCore.StovetopConfig.Project = StovetopInputHelper.Ask("[STOVE] Enter project name");

        string runtime = CommandRegistry.GetPositionalArgument("init", 1) ?? "";
        
        string runtimeAsked = string.IsNullOrEmpty(runtime) ? StovetopInputHelper.Ask("[STOVE] Enter project runtime", "dotnet") : runtime;
        StovetopCore.StovetopConfig.Runtime = runtimeAsked;
        
        StovetopCore.StovetopConfig.RunCommand = StovetopInputHelper.Ask("[STOVE] Enter run command", "run --");
        StovetopCore.StovetopConfig.BuildCommand = StovetopInputHelper.Ask("[STOVE] Enter build command", "build");

        StovetopCore.StovetopConfig.Aliases["r"] = "run";
        StovetopCore.StovetopConfig.Aliases["b"] = "build";

        if (StovetopCore.StovetopConfigExists)
        {
            if (!StovetopInputHelper.Confirm("[STOVE] Config already exists. Overwrite?", false))
            {
                StovetopCore.StovetopLogger?.Warn("Aborted: existing configuration preserved.");
                return;
            }

            // create a backup version if overwriting stove config file
            BackupCommand.CreateBackup();
        }
        
        if (StovetopInputHelper.Confirm((StovetopCore.StovetopConfig.Project != "")
            ? $"[STOVE] Save config for {StovetopCore.StovetopConfig.Project}?"
            : "[STOVE] Save this stove config?"
        ))
        {
            StovetopCore.SaveConfig();
            StovetopCore.StovetopLogger?.Info($"Config saved to {StovetopCore.StovetopConfigPath}");
            StovetopCore.StovetopLogger?.Success("Stovetop ready! Use 'stove run' to test your setup.");
        }
        else
        {
            StovetopCore.StovetopLogger?.Warn("Configuration process aborted.");
        }
    }
}
