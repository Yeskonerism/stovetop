using System.Diagnostics;
using Stovetop.stovetop;

namespace Stovetop.Commands.Pipeline;

//
// RunCommand.cs
// The class containing the necessary methods for running and building
// a project from Stovetop's config file
//
// Oliver Hughes
// 22-oct-25
//
public class RunCommand
{
    public static void Run()
    {
        if (StovetopCore.STOVETOP_CONFIG != null)
        {
            string[] args = CommandParser.ParseArguments(StovetopCore.STOVETOP_CONFIG.RunCommand);

            var runProcess = new ProcessStartInfo
            {
                FileName = StovetopCore.STOVETOP_RUNTIME,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
            };

            foreach (var arg in args)
                runProcess.ArgumentList.Add(arg);

            StovetopHookHandler.ExecuteHook(HookType.PreRun);

            StovetopCore.STOVETOP_LOGGER.Info("Running main project...");
            
            // run primary stove process
            var procress = Process.Start(runProcess);
            procress.WaitForExit();
            
            if(procress.ExitCode != 0)
                StovetopCore.STOVETOP_LOGGER.Error(procress.StandardError.ReadToEnd());
            else
                StovetopCore.STOVETOP_LOGGER.Success("Stove has served your project successfully.");
            
            // post-run hook
            StovetopHookHandler.ExecuteHook(HookType.PostRun);
        }
    }
}
