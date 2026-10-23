param([string]$OutputDirectory = (Join-Path $PSScriptRoot '..\local\windows-build'))
$ErrorActionPreference = 'Stop'
$outputFolder = [IO.Path]::GetFullPath($OutputDirectory)
[IO.Directory]::CreateDirectory($outputFolder) | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
if (-not (Test-Path -LiteralPath $compiler)) { throw '.NET Framework compiler not found.' }
$binary = Join-Path $outputFolder 'SecurityCheckup.exe'
& $compiler /nologo /codepage:65001 /target:winexe /platform:anycpu /optimize+ /warnaserror+ "/out:$binary" "/win32manifest:$PSScriptRoot\app.manifest" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll "$PSScriptRoot\Engine.cs" "$PSScriptRoot\Program.cs" "$PSScriptRoot\Tests.cs"
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed.' }
$testReport = Join-Path $outputFolder 'self-test.txt'
$testProcess = Start-Process -FilePath $binary -ArgumentList @('--self-test', ('"{0}"' -f $testReport)) -PassThru -Wait -WindowStyle Hidden
if ($testProcess.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $testReport)) { throw 'Windows boundary tests failed; inspect error report.' }
Get-Content -LiteralPath $testReport
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.zh-TW.txt') -Destination $outputFolder
$digest = (Get-FileHash -LiteralPath $binary -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText((Join-Path $outputFolder 'SHA256SUMS.txt'), ($digest + '  SecurityCheckup.exe' + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
Write-Output ('Built and tested: ' + $binary)
