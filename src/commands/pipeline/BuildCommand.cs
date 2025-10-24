using System.Diagnostics;
using Stovetop.stovetop;

namespace Stovetop.Commands.Pipeline;

public class BuildCommand
{
    public static void Run()
    {
        if (StovetopCore.STOVETOP_CONFIG_EXISTS)
        {
            string[] args = CommandParser.ParseArguments(StovetopCore.STOVETOP_CONFIG.BuildCommand);

            var buildProcess = new ProcessStartInfo
            {
                FileName = StovetopCore.STOVETOP_RUNTIME,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
            };
            
            foreach (var arg in args)
                buildProcess.ArgumentList.Add(arg);
            
            StovetopHookHandler.ExecuteHook(HookType.PreBuild);
            
            var process = Process.Start(buildProcess);
            process.WaitForExit();
            
            StovetopHookHandler.ExecuteHook(HookType.PostBuild);
            
            if(process.ExitCode != 0)
                StovetopCore.STOVETOP_LOGGER.Error(process.StandardError.ReadToEnd());
            else
                StovetopCore.STOVETOP_LOGGER.Success("Stove has cooked your project successfully. Serve with 'stove run'");
        }
    }
}