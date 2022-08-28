
param([string]$projDir, [string]$publishDir);

#"Settings", "Resources" | Copy-Item -Destination $projDir$publishDir -Recurse -Force

function CustomCopy
{
    param([string]$srcDir)
    xcopy $srcDir $projDir$publishDir$srcDir\ /s /e /d /f /y
    return
}

CustomCopy "Settings"
CustomCopy "Resources"
write-host "ProjectDirPublishDir: $($projDir)$($publishDir)"