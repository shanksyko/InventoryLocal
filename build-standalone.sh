#!/bin/bash
# Script para compilar o executável único (self-contained) do Inventory System
# Este script cria um EXE que não requer .NET instalado na máquina

# Cores para output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}   Compilando Executável Único${NC}"
echo -e "${GREEN}   Inventory System - Self Contained${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Diretório do script
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_FILE="$SCRIPT_DIR/src/InventarioSistem.WinForms/InventarioSistem.WinForms.csproj"
OUTPUT_DIR="$SCRIPT_DIR/publish"

if [ ! -f "$PROJECT_FILE" ]; then
    echo -e "${RED}❌ Arquivo de projeto não encontrado: $PROJECT_FILE${NC}"
    exit 1
fi

echo -e "${YELLOW}📁 Projeto: $PROJECT_FILE${NC}"
echo -e "${YELLOW}📦 Saída: $OUTPUT_DIR${NC}"
echo ""

# Limpar build anterior
echo -e "${YELLOW}🧹 Limpando build anterior...${NC}"
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"
echo -e "${GREEN}✅ Limpeza concluída${NC}"
echo ""

# Publicar (compilar executável único)
echo -e "${YELLOW}🔨 Compilando executável único (self-contained)...${NC}"
echo -e "${YELLOW}   (Isso pode levar alguns minutos na primeira vez)${NC}"
echo ""

dotnet publish "$PROJECT_FILE" \
    -c Release \
    -o "$OUTPUT_DIR" \
    --self-contained \
    -r win-x64 \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:DebugType=embedded

if [ $? -ne 0 ]; then
    echo ""
    echo -e "${RED}❌ Erro na compilação!${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}   ✅ Compilação Concluída com Sucesso!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Informações do executável
EXE_PATH="$OUTPUT_DIR/InventorySystem.exe"
if [ -f "$EXE_PATH" ]; then
    EXE_SIZE=$(du -h "$EXE_PATH" | cut -f1)
    echo -e "${GREEN}📌 Executável: $EXE_PATH${NC}"
    echo -e "${GREEN}📊 Tamanho: $EXE_SIZE${NC}"
    echo ""
    echo -e "${GREEN}🚀 O executável está pronto para distribuição!${NC}"
    echo -e "${GREEN}   Não requer .NET instalado na máquina do usuário${NC}"
    echo ""
    echo -e "${YELLOW}💾 Para usar:${NC}"
    echo -e "${YELLOW}   1. Coloque o arquivo '$EXE_PATH' em qualquer pasta${NC}"
    echo -e "${YELLOW}   2. Duplo clique para executar${NC}"
    echo -e "${YELLOW}   3. Coloque o banco InventorySystem.accdb na mesma pasta (ou configure o caminho)${NC}"
else
    echo -e "${YELLOW}⚠️  Aviso: Arquivo executável não encontrado${NC}"
fi

echo ""
