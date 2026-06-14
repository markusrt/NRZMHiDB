#Requires -Version 5.1
<#
.SYNOPSIS
    One-time migration: replaces the hardcoded lab/medical director names and the personal
    contact block in the NRZMHi Word report templates with docxtemplater placeholders.

    - "Gesamtleitung: PD Dr. rer. nat. Heike Claus"      -> "Gesamtleitung: {LabDirector}"
    - "ärztliche Leitung: PD Dr. med. Thiên-Trí Lâm"     -> "ärztliche Leitung: {MedicalDirector}"
    - the three personal contact blocks (Heike Claus /   -> "{Contacts}"
      Thiên-Trí Lâm / Katherina Heroth)

    The replacement is applied to the shared include files AND to the cached INCLUDETEXT
    field results embedded in every main template, because docxtemplater fills the cached
    copy (not the include) at report generation time.

    The script is idempotent: parts that already contain the placeholders are skipped.
.PARAMETER Files
    Explicit list of .docx files to process.
.PARAMETER DryRun
    Only report which parts would change; do not write.
#>
param(
    [Parameter(Mandatory = $true)]
    [string[]] $Files,
    [switch] $DryRun
)

Add-Type -AssemblyName System.IO.Compression.FileSystem
$ErrorActionPreference = 'Stop'

$wns = 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'

function Get-ParaText($p, $nsm) {
    $ts = $p.SelectNodes('.//w:t', $nsm)
    return (-join ($ts | ForEach-Object { $_.InnerText }))
}

function Set-ParaText($p, [string]$newText, $nsm) {
    $firstRun = $p.SelectSingleNode('w:r', $nsm)
    $rPrClone = $null
    if ($firstRun) {
        $rPr = $firstRun.SelectSingleNode('w:rPr', $nsm)
        if ($rPr) { $rPrClone = $rPr.CloneNode($true) }
    }
    foreach ($name in @('w:r', 'w:proofErr', 'w:bookmarkStart', 'w:bookmarkEnd')) {
        foreach ($n in @($p.SelectNodes($name, $nsm))) { [void]$p.RemoveChild($n) }
    }
    $newRun = $p.OwnerDocument.CreateElement('w', 'r', $wns)
    if ($rPrClone) { [void]$newRun.AppendChild($p.OwnerDocument.ImportNode($rPrClone, $true)) }
    $wt = $p.OwnerDocument.CreateElement('w', 't', $wns)
    $space = $p.OwnerDocument.CreateAttribute('xml', 'space', 'http://www.w3.org/XML/1998/namespace')
    $space.Value = 'preserve'
    [void]$wt.Attributes.Append($space)
    [void]$wt.AppendChild($p.OwnerDocument.CreateTextNode($newText))
    [void]$newRun.AppendChild($wt)
    [void]$p.AppendChild($newRun)
}

function Process-PartXml([string]$xml) {
    $decl = ''
    if ($xml -match '^\s*<\?xml[^>]*\?>') { $decl = $matches[0] }

    $doc = New-Object System.Xml.XmlDocument
    $doc.PreserveWhitespace = $true
    $doc.LoadXml($xml)
    $nsm = New-Object System.Xml.XmlNamespaceManager($doc.NameTable)
    $nsm.AddNamespace('w', $wns)
    $changed = $false

    # --- Director lines (single paragraph each, may span multiple runs) ---
    foreach ($p in @($doc.SelectNodes('//w:p', $nsm))) {
        $t = (Get-ParaText $p $nsm).Trim()
        if ($t -eq 'Gesamtleitung: PD Dr. rer. nat. Heike Claus') {
            Set-ParaText $p 'Gesamtleitung: {LabDirector}' $nsm; $changed = $true
        }
        elseif ($t -eq 'ärztliche Leitung: PD Dr. med. Thiên-Trí Lâm') {
            Set-ParaText $p 'ärztliche Leitung: {MedicalDirector}' $nsm; $changed = $true
        }
    }

    # --- Personal contact block -> single {Contacts} placeholder ---
    # The personal contacts live inside a VML textbox, right after the constant institutional
    # e-mail (nrzmhi@uni-wuerzburg.de). Anchoring on that e-mail (instead of a specific person)
    # makes the replacement robust against template-specific contact names.
    $instEmail = 'nrzmhi@uni-wuerzburg.de'
    foreach ($tb in @($doc.SelectNodes('//w:txbxContent', $nsm))) {
        $tbParas = @($tb.SelectNodes('.//w:p', $nsm))

        $already = $false
        foreach ($pp in $tbParas) { if ((Get-ParaText $pp $nsm).Trim() -eq '{Contacts}') { $already = $true; break } }
        if ($already) { continue }

        $instIdx = -1
        for ($i = 0; $i -lt $tbParas.Count; $i++) {
            if ((Get-ParaText $tbParas[$i] $nsm).Trim() -eq $instEmail) { $instIdx = $i; break }
        }
        if ($instIdx -lt 0) { continue }

        $startIdx = -1
        for ($i = $instIdx + 1; $i -lt $tbParas.Count; $i++) {
            if ((Get-ParaText $tbParas[$i] $nsm).Trim().Length -gt 0) { $startIdx = $i; break }
        }
        if ($startIdx -lt 0) { continue }

        Set-ParaText $tbParas[$startIdx] '{Contacts}' $nsm
        for ($k = $startIdx + 1; $k -lt $tbParas.Count; $k++) {
            [void]$tbParas[$k].ParentNode.RemoveChild($tbParas[$k])
        }
        $changed = $true
    }

    $out = $doc.OuterXml
    if ($out -notmatch '^\s*<\?xml' -and $decl -ne '') { $out = $decl + $out }
    return @($changed, $out)
}

function Update-Docx([string]$path, [bool]$dry) {
    $changedParts = @()
    $mode = [System.IO.Compression.ZipArchiveMode]::Update
    $zip = [System.IO.Compression.ZipFile]::Open($path, $mode)
    try {
        $entries = @($zip.Entries | Where-Object { $_.FullName -match '^word/(document|header\d+|footer\d+)\.xml$' })
        foreach ($entry in $entries) {
            $sr = New-Object System.IO.StreamReader($entry.Open())
            $content = $sr.ReadToEnd(); $sr.Close()
            $res = Process-PartXml $content
            if ($res[0]) {
                $changedParts += $entry.FullName
                if (-not $dry) {
                    $stream = $entry.Open()
                    $stream.SetLength(0)
                    $sw = New-Object System.IO.StreamWriter($stream, (New-Object System.Text.UTF8Encoding($false)))
                    $sw.Write($res[1]); $sw.Flush(); $sw.Dispose()
                }
            }
        }
    }
    finally { $zip.Dispose() }
    return $changedParts
}

foreach ($file in $Files) {
    if (-not (Test-Path $file)) { Write-Warning "Not found: $file"; continue }
    $parts = Update-Docx $file $DryRun.IsPresent
    if ($parts.Count -gt 0) {
        $verb = if ($DryRun) { 'WOULD CHANGE' } else { 'CHANGED' }
        Write-Host ("{0}: {1} [{2}]" -f $verb, (Split-Path $file -Leaf), ($parts -join ', '))
    }
    else {
        Write-Host ("UNCHANGED: {0}" -f (Split-Path $file -Leaf))
    }
}
