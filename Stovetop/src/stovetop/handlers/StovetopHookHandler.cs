using System.Diagnostics;
using Wasabi;
using static System.Environment;

namespace Stovetop.stovetop.handlers;

public enum HookType
{
    PreRun,
    PostRun,
    PreBuild,
    PostBuild,
    PreDeploy,
    PostDeploy,
}

public static class StovetopHookHandler
{
    public static void ExecuteHook(HookType hookType)
    {
        string? hookCommand = GetHookCommand(hookType);

        if (string.IsNullOrWhiteSpace(hookCommand))
        {
            StovetopCore.StovetopLogger?.Debug($"No {hookType} hook configured, skipping");
            return;
        }

        try
        {
            if (StovetopCore.RunVerbose)
                StovetopCore.StovetopLogger?.Info($"Running {hookType} hook...");

            var variables = StovetopCore.StovetopConfig?.Stovetop.Variables ?? new();
            variables["PROJECT"] = StovetopCore.StovetopConfig?.Project ?? "";
            variables["VERSION"] = StovetopCore.StovetopConfig?.Version ?? "";
            
            if(StovetopCore.RunSilent)
                variables["silent"] = "true";

            WasabiInterpreter.Execute(hookCommand, variables);
        }
        catch (Exception ex)
        {
            StovetopCore.StovetopLogger?.Error($"{hookType} hook failed: {ex.Message}");
        }
    }

    private static string? GetHookCommand(HookType hookType)
    {
        if (StovetopCore.StovetopConfig == null)
            return null;

        if (StovetopCore.StovetopConfigRoot != null)
        {
            string hookPath = Path.Combine(StovetopCore.StovetopConfigRoot, "scripts/hooks");

            return hookType switch
            {
                HookType.PreRun => hookPath + "/pre-run.wasabi",
                HookType.PostRun => hookPath + "/post-run.wasabi",
                HookType.PreBuild => hookPath + "/pre-build.wasabi",
                HookType.PostBuild => hookPath + "/post-build.wasabi",
                _ => null,
            };
        }

        return null;
    }

    private static (string fileName, string arguments) ParseHookCommand(string command)
    {
        command = command.Trim();

        // Handle quoted filenames: "my script.sh" arg1 arg2
        if (command.StartsWith('"'))
        {
            int closingQuote = command.IndexOf('"', 1);
            if (closingQuote > 0)
            {
                string fileName = command.Substring(1, closingQuote - 1);
                string arguments = command.Substring(closingQuote + 1).Trim();
                return (fileName, arguments);
            }
        }

        // Simple case: filename arg1 arg2
        int firstSpace = command.IndexOf(' ');
        if (firstSpace > 0)
        {
            string fileName = command.Substring(0, firstSpace);
            string arguments = command.Substring(firstSpace + 1);
            return (fileName, arguments);
        }

        // No arguments (yet!)
        return (command, "");
    }

    public static void CreateDefaultHookScripts()
    {
        if (StovetopCore.StovetopConfigRoot != null)
        {
            // Create default hook scripts
            if (StovetopCore.StovetopScriptRoot != null)
            {
                string hooksDir = Path.Combine(StovetopCore.StovetopScriptRoot, "hooks");

                CreateHookScript(
                    Path.Combine(hooksDir, "pre-run.wasabi"),
                    "log.info 'Project starting...'"
                );

                CreateHookScript(
                    Path.Combine(hooksDir, "post-run.wasabi"),
                    "log.info 'Project finished.'"
                );

                CreateHookScript(
                    Path.Combine(hooksDir, "pre-build.wasabi"),
                    "log.info 'Project building...'"
                );

                CreateHookScript(
                    Path.Combine(hooksDir, "post-build.wasabi"),
                    "log.info 'Project built.'"
                );
            }
        }
    }

    private static void CreateHookScript(string path, string content)
    {
        if (!File.Exists(path))
        {
            File.WriteAllText(path, content);
        }
    }
}
