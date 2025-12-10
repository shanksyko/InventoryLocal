#!/bin/bash
# Script para juntar as partes do InventorySystem.exe
# Execute este arquivo na pasta onde estão as partes

echo ""
echo "========================================"
echo "  Juntando InventorySystem.exe"
echo "========================================"
echo ""

# Verificar se as partes existem
if [ ! -f "InventorySystem.exe.part_aa" ]; then
    echo "❌ ERRO: Arquivo InventorySystem.exe.part_aa não encontrado!"
    echo "Certifique-se que todas as partes estão nesta pasta:"
    echo "- InventorySystem.exe.part_aa"
    echo "- InventorySystem.exe.part_ab"
    echo "- InventorySystem.exe.part_ac"
    echo "- InventorySystem.exe.part_ad"
    exit 1
fi

echo "Procurando partes..."
[ -f "InventorySystem.exe.part_aa" ] && echo "✓ Encontrado: InventorySystem.exe.part_aa (50 MB)"
[ -f "InventorySystem.exe.part_ab" ] && echo "✓ Encontrado: InventorySystem.exe.part_ab (50 MB)"
[ -f "InventorySystem.exe.part_ac" ] && echo "✓ Encontrado: InventorySystem.exe.part_ac (50 MB)"
[ -f "InventorySystem.exe.part_ad" ] && echo "✓ Encontrado: InventorySystem.exe.part_ad (17 MB)"
echo ""

echo "Juntando arquivos..."
echo "Isso pode levar alguns segundos..."
echo ""

# Juntar as partes
cat InventorySystem.exe.part_aa InventorySystem.exe.part_ab InventorySystem.exe.part_ac InventorySystem.exe.part_ad > InventorySystem.exe

if [ -f "InventorySystem.exe" ]; then
    FILE_SIZE=$(du -h InventorySystem.exe | cut -f1)
    echo ""
    echo "========================================"
    echo "✓ SUCESSO! Arquivo criado:"
    echo "  InventorySystem.exe ($FILE_SIZE)"
    echo ""
    echo "Você pode deletar as partes:"
    echo "rm InventorySystem.exe.part_*"
    echo ""
    echo "Pronto para usar!"
    echo "========================================"
else
    echo ""
    echo "❌ ERRO: Não conseguiu juntar os arquivos!"
    echo "Verifique se todas as partes estão presentes."
    echo "========================================"
    exit 1
fi
