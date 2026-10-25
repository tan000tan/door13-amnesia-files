$logPath = "D:\TDM\Tools\logs\2_compile.log"
$bytes = [System.IO.File]::ReadAllBytes($logPath)
try { $text = [System.Text.Encoding]::UTF8.GetString($bytes) }
catch { $text = [System.Text.Encoding]::Default.GetString($bytes) }
$lines = $text -split "`r`n"
$start = [Math]::Max(0, $lines.Length - 40)
for ($i = $start; $i -lt $lines.Length; $i++) { Write-Host $lines[$i] }
