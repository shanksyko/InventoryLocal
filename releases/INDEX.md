# 🎯 Inventory System v1.0.0

## 📦 Download e Instalação

### Arquivo Principal
- **InventorySystem-v1.0.0.zip** (6.8 MB)
  - Aplicação completa pronta para usar
  - Contém todos os executáveis e dependências
  - Comprimido para fácil download

### Como Instalar

#### Opção 1: Instalação Automática (Recomendado)
```bash
1. Baixe InventorySystem-v1.0.0.zip
2. Extraia em uma pasta
3. Execute Install.bat como Administrador
4. Siga as instruções
```

**Resultado:**
- ✅ Aplicação instalada em `C:\Program Files\InventorySystem`
- ✅ Atalho criado na Área de Trabalho
- ✅ Entrada criada no Menu Iniciar
- ✅ Desinstalador incluído

#### Opção 2: Uso Direto
```bash
1. Extraia InventorySystem-v1.0.0.zip
2. Clique duas vezes em InventorySystem.exe
3. Configure banco de dados (LocalDB padrão)
```

## 🚀 Primeiro Uso

### Configuração Automática
Na primeira execução, o sistema:
1. Detecta LocalDB disponível
2. Se não encontrar, oferece alternativas:
   - ✅ **LocalDB** (padrão, sem instalação)
   - ✅ **SQL Server** (servidor remoto)
   - ✅ **Arquivo .mdf** (rede/local)

### Login Padrão
```
Usuário: admin
Senha:   L9l337643k#$
```

⚠️ **Altere imediatamente após o primeiro login!**

## 📚 Documentação

- **README.md** - Guia completo de instalação e uso
- **RELEASE_NOTES.md** - Detalhes técnicos e features
- **sqlserver.config.example.json** - Arquivo de configuração

## ✨ Principais Features

### 🔐 Segurança
- BCrypt com 12 rounds (OWASP recomendado)
- Rate limiting automático
- Validação de senha forte
- Auditoria completa de operações

### 💾 Banco de Dados
- **LocalDB** - Automático, sem instalação
- **SQL Server** - Suporte a versões 2012+
- **Arquivo .mdf** - Com suporte a caminhos UNC (\\servidor\compartilhamento)

### 🔄 Migração Automática
Ao mudar de banco de dados:
- ✅ Detecta dados anteriores
- ✅ Pergunta se deseja migrar
- ✅ Copia dados automaticamente
- ✅ Valida integridade

### 🎨 Interface Moderna
- 100% Responsiva
- Componentes personalizados
- Ícones integrados
- Cores profissionais

### 📊 Funcionalidades
- 11 tipos de dispositivos
- Controle de usuários
- Auditoria de operações
- Filtros e busca avançada
- Exportação CSV/XLSX
- Relatórios

## 📈 Qualidade

✅ **0 Warnings** - Compilação limpa  
✅ **0 Erros** - Build Release bem-sucedido  
✅ **15,000+ linhas** de código  
✅ **80+ classes** implementadas  
✅ **500+ métodos** funcionalidades  

## 🖥️ Requisitos

- **SO**: Windows 7 ou superior
- **Memória**: 512 MB mínimo
- **Disco**: 500 MB livre
- **.NET**: Runtime 8.0 (download automático se necessário)

## 🔧 Suporte

### Erros Comuns

**"LocalDB não disponível"**
- Instale .NET Runtime 8.0
- Use SQL Server ou Arquivo .mdf

**"Não consegui conectar ao servidor"**
- Verifique nome do servidor
- Confirme que SQL Server está rodando

**"Arquivo .mdf não encontrado"**
- Confirme caminho UNC correto
- Verifique permissões da pasta

### Contato
- GitHub: https://github.com/shanksyko/InventoryLocal
- Issues: Abra uma issue no repositório

## 📋 Roadmap v2.0

- [ ] API REST
- [ ] App Mobile
- [ ] Sincronização em Nuvem
- [ ] Dashboard com Gráficos
- [ ] Notificações
- [ ] Backup Automático

---

**Versão**: 1.0.0  
**Data**: 11 de Dezembro de 2025  
**Status**: ✅ Pronto para Produção
