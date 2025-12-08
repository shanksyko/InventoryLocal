using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.Access.Db;
using InventarioSistem.WinForms.Forms;

namespace InventarioSistem.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var factory = new AccessConnectionFactory();
        var store = new AccessInventoryStore(factory);

        try
        {
            var databasePath = AccessDatabaseManager.ResolveActiveDatabasePath();
            factory = new AccessConnectionFactory(databasePath);
            store = new AccessInventoryStore(factory);
            store.EnsureSchemaAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Banco Access não encontrado. Configure um banco pelo botão 'Banco Access...'.\n\n" + ex.Message,
                "Banco não configurado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        Application.Run(new MainForm(store));
    }
}
