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

        StovetopCore.StovetopConfig = new StovetopConfig();

        StovetopCore.StovetopConfig.Project = StovetopInputHandler.Ask(
            "[STOVE] Enter project name",
            Path.GetFileName(Directory.GetCurrentDirectory())
        );

        string runtime = CommandRegistry.GetPositionalArgument("init", 1) ?? "";

        string runtimeAsked = string.IsNullOrEmpty(runtime)
            ? StovetopInputHandler.Ask("[STOVE] Enter project runtime", "dotnet")
            : runtime;

        StovetopCore.StovetopConfig.Stovetop.Runtime.Type = runtimeAsked;

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
        bool isCompiledLanguage = compiledLanguageRuntimes.Contains(runtimeAsked.ToLower());

        // If it's a compiled language or --executable flag is set, ask for executable
        if (executableFlag || isCompiledLanguage)
        {
            string defaultExecutable = isCompiledLanguage
                ? $"./bin/{StovetopCore.StovetopConfig.Project}"
                : $"bin/{StovetopCore.StovetopConfig.Project}";

            StovetopCore.StovetopConfig.Stovetop.Commands.Executable = StovetopInputHandler.Ask(
                $"[STOVE] Enter executable path",
                defaultExecutable
            );
        }
        else
        {
            StovetopCore.StovetopConfig.Stovetop.Commands.Run = StovetopInputHandler.Ask(
                $"[STOVE] Enter run command",
                "run --"
            );
        }

        StovetopCore.StovetopConfig.Stovetop.Commands.Build = StovetopInputHandler.Ask(
            "[STOVE] Enter build command",
            "build"
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
