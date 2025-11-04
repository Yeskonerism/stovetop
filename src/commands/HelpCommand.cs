namespace Stovetop.Commands;

public class HelpCommand
{
    public static void Run()
    {
        Console.WriteLine("STOVETOP v1.0.0 — Custom Project Config Builder\n" +
                          "Manage builds, runtime configs, hooks, and project profiles with ease.\n\n" +
                          "USAGE:\n" +
                          "stove <command> [options]\n");

        foreach (var command in CommandRegistry.Commands)
        {
            Console.Write(command.Name + "\t\t" + command.Description + " [");
            
            for(int i = 0; i < command.Aliases.Length; i++)
            {
                if (i > 0) Console.Write(", ");
                Console.Write(command.Aliases[i]);
            }
            
            Console.Write("]");
            Console.WriteLine();
        }
    }
}