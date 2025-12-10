# Mass migration script to replace Access references with SQL Server equivalents
# This script updates all C# files to use SQL Server classes instead of Access/ODBC

$ErrorActionPreference = "Stop"

Write-Host "=== SQL Server Migration Script ===" -ForegroundColor Cyan
Write-Host ""

$replacements = @(
    @{Old = "AccessInventoryStore"; New = "SqlServerInventoryStore"},
    @{Old = "AccessConnectionFactory"; New = "SqlServerConnectionFactory"},
    @{Old = "AccessDatabaseManager"; New = "SqlServerDatabaseManager"},
    @{Old = "AccessSchemaManager"; New = "SqlServerSchemaManager"},
    @{Old = "UserStore"; New = "SqlServerUserStore"}
)

$folders = @(
    "c:\Repositorio\InventoryLocal\src\InventarioSistem.WinForms\Forms",
    "c:\Repositorio\InventoryLocal\src\InventarioSistem.Cli"
)

$filesChanged = 0
$totalReplacements = 0

foreach ($folder in $folders) {
    if (!(Test-Path $folder)) {
        Write-Host "Skipping non-existent folder: $folder" -ForegroundColor Yellow
        continue
    }
    
    $files = Get-ChildItem -Path $folder -Filter "*.cs" -File
    
    foreach ($file in $files) {
        $content = Get-Content $file.FullName -Raw
        $originalContent = $content
        $fileChanged = $false
        
        foreach ($replacement in $replacements) {
            $old = $replacement.Old
            $new = $replacement.New
            
            if ($content -match [regex]::Escape($old)) {
                # Replace class references
                $content = $content -replace "\b$old\b", $new
                $fileChanged = $true
                $totalReplacements++
            }
        }
        
        if ($fileChanged) {
            Set-Content -Path $file.FullName -Value $content -NoNewline
            Write-Host "✓ Updated: $($file.Name)" -ForegroundColor Green
            $filesChanged++
        }
    }
}

Write-Host ""
Write-Host "=== Migration Complete ===" -ForegroundColor Cyan
Write-Host "Files changed: $filesChanged" -ForegroundColor Green
Write-Host "Total replacements: $totalReplacements" -ForegroundColor Green
Write-Host ""
Write-Host "Next: Run dotnet build to verify compilation" -ForegroundColor Yellow
