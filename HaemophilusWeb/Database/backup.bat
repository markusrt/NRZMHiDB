REM Backup
mkdir "%~dp0%date:~-4,4%%date:~-7,2%%date:~-10,2%"
sqlcmd -b -S localhost\SQLEXPRESS -v BackupPath="%~dp0%date:~-4,4%%date:~-7,2%%date:~-10,2%" -E -i %~dp0backup.sql

REM Restore to test DB
REM sqlcmd -b -S localhost\SQLEXPRESS -v BackupPath="%~dp0%date:~-4,4%%date:~-7,2%%date:~-10,2%" -v ResoreDb="HaemophilusWebTest" -E -i %~dp0restore.sql

REM Restore to readonly DB
REM sqlcmd -b -S localhost\SQLEXPRESS -v BackupPath="%~dp0%date:~-4,4%%date:~-7,2%%date:~-10,2%" -v ResoreDb="HaemophilusWebReadOnly" -E -i %~dp0restore.sql