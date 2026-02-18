using System.Diagnostics;
using Stovetop.Commands;
using Stovetop.stovetop;
using Stovetop.stovetop.handlers;

namespace Stovetop.commands.user;

/// <summary>
/// Executes Wasabi scripts with access to Stovetop variables and configuration.
/// </summary>
public class ScriptCommand
{
    public static void Run()
    {
        string? scriptName = CommandRegistry.GetPositionalArgument("script", 1);

        if (string.IsNullOrEmpty(scriptName))
        {
            StovetopCore.StovetopLogger?.Error("No script name specified");
            return;
        }

        // Check if it's a config-defined script first
        if (
            StovetopCore.StovetopConfig?.Scripts.TryGetValue(scriptName, out string? scriptContent)
            == true
        )
        {
            ExecuteInlineScript(scriptContent);
        }
        else
        {
            if (!File.Exists(scriptName))
            {
                StovetopCore.StovetopLogger?.Error("Script not found");
            }
            else
            {
                ExecuteShellScript(scriptName);
            }
        }
    }

    private static void ExecuteShellScript(string command)
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

    private static void ExecuteInlineScript(string scriptContent)
    {
        string templatedScript = StovetopVariableHandler.SubstituteVariables(scriptContent);
        ExecuteShellScript(templatedScript);
    }
}
