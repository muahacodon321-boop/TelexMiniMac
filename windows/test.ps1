param([switch]$Integration)
$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
$testExe = Join-Path $env:TEMP ('TelexMiniTests-' + [Guid]::NewGuid().ToString('N') + '.exe')
$wpf = Join-Path (Split-Path $compiler) 'WPF'
try {
    & $compiler /nologo /target:exe /main:TelexMini.Tests /utf8output /reference:System.Windows.Forms.dll /reference:System.Drawing.dll "/reference:$wpf\UIAutomationClient.dll" "/reference:$wpf\UIAutomationTypes.dll" "/reference:$wpf\WindowsBase.dll" "/out:$testExe" "$PSScriptRoot\Tests.cs" "$PSScriptRoot\Program.cs" "$PSScriptRoot\Keyboard.cs" "$PSScriptRoot\TelexEngine.cs" "$PSScriptRoot\EnglishLexicon.cs" "$PSScriptRoot\UserPreferences.cs" "$PSScriptRoot\ModernChrome.cs" "$PSScriptRoot\DictionaryEditor.cs" "$PSScriptRoot\InputContext.cs"
    if ($LASTEXITCODE -ne 0) { throw 'Test compilation failed.' }
    if ($Integration) { & $testExe --integration } else { & $testExe }
    if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
} finally {
    if (Test-Path -LiteralPath $testExe) { Remove-Item -LiteralPath $testExe }
}
