using System.Diagnostics;
using System.Text.Json;
using Stovetop.Config;

namespace Stovetop.Commands;

public class RunCommand
{
    public static void Run()
    {
        // check if config file exists in current directory
        if (!File.Exists("stovetop.json"))
            throw new Exception("[STOVE] Error: stovetop.json not found\nTry running:\n\tstove init");

        string stoveConfig = File.ReadAllText("stovetop.json");
        StovetopConfig? jsonConfig = JsonSerializer.Deserialize<StovetopConfig>(stoveConfig);

        if (jsonConfig == null || string.IsNullOrEmpty(jsonConfig.Runtime) || string.IsNullOrEmpty(jsonConfig.RunCommand))
            throw new Exception("[STOVE] Invalid stovetop.json: missing runtime or run command");

        List<string> args = new List<string>();
        string currentToken = "";
        bool inQuotes = false;

        foreach (char c in jsonConfig.RunCommand)
        {
            if (c == '"' || c == '\'')
            {
                inQuotes = !inQuotes; // toggle
            }
            else if (c == ' ' && !inQuotes)
            {
                if (!string.IsNullOrEmpty(currentToken))
                {
                    args.Add(currentToken);
                    currentToken = "";
                }
            }
            else
            {
                currentToken += c;
            }
        }

        if (!string.IsNullOrEmpty(currentToken))
            args.Add(currentToken);

        var runProcess = new ProcessStartInfo
        {
            FileName = jsonConfig.Runtime,
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
        };

        foreach (var arg in args)
            runProcess.ArgumentList.Add(arg);

        Console.WriteLine($"[STOVE] Starting process...");
        
        var procress = Process.Start(runProcess);
        procress.WaitForExit();
        
        if(procress.ExitCode != 0)
            Console.WriteLine($"[STOVE] Error: process exited with code {procress.ExitCode}");
    }

    public static void Build()
    {
        if (!File.Exists("stovetop.json"))
            throw new Exception("[STOVE] Error: stovetop.json not found\nTry running:\n\tstove init");

        string stoveConfig = File.ReadAllText("stovetop.json");
        StovetopConfig? jsonConfig = JsonSerializer.Deserialize<StovetopConfig>(stoveConfig);

        if (jsonConfig == null || string.IsNullOrEmpty(jsonConfig.Runtime) || string.IsNullOrEmpty(jsonConfig.BuildCommand))
            throw new Exception("[STOVE] Invalid stovetop.json: missing runtime or build command");

        var buildProcess = new ProcessStartInfo
        {
            FileName = jsonConfig.Runtime,
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false
        };

        buildProcess.ArgumentList.Add(jsonConfig.BuildCommand);

        var process = Process.Start(buildProcess);
        process.WaitForExit();
        
        Console.WriteLine("[STOVE] Stove has successfully cooked your program, serve with \"stove run\"");
    }
}
