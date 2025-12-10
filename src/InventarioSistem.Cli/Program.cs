using System;
using System.IO;
using System.Linq;
using InventarioSistem.Access;
using InventarioSistem.Access.Config;
using InventarioSistem.Access.Db;
using InventarioSistem.Access.Schema;
using InventarioSistem.Core.Devices;
using InventarioSistem.Core.Logging;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Inicializa SQL Server infrastructure
var config = new SqlServerConfig();
var factory = new SqlServerConnectionFactory(config.ConnectionString);
var store = new SqlServerInventoryStore(factory);

try
{
    var connectionString = config.ConnectionString;
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        Console.WriteLine($"Conectado ao SQL Server");
        InventoryLogger.Info("CLI", $"SQL Server configurado");
        
        // Ensure schema
        SqlServerSchemaManager.EnsureRequiredTables(factory);
    }
    else
    {
        Console.WriteLine("Nenhuma conexão SQL Server configurada.");
        Console.WriteLine("Edite sqlserver.config.json para configurar.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Aviso: erro ao conectar SQL Server: {ex.Message}");
}

while (true)
{
    Console.Clear();
    Console.WriteLine("=== InventarioSistem (CLI) ===");
    Console.WriteLine("1 - Cadastrar Computador");
    Console.WriteLine("2 - Cadastrar Tablet");
    Console.WriteLine("3 - Cadastrar Coletor Android");
    Console.WriteLine("4 - Cadastrar Celular");
    Console.WriteLine("5 - Listar tudo");
    Console.WriteLine("9 - Selecionar banco Access existente");
    Console.WriteLine("0 - Sair");
    Console.Write("Opção: ");

    var opcao = Console.ReadLine();

    try
    {
        switch (opcao)
        {
            case "1":
                CadastrarComputador(store);
                break;
            case "2":
                CadastrarTablet(store);
                break;
            case "3":
                CadastrarColetor(store);
                break;
            case "4":
                CadastrarCelular(store);
                break;
            case "5":
                ListarTudo(store);
                break;
            case "9":
                SelecionarBancoAccessCli();
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Opção inválida.");
                Pausar();
                break;
        }
    }
    catch (FileNotFoundException ex)
    {
        Console.WriteLine("Erro de banco: " + ex.Message);
        Console.WriteLine("Dica: use a opção 9 para configurar conexão SQL Server.");
        Pausar();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Erro inesperado: " + ex.Message);
        Pausar();
    }
}

static void CadastrarComputador(SqlServerInventoryStore store)
{
    Console.Clear();
    Console.WriteLine("=== Novo Computador ===");
    Console.Write("Host: ");
    var host = Console.ReadLine() ?? string.Empty;

    Console.Write("N/S: ");
    var ns = Console.ReadLine() ?? string.Empty;

    Console.Write("Proprietário: ");
    var prop = Console.ReadLine() ?? string.Empty;

    Console.Write("Departamento: ");
    var dept = Console.ReadLine() ?? string.Empty;

    Console.Write("Matrícula: ");
    var mat = Console.ReadLine() ?? string.Empty;

    var comp = new Computer
    {
        Host = host,
        SerialNumber = ns,
        Proprietario = prop,
        Departamento = dept,
        Matricula = mat
    };

    store.AddComputer(comp);

    InventoryLogger.Info("CLI",
        $"Computador cadastrado via CLI: Host='{host}', NS='{ns}', Proprietario='{prop}', Departamento='{dept}', Matricula='{mat}'");

    Console.WriteLine("Computador cadastrado!");
    Pausar();
}

static void CadastrarTablet(SqlServerInventoryStore store)
{
    Console.Clear();
    Console.WriteLine("=== Novo Tablet ===");
    Console.Write("Host: ");
    var host = Console.ReadLine() ?? string.Empty;

    Console.Write("N/S: ");
    var ns = Console.ReadLine() ?? string.Empty;

    Console.Write("Local: ");
    var local = Console.ReadLine() ?? string.Empty;

    Console.Write("Responsável: ");
    var resp = Console.ReadLine() ?? string.Empty;

    Console.Write("IMEIs (separados por ';'): ");
    var imeiInput = Console.ReadLine() ?? string.Empty;
    var imeis = imeiInput
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .ToList();

    var tablet = new Tablet
    {
        Host = host,
        SerialNumber = ns,
        Local = local,
        Responsavel = resp,
        Imeis = imeis
    };

    store.AddTablet(tablet);

    InventoryLogger.Info("CLI",
        $"Tablet cadastrado via CLI: Host='{host}', NS='{ns}', Local='{local}', Responsavel='{resp}', IMEIs='{string.Join(";", imeis)}'");

    Console.WriteLine("Tablet cadastrado!");
    Pausar();
}

static void CadastrarColetor(SqlServerInventoryStore store)
{
    Console.Clear();
    Console.WriteLine("=== Novo Coletor Android ===");
    Console.Write("Host: ");
    var host = Console.ReadLine() ?? string.Empty;

    Console.Write("NS: ");
    var ns = Console.ReadLine() ?? string.Empty;

    Console.Write("MAC: ");
    var mac = Console.ReadLine() ?? string.Empty;

    Console.Write("IP: ");
    var ip = Console.ReadLine() ?? string.Empty;

    Console.Write("Local: ");
    var local = Console.ReadLine() ?? string.Empty;

    var coletor = new ColetorAndroid
    {
        Host = host,
        SerialNumber = ns,
        MacAddress = mac,
        IpAddress = ip,
        Local = local
    };

    store.AddColetor(coletor);

    InventoryLogger.Info("CLI",
        $"Coletor cadastrado via CLI: Host='{host}', NS='{ns}', MAC='{mac}', IP='{ip}', Local='{local}'");

    Console.WriteLine("Coletor cadastrado!");
    Pausar();
}

static void CadastrarCelular(SqlServerInventoryStore store)
{
    Console.Clear();
    Console.WriteLine("=== Novo Celular ===");
    Console.Write("Hostname: ");
    var host = Console.ReadLine() ?? string.Empty;

    Console.Write("Modelo: ");
    var modelo = Console.ReadLine() ?? string.Empty;

    Console.Write("Número: ");
    var numero = Console.ReadLine() ?? string.Empty;

    Console.Write("Proprietário: ");
    var prop = Console.ReadLine() ?? string.Empty;

    Console.Write("IMEI1: ");
    var imei1 = Console.ReadLine() ?? string.Empty;

    Console.Write("IMEI2: ");
    var imei2 = Console.ReadLine() ?? string.Empty;

    var cel = new Celular
    {
        Hostname = host,
        Modelo = modelo,
        Numero = numero,
        Proprietario = prop,
        Imei1 = imei1,
        Imei2 = imei2
    };

    store.AddCelular(cel);

    InventoryLogger.Info("CLI",
        $"Celular cadastrado via CLI: Host='{host}', Modelo='{modelo}', Numero='{numero}', Proprietario='{prop}', IMEI1='{imei1}', IMEI2='{imei2}'");

    Console.WriteLine("Celular cadastrado!");
    Pausar();
}

static void ListarTudo(SqlServerInventoryStore store)
{
    Console.Clear();
    Console.WriteLine("=== Inventário Geral ===");

    Console.WriteLine("\nComputadores:");
    foreach (var c in store.GetAllComputers())
        Console.WriteLine(c);

    Console.WriteLine("\nTablets:");
    foreach (var t in store.GetAllTablets())
        Console.WriteLine(t);

    Console.WriteLine("\nColetores Android:");
    foreach (var col in store.GetAllColetores())
        Console.WriteLine(col);

    Console.WriteLine("\nCelulares:");
    foreach (var cel in store.GetAllCelulares())
        Console.WriteLine(cel);

    Pausar();
}

static void SelecionarBancoAccessCli()
{
    Console.Clear();
    Console.WriteLine("=== Configurar SQL Server ===");
    Console.WriteLine("Informe a connection string SQL Server.");
    Console.WriteLine("Exemplo: Server=localhost;Database=InventarioLocal;Integrated Security=true;");
    Console.WriteLine();
    Console.Write("Connection String: ");

    var connStr = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(connStr))
    {
        Console.WriteLine("Connection string inválida.");
        Pausar();
        return;
    }

    // Salva configuração
    var config = new SqlServerConfig();
    config.ConnectionString = connStr;
    config.Save();

    Console.WriteLine("✔ Configuração salva em sqlserver.config.json");
    InventoryLogger.Info("CLI", $"SQL Server configurado via CLI");

    // Testa conexão
    var factory = new SqlServerConnectionFactory(config.ConnectionString);
    try
    {
        using var conn = factory.CreateConnection();
        conn.Open();
        Console.WriteLine("✔ Conexão SQL Server testada com sucesso.");
        conn.Close();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✖ Erro ao conectar: {ex.Message}");
        Pausar();
        return;
    }

    // Verifica tabelas
    Console.WriteLine();
    Console.Write("Deseja criar/verificar tabelas agora? (S/N): ");
    var respCreate = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

    if (respCreate == "S")
    {
        try
        {
            SqlServerSchemaManager.EnsureRequiredTables(factory);
            Console.WriteLine("✔ Tabelas criadas/verificadas com sucesso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✖ Erro ao criar tabelas: {ex.Message}");
            Pausar();
            return;
        }
    }

    Pausar();
}

static void Pausar()
{
    Console.WriteLine("\nPressione ENTER para continuar...");
    Console.ReadLine();
}
