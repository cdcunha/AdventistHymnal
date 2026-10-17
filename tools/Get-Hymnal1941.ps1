<#
.SYNOPSIS
    One-time helper that downloads the 1941 "Church Hymnal" lyrics from adventisthymns.com
    and writes them as JSON files matching the AdventistHymnal Database schema.

.EXAMPLE
    .\tools\Get-Hymnal1941.ps1
    .\tools\Get-Hymnal1941.ps1 -First 5          # quick test with 5 hymns
    .\tools\Get-Hymnal1941.ps1 -Force            # re-download and overwrite everything

.NOTES
    Raw pages are cached in tools\cache so re-runs don't hit the website again.
    Review the site's terms and each hymn's rights before sharing the generated files publicly.
#>
[CmdletBinding()]
param(
    [string]$CollectionId = 'SDA-1941-en',
    [string]$BaseUrl = 'https://adventisthymns.com/en/1941',
    [string]$OutputPath = (Join-Path $PSScriptRoot "..\Database\$CollectionId"),
    [string]$CachePath = (Join-Path $PSScriptRoot 'cache'),
    [int]$DelayMs = 300,
    [int]$First = 0,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Web
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 -bor [Net.SecurityProtocolType]::Tls13

New-Item -ItemType Directory -Force -Path $OutputPath, $CachePath | Out-Null
$OutputPath = (Resolve-Path $OutputPath).Path

function Get-Page([string]$Url, [string]$CacheName) {
    $file = Join-Path $CachePath $CacheName
    if (-not $Force -and (Test-Path $file)) {
        return [IO.File]::ReadAllText($file, [Text.Encoding]::UTF8)
    }
    $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -UserAgent 'AdventistHymnal-importer (personal church use)'
    $content = [Text.Encoding]::UTF8.GetString($response.RawContentStream.ToArray())
    [IO.File]::WriteAllText($file, $content, (New-Object Text.UTF8Encoding $false))
    Start-Sleep -Milliseconds $DelayMs
    return $content
}

function ConvertFrom-Html([string]$Text) {
    [System.Web.HttpUtility]::HtmlDecode(($Text -replace '<!--.*?-->', '' -replace '<[^>]+>', '')).Trim()
}

function Get-IndexLinks {
    $indexHtml = Get-Page "$BaseUrl/numbers/1-99" 'numbers-1-99.html'
    $ranges = [regex]::Matches($indexHtml, 'href="/en/1941/numbers/(\d+-\d+)"') |
        ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique
    if ($ranges -notcontains '1-99') { $ranges = @('1-99') + $ranges }

    $links = @{}
    foreach ($range in $ranges) {
        $html = Get-Page "$BaseUrl/numbers/$range" "numbers-$range.html"
        foreach ($m in [regex]::Matches($html, 'href="(/en/1941/lyrics/(\d+)/[^"#?]+)"')) {
            $number = [int]$m.Groups[2].Value
            if (-not $links.ContainsKey($number)) { $links[$number] = "https://adventisthymns.com$($m.Groups[1].Value)" }
        }
    }
    return $links
}

function Get-StanzaType([string]$Label) {
    # Returns @(type, number); a lone unnumbered stanza uses number 0 (e.g. "Chorus")
    if ($Label -match '^(Verse|Chorus|Refrain|Bridge|Intro)\s*(\d*)$') {
        $type = switch ($Matches[1]) { 'Refrain' { 'Chorus' } default { $Matches[1] } }
        $number = if ($Matches[2]) { [int]$Matches[2] } else { 0 }
        return @($type, $number)
    }
    throw "Unrecognised stanza label '$Label'"
}

function ConvertTo-Hymn([int]$Number, [string]$Html) {
    $title = ConvertFrom-Html ([regex]::Match($Html, '<h1[^>]*>(.*?)</h1>', 'Singleline').Groups[1].Value)

    $author = $null
    $ld = [regex]::Match($Html, '"lyricist":\[\{[^\]]*?"name":"((?:[^"\\]|\\.)*)"')
    if ($ld.Success) { $author = [Text.RegularExpressions.Regex]::Unescape($ld.Groups[1].Value) }

    $rights = ConvertFrom-Html ([regex]::Match($Html, '<dt[^>]*>Rights</dt><dd[^>]*>(.*?)</dd>', 'Singleline').Groups[1].Value)
    $copyright = if ($rights -match '^public domain') { 'PublicDomain' } else { 'Copyrighted' }

    $stanzas = @()
    $verses = @()
    $chorusRefs = @()
    $pattern = '<h3 class="stz__label[^"]*">(.*?)</h3><p class="stz__body">(.*?)</p>'
    foreach ($m in [regex]::Matches($Html, $pattern, 'Singleline')) {
        $label = ConvertFrom-Html $m.Groups[1].Value
        $type, $stanzaNumber = Get-StanzaType $label
        $lines = @($m.Groups[2].Value -split '<br\s*/?>' | ForEach-Object { ConvertFrom-Html $_ } | Where-Object { $_ })
        $stanzas += [ordered]@{
            Lines     = $lines
            Reference = [ordered]@{ Type = $type; Number = $stanzaNumber }
        }
        $ref = if ($stanzaNumber -gt 0) { "$type $stanzaNumber" } else { $type }
        if ($type -eq 'Chorus') { $chorusRefs += $ref } else { $verses += $ref }
    }
    if ($stanzas.Count -eq 0) { throw "No stanzas found for hymn $Number" }

    # The site lists a refrain once; sing it after every verse when there is exactly one.
    $order = @()
    if ($chorusRefs.Count -eq 1) {
        foreach ($v in $verses) { $order += $v; $order += $chorusRefs[0] }
    } else {
        $order = @($stanzas | ForEach-Object { if ($_.Reference.Number -gt 0) { "$($_.Reference.Type) $($_.Reference.Number)" } else { $_.Reference.Type } })
    }

    [ordered]@{
        Number      = $Number
        Title       = $title
        Author      = $author
        Copyright   = $copyright
        CcliNumber  = $null
        Stanzas     = $stanzas
        StanzaOrder = $order
    }
}

function Get-SafeFileName([string]$Name) {
    $invalid = [IO.Path]::GetInvalidFileNameChars() -join ''
    ($Name -replace "[$([regex]::Escape($invalid))]", '').Trim()
}

$links = Get-IndexLinks
$numbers = @($links.Keys | Sort-Object)
if ($First -gt 0) { $numbers = $numbers | Select-Object -First $First }
Write-Host "Found $($links.Count) hymns in the index; processing $($numbers.Count)."

$failed = @()
$i = 0
foreach ($number in $numbers) {
    $i++
    try {
        $html = Get-Page $links[$number] ("lyrics-{0:000}.html" -f $number)
        $hymn = ConvertTo-Hymn $number $html
        $fileName = "{0:000} - {1}.json" -f $number, (Get-SafeFileName $hymn.Title)
        $json = $hymn | ConvertTo-Json -Depth 6
        [IO.File]::WriteAllText((Join-Path $OutputPath $fileName), $json, (New-Object Text.UTF8Encoding $true))
        Write-Host ("[{0}/{1}] #{2} {3}" -f $i, $numbers.Count, $number, $hymn.Title)
    } catch {
        $failed += $number
        Write-Warning "Hymn #$number failed: $($_.Exception.Message)"
    }
}

Write-Host "Done. Output: $OutputPath"
if ($failed.Count) { Write-Warning "Failed hymns: $($failed -join ', ')" }
