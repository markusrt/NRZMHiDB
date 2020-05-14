REM This script needs to run as scheduled task in order to fulfill
REM data privacy regulations on the application server

ForFiles /p "%SystemDrive%\inetpub\wwwroot\Haemophilus\Logs" /s /d -30 /c "cmd /c del /q @file"
ForFiles /p "%SystemDrive%\inetpub\logs\LogFiles" /s /d -30 /c "cmd /c del /q @file"