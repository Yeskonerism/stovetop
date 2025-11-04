using Stovetop.stovetop;

namespace Stovetop.Commands.Config;

public class BackupCommand
{
    public static void Run()
    {
        string? subcommand = CommandRegistry.GetSubcommand("backup", "create");

        switch (subcommand?.ToLower())
        {
            case "list" or "ls" or "view":
                ListBackups();
                break;
            case "revert":
                HandleRevert();
                break;
            default:
                CreateBackup();
                break;
        }
    }

    private static void HandleRevert()
    {
        string? backupId = CommandRegistry.GetPositionalArgument("backup",2);
        
        if (string.IsNullOrEmpty(backupId))
        {
            return;
        }
        
        string? resolvedBackupId = ResolveBackupId(backupId);
        
        if (string.IsNullOrEmpty(resolvedBackupId))
        {
            StovetopCore.StovetopLogger?.Error("No backups available");
            return;
        }
        
        RevertToBackup(resolvedBackupId);
    }
    
    private static string? ResolveBackupId(string backupId)
    {
        return (backupId == "latest") ? GetLatestBackup() : backupId;
    }

    private static string[]? BackupList()
    {
        if (StovetopCore.StovetopBackupRoot != null)
            return Directory.GetFiles(StovetopCore.StovetopBackupRoot, "*-stovetop-backup.json");

        return null;
    }
    
    private static void ListBackups()
    {
        if (!Directory.Exists(StovetopCore.StovetopBackupRoot))
        {
            StovetopCore.StovetopLogger?.Warn("No backups directory found");
            return;
        }
    
        if (!HasBackups())
        {
            StovetopCore.StovetopLogger?.Info("No backups found");
            return;
        }
    
        Console.WriteLine($"[STOVE] You have {BackupList()!.Length} backup(s):\n");

        bool showInfo = CommandRegistry.GetPositionalArgument("backup", 2) == "-i";
        
        foreach (var backup in BackupList()!)
        {
            string fileName = Path.GetFileName(backup);
            string backupId = fileName.Replace("-stovetop-backup.json", "");
            
            // TODO | Show "latest" after displaying backup ID if its the most recent backup
            if (showInfo)
            {
                var fileInfo = new FileInfo(backup);
                Console.WriteLine($"  {backupId} (Created: {fileInfo.CreationTime}, Size: {fileInfo.Length} bytes)");
            }
            else
            {
                Console.WriteLine($"  {backupId}");
            }
        }
    }
    
    public static void CreateBackup()
    {
        if (StovetopCore.StovetopBackupRoot != null)
        {
            string backupPath = Path.Combine(StovetopCore.StovetopBackupRoot, $"{DateTime.Now:yyyy-MM-dd-HH:mm:ss}-stovetop-backup.json");

            if (File.Exists(StovetopCore.StovetopConfigPath))
            {
                File.Copy(StovetopCore.StovetopConfigPath, backupPath, true);
                StovetopCore.StovetopLogger?.Success($"Backup created: {backupPath}");
            }
        }
    }
    
    public static void RevertToBackup(string backupId)
    {
        if (StovetopCore.StovetopConfigRoot != null)
        {
            string backupPath = Path.Combine(StovetopCore.StovetopConfigRoot, "cache/backups");
            string backupFile = Path.Combine(backupPath, $"{backupId}-stovetop-backup.json");
    
            if (!File.Exists(backupFile))
            {
                StovetopCore.StovetopLogger?.Error($"Backup '{backupId}' not found");
                return;
            }
    
            try
            {
                StovetopCore.StovetopLogger?.Info($"Creating safety backup before reverting...");
                CreateBackup();
        
                StovetopCore.StovetopLogger?.Info($"Reverting to backup: {backupId}");
                if (StovetopCore.StovetopConfigPath != null)
                    File.Copy(backupFile, StovetopCore.StovetopConfigPath, true);

                StovetopCore.StovetopLogger?.Success($"Successfully reverted to backup: {backupId}");
            }
            catch (Exception e)
            {
                StovetopCore.StovetopLogger?.Error($"Failed to revert: {e.Message}");
            }
        }
    }
    
    // get the most recent backup
    public static string? GetLatestBackup()
    {
        if (StovetopCore.StovetopConfigRoot != null)
        {
            string backupPath = Path.Combine(StovetopCore.StovetopConfigRoot, "cache/backups");
    
            if (!Directory.Exists(backupPath))
                return null;
    
            var backups = Directory.GetFiles(backupPath, "*-stovetop-backup.json")
                .OrderByDescending(f => File.GetCreationTime(f))
                .ToArray();
    
            if (backups.Length == 0)
                return null;
    
            string fileName = Path.GetFileName(backups[0]);
            return fileName.Replace("-stovetop-backup.json", "");
        }

        return null;
    }

    public static bool HasBackups()
    {
        if(!Directory.Exists(StovetopCore.StovetopBackupRoot))
            return false;
        
        if(Directory.GetFiles(StovetopCore.StovetopBackupRoot, "*-stovetop-backup.json").Length > 0)
            return true;
        
        return false;
    }
    
    public static bool BackupExists(string backupId)
    {
        return File.Exists(Path.Combine(StovetopCore.StovetopBackupRoot, $"{backupId}-stovetop-backup.json"));
    }
    
    // TODO | Clean backups method, with numbered limit, max/min date etc.
    public static void CleanBackups()
    {
        
    }
}