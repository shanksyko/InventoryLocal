using InventarioSistem.Access;
using InventarioSistem.Core.Entities;
using InventarioSistem.Core.Utilities;

var databasePath = Path.Combine(AppContext.BaseDirectory, "inventario.accdb");
var connectionFactory = new AccessConnectionFactory(databasePath);
var store = new AccessInventoryStore(connectionFactory);
await store.EnsureSchemaAsync();

Console.WriteLine("Inventário de Dispositivos - CLI");
Console.WriteLine("===============================");

while (true)
{
    Console.WriteLine();
    Console.WriteLine("1 - Cadastrar dispositivo");
    Console.WriteLine("2 - Listar dispositivos");
    Console.WriteLine("3 - Buscar dispositivos");
    Console.WriteLine("4 - Editar dispositivo");
    Console.WriteLine("5 - Excluir dispositivo");
    Console.WriteLine("6 - Relatórios");
    Console.WriteLine("0 - Sair");
    Console.Write("Escolha: ");
    var option = Console.ReadLine();

    switch (option)
    {
        case "1":
            await CreateDeviceAsync(store);
            break;
        case "2":
            await ListDevicesAsync(store);
            break;
        case "3":
            await SearchDevicesAsync(store);
            break;
        case "4":
            await EditDeviceAsync(store);
            break;
        case "5":
            await DeleteDeviceAsync(store);
            break;
        case "6":
            await ShowReportsAsync(store);
            break;
        case "0":
            return;
        default:
            Console.WriteLine("Opção inválida.");
            break;
    }
}

static async Task CreateDeviceAsync(AccessInventoryStore store)
{
    var device = PromptDevice();
    await store.AddAsync(device);
    Console.WriteLine("Cadastro concluído com sucesso!");
}

static async Task ListDevicesAsync(AccessInventoryStore store)
{
    var devices = await store.ListAsync();
    foreach (var device in devices)
    {
        Console.WriteLine(device.ToMultilineString());
        Console.WriteLine(new string('-', 40));
    }
}

static async Task SearchDevicesAsync(AccessInventoryStore store)
{
    Console.Write("Termo de busca: ");
    var term = Console.ReadLine() ?? string.Empty;
    var devices = await store.SearchAsync(term);
    foreach (var device in devices)
    {
        Console.WriteLine(device.ToMultilineString());
        Console.WriteLine(new string('-', 40));
    }
}

static async Task EditDeviceAsync(AccessInventoryStore store)
{
    Console.Write("ID do dispositivo: ");
    if (!int.TryParse(Console.ReadLine(), out var id))
    {
        Console.WriteLine("ID inválido.");
        return;
    }

    var device = await store.GetByIdAsync(id);
    if (device == null)
    {
        Console.WriteLine("Dispositivo não encontrado.");
        return;
    }

    Console.WriteLine("Deixe o campo vazio para manter o valor atual.");
    Console.WriteLine();

    Console.WriteLine($"Patrimônio ({device.Patrimonio}): ");
    var patrimonio = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(patrimonio))
    {
        device.Patrimonio = patrimonio;
    }

    Console.WriteLine($"Marca ({device.Marca}): ");
    var marca = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(marca))
    {
        device.Marca = marca;
    }

    Console.WriteLine($"Modelo ({device.Modelo}): ");
    var modelo = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(modelo))
    {
        device.Modelo = modelo;
    }

    Console.WriteLine($"Número de Série ({device.NumeroSerie}): ");
    var serie = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(serie))
    {
        device.NumeroSerie = serie;
    }

    Console.WriteLine($"IMEI ({device.Imei}): ");
    var imei = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(imei))
    {
        if (!ImeiUtility.IsValid(imei))
        {
            Console.WriteLine("IMEI inválido. Alteração cancelada.");
            return;
        }

        device.Imei = ImeiUtility.Normalize(imei);
    }

    Console.WriteLine($"Responsável ({device.Responsavel}): ");
    var responsavel = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(responsavel))
    {
        device.Responsavel = responsavel;
    }

    Console.WriteLine($"Localização ({device.Localizacao}): ");
    var localizacao = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(localizacao))
    {
        device.Localizacao = localizacao;
    }

    Console.WriteLine($"Observações ({device.Observacoes}): ");
    var obs = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(obs))
    {
        device.Observacoes = obs;
    }

    device.AtualizadoEm = DateTime.UtcNow;
    await store.UpdateAsync(device);
    Console.WriteLine("Dispositivo atualizado!");
}

static async Task DeleteDeviceAsync(AccessInventoryStore store)
{
    Console.Write("ID do dispositivo para exclusão: ");
    if (!int.TryParse(Console.ReadLine(), out var id))
    {
        Console.WriteLine("ID inválido.");
        return;
    }

    await store.DeleteAsync(id);
    Console.WriteLine("Registro excluído.");
}

static async Task ShowReportsAsync(AccessInventoryStore store)
{
    var totals = await store.CountByTypeAsync();
    Console.WriteLine("Total por tipo:");
    foreach (var total in totals)
    {
        Console.WriteLine($" - {total.Key}: {total.Value}");
    }

    Console.WriteLine();
    Console.WriteLine("Dispositivos sem IMEI:");
    var missingImei = await store.DevicesMissingImeiAsync();
    foreach (var device in missingImei)
    {
        Console.WriteLine($" - {device}");
    }

    Console.WriteLine();
    Console.Write("Filtrar por localização (enter para ignorar): ");
    var location = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(location))
    {
        var byLocation = await store.DevicesByLocationAsync(location);
        Console.WriteLine($"Total no local '{location}': {byLocation.Count}");
    }
}

static Device PromptDevice()
{
    Console.WriteLine("Tipos disponíveis: 1-Computador, 2-Tablet, 3-Coletor Android, 4-Celular");
    Console.Write("Tipo: ");
    var tipo = Console.ReadLine();
    Device device = tipo switch
    {
        "1" => new Computer(),
        "2" => new Tablet(),
        "3" => new ColetorAndroid(),
        "4" => new Celular(),
        _ => new Computer()
    };

    Console.Write("Patrimônio: ");
    device.Patrimonio = Console.ReadLine() ?? string.Empty;

    Console.Write("Marca: ");
    device.Marca = Console.ReadLine() ?? string.Empty;

    Console.Write("Modelo: ");
    device.Modelo = Console.ReadLine() ?? string.Empty;

    Console.Write("Número de Série: ");
    device.NumeroSerie = Console.ReadLine() ?? string.Empty;

    if (device is Tablet || device is Celular || device is ColetorAndroid)
    {
        Console.Write("IMEI (opcional): ");
        var imei = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(imei))
        {
            if (!ImeiUtility.IsValid(imei))
            {
                throw new InvalidOperationException("IMEI inválido");
            }

            device.Imei = ImeiUtility.Normalize(imei);
        }
    }

    if (device is Computer computer)
    {
        Console.Write("Sistema operacional: ");
        computer.SistemaOperacional = Console.ReadLine() ?? string.Empty;
        Console.Write("Processador: ");
        computer.Processador = Console.ReadLine() ?? string.Empty;
        Console.Write("Memória RAM (GB): ");
        int.TryParse(Console.ReadLine(), out var ram);
        computer.MemoriaRamGb = ram;
        Console.Write("Armazenamento (GB): ");
        int.TryParse(Console.ReadLine(), out var armazenamento);
        computer.ArmazenamentoGb = armazenamento;
    }

    if (device is Tablet tablet)
    {
        Console.Write("Versão do Android: ");
        tablet.VersaoAndroid = Console.ReadLine() ?? string.Empty;
        Console.Write("Linha telefônica: ");
        tablet.LinhaTelefonica = Console.ReadLine() ?? string.Empty;
        Console.Write("Possui teclado destacável? (s/n): ");
        tablet.PossuiTeclado = Console.ReadLine()?.Trim().ToLowerInvariant() == "s";
    }

    if (device is ColetorAndroid coletor)
    {
        Console.Write("Versão do Android: ");
        coletor.VersaoAndroid = Console.ReadLine() ?? string.Empty;
        Console.Write("Fabricante/Scanner: ");
        coletor.FabricanteScanner = Console.ReadLine() ?? string.Empty;
        Console.Write("Possui base/carregador? (s/n): ");
        coletor.PossuiCarregadorBase = Console.ReadLine()?.Trim().ToLowerInvariant() == "s";
    }

    if (device is Celular celular)
    {
        Console.Write("Linha telefônica: ");
        celular.LinhaTelefonica = Console.ReadLine() ?? string.Empty;
        Console.Write("Versão do Android: ");
        celular.VersaoAndroid = Console.ReadLine() ?? string.Empty;
        Console.Write("Linha corporativa? (s/n): ");
        celular.Corporativo = Console.ReadLine()?.Trim().ToLowerInvariant() == "s";
    }

    Console.Write("Responsável: ");
    device.Responsavel = Console.ReadLine() ?? string.Empty;

    Console.Write("Localização: ");
    device.Localizacao = Console.ReadLine() ?? string.Empty;

    Console.Write("Observações: ");
    device.Observacoes = Console.ReadLine();

    device.AtualizadoEm = DateTime.UtcNow;
    return device;
}
