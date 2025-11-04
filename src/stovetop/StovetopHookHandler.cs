using System.Diagnostics;
using static System.Environment;

namespace Stovetop.stovetop;

public enum HookType
{
    PreRun,
    PostRun,
    PreBuild,
    PostBuild,
    PreDeploy,
    PostDeploy
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
            StovetopCore.StovetopLogger?.Info($"Running {hookType} hook...");
            
            var (fileName, arguments) = ParseHookCommand(hookCommand);
            
            var hookProcess = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = true
            };

            var process = Process.Start(hookProcess);
            if (process == null)
            {
                StovetopCore.StovetopLogger?.Error($"{hookType} hook failed to start");
                return;
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string error = process.StandardError.ReadToEnd();
                StovetopCore.StovetopLogger?.Warn($"{hookType} hook exited with code {process.ExitCode}");
                if (!string.IsNullOrEmpty(error))
                    StovetopCore.StovetopLogger?.Error(error);
            }
            else
            {
                StovetopCore.StovetopLogger?.Success($"{hookType} hook completed");
            }
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
                HookType.PreRun => hookPath + "/preRunHook.sh",
                HookType.PostRun => hookPath + "/postRunHook.sh",
                HookType.PreBuild => hookPath + "/preBuildHook.sh",
                HookType.PostBuild => hookPath + "/postBuildHook.sh",
                _ => null
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
                    Path.Combine(hooksDir, "preRunHook.sh"),
                    "#!/bin/bash\necho '[HOOK] Starting project...'"
                );

                CreateHookScript(
                    Path.Combine(hooksDir, "postRunHook.sh"),
                    "#!/bin/bash\necho '[HOOK] Project finished.'"
                );
        
                CreateHookScript(
                    Path.Combine(hooksDir, "preBuildHook.sh"),
                    "#!/bin/bash\necho '[HOOK] Project building...'"
                );
        
                CreateHookScript(
                    Path.Combine(hooksDir, "postBuildHook.sh"),
                    "#!/bin/bash\necho '[HOOK] Project built.'"
                );
            }
        }
    }

    private static void CreateHookScript(string path, string content)
    {
        if (!File.Exists(path))
        {
            File.WriteAllText(path, content);
            
            // Make executable on Unix systems
            if (OSVersion.Platform == PlatformID.Unix)
            {
                var chmod = Process.Start("chmod", $"+x {path}");
                chmod?.WaitForExit();
            }
        }
    }
}