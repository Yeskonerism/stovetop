using Stovetop.stovetop;

namespace Stovetop.Commands.Config;

public class RevertCommand
{
    public static void Run(string id)
    {
        StovetopBackup.RevertToBackup(id);
    }
}