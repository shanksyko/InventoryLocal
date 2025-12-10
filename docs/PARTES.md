# 📦 Executável em Partes - Como Usar

## 🎯 O Problema

O GitHub tem limite de **100 MB** por arquivo. O executável é **167 MB**, então foi dividido em 4 partes menores.

## 📂 Arquivos que Você Receberá

```
InventorySystem.exe.part_aa  (50 MB)
InventorySystem.exe.part_ab  (50 MB)
InventorySystem.exe.part_ac  (50 MB)
InventorySystem.exe.part_ad  (17 MB)
JUNTAR.bat                    (Windows)
juntar.sh                     (Linux/Mac)
```

**Total:** 167 MB (quando juntados)

---

## 🚀 Como Juntar no Windows

### **Opção 1: Duplo Clique (Mais Fácil)**
1. Coloque todos os arquivos na mesma pasta
2. Coloque também o arquivo `JUNTAR.bat` na mesma pasta
3. **Duplo clique** em `JUNTAR.bat`
4. Pronto! `InventorySystem.exe` será criado automaticamente

### **Opção 2: PowerShell**
```powershell
# Abra PowerShell na pasta onde estão as partes

# Juntar os arquivos
Get-Content InventorySystem.exe.part_aa, `
           InventorySystem.exe.part_ab, `
           InventorySystem.exe.part_ac, `
           InventorySystem.exe.part_ad | `
Set-Content -Encoding Byte InventorySystem.exe

# Verificar tamanho
(Get-Item InventorySystem.exe).Length / 1MB
```

### **Opção 3: CMD (Prompt de Comando)**
```cmd
REM Abra CMD na pasta onde estão as partes

type InventorySystem.exe.part_aa + ^
     InventorySystem.exe.part_ab + ^
     InventorySystem.exe.part_ac + ^
     InventorySystem.exe.part_ad > InventorySystem.exe
```

---

## 🚀 Como Juntar em Linux/Mac

### **Opção 1: Script Automático (Mais Fácil)**
```bash
# Coloque todos os arquivos na mesma pasta
# Coloque também o arquivo juntar.sh na mesma pasta

chmod +x juntar.sh
./juntar.sh
```

### **Opção 2: Comando Manual**
```bash
# Abra terminal na pasta onde estão as partes

cat InventorySystem.exe.part_aa \
    InventorySystem.exe.part_ab \
    InventorySystem.exe.part_ac \
    InventorySystem.exe.part_ad > InventorySystem.exe

# Verificar tamanho
ls -lh InventorySystem.exe
```

---

## ✅ Verificação

Após juntar, o arquivo `InventorySystem.exe` deve ter:
- **Tamanho:** ~167 MB
- **Tipo:** PE32+ executable (Windows)

### **Verificar no Windows**
```powershell
(Get-Item InventorySystem.exe).Length / 1MB
# Deve mostrar: ~167 MB
```

### **Verificar no Linux/Mac**
```bash
ls -lh InventorySystem.exe
du -h InventorySystem.exe
# Deve mostrar: 167M
```

---

## 🧹 Limpeza (Opcional)

Após verificar que o `InventorySystem.exe` foi criado corretamente, você pode deletar as partes:

### **Windows**
```cmd
del InventorySystem.exe.part_aa
del InventorySystem.exe.part_ab
del InventorySystem.exe.part_ac
del InventorySystem.exe.part_ad
```

### **Linux/Mac**
```bash
rm InventorySystem.exe.part_*
```

---

## 🚀 Próximo Passo

Após juntar o arquivo, siga as instruções de instalação em `DOWNLOAD.md`:

1. Coloque `InventorySystem.exe` em uma pasta
2. Coloque `InventorySystem.accdb` (seu banco) na mesma pasta
3. Duplo clique em `InventorySystem.exe`
4. Login com:
   - Usuário: `admin`
   - Senha: `L9l337643k#$`
5. Altere a senha imediatamente!

---

## ⚠️ Troubleshooting

### **Erro: "O arquivo está corrompido"**
- ❌ Certifique-se que **TODAS** as partes foram baixadas
- ❌ Verifique se nenhuma parte foi truncada
- ✅ Refaça o download de todas as partes

### **Erro: "Arquivo não é um executável válido"**
- ❌ Verifique a ordem das partes (aa → ab → ac → ad)
- ❌ Use o script `JUNTAR.bat` ou `juntar.sh` ao invés de fazer manualmente
- ✅ Tente novamente com o script

### **Processo lento**
- Isso é normal! Juntando 4 arquivos de 50+ MB pode levar alguns segundos
- Aguarde o final do processo

---

## 📋 Resumo das Partes

| Parte | Tamanho | Offset | Para Quando |
|-------|---------|--------|------------|
| aa | 50 MB | 0 MB | Primeiros 50 MB |
| ab | 50 MB | 50 MB | Próximos 50 MB |
| ac | 50 MB | 100 MB | Próximos 50 MB |
| ad | 17 MB | 150 MB | Últimos 17 MB |
| **Total** | **167 MB** | - | Arquivo completo |

---

## ✨ Curiosidade

Se você tivesse que fazer isso "do jeito antigo":
- 💿 Gravaria em 3-4 DVDs (4.7 GB cada)
- 💾 Ou em 6-7 Pen drives USB (8 GB cada)
- 📧 Ou enviaria por email em múltiplos anexos

Agora você tem tudo em 4 arquivos simples! 🎉

---

**Data:** Dezembro 2024  
**Versão:** InventorySystem v1.0
