# 📦 Inventory System - Build Release v1.0.0

## ✅ Status do Build
- **Compilação**: ✅ Sucesso (Release)
- **Warnings**: ✅ 0
- **Erros**: ✅ 0
- **Data**: 2025-12-11
- **Versão .NET**: 8.0

## 📥 Versões Disponíveis

### 🎯 Versão Completa (Recomendada)
- **Arquivo**: `InventorySystem-v1.0.0-Complete.zip` (70 MB)
- **Inclui**: .NET Runtime 8.0 embutido
- **Vantagem**: Funciona mesmo sem .NET instalado
- **Uso**: Ideal para distribuição / máquinas limpas
- **Instalação**: Extrair e executar imediatamente

### 💾 Versão Leve
- **Arquivo**: `InventorySystem-v1.0.0-Lite.zip` (6.8 MB)
- **Requer**: .NET Runtime 8.0 pré-instalado
- **Vantagem**: Arquivo pequeno
- **Uso**: Ideal para máquinas com .NET 8.0 já instalado
- **Instalação**: Extrair, baixar .NET se necessário, executar

## 📋 Recursos Inclusos

### 🔐 Segurança
- ✅ Autenticação com BCrypt (12 rounds)
- ✅ Rate limiting (5 tentativas/5 minutos)
- ✅ Validação de senha forte
- ✅ Controle de acesso baseado em função (RBAC)

### 💾 Banco de Dados (3 Modos)
- ✅ **LocalDB** - Automático, sem instalação
- ✅ **SQL Server** - Servidor remoto/rede
- ✅ **Arquivo .mdf** - Banco em arquivo (rede/local)

### 🔄 Migração de Dados
- ✅ Migração automática entre bancos
- ✅ Validação de integridade
- ✅ Backup de dados
- ✅ Log em tempo real

### 🎨 Interface
- ✅ UI 100% responsiva
- ✅ Temas com cores personalizadas
- ✅ Componentes reutilizáveis
- ✅ Ícones integrados

### 📊 Funcionalidades
- ✅ Gerenciamento de dispositivos (11 tipos)
- ✅ Controle de usuários
- ✅ Auditoria de operações
- ✅ Filtros e busca avançada
- ✅ Exportação (CSV/XLSX)
- ✅ Relatórios

## 🚀 Como Instalar

### Pré-requisitos

**Versão Completa:**
- Windows 7 ou superior
- 500 MB de espaço em disco
- Nada mais! (.NET já vem incluído)

**Versão Leve:**
- Windows 7 ou superior
- 500 MB de espaço em disco
- .NET Runtime 8.0 ([download aqui](https://dotnet.microsoft.com/download/dotnet/8.0))

### Passo 1: Extrair o ZIP

Escolha uma das versões:
- **Completa**: InventorySystem-v1.0.0-Complete.zip (70 MB)
- **Leve**: InventorySystem-v1.0.0-Lite.zip (6.8 MB)

### Passo 2: Executar
```bash
# Execute o arquivo InventorySystem.exe
InventorySystem.exe
```

### Passo 3: Configurar Banco de Dados
Na primeira execução:
1. Escolha o modo de banco:
   - **LocalDB** (recomendado) - Automático, sem configuração
   - **SQL Server** - Conexão a servidor
   - **Arquivo .mdf** - Arquivo em rede/local

2. Se escolher SQL Server ou .mdf:
   - Sistema detectará dados anteriores
   - Oferecerá migração automática

3. Se houver dados para migrar:
   - Sistema copia automaticamente
   - Valida integridade
   - Começa com dados já presentes

### Credenciais Padrão
**Primeira execução com banco novo:**
- Usuário: `admin`
- Senha: `L9l337643k#$`

⚠️ **Altere a senha imediatamente após o primeiro login!**

## 🔧 Configuração Avançada

### Arquivo de Configuração
Localizado em: `%LOCALAPPDATA%\InventoryLocal\sqlserver.config.json`

```json
{
  "ConnectionString": "...",
  "UseLocalDb": true
}
```

### LocalDB
- Banco salvo em: `%LOCALAPPDATA%\InventoryLocal\InventoryLocal.mdf`
- Acesso: `(localdb)\InventoryLocal`
- Editor: SQL Server Management Studio (grátis)

### SQL Server Remoto
Formato de conexão:
```
servidor\instancia
localhost\SQLEXPRESS
192.168.1.100
servidor.com\SQL2019
```

## 📁 Estrutura de Pastas

```
InventorySystem-v1.0.0.zip
├── InventorySystem.exe          (Aplicação principal)
├── InventarioSistem.Core.dll    (Lógica de negócios)
├── InventarioSistem.Access.dll  (Acesso a dados)
├── *.pdb                        (Símbolos de debug)
└── *.dll                        (Dependências)
```

## 🆘 Troubleshooting

### Erro: "LocalDB não está disponível"
**Solução**: 
- Instale o .NET Runtime 8.0
- Escolha "SQL Server" ou "Arquivo .mdf" na configuração

### Erro: "Não consegui conectar ao servidor"
**Solução**:
- Verifique nome do servidor
- Verifique se SQL Server está rodando
- Confirme permissões de rede

### Erro: "Arquivo .mdf não encontrado"
**Solução**:
- Confirme que o caminho UNC está correto
- Verifique permissões da pasta compartilhada
- Teste com `\\servidor\compartilhamento\arquivo.mdf`

## 📞 Suporte

Para relatório de bugs ou sugestões:
- GitHub: https://github.com/shanksyko/InventoryLocal
- Issues: Abra uma issue no repositório

## 📄 Licença

Este projeto está sob licença MIT. Veja LICENSE para detalhes.

---

**Build Release**: v1.0.0
**Compilado em**: 11 de Dezembro de 2025
**Status**: ✅ Pronto para produção
