using Stovetop.Commands.Config;
using Stovetop.stovetop;
using Stovetop.stovetop.handlers;

namespace Stovetop.Commands.Pipeline;

public static class InitCommand
{
    public static void Run()
    {
        StovetopCore.CreateDefaultStructure();

        StovetopCore.StovetopConfig = new StovetopConfig
        {
            WorkingDirectory = Directory.GetCurrentDirectory(),
        };

        StovetopCore.StovetopConfig.Project = StovetopInputHandler.Ask("[STOVE] Enter project name");

        string runtime = CommandRegistry.GetPositionalArgument("init", 1) ?? "";

        string runtimeAsked = string.IsNullOrEmpty(runtime)
            ? StovetopInputHandler.Ask("[STOVE] Enter project runtime", "dotnet")
            : runtime;
        StovetopCore.StovetopConfig.Runtime = runtimeAsked;

        StovetopCore.StovetopConfig.RunCommand = StovetopInputHandler.Ask(
            "[STOVE] Enter run command",
            "run --"
        );
        StovetopCore.StovetopConfig.BuildCommand = StovetopInputHandler.Ask(
            "[STOVE] Enter build command",
            "build"
        );

        if (StovetopCore.StovetopConfigExists)
        {
            if (!StovetopInputHandler.Confirm("[STOVE] Config already exists. Overwrite?", false))
            {
                StovetopCore.StovetopLogger?.Warn("Aborted: existing configuration preserved.");
                return;
            }

            // create a backup version if overwriting stove config file
            BackupCommand.CreateBackup();
        }

        if (
            StovetopInputHandler.Confirm(
                (StovetopCore.StovetopConfig.Project != "")
                    ? $"[STOVE] Save config for {StovetopCore.StovetopConfig.Project}?"
                    : "[STOVE] Save this stove config?"
            )
        )
        {
            StovetopCore.SaveConfig();
            StovetopCore.StovetopLogger?.Info($"Config saved to {StovetopCore.StovetopConfigPath}");
            StovetopCore.StovetopLogger?.Success(
                "Stovetop ready! Use 'stove run' to test your setup."
            );
        }
        else
        {
            StovetopCore.StovetopLogger?.Warn("Configuration process aborted.");
        }
    }
}
