namespace Stovetop.Exceptions;

public class StovetopBackupNotFoundException : Exception
{
    public StovetopBackupNotFoundException(string backupId) : base($"Backup '{backupId}' does not exist.") { }
    public StovetopBackupNotFoundException() : base("No backups available.") { }
}

