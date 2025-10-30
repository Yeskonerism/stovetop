using Stovetop.stovetop;

namespace Stovetop.commands.config;

public class BackupCommand
{
    public static void Run()
    {
        StovetopBackup.CreateBackup();
    }
}