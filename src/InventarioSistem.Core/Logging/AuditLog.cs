using System;
using InventarioSistem.Core.Entities;

namespace InventarioSistem.Core.Logging;

/// <summary>
/// Log de auditoria de usuários e ações administrativas
/// </summary>
public static class AuditLog
{
    public static void LogUserAction(string username, string action, string details = "")
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var logMessage = $"[AUDITORIA] {timestamp} | Usuário: {username} | Ação: {action}";
        
        if (!string.IsNullOrWhiteSpace(details))
        {
            logMessage += $" | Detalhes: {details}";
        }
        
        InventoryLogger.Info("Auditoria", logMessage);
    }

    public static void LogLogin(string username, bool success)
    {
        var status = success ? "SUCESSO" : "FALHA";
        LogUserAction(username, $"LOGIN ({status})", "");
    }

    public static void LogPasswordChange(string username, string changedBy)
    {
        LogUserAction(changedBy, "RESET_SENHA", $"Usuário: {username}");
    }

    public static void LogUserCreation(string newUsername, string createdBy)
    {
        LogUserAction(createdBy, "CRIAR_USUÁRIO", $"Novo usuário: {newUsername}");
    }

    public static void LogUserDeletion(string deletedUsername, string deletedBy)
    {
        LogUserAction(deletedBy, "DELETAR_USUÁRIO", $"Usuário deletado: {deletedUsername}");
    }

    public static void LogUserUpdate(string username, string updatedBy, string updatedField)
    {
        LogUserAction(updatedBy, "EDITAR_USUÁRIO", $"Usuário: {username} | Campo: {updatedField}");
    }
}
