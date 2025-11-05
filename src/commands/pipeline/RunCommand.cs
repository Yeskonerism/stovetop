using System.Diagnostics;
using Stovetop.stovetop;

namespace Stovetop.Commands.Pipeline;

public class RunCommand
{
    public static void Run()
    {
        if (StovetopCore.StovetopConfig != null)
        {
            string[] arguments = CommandParser.ParseArguments(
                StovetopCore.StovetopConfig.RunCommand
            );

            var runProcess = new ProcessStartInfo
            {
                FileName = StovetopCore.StovetopRuntime,
                UseShellExecute = false,
            };

            foreach (var arg in arguments)
                runProcess.ArgumentList.Add(arg);

            StovetopHookHandler.ExecuteHook(HookType.PreRun);

            StovetopCore.StovetopLogger?.Info("Running main project...");

            // run primary stove process
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
}
