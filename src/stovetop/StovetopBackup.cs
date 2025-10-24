namespace Stovetop.stovetop;

public class StovetopBackup
{
    public static void CreateBackup()
    {
        string backupPath = Path.Combine(Path.Combine(StovetopCore.STOVETOP_CONFIG_ROOT, "cache/backups"), $"{DateTime.Now:yyyy-MM-dd-HH:mm:ss}-stovetop-backup.json");

        if (File.Exists(StovetopCore.STOVETOP_CONFIG_PATH))
            File.Copy(StovetopCore.STOVETOP_CONFIG_PATH, backupPath, true);
    }
    
    public static void RevertToBackup(string backupId)
    {
        string backupPath = Path.Combine(StovetopCore.STOVETOP_CONFIG_ROOT, "cache/backups");
        string backupFile = Path.Combine(backupPath, $"{backupId}-stovetop-backup.json");
    
        if (!File.Exists(backupFile))
        {
            StovetopCore.STOVETOP_LOGGER.Error($"Backup '{backupId}' not found");
            return;
        }
    
        try
        {
            StovetopCore.STOVETOP_LOGGER.Info($"Creating safety backup before reverting...");
            CreateBackup();
        
            StovetopCore.STOVETOP_LOGGER.Info($"Reverting to backup: {backupId}");
            File.Copy(backupFile, StovetopCore.STOVETOP_CONFIG_PATH, true);
        
            StovetopCore.STOVETOP_LOGGER.Success($"Successfully reverted to backup: {backupId}");
        }
        catch (Exception e)
        {
            StovetopCore.STOVETOP_LOGGER.Error($"Failed to revert: {e.Message}");
        }
    }
    
    // get the most recent backup
    public static string? GetLatestBackup()
    {
        string backupPath = Path.Combine(StovetopCore.STOVETOP_CONFIG_ROOT, "cache/backups");
    
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

    public static void CleanBackups()
    {
        
    }
    
    // -i, --info for viewing info
    public static void ViewBackups(bool showInfo = false)
    {
        string backupPath = Path.Combine(StovetopCore.STOVETOP_CONFIG_ROOT, "cache/backups");
    
        if (!Directory.Exists(backupPath))
        {
            StovetopCore.STOVETOP_LOGGER.Warn("No backups directory found");
            return;
        }
    
        string[] backups = Directory.GetFiles(backupPath, "*-stovetop-backup.json");
    
        if (backups.Length == 0)
        {
            StovetopCore.STOVETOP_LOGGER.Info("No backups found");
            return;
        }
    
        Console.WriteLine($"[STOVE] You have {backups.Length} backup(s):\n");
    
        foreach (var backup in backups)
        {
            string fileName = Path.GetFileName(backup);
            string backupId = fileName.Replace("-stovetop-backup.json", "");
        
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
}