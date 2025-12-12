using System;
using Microsoft.Data.SqlClient;

namespace InventarioSistem.Access;

/// <summary>
/// Verifica disponibilidade de LocalDB e oferece diagnostics/soluções
/// </summary>
public static class LocalDbChecker
{
    /// <summary>
    /// Verifica se LocalDB está instalado e disponível
    /// </summary>
    public static bool IsAvailable(out string? errorMessage)
    {
        errorMessage = null;
        try
        {
            var connString = $"Data Source=(LocalDB)\\mssqllocaldb;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=5;";
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                conn.Close();
            }
            return true;
        }
        catch (SqlException ex) when (ex.Message.Contains("Local Database Runtime") || ex.Message.Contains("Unable to locate"))
        {
            errorMessage = "LocalDB não encontrado ou não está instalado.";
            return false;
        }
        catch (SqlException ex) when (ex.Number == 40 || ex.Message.Contains("not accessible"))
        {
            errorMessage = "LocalDB não está respondendo. Verifique se SQL Server Express está instalado.";
            return false;
        }
        catch (Exception ex)
        {
            errorMessage = $"Erro ao verificar LocalDB: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// Retorna sugestões para resolver problema
    /// </summary>
    public static string GetSolutions()
    {
        return @"
📋 SOLUÇÕES:

1️⃣  Instalar LocalDB:
   • Baixar SQL Server Express com LocalDB:
     https://www.microsoft.com/pt-br/sql-server/sql-server-express
   • Selecionar opção ""Local Database Runtime""

2️⃣  Usar SQL Server na rede:
   • Na tela de configuração, escolha ""SQL Server (Servidor/Rede)""
   • Informe hostname e credenciais

3️⃣  Habilitar LocalDB (já instalado):
   • Abrir Painel de Controle > Programas > Programas e Recursos
   • Encontrar ""Microsoft SQL Server Express""
   • Clicar em ""Alterar""
   • Marcar ""Local Database Runtime""

❓ Precisa de ajuda? Contacte o suporte.
";
    }
}
