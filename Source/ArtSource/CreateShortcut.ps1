$TargetScriptPath = Join-Path -Path $PSScriptRoot -ChildPath "ClearColorTool.ps1"
$ShortcutPath = Join-Path -Path $PSScriptRoot -ChildPath "ClearColorTool.lnk"
$TargetPath = "powershell.exe"
$Arguments = "-NoProfile -ExecutionPolicy Bypass -File `"$TargetScriptPath`""

$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut($ShortcutPath)
$Shortcut.TargetPath = $TargetPath
$Shortcut.Arguments = $Arguments
$Shortcut.WorkingDirectory = $PSScriptRoot
$Shortcut.Save()