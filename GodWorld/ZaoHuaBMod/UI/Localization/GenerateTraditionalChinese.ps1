param(
    [Parameter(Mandatory=$true)]
    [string]$SourceDir,

    [Parameter(Mandatory=$true)]
    [string]$OutputFile
)

# Skip if already exists to avoid overwriting manual translations
if (Test-Path $OutputFile)
{
    Write-Host "[LangGen] $OutputFile already exists, skipping."
    exit 0
}

Add-Type -AssemblyName Microsoft.VisualBasic | Out-Null

function Extract-Keys($content, $prefix)
{
    $result = @()
    $search = $prefix + '"'
    $index = 0
    while (($i = $content.IndexOf($search, $index)) -ge 0)
    {
        $start = $i + $search.Length
        $end = $content.IndexOf('"', $start)
        if ($end -lt 0) { break }
        $result += $content.Substring($start, $end - $start)
        $index = $end + 1
    }
    return $result
}

$keys = [System.Collections.Generic.SortedSet[string]]::new()

Get-ChildItem -Path $SourceDir -Recurse -Filter *.cs |
    Where-Object { $_.FullName -notmatch '\\(obj|bin|Properties)\\' } |
    ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        foreach ($key in (Extract-Keys $content 'Loc.Get('))
        {
            [void]$keys.Add($key)
        }
        foreach ($key in (Extract-Keys $content 'Loc.Format('))
        {
            [void]$keys.Add($key)
        }
    }

$outputDir = Split-Path -Parent $OutputFile
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("# 自动生成的繁中文件")
[void]$sb.AppendLine()

foreach ($key in $keys)
{
    $value = [Microsoft.VisualBasic.Strings]::StrConv($key, [Microsoft.VisualBasic.VbStrConv]::TraditionalChinese, 0)
    [void]$sb.AppendLine($key + '=' + $value)
}

[System.IO.File]::WriteAllText($OutputFile, $sb.ToString(), [System.Text.Encoding]::UTF8)
Write-Host "[LangGen] Generated $OutputFile with $($keys.Count) entries."
