using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using POSApp.Data.Models;

namespace POSApp.Data.Services;

public sealed class DatabaseBackupService
{
    private string GetDatabasePath()
    {
        using var db = new LocalDbContext(); using var c = new SqliteConnection(db.Database.GetConnectionString()); c.Open(); using var cmd=c.CreateCommand();cmd.CommandText="PRAGMA database_list;";using var r=cmd.ExecuteReader();while(r.Read()){var name=r.GetString(1);if(name=="main")return r.GetString(2);}throw new InvalidOperationException("Database path unavailable.");
    }
    public string Backup(User actor, string? directory = null)
    {
        PosAuthorizationService.Require(PosAuthorizationService.IsAdmin(actor), "Only Admin can create database backups.");
        var source=GetDatabasePath(); directory ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"POSApp-Backups");Directory.CreateDirectory(directory);var target=Path.Combine(directory,$"POSApp_{DateTime.Now:yyyyMMdd_HHmmss}.db");File.Copy(source,target,false);return target;
    }
    public List<FileInfo> History(User actor){PosAuthorizationService.Require(PosAuthorizationService.IsAdmin(actor));var dir=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"POSApp-Backups");if(!Directory.Exists(dir))return new();return new DirectoryInfo(dir).GetFiles("*.db").OrderByDescending(x=>x.LastWriteTimeUtc).Take(100).ToList();}
    public void Restore(User actor,string backupFile){PosAuthorizationService.Require(PosAuthorizationService.IsAdmin(actor),"Only Admin can restore the database.");if(!File.Exists(backupFile))throw new FileNotFoundException("Backup not found.");var source=backupFile;var target=GetDatabasePath();var safety=target+".pre_restore_"+DateTime.Now.ToString("yyyyMMdd_HHmmss");File.Copy(target,safety,true);File.Copy(source,target,true);}
}
