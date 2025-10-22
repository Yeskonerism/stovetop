using System.Diagnostics;
using System.Text.Json;
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

            // pre-run hook
            if (!string.IsNullOrEmpty(StovetopCore.STOVETOP_CONFIG.PreRun))
            {
                var firstSpacePre = StovetopCore.STOVETOP_CONFIG.PreRun.IndexOf(' ');
                string fileNamePre = StovetopCore.STOVETOP_CONFIG.PreRun.Substring(0, firstSpacePre);
                string argumentsPre = StovetopCore.STOVETOP_CONFIG.PreRun.Substring(firstSpacePre + 1);

                var preRunProcess = new ProcessStartInfo
                {
                    FileName = fileNamePre,
                    Arguments = argumentsPre,
                    UseShellExecute = false
                };
                Process.Start(preRunProcess)?.WaitForExit();

            }

            // run primary stove process
            var procress = Process.Start(runProcess);
            procress.WaitForExit();
        }

        // post-run hook
        if (!string.IsNullOrEmpty(StovetopCore.STOVETOP_CONFIG.PostRun))
        {
            var firstSpacePost = StovetopCore.STOVETOP_CONFIG.PostRun.IndexOf(' ');
            string fileNamePost = StovetopCore.STOVETOP_CONFIG.PostRun.Substring(0, firstSpacePost);
            string argumentsPost = StovetopCore.STOVETOP_CONFIG.PostRun.Substring(firstSpacePost + 1);

            var postRunProcess = new ProcessStartInfo
            {
                FileName = fileNamePost,
                Arguments = argumentsPost,
                UseShellExecute = false,
            };

            Process.Start(postRunProcess)?.WaitForExit();
        }
    }
}
