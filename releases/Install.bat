@echo off
REM ============================================================
REM Inventory System - Script de Instalação
REM ============================================================
REM Requisitos: Windows 7+, .NET Runtime 8.0
REM ============================================================

setlocal enabledelayedexpansion

echo.
echo ╔════════════════════════════════════════════════════════╗
echo ║      Inventory System v1.0.0 - Instalador             ║
echo ╚════════════════════════════════════════════════════════╝
echo.

REM Verificar se é administrador
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ ERRO: Este script precisa ser executado como Administrador
    echo.
    echo Clique com botão direito no arquivo e selecione "Executar como administrador"
    pause
    exit /b 1
)

echo ✅ Executando como Administrador
echo.

REM Verificar .NET Runtime
echo 🔍 Verificando .NET Runtime 8.0...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ .NET Runtime 8.0 não encontrado
    echo.
    echo 📥 Baixando .NET Runtime 8.0...
    powershell -Command "Start-Process 'https://dotnet.microsoft.com/download/dotnet/8.0' -Wait"
    echo.
    echo ⚠️  Instale o .NET Runtime 8.0 e execute este script novamente
    pause
    exit /b 1
)
echo ✅ .NET Runtime encontrado

REM Definir diretório de instalação
set "INSTALL_DIR=%ProgramFiles%\InventorySystem"

echo.
echo 📁 Diretório de instalação: %INSTALL_DIR%
echo.

REM Criar diretório
if not exist "%INSTALL_DIR%" (
    echo 📂 Criando diretório...
    mkdir "%INSTALL_DIR%"
    if %errorlevel% neq 0 (
        echo ❌ ERRO ao criar diretório
        pause
        exit /b 1
    )
    echo ✅ Diretório criado
)

REM Extrair arquivos
echo.
echo 📦 Extraindo arquivos...

REM Procurar ZIP no mesmo diretório
set "ZIP_FILE="
for %%f in (*.zip) do (
    set "ZIP_FILE=%%f"
)

if "!ZIP_FILE!"=="" (
    echo ❌ ERRO: Arquivo ZIP não encontrado no diretório
    echo.
    echo Por favor, coloque este script e o arquivo InventorySystem-v*.zip na mesma pasta
    pause
    exit /b 1
)

echo 📥 Extraindo: !ZIP_FILE!
powershell -Command "Expand-Archive -Path '!ZIP_FILE!' -DestinationPath '%INSTALL_DIR%' -Force"

if %errorlevel% neq 0 (
    echo ❌ ERRO ao extrair arquivos
    pause
    exit /b 1
)
echo ✅ Arquivos extraídos

REM Criar atalho na área de trabalho
echo.
echo 🔗 Criando atalho na Área de Trabalho...

set "DESKTOP=%USERPROFILE%\Desktop"
set "SHORTCUT=%DESKTOP%\Inventory System.lnk"

powershell -Command ^
    "$WshShell = New-Object -ComObject WScript.Shell; " ^
    "$Shortcut = $WshShell.CreateShortcut('%SHORTCUT%'); " ^
    "$Shortcut.TargetPath = '%INSTALL_DIR%\InventorySystem.exe'; " ^
    "$Shortcut.WorkingDirectory = '%INSTALL_DIR%'; " ^
    "$Shortcut.Description = 'Inventory System v1.0.0'; " ^
    "$Shortcut.Save()"

if %errorlevel% equ 0 (
    echo ✅ Atalho criado em: %DESKTOP%
) else (
    echo ⚠️  Atalho não foi criado (não é crítico)
)

REM Criar atalho no Menu Iniciar
echo 📍 Criando entrada no Menu Iniciar...

set "START_MENU=%APPDATA%\Microsoft\Windows\Start Menu\Programs"
if not exist "%START_MENU%\Inventory System" (
    mkdir "%START_MENU%\Inventory System"
)

powershell -Command ^
    "$WshShell = New-Object -ComObject WScript.Shell; " ^
    "$Shortcut = $WshShell.CreateShortcut('%START_MENU%\Inventory System\Inventory System.lnk'); " ^
    "$Shortcut.TargetPath = '%INSTALL_DIR%\InventorySystem.exe'; " ^
    "$Shortcut.WorkingDirectory = '%INSTALL_DIR%'; " ^
    "$Shortcut.Description = 'Inventory System v1.0.0'; " ^
    "$Shortcut.Save()"

echo ✅ Entrada criada no Menu Iniciar

REM Criar arquivo de desinstalação
echo.
echo 📋 Criando desinstalador...

set "UNINSTALL=%INSTALL_DIR%\Uninstall.bat"

(
    echo @echo off
    echo echo.
    echo echo Desinstalando Inventory System...
    echo echo.
    echo rmdir /s /q "%INSTALL_DIR%"
    echo del /f /q "%SHORTCUT%"
    echo rmdir /s /q "%START_MENU%\Inventory System"
    echo echo.
    echo echo ✅ Desinstalação concluída
    echo pause
) > "%UNINSTALL%"

echo ✅ Desinstalador criado

REM Mensagem final
echo.
echo ╔════════════════════════════════════════════════════════╗
echo ║       ✅ INSTALAÇÃO CONCLUÍDA COM SUCESSO!            ║
echo ╚════════════════════════════════════════════════════════╝
echo.
echo 📁 Caminho: %INSTALL_DIR%
echo 🔗 Atalho: %SHORTCUT%
echo 📍 Menu Iniciar: %START_MENU%\Inventory System
echo.
echo 🚀 Para iniciar a aplicação:
echo    1. Procure "Inventory System" no Menu Iniciar
echo    2. Ou clique no atalho na Área de Trabalho
echo.
echo 🔐 Credenciais padrão (primeira execução):
echo    Usuário: admin
echo    Senha: L9l337643k#$
echo.
echo ⚠️  IMPORTANTE: Altere a senha imediatamente após o login!
echo.
echo 📖 Para mais informações, leia: README.md
echo.

REM Perguntar se deseja iniciar agora
set /p LAUNCH="Deseja iniciar a aplicação agora? (S/N): "
if /i "!LAUNCH!"=="S" (
    start "" "%INSTALL_DIR%\InventorySystem.exe"
)

echo.
echo ✅ Instalação finalizada!
pause
