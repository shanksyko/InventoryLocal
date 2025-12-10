# Análise de Segurança - Inventory System

**Data:** 10/12/2025  
**Desenvolvedor:** Giancarlo Conrado Romualdo

---

## ✅ PONTOS FORTES

### 1. **Hash de Senhas**
- ✅ Usa SHA256 para hash de senhas
- ✅ Senhas nunca são armazenadas em texto puro
- ✅ Método `HashPassword()` e `VerifyPassword()` corretamente implementados
- ⚠️ **RECOMENDAÇÃO:** Migrar para algoritmos mais seguros (bcrypt, Argon2, ou PBKDF2 com salt)

### 2. **SQL Injection Protection**
- ✅ Usa parametrização em TODAS as queries (`AddWithValue()`)
- ✅ Nunca concatena strings SQL com input do usuário
- ✅ Exemplo: `cmd.Parameters.AddWithValue("@username", username)`

### 3. **Auditoria**
- ✅ Sistema de logs de auditoria implementado
- ✅ Registra login, logoff e ações críticas
- ✅ Arquivo: `AuditLog.cs` e `InventoryLogger.cs`

### 4. **Controle de Acesso**
- ✅ Sistema de roles (Admin, Usuario, Visualizador)
- ✅ Verificação de permissões baseada em `UserRole`
- ✅ Visualizador tem apenas leitura
- ✅ Admin pode gerenciar usuários

### 5. **Validação de Entrada**
- ✅ Campos obrigatórios validados
- ✅ Verificação de usuário ativo antes do login
- ✅ Verificação de username vazio

---

## ⚠️ VULNERABILIDADES IDENTIFICADAS

### 1. **CRÍTICO: Algoritmo de Hash Fraco**
**Localização:** `src/InventarioSistem.Core/Entities/User.cs`

**Problema:**
```csharp
public static string HashPassword(string password)
{
    using (var sha256 = SHA256.Create())
    {
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
```

**Risco:**
- SHA256 sem salt é vulnerável a ataques de rainbow table
- SHA256 é muito rápido, permitindo brute force
- Mesma senha sempre gera mesmo hash

**Recomendação:**
- Usar bcrypt, Argon2, ou PBKDF2
- Adicionar salt único por usuário
- Implementar trabalho computacional (key stretching)

---

### 2. **MÉDIO: Senha Hardcoded**
**Localização:** Múltiplos arquivos

**Problema:**
```csharp
// Program.cs linha 71
PasswordHash = User.HashPassword("L9l337643k#$")

// UserStore.cs linha 81
User.HashPassword("L9l337643k#$")

// LoginForm.cs linha 174
&& password == "L9l337643k#$"
```

**Risco:**
- Senha do admin está no código fonte
- Visível no repositório Git
- Dificulta rotação de credenciais

**Recomendação:**
- Forçar troca de senha no primeiro login
- Usar variáveis de ambiente ou arquivo de configuração criptografado

---

### 3. **MÉDIO: Falta de Rate Limiting**
**Localização:** `LoginForm.cs`

**Problema:**
- Não há limite de tentativas de login
- Permite brute force infinito
- Não há delay entre tentativas

**Recomendação:**
```csharp
// Implementar:
- Máximo 5 tentativas por username
- Bloqueio temporário após 5 falhas (15 minutos)
- Delay progressivo entre tentativas (1s, 2s, 4s...)
- Log de tentativas suspeitas
```

---

### 4. **BAIXO: Falta de Validação de Complexidade de Senha**
**Localização:** `UserEditForm.cs`, `PasswordResetDialog.cs`

**Problema:**
- Não valida força da senha
- Permite senhas fracas como "123456"
- Não exige caracteres especiais, maiúsculas, etc.

**Recomendação:**
```csharp
// Implementar validação:
- Mínimo 8 caracteres
- Pelo menos 1 maiúscula
- Pelo menos 1 minúscula
- Pelo menos 1 número
- Pelo menos 1 caractere especial
- Não permitir senhas comuns (blacklist)
```

---

### 5. **BAIXO: Falta de Timeout de Sessão**
**Problema:**
- Usuário permanece logado indefinidamente
- Não há logout automático por inatividade

**Recomendação:**
```csharp
// Implementar:
- Timeout de 30 minutos de inatividade
- Renovação de sessão em ações do usuário
- Aviso antes de deslogar automaticamente
```

---

### 6. **BAIXO: Logs Sensíveis**
**Localização:** `AuditLog.cs`

**Verificar:**
- ✅ Senhas NÃO são logadas
- ✅ Apenas username é registrado
- ⚠️ Verificar se dados sensíveis não vazam em outros logs

---

## 🔐 RECOMENDAÇÕES PRIORITÁRIAS

### **PRIORIDADE ALTA (Implementar Imediatamente)**

1. **Migrar de SHA256 para Bcrypt/Argon2**
```csharp
// Instalar: dotnet add package BCrypt.Net-Next
using BCrypt.Net;

public static string HashPassword(string password)
{
    return BCrypt.HashPassword(password, BCrypt.GenerateSalt(12));
}

public bool VerifyPassword(string password)
{
    return BCrypt.Verify(password, PasswordHash);
}
```

2. **Implementar Rate Limiting no Login**
```csharp
private static Dictionary<string, (int attempts, DateTime lastAttempt)> _loginAttempts = new();

private bool IsRateLimited(string username)
{
    if (!_loginAttempts.TryGetValue(username, out var data))
        return false;

    if (data.attempts >= 5 && DateTime.Now - data.lastAttempt < TimeSpan.FromMinutes(15))
        return true;

    if (DateTime.Now - data.lastAttempt > TimeSpan.FromMinutes(15))
        _loginAttempts.Remove(username);

    return false;
}
```

3. **Forçar Troca de Senha no Primeiro Login**
```csharp
public bool IsFirstLogin { get; set; } = true;

// No login, verificar:
if (user.IsFirstLogin)
{
    var resetDialog = new PasswordResetDialog(user, true);
    // Forçar nova senha
}
```

### **PRIORIDADE MÉDIA**

4. **Validação de Complexidade de Senha**
5. **Timeout de Sessão por Inatividade**
6. **Criptografia da Connection String SQL Server**
7. **Logs de Acesso Centralizado**

### **PRIORIDADE BAIXA**

8. **Autenticação de Dois Fatores (2FA)**
9. **Integração com Active Directory (já planejado)**
10. **Criptografia de Comunicação (se houver rede)**

---

## 📊 SCORE DE SEGURANÇA

| Categoria | Score | Nota |
|-----------|-------|------|
| Autenticação | 6/10 | Básica mas funcional, precisa melhorias |
| Autorização | 8/10 | Boa implementação de roles |
| Proteção SQL | 10/10 | Excelente uso de parametrização |
| Auditoria | 7/10 | Logs básicos, falta centralização |
| Validação | 5/10 | Falta validação de complexidade |
| Criptografia | 4/10 | Hash básico, precisa upgrade urgente |

**SCORE GERAL: 6.7/10** - ⚠️ BOM, mas com melhorias necessárias

---

## ✅ CONFORMIDADES

- ✅ Não armazena senhas em texto puro
- ✅ Usa parametrização SQL (previne SQL Injection)
- ✅ Sistema de auditoria básico
- ✅ Controle de acesso baseado em roles
- ✅ Logs de login/logout

## ❌ NÃO CONFORMIDADES

- ❌ Algoritmo de hash inadequado (SHA256 sem salt)
- ❌ Senha admin hardcoded no código
- ❌ Falta rate limiting
- ❌ Falta validação de complexidade de senha
- ❌ Falta timeout de sessão

---

## 🎯 PLANO DE AÇÃO

### Fase 1 (Urgente - 1 semana)
- [ ] Migrar para bcrypt/Argon2
- [ ] Implementar rate limiting no login
- [ ] Remover senha hardcoded

### Fase 2 (Importante - 2 semanas)
- [ ] Validação de complexidade de senha
- [ ] Forçar troca no primeiro login
- [ ] Timeout de sessão

### Fase 3 (Melhorias - 1 mês)
- [ ] Logs centralizados
- [ ] Criptografia da connection string
- [ ] Auditoria detalhada

---

## 📝 NOTAS FINAIS

O sistema possui uma base de segurança **razoável** para uso interno, mas **NÃO está pronto para produção** sem as melhorias críticas.

**Para uso em ambiente corporativo:**
1. Implementar **TODAS** as recomendações de PRIORIDADE ALTA
2. Considerar integração com Active Directory
3. Backup criptografado do banco de dados SQL Server
4. Política de rotação de senhas

**Desenvolvido por:** Giancarlo Conrado Romualdo  
**Revisão Recomendada:** Trimestral
