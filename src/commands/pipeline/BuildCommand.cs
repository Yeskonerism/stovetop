using System.Diagnostics;
using Stovetop.stovetop;

namespace Stovetop.Commands.Pipeline;

public class BuildCommand
{
    public static void Run()
    {
        if (StovetopCore.StovetopConfigExists)
        {
            string[] arguments = CommandParser.ParseArguments(StovetopCore.StovetopConfig?.BuildCommand);

            var buildProcess = new ProcessStartInfo
            {
                FileName = StovetopCore.StovetopRuntime,
                UseShellExecute = false,
            };
            
            foreach (var arg in arguments)
                buildProcess.ArgumentList.Add(arg);
            
            StovetopHookHandler.ExecuteHook(HookType.PreBuild);
            
            var process = Process.Start(buildProcess);
            if (process == null)
            {
                StovetopCore.StovetopLogger?.Error("Failed to start build process");
                return;
            }
            process.WaitForExit();
            
            StovetopHookHandler.ExecuteHook(HookType.PostBuild);
            
            if(process.ExitCode != 0)
                StovetopCore.StovetopLogger?.Error($"Stove failed to build your project. Exited with code: {process.ExitCode}");
            else
                StovetopCore.StovetopLogger?.Success("Stove has cooked your project successfully. Serve with 'stove run'");
        }
    }
}