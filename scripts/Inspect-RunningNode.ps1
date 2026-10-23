# Read file-version metadata only. Does not execute, update or stop any process.
[CmdletBinding()]
param()

[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$processes = @(Get-Process -Name node -ErrorAction SilentlyContinue)
$unreadable = 0
$runtimePaths = @()
foreach ($runtimeProcess in $processes) {
    try {
        $runtimePath = $runtimeProcess.Path
        if ($runtimePath) { $runtimePaths += $runtimePath } else { $unreadable++ }
    } catch { $unreadable++ }
}
foreach ($runtimePath in ($runtimePaths | Sort-Object -Unique)) {
    try {
        $runtimeFile = Get-Item -LiteralPath $runtimePath -ErrorAction Stop
        [PSCustomObject]@{
            Path = $runtimeFile.FullName
            FileMetadataVersion = $runtimeFile.VersionInfo.ProductVersion
            Scope = 'File on disk; does not prove loaded process bytes were updated'
        }
    } catch { $unreadable++ }
}
Write-Host ('Node processes observed: {0}; unreadable metadata: {1}' -f $processes.Count, $unreadable)
Write-Host 'This misses stopped apps, renamed runtimes, Electron, containers and other users. File metadata is not proof that a process restarted.'
Write-Host 'Do not share raw local paths publicly. Compare versions with the official advisory and update app-managed runtimes through the app vendor.'
if ($unreadable -gt 0) { exit 2 }
