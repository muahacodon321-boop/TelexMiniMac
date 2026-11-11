param([string]$OutputPath = "$PSScriptRoot\TelexMini.exe")
$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
$wpf = Join-Path (Split-Path $compiler) 'WPF'
& $compiler /nologo /target:winexe /platform:anycpu /optimize+ /utf8output /reference:System.Windows.Forms.dll /reference:System.Drawing.dll "/reference:$wpf\UIAutomationClient.dll" "/reference:$wpf\UIAutomationTypes.dll" "/reference:$wpf\WindowsBase.dll" "/out:$OutputPath" "$PSScriptRoot\Program.cs" "$PSScriptRoot\Keyboard.cs" "$PSScriptRoot\TelexEngine.cs" "$PSScriptRoot\EnglishLexicon.cs" "$PSScriptRoot\UserPreferences.cs" "$PSScriptRoot\ModernChrome.cs" "$PSScriptRoot\DictionaryEditor.cs" "$PSScriptRoot\InputContext.cs"
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
Write-Host "Built: $OutputPath"
