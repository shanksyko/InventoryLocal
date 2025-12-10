# 📦 Guia de Distribuição - Executável Único

## 🎯 O que é um Executável Único?

Um executável único (self-contained) é um arquivo `.exe` que contém **tudo** o que precisa para rodar:
- ✅ Seu aplicativo
- ✅ Runtime do .NET 8.0
- ✅ Todas as dependências (bibliotecas)

**Benefícios:**
- 🚀 Não precisa instalar .NET na máquina do usuário
- 📦 Um único arquivo `.exe` para distribuir
- 💾 ~120-150 MB (tamanho típico)
- 🔒 Seguro e portátil

---

## 📝 Como Criar o Executável Único

### Opção 1: PowerShell (Windows)

```powershell
# Abra o PowerShell como Administrador
cd C:\caminho\do\projeto\InventoryLocal
.\build-standalone.ps1
```

### Opção 2: Bash (Linux/WSL)

```bash
cd /caminho/do/projeto/InventoryLocal
chmod +x build-standalone.sh
./build-standalone.sh
```

### Opção 3: Comando Manual (Qualquer Sistema)

```bash
cd src/InventarioSistem.WinForms

dotnet publish -c Release \
    -o ../../publish \
    --self-contained \
    -r win-x64 \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true
```

---

## 📂 Resultado

Após compilar, você terá:

```
publish/
├── InventorySystem.exe         ← 🎯 Este é o arquivo que você distribui!
├── InventorySystem.pdb         (informações de debug, opcional)
└── (vários arquivos de suporte)
```

---

## 🚀 Como Usar o Executável

### Para Você (Desenvolvimento)

```bash
# Copie o banco de dados
cp InventorySystem.accdb publish/

# Execute
./publish/InventorySystem.exe
```

### Para Usuários (Distribuição)

1. **Distribua apenas o arquivo `InventorySystem.exe`**
2. Usuário coloca em qualquer pasta (ex: `C:\InventorySystem\`)
3. Duplo clique para executar
4. Se tiver um banco de dados, coloque na mesma pasta que o `.exe`

---

## 📊 Tamanho Esperado

| Tipo | Tamanho |
|------|---------|
| InventorySystem.exe | ~120-150 MB |
| InventorySystem.accdb | ~100-500 KB (depende dos dados) |
| **Total** | **~120-150 MB** |

---

## ⚙️ Configurações Avançadas

### Comprimir o Executável

```bash
# Windows: Use o 7-Zip ou WinRAR
7z a -tzip InventorySystem.zip InventorySystem.exe

# Tamanho comprimido: ~30-40 MB
```

### Remover Símbolos de Debug

Edite o `.csproj`:

```xml
<DebugType>none</DebugType>
```

Isso reduz o tamanho em ~20%.

---

## 🔍 Verificação

Depois de criar o executável, verifique:

```powershell
# PowerShell
$file = "publish/InventorySystem.exe"
$size = (Get-Item $file).Length / 1MB
Write-Host "Tamanho: $([Math]::Round($size, 2)) MB"
```

---

## 📋 Requisitos do Usuário

O usuário final precisa de:
- Windows 7 SP1 ou superior (64-bit)
- ~150 MB de espaço em disco
- **Não precisa**: .NET Runtime instalado ✨

---

## 🔄 Atualizar o Executável

Sempre que você fizer mudanças no código:

```bash
# Recompile
.\build-standalone.ps1

# Distribuir o novo InventorySystem.exe
```

---

## 📌 Dicas Importantes

1. **Banco de Dados:**
   - Mantenha o `InventorySystem.accdb` na mesma pasta do `.exe`
   - Ou configure o caminho no arquivo de configuração

2. **Primeira Execução:**
   - Pode demorar alguns segundos (está extraindo .NET runtime)
   - Próximas execuções são mais rápidas

3. **Distribuição:**
   - Crie um instalador com NSIS ou MSI para melhor experiência
   - Ou simplesmente distribua o `.exe` via e-mail/USB

4. **Assinatura de Código:**
   - Para maior confiança, assine o `.exe` com certificado digital

---

## 🆘 Troubleshooting

### Erro: "dotnet: command not found"
- Instale o .NET SDK em sua máquina de desenvolvimento
- O usuário final não precisa instalar nada!

### Erro: "Could not find a part of the path"
- Verifique se está no diretório correto
- Use caminhos absolutos nos scripts

### Executável muito grande
- Isso é normal (~120-150 MB)
- Inclui todo o runtime do .NET
- Comprima com 7-Zip para distribuir (~30-40 MB)

---

## ✨ Próximas Melhorias

- [ ] Criar instalador `.msi` profissional
- [ ] Asinar digitalmente o executável
- [ ] Criar auto-updater
- [ ] Empacotar com o banco de dados

---

**Desenvolvido por:** Giancarlo Conrado Romualdo  
**Data:** Dezembro 2024
