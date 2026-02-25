using System.Diagnostics;
using Stovetop.Exceptions;
using Stovetop.stovetop;
using Stovetop.stovetop.handlers;

namespace Stovetop.Commands.Pipeline;

public class BuildCommand
{
    public static void Run()
    {
        if (!StovetopCore.StovetopConfigExists)
            throw new StovetopNonexistentConfigException();

        if (StovetopCore.StovetopConfig == null)
            throw new StovetopUninitialisedException();

        if (string.IsNullOrEmpty(StovetopCore.StovetopRuntime))
            throw new StovetopNonexistentRuntimeException();

        string? buildCmd = null;
        StovetopCore.StovetopConfig?.Commands.TryGetValue("build", out buildCmd);
        string buildCommand = StovetopVariableHandler.SubstituteVariables(buildCmd ?? "");

        string[] arguments = CommandParser.ParseArguments(buildCommand);

        var buildProcess = new ProcessStartInfo
        {
            FileName = StovetopCore.StovetopRuntime,
            UseShellExecute = false,
        };

        foreach (var arg in arguments)
            buildProcess.ArgumentList.Add(arg);

        if (!StovetopCore.RunHookless)
            StovetopHookHandler.ExecuteHook(HookType.PreBuild);

        StovetopCore.StovetopLogger?.Info("Starting build process...");

        var process = Process.Start(buildProcess);
        if (process == null)
            throw new StovetopProcessStartFailedException();
        process.WaitForExit();

        if (process.ExitCode != 0)
            StovetopCore.StovetopLogger?.Error(
                $"Stove failed to build your project. Exited with code: {process.ExitCode}"
            );

        if (!StovetopCore.RunHookless)
            StovetopHookHandler.ExecuteHook(HookType.PostBuild);

        if (process.ExitCode == 0)
        {
            StovetopCore.StovetopLogger?.Success(
                "Stove has cooked your project successfully. Serve with 'stove run'"
            );
        }
    }
}
