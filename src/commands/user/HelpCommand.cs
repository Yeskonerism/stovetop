using Stovetop.Commands;
using Stovetop.stovetop;

namespace Stovetop.commands.user;

public class HelpCommand
{
    public static void Run()
    {
        string? commandArgument = CommandRegistry.GetPositionalArgument("help", 1);

        if (string.IsNullOrEmpty(commandArgument))
        {
            Console.WriteLine(
                "STOVETOP v1.0.0 — Custom Project Config Builder\n"
                    + "Manage builds, runtime configs, hooks, and project profiles with ease.\n\n"
                    + "USAGE:\n"
                    + "stove <command> [options]\n"
            );

            foreach (var command in CommandRegistry.Commands)
            {
                Console.Write(command.Name + "\t\t" + command.Description + " [");

                if (command.Aliases != null)
                    for (int i = 0; i < command.Aliases.Length; i++)
                    {
                        if (i > 0)
                            Console.Write(", ");
                        Console.Write(command.Aliases[i]);
                    }

                Console.Write("]");
                Console.WriteLine();
            }
        }
        else
        {
            if (CommandRegistry.GetCommand(commandArgument) != null)
            {
                Console.WriteLine("USAGE:");
                Console.WriteLine(CommandRegistry.GetCommand(commandArgument)?.Usage);
                Console.WriteLine(CommandRegistry.GetCommand(commandArgument)?.Description);
            }
            else
            {
                StovetopCore.StovetopLogger?.Error($"Unknown command: {commandArgument}");
            }
        }
    }
}
