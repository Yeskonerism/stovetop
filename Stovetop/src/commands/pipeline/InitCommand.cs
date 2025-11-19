using Stovetop.Commands.Config;
using Stovetop.stovetop;
using Stovetop.stovetop.config;
using Stovetop.stovetop.handlers;

namespace Stovetop.Commands.Pipeline;

public static class InitCommand
{
    public static void Run()
    {
        StovetopCore.CreateDefaultStructure();

        // Get runtime from args or ask user
        string runtime = CommandRegistry.GetPositionalArgument("init", 1) ?? "";
        string runtimeAsked = string.IsNullOrEmpty(runtime)
            ? StovetopInputHandler.Ask("[STOVE] Enter project runtime", "dotnet")
            : runtime;

        // Load template based on runtime
        StovetopCore.StovetopConfig = StovetopConfigTemplater.LoadTemplate(runtimeAsked);

        // Override project name with directory name (always use current directory)
        string defaultProjectName = Path.GetFileName(Directory.GetCurrentDirectory());
        StovetopCore.StovetopConfig.Project = StovetopInputHandler.Ask(
            "[STOVE] Enter project name",
            defaultProjectName
        );

        // Runtime type is already set from template, but allow user to confirm/change
        StovetopCore.StovetopConfig.Stovetop.Runtime.Type = StovetopInputHandler.Ask(
            "[STOVE] Enter project runtime",
            StovetopCore.StovetopConfig.Stovetop.Runtime.Type
        );

        // Check if --executable flag is explicitly set
        bool executableFlag =
            CommandRegistry.CurrentArgs != null
            && (
                CommandRegistry.CurrentArgs.Contains("--executable")
                || CommandRegistry.CurrentArgs.Contains("-e")
                || CommandRegistry.CurrentArgs.Contains("--exec")
            );

        // Auto-detect compiled languages
        string[] compiledLanguageRuntimes = { "gcc", "g++", "clang", "cc", "rustc", "go", "zig" };
        bool isCompiledLanguage = compiledLanguageRuntimes.Contains(
            StovetopCore.StovetopConfig.Stovetop.Runtime.Type.ToLower()
        );

        // If it's a compiled language or --executable flag is set, ask for executable
        if (executableFlag || isCompiledLanguage)
        {
            // Use template default if available, otherwise generate default
            string defaultExecutable = $"bin/{StovetopCore.StovetopConfig.Project}";
            StovetopCore.StovetopConfig.Stovetop.Commands.Executable = StovetopInputHandler.Ask(
                "[STOVE] Enter executable path",
                defaultExecutable
            );
        }
        else
        {
            // Use template default for run command
            string defaultRun = !string.IsNullOrEmpty(
                StovetopCore.StovetopConfig.Stovetop.Commands.Run
            )
                ? StovetopCore.StovetopConfig.Stovetop.Commands.Run
                : "run --";

            StovetopCore.StovetopConfig.Stovetop.Commands.Run = StovetopInputHandler.Ask(
                "[STOVE] Enter run command",
                defaultRun
            );
        }

        // Use template default for build command
        string defaultBuild = !string.IsNullOrEmpty(
            StovetopCore.StovetopConfig.Stovetop.Commands.Build
        )
            ? StovetopCore.StovetopConfig.Stovetop.Commands.Build
            : "build";
        
        if(defaultBuild.Contains("bin/app"))
        {
            defaultBuild = defaultBuild.Replace("bin/app", $"bin/{StovetopCore.StovetopConfig.Project}");
        }
        
        StovetopCore.StovetopConfig.Stovetop.Commands.Build = StovetopInputHandler.Ask(
            "[STOVE] Enter build command",
            defaultBuild
        );
        
        

        if (StovetopCore.VerifyConfig(true))
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
