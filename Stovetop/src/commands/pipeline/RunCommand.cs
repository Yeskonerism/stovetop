using System.Diagnostics;
using Stovetop.Exceptions;
using Stovetop.stovetop;
using Stovetop.stovetop.handlers;

namespace Stovetop.Commands.Pipeline;

public class RunCommand
{
    public static void Run()
    {
        // verify runtime exists
        if (!StovetopCore.StovetopConfigExists)
            throw new StovetopNonexistentConfigException();

        // Determine what to run: prefer executable if set, otherwise use runtime + runCommand
        string? fileName;
        string[] arguments;

        string executableCmd = null;

        // Attempt to get executable command
        StovetopCore.StovetopConfig?.Commands.TryGetValue("executable", out executableCmd);

        // Determine execution mode
        bool useExecutable = !string.IsNullOrWhiteSpace(executableCmd);

        if (useExecutable)
        {
            fileName = StovetopVariableHandler.SubstituteVariables(executableCmd!);
            arguments = Array.Empty<string>();

            StovetopCore.StovetopLogger?.Info($"Using executable: {fileName}");
        }
        else
        {
            fileName = StovetopCore.StovetopRuntime;

            string runCmd = null;

            StovetopCore.StovetopConfig?.Commands.TryGetValue("run", out runCmd);
            string runCommand = StovetopVariableHandler.SubstituteVariables(runCmd ?? "");

            arguments = CommandParser.ParseArguments(runCommand);
        }

        var runProcess = new ProcessStartInfo { FileName = fileName, UseShellExecute = false };

        foreach (var arg in arguments)
            runProcess.ArgumentList.Add(arg);

        if (!StovetopCore.RunHookless)
            StovetopHookHandler.ExecuteHook(HookType.PreRun);

        // run primary stove process
        StovetopCore.StovetopLogger?.Info("Running main project...");

        var process = Process.Start(runProcess);
        if (process == null)
            throw new StovetopProcessStartFailedException();
        process.WaitForExit();

        if (process.ExitCode != 0)
            StovetopCore.StovetopLogger?.Error(
                $"Stove has failed to run your project. Exited with code: {process.ExitCode}"
            );
        else
            StovetopCore.StovetopLogger?.Success("Stove has served your project successfully.");

        // post-run hook
        if (!StovetopCore.RunHookless)
            StovetopHookHandler.ExecuteHook(HookType.PostRun);
    }
}
