using System.Windows.Forms;
using InventarioSistem.Access;
using InventarioSistem.WinForms.Forms;

namespace InventarioSistem.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var databasePath = Path.Combine(AppContext.BaseDirectory, "inventario.accdb");
        var factory = new AccessConnectionFactory(databasePath);
        var store = new AccessInventoryStore(factory);
        store.EnsureSchemaAsync().GetAwaiter().GetResult();

        Application.Run(new MainForm(store));
    }
}
