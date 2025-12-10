@echo off
REM Script para juntar as partes do InventorySystem.exe
REM Execute este arquivo na pasta onde estão as partes

echo.
echo ========================================
echo  Juntando InventorySystem.exe
echo ========================================
echo.

REM Verificar se as partes existem
if not exist "InventorySystem.exe.part_aa" (
    echo ERRO: Arquivo InventorySystem.exe.part_aa nao encontrado!
    echo Certifique-se que todas as partes estao nesta pasta:
    echo - InventorySystem.exe.part_aa
    echo - InventorySystem.exe.part_ab
    echo - InventorySystem.exe.part_ac
    echo - InventorySystem.exe.part_ad
    pause
    exit /b 1
)

echo Procurando partes...
if exist "InventorySystem.exe.part_aa" echo ✓ Encontrado: InventorySystem.exe.part_aa (50 MB)
if exist "InventorySystem.exe.part_ab" echo ✓ Encontrado: InventorySystem.exe.part_ab (50 MB)
if exist "InventorySystem.exe.part_ac" echo ✓ Encontrado: InventorySystem.exe.part_ac (50 MB)
if exist "InventorySystem.exe.part_ad" echo ✓ Encontrado: InventorySystem.exe.part_ad (17 MB)
echo.

echo Juntando arquivos...
echo Isso pode levar alguns segundos...
echo.

REM Juntar as partes (Windows)
type InventorySystem.exe.part_aa InventorySystem.exe.part_ab InventorySystem.exe.part_ac InventorySystem.exe.part_ad > InventorySystem.exe

if exist "InventorySystem.exe" (
    echo.
    echo ========================================
    echo ✓ SUCESSO! Arquivo criado:
    echo   InventorySystem.exe
    echo.
    echo Você pode deletar as partes:
    echo - InventorySystem.exe.part_aa
    echo - InventorySystem.exe.part_ab
    echo - InventorySystem.exe.part_ac
    echo - InventorySystem.exe.part_ad
    echo.
    echo Pronto para usar!
    echo ========================================
) else (
    echo.
    echo ERRO: Nao conseguiu juntar os arquivos!
    echo Verifique se todas as partes estao presentes.
    echo ========================================
)

pause
