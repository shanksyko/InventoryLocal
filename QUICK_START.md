# InventorySystem - Quick Start

## 🚀 Abrindo o Projeto

```bash
cd C:\Repositorio\InventoryLocal
code .
```

## ⌨️ Atalhos Essenciais

| Atalho | Ação |
|--------|------|
| `Ctrl+Shift+B` | Build (compilar) |
| `F5` | Debug (WinForms) |
| `Ctrl+F5` | Run sem debug |
| `Shift+Alt+F` | Formatar documento |
| `Ctrl+.` | Quick fix |
| `Ctrl+Shift+P` | Command palette |
| `Ctrl+P` | Buscar arquivo |
| `Ctrl+Shift+F` | Buscar em arquivos |
| `Ctrl+\`` | Abrir terminal |

## 🔨 Tasks Disponíveis

Aperte `Ctrl+Shift+P` → "Tasks: Run Task" → Escolha:

- **build** - Compila toda solução
- **clean** - Limpa build
- **restore** - Restaura NuGet
- **Run WinForms** - Executa WinForms
- **Run CLI** - Executa CLI

## 🐛 Debug

1. Abra o arquivo que quer debugar
2. Clique na margem esquerda para adicionar breakpoint (bolinha vermelha)
3. Pressione `F5`
4. Use:
   - `F10` - Step over
   - `F11` - Step into
   - `Shift+F11` - Step out
   - `F5` - Continue

## 📦 Estrutura

```
src/
├── InventarioSistem.Core/      - Models, utilitários
├── InventarioSistem.Access/    - Banco de dados
├── InventarioSistem.WinForms/  - Interface gráfica
└── InventarioSistem.Cli/       - Interface CLI
```

## 🔧 Primeiro Uso

1. **Instalar extensões recomendadas**
   - VS Code vai perguntar automaticamente
   - Ou: Extensions → "Show Recommended"

2. **Testar build**
   ```bash
   Ctrl+Shift+B
   ```

3. **Executar WinForms**
   ```bash
   F5
   ```

## 📝 Git Workflow

```bash
# Ver status
git status

# Adicionar arquivos
git add .

# Commit
git commit -m "feat: Descrição da mudança"

# Push
git push origin main
```

## 🆘 Problemas Comuns

### Build falha
```bash
dotnet clean
dotnet restore
dotnet build
```

### IntelliSense não funciona
```
Ctrl+Shift+P > "Reload Window"
```

### Configurações não aplicam
Feche e reabra o VS Code

## 📚 Documentação

- [CONFIGURACOES_IMPORTADAS.md](CONFIGURACOES_IMPORTADAS.md) - Config VS Code
- [BUGFIXES_AND_IMPROVEMENTS.md](BUGFIXES_AND_IMPROVEMENTS.md) - Bugs e melhorias
- [SQL_VALIDATION_REPORT.md](SQL_VALIDATION_REPORT.md) - Validação SQL

---

**Dúvidas?** Pergunte ao GitHub Copilot Chat!
