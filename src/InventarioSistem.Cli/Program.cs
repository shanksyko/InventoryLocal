using System;
using System.IO;
using System.Linq;
using InventarioSistem.Access;
using InventarioSistem.Access.Db;
using InventarioSistem.Access.Schema;
using InventarioSistem.Core.Devices;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Inicializa factory + store conforme a assinatura atual
var factory = new AccessConnectionFactory();
var store = new AccessInventoryStore(factory);

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
        Console.WriteLine("Dica: use a opção 9 para selecionar um arquivo .accdb válido.");
        Pausar();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Erro inesperado: " + ex.Message);
        Pausar();
    }
}

static void CadastrarComputador(AccessInventoryStore store)
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

    Console.WriteLine("Computador cadastrado!");
    Pausar();
}

static void CadastrarTablet(AccessInventoryStore store)
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

    Console.WriteLine("Tablet cadastrado!");
    Pausar();
}

static void CadastrarColetor(AccessInventoryStore store)
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

    Console.WriteLine("Coletor cadastrado!");
    Pausar();
}

static void CadastrarCelular(AccessInventoryStore store)
{
    Console.Clear();
    Console.WriteLine("=== Novo Celular ===");
    Console.Write("Modelo: ");
    var modelo = Console.ReadLine() ?? string.Empty;

    Console.Write("Número: ");
    var numero = Console.ReadLine() ?? string.Empty;

    Console.Write("Proprietário: ");
    var prop = Console.ReadLine() ?? string.Empty;

    Console.Write("IMEIs (separados por ';'): ");
    var imeiInput = Console.ReadLine() ?? string.Empty;
    var imeis = imeiInput
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .ToList();

    var cel = new Celular
    {
        Modelo = modelo,
        Numero = numero,
        Proprietario = prop,
        Imeis = imeis
    };

    store.AddCelular(cel);

    Console.WriteLine("Celular cadastrado!");
    Pausar();
}

static void ListarTudo(AccessInventoryStore store)
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
    Console.WriteLine("=== Selecionar banco Access existente ===");
    Console.WriteLine("Crie antes um arquivo .accdb pelo Access ou use um já existente.");
    Console.WriteLine();
    Console.Write("Informe o caminho completo do arquivo .accdb: ");

    var path = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
    {
        Console.WriteLine("Caminho inválido ou arquivo inexistente.");
        Pausar();
        return;
    }

    // Define o banco ativo
    AccessDatabaseManager.SetActiveDatabasePath(path);
    Console.WriteLine($"Banco definido: {path}");

    bool hasAllTables;
    try
    {
        hasAllTables = AccessSchemaManager.HasAllRequiredTables();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Erro ao verificar estrutura do banco: " + ex.Message);
        Pausar();
        return;
    }

    if (!hasAllTables)
    {
        Console.WriteLine();
        Console.WriteLine("Este banco não possui todas as tabelas padrão do InventarioSistem.");
        Console.Write("Deseja criá-las agora? (S/N): ");
        var respCreate = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

        if (respCreate == "S")
        {
            try
            {
                AccessSchemaManager.EnsureRequiredTables();
                Console.WriteLine("Tabelas criadas/ajustadas com sucesso.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao criar tabelas: " + ex.Message);
                Pausar();
                return;
            }
        }
    }

    Console.WriteLine();
    Console.Write("Deseja exibir um resumo deste banco agora? (S/N): ");
    var resp = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

    if (resp == "S")
    {
        try
        {
            var summary = AccessDatabaseManager.GetDatabaseSummary(path);
            Console.WriteLine();
            Console.WriteLine(summary);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao obter resumo do banco: " + ex.Message);
        }
    }

    Pausar();
}

static void Pausar()
{
    Console.WriteLine("\nPressione ENTER para continuar...");
    Console.ReadLine();
}
