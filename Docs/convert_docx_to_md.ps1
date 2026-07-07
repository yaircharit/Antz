$docs = Get-ChildItem -Path 'c:\Users\yairc\Documents\MyProjects\Antz\Docs' -Filter '*.docx'
foreach ($file in $docs) {
    try {
        $temp = Join-Path $env:TEMP ("docx2md_" + [IO.Path]::GetFileNameWithoutExtension($file.Name) + "_" + [guid]::NewGuid().ToString())
        New-Item -ItemType Directory -Path $temp | Out-Null
        $zipPath = Join-Path $temp ($file.BaseName + '.zip')
        Copy-Item -Path $file.FullName -Destination $zipPath -Force
        Expand-Archive -Path $zipPath -DestinationPath $temp -Force

        $docXml = Join-Path $temp 'word\\document.xml'
        if (Test-Path $docXml) {
            $xmlText = Get-Content -Path $docXml -Raw

            $xml = [xml]$xmlText
            $ns = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
            $ns.AddNamespace('w','http://schemas.openxmlformats.org/wordprocessingml/2006/main')

            $paraNodes = $xml.SelectNodes('//w:body/w:p', $ns)
            $out = ""
            $counter = 1

            foreach ($p in $paraNodes) {
                $styleNode = $p.SelectSingleNode('w:pPr/w:pStyle', $ns)
                $style = if ($styleNode -ne $null) { $styleNode.Attributes['w:val'].Value } else { '' }

                $texts = $p.SelectNodes('.//w:t', $ns) | ForEach-Object { $_.InnerText }
                $text = ($texts -join '')

                if ([string]::IsNullOrWhiteSpace($text)) { continue }

                if ($style -match 'Heading1') {
                    $out += "`n# $text`n`n"
                    $counter = 1
                    continue
                }
                elseif ($style -match 'Heading2') {
                    $out += "`n## $text`n`n"
                    $counter = 1
                    continue
                }
                else {
                    $out += "$counter. $text`n`n"
                    $counter++
                }
            }

            $mdPath = Join-Path $file.DirectoryName (($file.BaseName) + '.md')
            $out | Out-File -FilePath $mdPath -Encoding utf8
            Write-Host "Converted $($file.Name) -> $([IO.Path]::GetFileName($mdPath))"
        }
        else {
            Write-Host "No document.xml in $($file.Name)"
        }

        Remove-Item -Recurse -Force $temp
    }
    catch {
        Write-Host "Error processing $($file.Name): $_"
    }
}
