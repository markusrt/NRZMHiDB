sqlcmd -b -S localhost\SQLEXPRESS -v BackupPath="%~dp0%date:~-4,4%%date:~-7,2%%date:~-10,2%" -v ResoreDb="HaemophilusWebTest" -E -i %~dp0restore.sql
pause