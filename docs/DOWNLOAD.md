# 📥 Download do Executável - Inventory System v1.0

## 🎯 Links de Download

### **Opção 1: GitHub Releases (Recomendado)**
👉 https://github.com/shanksyko/InventoryLocal/releases/tag/v1.0-release

### **Opção 2: Download Direto**
- **Arquivo:** `InventorySystem.exe`
- **Tamanho:** 167 MB
- **Localização:** `/publish/InventorySystem.exe`

### **Opção 3: Arquivo Comprimido**
- **Arquivo:** `InventorySystem-v1.0.tar.gz`
- **Tamanho:** 67 MB (mais compacto)
- **Localização:** `/publish/InventorySystem-v1.0.tar.gz`

---

## 📋 Pré-requisitos do Sistema

Seu computador precisa de:
- **Sistema Operacional:** Windows 7 SP1 ou superior (64-bit)
- **Processador:** x64 (Intel ou AMD)
- **Memória RAM:** Mínimo 2 GB
- **Espaço em Disco:** 200+ MB disponível
- **.NET:** ❌ NÃO precisa instalar (já incluído no executável)

---

## 🚀 Como Usar

### **Passo 1: Baixar o Executável**
1. Acesse: https://github.com/shanksyko/InventoryLocal/releases/tag/v1.0-release
2. Clique em `InventorySystem.exe` para baixar
3. Salve em uma pasta de sua escolha (ex: `C:\InventorySystem\`)

### **Passo 2: Preparar o Banco de Dados**
1. Se você tiver um banco de dados (`InventorySystem.accdb`), coloque na **mesma pasta** do `.exe`
2. Se não tiver, o sistema criará um novo banco automaticamente na primeira execução

### **Passo 3: Executar o Programa**
1. Duplo clique em `InventorySystem.exe`
2. Na primeira execução:
   - Usuário padrão: `admin`
   - Senha padrão: `L9l337643k#$`
   - ⚠️ **IMPORTANTE:** Altere a senha após o primeiro login!

### **Passo 4: Primeiro Acesso**
- Faça login como `admin`
- Mude a senha para algo seguro
- Configure o banco de dados (selecione o arquivo `.accdb`)
- Pronto! Você pode usar o sistema

---

## 🔒 Informações de Segurança

### **Primeira Execução**
- Pode levar **10-15 segundos** (está extrayendo o .NET runtime)
- Próximas execuções são **muito mais rápidas** (2-3 segundos)

### **Senha de Admin Padrão**
```
Usuário: admin
Senha: L9l337643k#$
```

⚠️ **OBRIGATÓRIO:** Altere esta senha imediatamente!

### **Requisitos de Senha Forte**
Qualquer nova senha deve conter:
- ✅ Mínimo 8 caracteres
- ✅ Letra maiúscula (A-Z)
- ✅ Letra minúscula (a-z)
- ✅ Número (0-9)
- ✅ Caractere especial (!@#$%^&*)

Exemplo de senha válida: `Senha@2024#Forte`

### **Proteção Contra Ataques**
- 🛡️ **BCrypt:** Senhas criptografadas com algoritmo militar
- ⏱️ **Rate Limiting:** Bloqueio automático após 5 tentativas erradas (15 minutos)
- 📊 **Auditoria:** Todos os logins são registrados

---

## 📂 Estrutura de Arquivos

Após baixar, sua pasta deve ser assim:

```
C:\InventorySystem\
├── InventorySystem.exe          ← Executável principal
└── InventorySystem.accdb        ← Banco de dados (criado automaticamente)
```

---

## 🆘 Troubleshooting

### **O programa não inicia**
- Verifique se você tem Windows 7 SP1 ou superior (64-bit)
- Tente copiar para outra pasta (ex: `C:\Temp\`)
- Desabilite antivírus temporariamente e tente novamente

### **Esqueceu a senha de admin**
- ❌ Não há "recuperação de senha" no sistema
- ✅ Solução: Delete o arquivo `InventorySystem.accdb` da pasta
- Ao reiniciar, um novo banco será criado com admin padrão

### **Arquivo .accdb corrompido**
1. Faça backup do arquivo corrompido
2. Delete-o
3. Reinicie o programa para criar um novo banco
4. Restaure seus dados manualmente

### **Erro "The system cannot find the specified path"**
- Certifique-se que a pasta tem **permissão de escrita**
- Tente executar como Administrador (clique direito → "Executar como administrador")

---

## 📞 Suporte Técnico

### **Requisitos Técnicos**
- Esta é uma aplicação Windows Forms
- Requer Windows 64-bit
- Requer ~200 MB de espaço em disco

### **Banco de Dados**
- Usa Microsoft Access (.accdb)
- Compatível com Access 2010+
- Pode ser aberto também no Microsoft Access se necessário

### **Relatórios e Exportação**
- Suporta exportação para Excel (.xlsx)
- Suporta visualização de relatórios em PDF

---

## ✨ Características da Versão 1.0

### **Segurança**
- ✅ Autenticação com BCrypt (padrão military-grade)
- ✅ Rate limiting contra força bruta
- ✅ Validação de senha complexa
- ✅ Logs de auditoria completos

### **Interface**
- ✅ Ícone personalizado
- ✅ Interface moderna Windows Forms
- ✅ Múltiplas abas por tipo de equipamento
- ✅ Gráficos e dashboards

### **Funcionalidades**
- ✅ Gerenciamento de inventário
- ✅ Suporte para múltiplos tipos de dispositivos
- ✅ Filtros avançados de busca
- ✅ Exportação para Excel
- ✅ Relatórios customizáveis

---

## 📝 Changelog

### **v1.0 (2024-12)**
- 🎉 Versão inicial lançada
- 🔐 Implementação de segurança aprimorada
- 🎨 Interface com ícone personalizado
- 📦 Executável único (self-contained)
- 📊 Dashboards e relatórios
- ✅ Compatível com Access Database

---

## 📜 Licença & Créditos

**Desenvolvido por:** Giancarlo Conrado Romualdo  
**Licença:** Proprietary (Todos os direitos reservados)  
**Data:** Dezembro 2024

---

## 🔗 Links Úteis

- 📦 **Releases:** https://github.com/shanksyko/InventoryLocal/releases
- 📚 **Documentação:** https://github.com/shanksyko/InventoryLocal/docs
- 🐛 **Reportar Bug:** https://github.com/shanksyko/InventoryLocal/issues
- ⭐ **GitHub:** https://github.com/shanksyko/InventoryLocal

---

## ❓ FAQ - Perguntas Frequentes

### **P: Preciso instalar .NET?**
R: Não! O executável já inclui tudo que precisa.

### **P: Funciona em Windows 32-bit?**
R: Não, apenas em Windows 64-bit (x64).

### **P: Posso usar em rede?**
R: Sim! Coloque o banco de dados em compartilhamento de rede (ex: `\\servidor\inventario\InventorySystem.accdb`).

### **P: Os dados são seguros?**
R: Sim! As senhas são criptografadas com BCrypt. O banco de dados é Access padrão.

### **P: Posso executar em Mac/Linux?**
R: Não, apenas em Windows. Para Mac/Linux seria necessária uma versão diferente.

### **P: Qual é o limite de usuários?**
R: Sem limite técnico. Access suporta até ~2 GB de dados por arquivo.

---

**Versão:** 1.0  
**Última atualização:** Dezembro 10, 2024
