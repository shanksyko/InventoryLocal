namespace InventarioSistem.Core.Entities;

/// <summary>
/// Roles de usuário para controle de acesso
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Acesso total: criar, editar, excluir, gerenciar usuários
    /// </summary>
    Admin,

    /// <summary>
    /// Acesso somente-leitura: visualizar dados, exportar, mas sem edição
    /// </summary>
    Visualizador
}
