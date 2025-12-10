# 🚀 Como Compilar o Inventory System

## 📋 Pré-requisitos

Você precisa ter instalado:

1. **Visual Studio 2022** ou **Visual Studio Code**
   - https://visualstudio.microsoft.com/

2. **.NET 8.0 SDK**
   - https://dotnet.microsoft.com/download/dotnet/8.0

3. **Git** (para clonar o repositório)
   - https://git-scm.com/

---

## 📥 Passo 1: Clonar o Repositório

```bash
git clone https://github.com/shanksyko/InventoryLocal.git
cd InventoryLocal
```

---

## 🔨 Passo 2: Compilar o Projeto

### **Opção A: Usando Visual Studio 2022** (Mais Fácil)

1. Abra o arquivo `InventoryLocal.sln` no Visual Studio 2022
2. Espere o VS carregar todos os projetos
3. Menu → **Build** → **Build Solution** (ou Ctrl+Shift+B)
4. Pronto! O programa compilou

### **Opção B: Usando a Linha de Comando**

```bash
# Build em modo Debug (desenvolvimento)
dotnet build

# Build em modo Release (produção)
dotnet build -c Release
```

---

## ▶️ Passo 3: Executar o Programa

### **Opção A: Do Visual Studio**
- Pressione **F5** ou clique em **Start/Run** (▶️)

### **Opção B: Da Linha de Comando**

```bash
# Modo Debug
dotnet run --project src/InventarioSistem.WinForms

# Modo Release (mais rápido)
dotnet run -c Release --project src/InventarioSistem.WinForms
```

---

## 📦 Passo 4: Criar o Executável Compilado

Se você quer gerar um executável `.exe` que não precisa do .NET instalado:

```bash
# Executável único (self-contained) - Recomendado
dotnet publish src/InventarioSistem.WinForms/InventarioSistem.WinForms.csproj \
    -c Release \
    -o ./publish \
    --self-contained \
    -r win-x64 \
    -p:PublishSingleFile=true

# O executável estará em: ./publish/InventorySystem.exe
```

**Resultado:**
- `InventorySystem.exe` (~170 MB)
- Não requer .NET instalado
- Pronto para distribuição

---

## 📂 Estrutura do Projeto

```
InventoryLocal/
├── InventoryLocal.sln              ← Solução principal
├── src/
│   ├── InventarioSistem.Core/      ← Lógica de negócio
│   ├── InventarioSistem.Access/    ← Acesso ao banco (Access)
│   ├── InventarioSistem.WinForms/  ← Interface gráfica (Windows Forms)
│   └── InventarioSistem.Cli/       ← CLI (linha de comando)
├── docs/                            ← Documentação
└── README.md
```

---

## 🔧 Configuração

### **Banco de Dados**
- Automático: Criado na primeira execução
- Local padrão: Mesma pasta que o executável
- Formato: Microsoft Access (.accdb)

### **Credenciais Padrão**
```
Usuário: admin
Senha: L9l337643k#$
```

⚠️ **Altere a senha na primeira execução!**

---

## 🛠️ Troubleshooting

### **Erro: ".NET 8.0 not found"**
```bash
# Instale o .NET 8.0 SDK
# Windows: https://dotnet.microsoft.com/download/dotnet/8.0
# Linux/Mac: Você pode instalar via package manager
```

### **Erro: "The project file 'InventoryLocal.sln' was not found"**
- Certifique-se que está na pasta raiz do projeto
- Use `ls` ou `dir` para verificar se `InventoryLocal.sln` existe

### **Erro: "The name 'BCrypt' does not exist"**
- Execute: `dotnet restore`
- Aguarde as dependências serem baixadas

### **Compilação lenta na primeira vez**
- Normal! Está baixando o .NET 8.0 e todas as dependências
- Próximas compilações são mais rápidas

---

## 📊 Tempo Esperado

| Ação | Tempo |
|------|-------|
| Clonar repositório | 1-2 min |
| dotnet restore | 2-5 min (1ª vez) |
| dotnet build | 30-60 seg |
| dotnet run | 5-10 seg |
| dotnet publish | 5-10 min |

---

## 🚀 Comandos Úteis

```bash
# Ver versão do .NET instalada
dotnet --version

# Restaurar dependências
dotnet restore

# Compilar apenas
dotnet build

# Compilar em Release (otimizado)
dotnet build -c Release

# Executar
dotnet run --project src/InventarioSistem.WinForms

# Publicar (criar executável)
dotnet publish src/InventarioSistem.WinForms/InventarioSistem.WinForms.csproj \
    -c Release -o ./publish --self-contained -r win-x64 \
    -p:PublishSingleFile=true

# Limpar arquivos compilados
dotnet clean
```

---

## 📚 Documentação Adicional

- **SECURITY_IMPROVEMENTS.md** - Melhorias de segurança implementadas
- **DISTRIBUICAO.md** - Guia de distribuição do executável

---

## ✅ Verificação Final

Após compilar, você deve ver:
- ✅ "Build succeeded" (sem erros)
- ✅ Programa abri sem problemas
- ✅ Tela de login apareceu

Se tudo funcionar, você está pronto! 🎉

---

**Desenvolvido por:** Giancarlo Conrado Romualdo  
**Última atualização:** Dezembro 2024  
**Versão .NET:** 8.0
