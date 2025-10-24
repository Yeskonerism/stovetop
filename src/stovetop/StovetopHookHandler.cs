using System.Diagnostics;

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
            StovetopCore.STOVETOP_LOGGER.Debug($"No {hookType} hook configured, skipping");
            return;
        }

        try
        {
            StovetopCore.STOVETOP_LOGGER.Info($"Running {hookType} hook...");
            
            var (fileName, arguments) = ParseHookCommand(hookCommand);
            
            var hookProcess = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };

            var process = Process.Start(hookProcess);
            if (process == null)
            {
                StovetopCore.STOVETOP_LOGGER.Error($"{hookType} hook failed to start");
                return;
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string error = process.StandardError.ReadToEnd();
                StovetopCore.STOVETOP_LOGGER.Warn($"{hookType} hook exited with code {process.ExitCode}");
                if (!string.IsNullOrEmpty(error))
                    StovetopCore.STOVETOP_LOGGER.Error(error);
            }
            else
            {
                StovetopCore.STOVETOP_LOGGER.Success($"{hookType} hook completed");
            }
        }
        catch (Exception ex)
        {
            StovetopCore.STOVETOP_LOGGER.Error($"{hookType} hook failed: {ex.Message}");
        }
    }

    private static string? GetHookCommand(HookType hookType)
    {
        if (StovetopCore.STOVETOP_CONFIG == null)
            return null;

        return hookType switch
        {
            HookType.PreRun => StovetopCore.STOVETOP_CONFIG.HookPath + "/preRunHook.sh",
            HookType.PostRun => StovetopCore.STOVETOP_CONFIG.HookPath + "/postRunHook.sh",
            HookType.PreBuild => StovetopCore.STOVETOP_CONFIG.HookPath + "/preBuildHook.sh",
            HookType.PostBuild => StovetopCore.STOVETOP_CONFIG.HookPath + "/postBuildHook.sh",
            _ => null
        };
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
        string scriptsDir = Path.Combine(StovetopCore.STOVETOP_CONFIG_ROOT, "scripts");
        string hooksDir = Path.Combine(scriptsDir, "hooks");
        
        Directory.CreateDirectory(hooksDir);

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

    private static void CreateHookScript(string path, string content)
    {
        if (!File.Exists(path))
        {
            File.WriteAllText(path, content);
            
            // Make executable on Unix systems
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                var chmod = Process.Start("chmod", $"+x {path}");
                chmod?.WaitForExit();
            }
        }
    }
}