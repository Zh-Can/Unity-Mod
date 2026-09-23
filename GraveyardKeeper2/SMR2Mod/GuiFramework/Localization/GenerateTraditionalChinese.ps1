param(
    [Parameter(Mandatory=$true)]
    [string]$SourceDir,

    [Parameter(Mandatory=$true)]
    [string]$OutputFile,

    [Parameter(Mandatory=$false)]
    [string[]]$Files
)

# Remove existing file first to ensure fresh generation
if (Test-Path $OutputFile)
{
    Remove-Item $OutputFile -Force
    Write-Host "LangGen: Removed existing $OutputFile"
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
        # Skip interpolated strings ($"...")
        if ($start -lt $content.Length -and $content[$start] -eq [char]'$')
        {
            $index = $start + 1
            continue
        }
        $end = $content.IndexOf('"', $start)
        if ($end -lt 0) { break }
        $result += $content.Substring($start, $end - $start)
        $index = $end + 1
    }
    return $result
}

function Process-File($filePath)
{
    $content = Get-Content $filePath -Raw -Encoding UTF8
    $result = [System.Collections.Generic.SortedSet[string]]::new()
    
    foreach ($key in (Extract-Keys $content 'Loc.Get('))
    {
        [void]$result.Add($key)
    }
    foreach ($key in (Extract-Keys $content 'Loc.Format('))
    {
        [void]$result.Add($key)
    }
    foreach ($key in (Extract-Keys $content 'Label('))
    {
        [void]$result.Add($key)
    }
    foreach ($key in (Extract-Keys $content 'Button('))
    {
        [void]$result.Add($key)
    }
    foreach ($key in (Extract-Keys $content 'Toggle('))
    {
        [void]$result.Add($key)
    }
    foreach ($key in (Extract-Keys $content 'Slider('))
    {
        [void]$result.Add($key)
    }
    foreach ($key in (Extract-Keys $content 'Foldout('))
    {
        [void]$result.Add($key)
    }
    
    return $result
}

$keys = [System.Collections.Generic.SortedSet[string]]::new()

if ($Files)
{
    # Support comma-separated values from command line (-Files "a","b" becomes one entry "a,b")
    $resolvedFiles = $Files | ForEach-Object { $_.Split(',', [StringSplitOptions]::RemoveEmptyEntries) } | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne '' }
    
    # Scan only specified files
    foreach ($file in $resolvedFiles)
    {
        $resolved = if ([System.IO.Path]::IsPathRooted($file)) {
            $file
        } else {
            Join-Path $SourceDir $file
        }
        if (Test-Path $resolved)
        {
            Write-Host "LangGen: Scanning $resolved"
            foreach ($k in (Process-File $resolved))
            {
                [void]$keys.Add($k)
            }
        }
        else
        {
            Write-Host "LangGen: WARNING File not found: $resolved"
        }
    }
}
else
{
    # Default: scan all .cs files recursively
    Get-ChildItem -Path $SourceDir -Recurse -Filter *.cs |
        Where-Object { $_.FullName -notmatch '\\(obj|bin|Properties)\\' } |
        ForEach-Object {
            foreach ($k in (Process-File $_.FullName))
            {
                [void]$keys.Add($k)
            }
        }
}

$outputDir = Split-Path -Parent $OutputFile
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("# auto generated traditional chinese file")
[void]$sb.AppendLine()

foreach ($key in $keys)
{
    $value = [Microsoft.VisualBasic.Strings]::StrConv($key, [Microsoft.VisualBasic.VbStrConv]::TraditionalChinese, 0)
    # Skip entries where key equals its traditional Chinese form
    if ($key -eq $value) { continue }
    [void]$sb.AppendLine($key + '=' + $value)
}

[System.IO.File]::WriteAllText($OutputFile, $sb.ToString(), [System.Text.Encoding]::UTF8)
Write-Host "LangGen done."
