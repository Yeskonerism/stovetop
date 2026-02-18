using Stovetop.Commands.Config;
using Stovetop.ConfigParser;
using Stovetop.stovetop;
using Stovetop.stovetop.handlers;

namespace Stovetop.Commands.Pipeline;

public static class InitCommand
{
    public static void Run()
    {
        StovetopCore.CreateDefaultStructure();

        bool loadTemplate = CommandRegistry.CurrentArgs?.Contains("--template") ?? false;
        bool noPrompt =
            CommandRegistry.CurrentArgs?.Contains("--no-prompt")
            ?? false
                || CommandRegistry.CurrentArgs!.Contains("-y")
                || CommandRegistry.CurrentArgs!.Contains("--yes");

        // Determine which template to load
        string? template = GetTemplateToLoad(loadTemplate, noPrompt);

        // Load template based on runtime/template
        if (template != null)
            StovetopCore.StovetopConfig = StovetopConfigTemplater.LoadTemplate(template);

        // Handle non-interactive mode (--no-prompt / -y / --yes)
        if (noPrompt)
        {
            ApplyDefaultsAndSave();
            return;
        }

        // === INTERACTIVE MODE: Prompt for all values ===

        // Override project name with directory name (always use current directory)
        string defaultProjectName = Path.GetFileName(Directory.GetCurrentDirectory());
        if (StovetopCore.StovetopConfig != null)
        {
            StovetopCore.StovetopConfig.Project = StovetopInputHandler.Ask(
                "[STOVE] Enter project name",
                defaultProjectName
            );

            // Runtime type is already set from template, but allow user to confirm/change
            StovetopCore.StovetopConfig.Runtime = StovetopInputHandler.Ask(
                "[STOVE] Enter project runtime",
                StovetopCore.StovetopConfig.Runtime
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
            string[] compiledLanguageRuntimes =
            {
                "gcc",
                "g++",
                "clang",
                "cc",
                "rustc",
                "go",
                "zig",
            };
            bool isCompiledLanguage = compiledLanguageRuntimes.Contains(
                StovetopCore.StovetopConfig.Runtime.ToLower()
            );

            string defaultExecutable = $"${{OUT}}/{StovetopCore.StovetopConfig.Project}";

            // If it's a compiled language or --executable flag is set, ask for executable
            if (executableFlag || isCompiledLanguage)
            {
                // Use template default if available, otherwise generate default
                StovetopCore.StovetopConfig.Commands["executable"] = StovetopInputHandler.Ask(
                    "[STOVE] Enter executable path",
                    defaultExecutable
                );
            }
            else
            {
                // Use template default for run command
                string defaultRun = StovetopCore.StovetopConfig.Commands.TryGetValue("run", out var runCmd)
                    && !string.IsNullOrEmpty(runCmd)
                    ? runCmd
                    : "run --";

                StovetopCore.StovetopConfig.Commands["run"] = StovetopInputHandler.Ask(
                    "[STOVE] Enter run command",
                    defaultRun
                );
            }

            // Use template default for build command
            string defaultBuild = StovetopCore.StovetopConfig.Commands.TryGetValue("build", out var buildCmd)
                && !string.IsNullOrEmpty(buildCmd)
                ? buildCmd
                : "build";

            StovetopCore.StovetopConfig.Commands["build"] = StovetopInputHandler.Ask(
                "[STOVE] Enter build command",
                defaultBuild.Replace("${OUT}/app", defaultExecutable)
            );

            if (StovetopCore.VerifyConfig(true))
            {
                if (
                    !StovetopInputHandler.Confirm(
                        "[STOVE] Config already exists. Overwrite?",
                        false
                    )
                )
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
                StovetopCore.StovetopLogger?.Info(
                    $"Config saved to {StovetopCore.StovetopConfigPath}"
                );
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

    /// <summary>
    /// Determines which template to load based on flags and arguments.
    /// </summary>
    private static string? GetTemplateToLoad(bool loadTemplate, bool noPrompt)
    {
        // If --template flag is used, get its value
        if (loadTemplate)
        {
            return CommandRegistry.GetFlagValue("--template");
        }

        // If positional argument is provided (e.g., "stove init dotnet")
        string? runtime = CommandRegistry.GetPositionalArgument("init", 0);
        
        Console.WriteLine(runtime);
        
        if (!string.IsNullOrEmpty(runtime))
        {
            return runtime;
        }

        // If in no-prompt mode and no template specified, use default
        if (noPrompt)
        {
            return "dotnet"; // Default template for non-interactive mode
        }

        // Interactive mode: ask user for runtime
        string runtimeAsked = StovetopInputHandler.Ask("[STOVE] Enter project runtime", "dotnet");
        return runtimeAsked;
    }

    /// <summary>
    /// Applies sensible defaults and saves config without prompting.
    /// Used when --no-prompt / -y / --yes flag is set.
    /// </summary>
    private static void ApplyDefaultsAndSave()
    {
        if (StovetopCore.StovetopConfig == null)
        {
            StovetopCore.StovetopLogger?.Error("Failed to load template configuration.");
            return;
        }

        // Set project name to current directory name
        string defaultProjectName = Path.GetFileName(Directory.GetCurrentDirectory());
        StovetopCore.StovetopConfig.Project = defaultProjectName;

        // Runtime type is already set from template, keep it as-is
        // Commands (build, run, executable) are already set from template

        // determine if executable
        string defaultExecutable = "${OUT}/" + defaultProjectName;

        if (StovetopCore.StovetopConfig.Commands.TryGetValue("executable", out var execCmd)
            && !string.IsNullOrEmpty(execCmd))
        {
            StovetopCore.StovetopConfig.Commands["executable"] = defaultExecutable;
            if (StovetopCore.StovetopConfig.Commands.TryGetValue("build", out var buildCmd))
            {
                StovetopCore.StovetopConfig.Commands["build"] = buildCmd.Replace("app", defaultProjectName);
            }
        }

        // Check if config already exists
        if (StovetopCore.VerifyConfig(true))
        {
            StovetopCore.StovetopLogger?.Warn(
                "Config already exists. Use interactive mode to overwrite."
            );
            return;
        }

        // Save the config
        StovetopCore.SaveConfig();
        StovetopCore.StovetopLogger?.Info($"Config saved to {StovetopCore.StovetopConfigPath}");
        StovetopCore.StovetopLogger?.Success("Stovetop ready! Use 'stove run' to test your setup.");
    }
}
