# Script para compilar o executável único (self-contained) do Inventory System
# Este script cria um EXE que não requer .NET instalado na máquina

# Cores para output
$Green = [System.ConsoleColor]::Green
$Yellow = [System.ConsoleColor]::Yellow
$Red = [System.ConsoleColor]::Red

Write-Host "========================================" -ForegroundColor $Green
Write-Host "   Compilando Executável Único" -ForegroundColor $Green
Write-Host "   Inventory System - Self Contained" -ForegroundColor $Green
Write-Host "========================================" -ForegroundColor $Green
Write-Host ""

# Diretório do projeto
$projectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectFile = "$projectDir\src\InventarioSistem.WinForms\InventarioSistem.WinForms.csproj"
$outputDir = "$projectDir\publish"

if (-not (Test-Path $projectFile)) {
    Write-Host "❌ Arquivo de projeto não encontrado: $projectFile" -ForegroundColor $Red
    exit 1
}

Write-Host "📁 Projeto: $projectFile" -ForegroundColor $Yellow
Write-Host "📦 Saída: $outputDir" -ForegroundColor $Yellow
Write-Host ""

# Limpar build anterior
Write-Host "🧹 Limpando build anterior..." -ForegroundColor $Yellow
Remove-Item -Path $outputDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
Write-Host "✅ Limpeza concluída" -ForegroundColor $Green
Write-Host ""

# Publicar (compilar executável único)
Write-Host "🔨 Compilando executável único (self-contained)..." -ForegroundColor $Yellow
Write-Host "   (Isso pode levar alguns minutos na primeira vez)" -ForegroundColor $Yellow
Write-Host ""

dotnet publish $projectFile `
    -c Release `
    -o $outputDir `
    --self-contained `
    -r win-x64 `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=embedded

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "❌ Erro na compilação!" -ForegroundColor $Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor $Green
Write-Host "   ✅ Compilação Concluída com Sucesso!" -ForegroundColor $Green
Write-Host "========================================" -ForegroundColor $Green
Write-Host ""

# Informações do executável
$exePath = "$outputDir\InventorySystem.exe"
if (Test-Path $exePath) {
    $exeSize = (Get-Item $exePath).Length / 1MB
    Write-Host "📌 Executável: $exePath" -ForegroundColor $Green
    Write-Host "📊 Tamanho: $([Math]::Round($exeSize, 2)) MB" -ForegroundColor $Green
    Write-Host ""
    Write-Host "🚀 O executável está pronto para distribuição!" -ForegroundColor $Green
    Write-Host "   Não requer .NET instalado na máquina do usuário" -ForegroundColor $Green
    Write-Host ""
    Write-Host "💾 Para usar:" -ForegroundColor $Yellow
    Write-Host "   1. Coloque o arquivo '$exePath' em qualquer pasta" -ForegroundColor $Yellow
    Write-Host "   2. Duplo clique para executar" -ForegroundColor $Yellow
    Write-Host "   3. Coloque o banco InventorySystem.accdb na mesma pasta (ou configure o caminho)" -ForegroundColor $Yellow
} else {
    Write-Host "⚠️  Aviso: Arquivo executável não encontrado" -ForegroundColor $Yellow
}

Write-Host ""
