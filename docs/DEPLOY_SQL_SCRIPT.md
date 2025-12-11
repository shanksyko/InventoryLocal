# 📦 Guia: Movimentar o Script SQL para Diferentes Ambientes

## 📍 Localização do Arquivo

```
/workspaces/InventoryLocal/scripts/create-schema.sql
```

**Caminho do repositório Git:**
```
https://github.com/shanksyko/InventoryLocal/blob/main/scripts/create-schema.sql
```

---

## 🚀 Como Usar o Script SQL

### 1️⃣ **Executar Localmente (SQL Server Management Studio)**

```sql
-- 1. Abra o SQL Server Management Studio
-- 2. Conecte ao seu servidor SQL Server Express
-- 3. Crie um novo banco de dados (ou use um existente)
-- 4. Abra o arquivo: scripts/create-schema.sql
-- 5. Clique em "Execute" ou pressione F5

-- Resultado:
-- ✅ Todas as 12 tabelas criadas
-- ✅ Índices para performance
-- ✅ Views úteis
-- ✅ Usuário admin criado
```

### 2️⃣ **Executar via PowerShell (Automático)**

```powershell
# Instale sqlcmd se não tiver
choco install sqlserver-cmdlineutils

# Execute o script
sqlcmd -S .\SQLEXPRESS `
       -d InventoryLocal `
       -U sa `
       -P "sua_senha" `
       -i "C:\caminho\para\scripts\create-schema.sql"

# Ou com autenticação Windows
sqlcmd -S .\SQLEXPRESS `
       -d InventoryLocal `
       -E `
       -i "C:\caminho\para\scripts\create-schema.sql"
```

### 3️⃣ **Executar via Linha de Comando (CMD)**

```cmd
REM Para SQL Server 2019+
sqlcmd -S SERVIDOR_SQL -d DATABASE -U usuario -P senha -i C:\caminho\create-schema.sql

REM Exemplo completo
sqlcmd -S localhost\SQLEXPRESS -d InventoryLocal -U sa -P Senha123! -i D:\scripts\create-schema.sql

REM Com autenticação Windows
sqlcmd -S localhost\SQLEXPRESS -d InventoryLocal -E -i D:\scripts\create-schema.sql
```

---

## 🌐 Movimentar o Script para um Servidor

### **Opção A: Copiar o arquivo para o servidor**

```bash
# Via SCP (Linux/Mac/Git Bash)
scp scripts/create-schema.sql usuario@servidor.com:/tmp/

# Via RDP (Windows)
# 1. Abra Remote Desktop
# 2. Copie o arquivo via Ctrl+C/V do seu computador
# 3. Cole no servidor

# Via FTP/SFTP
# 1. Conecte via FileZilla ou WinSCP
# 2. Upload: scripts/create-schema.sql para /scripts/ no servidor
```

### **Opção B: Executar remotamente**

```powershell
# Executar em servidor remoto via PowerShell
Invoke-Command -ComputerName "servidor.com" -ScriptBlock {
    sqlcmd -S .\SQLEXPRESS `
           -d InventoryLocal `
           -U sa `
           -P "senha" `
           -i "C:\scripts\create-schema.sql"
}
```

### **Opção C: Download direto do GitHub**

```powershell
# PowerShell - Baixar e executar direto
$url = "https://raw.githubusercontent.com/shanksyko/InventoryLocal/main/scripts/create-schema.sql"
$tempFile = "$env:TEMP\create-schema.sql"

# Baixar arquivo
Invoke-WebRequest -Uri $url -OutFile $tempFile

# Executar
sqlcmd -S .\SQLEXPRESS `
       -d InventoryLocal `
       -U sa `
       -P "Senha123!" `
       -i $tempFile

# Limpar
Remove-Item $tempFile
```

---

## 💾 Criar um Banco de Dados Novo Antes de Executar

```sql
-- Execute isso ANTES de rodar o create-schema.sql

-- 1. Conectar ao servidor master
USE master;

-- 2. Criar novo banco de dados
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'InventoryLocal')
BEGIN
    CREATE DATABASE [InventoryLocal]
    ON PRIMARY (
        NAME = N'InventoryLocal_Data',
        FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.SQLEXPRESS\MSSQL\DATA\InventoryLocal.mdf'
    )
    LOG ON (
        NAME = N'InventoryLocal_Log',
        FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.SQLEXPRESS\MSSQL\DATA\InventoryLocal_log.ldf'
    );
    
    PRINT 'Banco de dados InventoryLocal criado com sucesso!';
END
ELSE
BEGIN
    PRINT 'Banco de dados InventoryLocal já existe!';
END;

-- 3. Agora execute o create-schema.sql neste banco
```

---

## 🔑 Credenciais do Admin

**Criar usuário SQL Server para a aplicação:**

```sql
-- Conectar ao master primeiro
USE master;

-- Criar login SQL
CREATE LOGIN InventoryUser WITH PASSWORD = 'SenhaSegura123!@#';

-- Criar usuário no banco InventoryLocal
USE InventoryLocal;
CREATE USER InventoryUser FOR LOGIN InventoryUser;

-- Dar permissões
ALTER ROLE db_owner ADD MEMBER InventoryUser;

PRINT 'Usuário InventoryUser criado com sucesso!';
```

**Credenciais padrão da aplicação:**
```
Usuário Admin: admin
Senha Admin: L9l337643k#$
Role: Admin

Usuário BD: InventoryUser
Senha BD: SenhaSegura123!@#
```

---

## 📋 Checklist de Implantação

### Local (seu PC)
- [ ] SQL Server Express instalado
- [ ] Arquivo `create-schema.sql` acessível
- [ ] SQL Server Management Studio aberto
- [ ] Banco de dados criado
- [ ] Script executado sem erros
- [ ] Admin consegue fazer login

### Servidor (Produção)
- [ ] SQL Server 2019+ instalado no servidor
- [ ] Arquivo copiado para `/scripts/` no servidor
- [ ] Backup do banco existente (se houver)
- [ ] Usuário SQL Server criado com permissões
- [ ] Script executado no servidor
- [ ] Testar conectividade da aplicação
- [ ] Validar dados no banco via SSMS

---

## 🛠️ Script Auxiliar: Backup + Restauração

```sql
-- BACKUP do banco
BACKUP DATABASE [InventoryLocal]
TO DISK = 'C:\Backups\InventoryLocal_backup.bak'
WITH INIT, COMPRESSION;

-- RESTAURAÇÃO
RESTORE DATABASE [InventoryLocal]
FROM DISK = 'C:\Backups\InventoryLocal_backup.bak'
WITH REPLACE;

-- Validar integridade
DBCC CHECKDB (InventoryLocal) WITH NO_INFOMSGS;
```

---

## 🔄 Atualizar Schema (Nova Versão)

Se o script tiver mudanças:

```bash
# 1. Baixar versão nova do GitHub
git pull origin main

# 2. Executar script novo (ele verifica se tabelas existem)
sqlcmd -S localhost\SQLEXPRESS -d InventoryLocal -E -i scripts/create-schema.sql

# 3. Verificar erros
# Se tiver DROP TABLE, fazer backup primeiro!
```

---

## ⚠️ Cuidados Importantes

1. **Sempre fazer backup antes** de executar em produção
2. **Testar em ambiente de desenvolvimento** primeiro
3. **Verificar permissões** do usuário SQL Server
4. **Manter senha segura** - não compartilhe em logs
5. **Monitorar performance** após criar índices
6. **Validar integridade** do banco após execução

---

## 🆘 Troubleshooting

### Erro: "Cannot open database 'InventoryLocal'"
```sql
-- Solução: Criar banco primeiro
CREATE DATABASE [InventoryLocal];
```

### Erro: "Login failed for user 'sa'"
```powershell
# Solução: Verificar autenticação
# Use -E para Windows Auth ou -U/-P para SQL Auth
sqlcmd -S localhost\SQLEXPRESS -E
```

### Erro: "File 'create-schema.sql' not found"
```bash
# Solução: Verificar caminho completo
cd /workspaces/InventoryLocal
sqlcmd ... -i "$(pwd)/scripts/create-schema.sql"
```

### Tabelas já existem
```sql
-- O script tem IF NOT EXISTS, então é seguro rodar novamente
-- Ele criará apenas o que não existir
```

---

## 📞 Suporte

Para dúvidas ou problemas:
- GitHub Issues: https://github.com/shanksyko/InventoryLocal/issues
- Email: giancarlo@exemplo.com

---

**Desenvolvido por:** Giancarlo Conrado Romualdo  
**Última atualização:** Dezembro 2024  
**Versão:** 1.0
