using System.Diagnostics;
using Stovetop.stovetop;
using Stovetop.stovetop.handlers;

namespace Stovetop.Commands.Pipeline;

public class RunCommand
{
    public static void Run()
    {
        // verify runtime exists
        if(!StovetopCore.VerifyRuntime()) return;
        
        string[] arguments = CommandParser.ParseArguments(
            StovetopCore.StovetopConfig?.RunCommand
        );

        var runProcess = new ProcessStartInfo
        {
            FileName = StovetopCore.StovetopRuntime,
            UseShellExecute = false,
        };

        foreach (var arg in arguments)
            runProcess.ArgumentList.Add(arg);

        StovetopHookHandler.ExecuteHook(HookType.PreRun);

        // run primary stove process
        StovetopCore.StovetopLogger?.Info("Running main project...");

        var process = Process.Start(runProcess);
        if (process == null)
        {
            StovetopCore.StovetopLogger?.Error("Failed to start run process");
            return;
        }
        process.WaitForExit();

        if (process.ExitCode != 0)
            StovetopCore.StovetopLogger?.Error(
                $"Stove has failed to run your project. Exited with code: {process.ExitCode}"
            );
        else
            StovetopCore.StovetopLogger?.Success("Stove has served your project successfully.");

        // post-run hook
        StovetopHookHandler.ExecuteHook(HookType.PostRun);
    }
}
