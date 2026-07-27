# powershell.exe -ExecutionPolicy Bypass -File "GuiFramework\Localization\RenameNamespace.ps1" -TargetDir "C:\Projects\NewMod" -NewNamespace "NewMod"
param(
    [Parameter(Mandatory)] [string] $TargetDir,
    [Parameter(Mandatory)] [string] $NewNamespace,
    [string] $OldNamespace = "ZaoHuaBMod"
)

# 只扫根目录 .cs（调用方 using）和 GuiFramework 子目录（库定义）
$csFiles = Get-ChildItem -Path $TargetDir -Filter "*.cs" 2>$null
if (Test-Path "$TargetDir\GuiFramework") {
    $csFiles += Get-ChildItem -Path "$TargetDir\GuiFramework" -Recurse -Filter "*.cs" 2>$null
}

$csFiles = $csFiles | Where-Object { $_.FullName -notmatch '\\(obj|bin|Properties)\\' }

$count = 0
foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    if ($content.Contains($OldNamespace)) {
        $newContent = $content -replace $OldNamespace, $NewNamespace
        Set-Content $file.FullName -Value $newContent -NoNewline -Encoding UTF8
        Write-Host "  [OK] $($file.Name)"
        $count++
    }
}

Write-Host "Done. $count files updated."
