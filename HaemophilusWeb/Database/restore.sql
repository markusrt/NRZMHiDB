USE [master]

-- kill open sessions
if exists (select name from master.sys.databases where name=N'$(ResoreDb)')
begin
 ALTER DATABASE [$(ResoreDb)] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
end

-- delete history
if exists (select name from master.sys.databases where name=N'$(ResoreDb)')
begin
 EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'$(ResoreDb)';
end

if exists (select name from master.sys.databases where name=N'$(ResoreDb)')
begin
 Print 'DB exists'
 RESTORE DATABASE [$(ResoreDb)] FROM  DISK = N'$(BackupPath)\HaemophilusWeb.bak' WITH  FILE = 1,  MOVE N'HaemophilusWeb' TO N'C:\Program Files\Microsoft SQL Server\MSSQL11.SQLEXPRESS\MSSQL\DATA\$(ResoreDb).mdf',  MOVE N'HaemophilusWeb_Log' TO N'C:\Program Files\Microsoft SQL Server\MSSQL11.SQLEXPRESS\MSSQL\DATA\$(ResoreDb)_log.ldf',  NOUNLOAD,  REPLACE,  RECOVERY,  STATS = 10
end

if not exists (select name from master.sys.databases where name=N'$(ResoreDb)')
begin
  Print 'DB NOT exists'
  --restore database as
  RESTORE DATABASE [$(ResoreDb)] FROM  DISK = N'$(BackupPath)\HaemophilusWeb.bak' WITH  FILE = 1,  MOVE N'HaemophilusWeb' TO N'C:\Program Files\Microsoft SQL Server\MSSQL11.SQLEXPRESS\MSSQL\DATA\$(ResoreDb).mdf',  MOVE N'HaemophilusWeb_Log' TO N'C:\Program Files\Microsoft SQL Server\MSSQL11.SQLEXPRESS\MSSQL\DATA\$(ResoreDb)_log.ldf',  NOUNLOAD, STATS = 10
end
GO