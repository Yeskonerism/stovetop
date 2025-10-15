using System.Text.Json;
using Stovetop.Config;

namespace Stovetop.Commands;

public static class InitCommand
{
    public static void Run(string runtime)
    {
        var config = new StovetopConfig { Runtime = runtime };

        Console.Write("[STOVE] Enter project name -> ");
        config.Project = Console.ReadLine() ?? "";
        
        Console.Write("[STOVE] Enter run command -> ");
        config.RunCommand = Console.ReadLine() ?? "";
        
        Console.Write("[STOVE] Enter build command -> ");
        config.BuildCommand = Console.ReadLine() ?? "";
        
        Console.Write(
            (config.Project != "") 
                ? $"[STOVE] Would you like to save your stove config for {config.Project}? [Y/n] -> " 
                : "[STOVE] Would you like to save your stove config? -> "
            );

        if (Console.ReadLine() == "" || Console.ReadLine().ToLower() == "y")
        {
            string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("stovetop.json", json);
        }
    }
}