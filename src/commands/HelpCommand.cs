namespace Stovetop.Commands;

public class HelpCommand
{
    public static void Run()
    {
        Console.WriteLine("STOVETOP v1.0.0 — Custom Project Config Builder\n" +
                          "Manage builds, runtime configs, hooks, and project profiles with ease.\n\n" +
                          "USAGE:\n" +
                          "stove <command> [options]\n" +
                          "CORE COMMANDS:\n" +
                          "init\t\tInitialize a new project\n" +
                          "run\t\tRun the project\n" +
                          "build\t\tBuild the project\n" +
                          "backup\t\tCreate a backup of the current config\n" +
                          "help\t\tShow this help message\n");
    }
}