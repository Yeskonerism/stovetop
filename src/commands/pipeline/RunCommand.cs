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

        // Determine what to run: prefer executable if set, otherwise use runtime + runCommand
        string? fileName;
        string[] arguments;

        if (!string.IsNullOrEmpty(StovetopCore.StovetopConfig?.Stovetop.Commands.Executable))
        {
            // Running a compiled executable directly
            string executable = StovetopVariableHandler.SubstituteVariables(
                StovetopCore.StovetopConfig.Stovetop.Commands.Executable
            );
            StovetopCore.StovetopLogger?.Info($"Using executable: {executable}");
            fileName = executable;
            arguments = Array.Empty<string>(); // Executables typically don't need runtime args
        }
        else
        {
            // Running with a runtime (e.g., dotnet, python, node)
            fileName = StovetopCore.StovetopRuntime;
            string runCommand = StovetopVariableHandler.SubstituteVariables(
                StovetopCore.StovetopConfig?.Stovetop.Commands.Run ?? ""
            );
            arguments = CommandParser.ParseArguments(runCommand);
        }

        var runProcess = new ProcessStartInfo
        {
            FileName = fileName,
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
