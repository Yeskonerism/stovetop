using System.Diagnostics;

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

            ExecuteHookCommand(hookCommand);
        }
        catch (Exception ex)
        {
            StovetopCore.StovetopLogger?.Error($"{hookType} hook failed: {ex.Message}");
        }
    }

    private static void ExecuteHookCommand(string hookCommand)
    {
        // Execute as shell command with variable substitution
        string templatedCommand = StovetopVariableHandler.SubstituteVariables(hookCommand);
        ExecuteShellCommand(templatedCommand);
    }

    private static void ExecuteShellCommand(string command)
    {
        var process = new ProcessStartInfo
        {
            FileName = Environment.OSVersion.Platform == PlatformID.Win32NT ? "cmd" : "bash",
            Arguments =
                Environment.OSVersion.Platform == PlatformID.Win32NT
                    ? $"/c \"{command}\""
                    : $"-c \"{command}\"",
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
        };

        var proc = Process.Start(process);
        proc?.WaitForExit();
    }

    private static string? GetHookCommand(HookType hookType)
    {
        if (StovetopCore.StovetopConfig == null)
            return null;

        // Only check config hooks - no file fallback
        string? hookKey = hookType switch
        {
            HookType.PreRun => "pre_run",
            HookType.PostRun => "post_run",
            HookType.PreBuild => "pre_build",
            HookType.PostBuild => "post_build",
            HookType.PreDeploy => "pre_deploy",
            HookType.PostDeploy => "post_deploy",
            _ => null,
        };

        if (
            hookKey != null
            && StovetopCore.StovetopConfig.Hooks.TryGetValue(hookKey, out string? hookScript)
        )
        {
            return hookScript;
        }

        return null;
    }
}
