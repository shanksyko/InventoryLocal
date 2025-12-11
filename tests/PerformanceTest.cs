using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using InventarioSistem.Access;
using InventarioSistem.Access.Config;
using InventarioSistem.Access.Export;
using InventarioSistem.Core.Devices;
using InventarioSistem.Core.Entities;
using ComputerDevice = InventarioSistem.Core.Devices.Computer;
using TabletDevice = InventarioSistem.Core.Devices.Tablet;
using ColetorDevice = InventarioSistem.Core.Devices.ColetorAndroid;
using CelularDevice = InventarioSistem.Core.Devices.Celular;
using ImpressoraDevice = InventarioSistem.Core.Devices.Impressora;
using DectDevice = InventarioSistem.Core.Devices.DectPhone;
using CiscoDevice = InventarioSistem.Core.Devices.CiscoPhone;
using TelevisorDevice = InventarioSistem.Core.Devices.Televisor;
using RelogioDevice = InventarioSistem.Core.Devices.RelogioPonto;
using MonitorDevice = InventarioSistem.Core.Devices.Monitor;
using NobreakDevice = InventarioSistem.Core.Devices.Nobreak;

namespace InventarioSistem.Tests;

public class PerformanceTest
{
    private static SqlServerConnectionFactory? _factory;
    private static SqlServerInventoryStore? _store;
    private static SqlServerUserStore? _userStore;

    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== TESTE DE PERFORMANCE - INVENTÁRIO ===");
        Console.WriteLine();

        // Carregar configuração
        var config = SqlServerConfig.Load();
        if (string.IsNullOrWhiteSpace(config.ConnectionString))
        {
            Console.WriteLine("❌ Connection string não configurada!");
            Console.WriteLine("Configure em: sqlserver.config.json");
            return;
        }

        _factory = new SqlServerConnectionFactory(config.ConnectionString);
        _store = new SqlServerInventoryStore(_factory);
        _userStore = new SqlServerUserStore(_factory);

        Console.WriteLine("✅ Conectado ao banco de dados");
        Console.WriteLine();

        // Menu de testes
        while (true)
        {
            Console.WriteLine("Escolha um teste:");
            Console.WriteLine("1 - Inserir 100 computadores");
            Console.WriteLine("2 - Inserir 100 tablets");
            Console.WriteLine("3 - Inserir 100 celulares");
            Console.WriteLine("4 - Inserir 100 monitores");
            Console.WriteLine("5 - Inserir 100 nobreaks");
            Console.WriteLine("6 - Inserir 50 de cada tipo (550 total)");
            Console.WriteLine("7 - Inserir 500 de cada tipo (5500 total) - TESTE PESADO");
            Console.WriteLine("8 - Teste de leitura (listar todos)");
            Console.WriteLine("9 - Limpar todos os dados");
            Console.WriteLine("E - Teste de exportação XLSX");
            Console.WriteLine("A - Criar usuário admin");
            Console.WriteLine("0 - Sair");
            Console.WriteLine();
            Console.Write("Opção: ");

            var opcao = Console.ReadLine();
            Console.WriteLine();

            switch (opcao?.ToUpper())
            {
                case "1":
                    await InserirComputadores(100);
                    break;
                case "2":
                    await InserirTablets(100);
                    break;
                case "3":
                    await InserirCelulares(100);
                    break;
                case "4":
                    await InserirMonitores(100);
                    break;
                case "5":
                    await InserirNobreaks(100);
                    break;
                case "6":
                    await InserirTodosTipos();
                    break;
                case "7":
                    await InserirTodosTipos(500); // 5500 total
                    break;
                case "8":
                    await TestarLeitura();
                    break;
                case "9":
                    await LimparDados();
                    break;
                case "E":
                    await TestarExportacao();
                    break;
                case "A":
                    await CriarUsuarioAdmin();
                    break;
                case "0":
                    Console.WriteLine("Encerrando...");
                    return;
                default:
                    Console.WriteLine("❌ Opção inválida!");
                    break;
            }

            Console.WriteLine();
            if (!Console.IsInputRedirected)
            {
                Console.WriteLine("Pressione qualquer tecla para continuar...");
                Console.ReadKey();
                Console.Clear();
            }
        }
    }

    private static async Task InserirComputadores(int quantidade)
    {
        Console.WriteLine($"Inserindo {quantidade} computadores...");
        var sw = Stopwatch.StartNew();

        for (int i = 1; i <= quantidade; i++)
        {
            var computer = new ComputerDevice
            {
                Host = $"DESKTOP-TEST-{i:D4}",
                SerialNumber = $"SN-COMP-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                Proprietario = $"Usuário {i}",
                Departamento = $"Departamento {(i % 10) + 1}",
                Matricula = $"MAT{i:D5}"
            };

            _store!.AddComputer(computer);

            if (i % 10 == 0)
            {
                Console.Write($"\rProgresso: {i}/{quantidade} ({i * 100 / quantidade}%)");
            }
        }

        sw.Stop();
        Console.WriteLine();
        Console.WriteLine($"✅ {quantidade} computadores inseridos em {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"   Média: {sw.ElapsedMilliseconds / quantidade}ms por item");
    }

    private static async Task InserirTablets(int quantidade)
    {
        Console.WriteLine($"Inserindo {quantidade} tablets...");
        var sw = Stopwatch.StartNew();

        for (int i = 1; i <= quantidade; i++)
        {
            var tablet = new TabletDevice
            {
                Host = $"TABLET-TEST-{i:D4}",
                SerialNumber = $"SN-TAB-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                Local = $"Local {(i % 5) + 1}",
                Responsavel = $"Responsável {i}",
                Imeis = new List<string> { $"35{i:D13}", $"36{i:D13}" }
            };

            _store!.AddTablet(tablet);

            if (i % 10 == 0)
            {
                Console.Write($"\rProgresso: {i}/{quantidade} ({i * 100 / quantidade}%)");
            }
        }

        sw.Stop();
        Console.WriteLine();
        Console.WriteLine($"✅ {quantidade} tablets inseridos em {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"   Média: {sw.ElapsedMilliseconds / quantidade}ms por item");
    }

    private static async Task InserirCelulares(int quantidade)
    {
        Console.WriteLine($"Inserindo {quantidade} celulares...");
        var sw = Stopwatch.StartNew();

        for (int i = 1; i <= quantidade; i++)
        {
            var celular = new CelularDevice
            {
                CellName = $"CELL-{i:D4}",
                Modelo = $"Modelo-{(i % 10) + 1}",
                Numero = $"(11) 9{i:D8}",
                Usuario = $"Usuário {i}",
                Setor = $"Setor {(i % 5) + 1}",
                Imei1 = $"35{i:D13}",
                Imei2 = $"36{i:D13}"
            };

            _store!.AddCelular(celular);

            if (i % 10 == 0)
            {
                Console.Write($"\rProgresso: {i}/{quantidade} ({i * 100 / quantidade}%)");
            }
        }

        sw.Stop();
        Console.WriteLine();
        Console.WriteLine($"✅ {quantidade} celulares inseridos em {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"   Média: {sw.ElapsedMilliseconds / quantidade}ms por item");
    }

    private static async Task InserirMonitores(int quantidade)
    {
        Console.WriteLine($"Inserindo {quantidade} monitores...");
        var sw = Stopwatch.StartNew();

        for (int i = 1; i <= quantidade; i++)
        {
            var monitor = new MonitorDevice
            {
                Modelo = $"Monitor-{(i % 5) + 1}",
                SerialNumber = $"SN-MON-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                Local = $"Sala {(i % 20) + 1}",
                Responsavel = $"Usuário {i}",
                ComputadorVinculado = $"DESKTOP-{i:D4}"
            };

            _store!.AddMonitor(monitor);

            if (i % 10 == 0)
            {
                Console.Write($"\rProgresso: {i}/{quantidade} ({i * 100 / quantidade}%)");
            }
        }

        sw.Stop();
        Console.WriteLine();
        Console.WriteLine($"✅ {quantidade} monitores inseridos em {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"   Média: {sw.ElapsedMilliseconds / quantidade}ms por item");
    }

    private static async Task InserirNobreaks(int quantidade)
    {
        Console.WriteLine($"Inserindo {quantidade} nobreaks...");
        var sw = Stopwatch.StartNew();

        for (int i = 1; i <= quantidade; i++)
        {
            var nobreak = new Nobreak
            {
                Hostname = $"NOBREAK-{i:D4}",
                Local = $"Sala {(i % 20) + 1}",
                IpAddress = $"192.168.{(i / 254) + 1}.{(i % 254) + 1}",
                Modelo = $"APC-{(i % 3) + 1}",
                Status = i % 3 == 0 ? "Online" : "Offline",
                SerialNumber = $"SN-NOB-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
            };

            _store!.AddNobreak(nobreak);

            if (i % 10 == 0)
            {
                Console.Write($"\rProgresso: {i}/{quantidade} ({i * 100 / quantidade}%)");
            }
        }

        sw.Stop();
        Console.WriteLine();
        Console.WriteLine($"✅ {quantidade} nobreaks inseridos em {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"   Média: {sw.ElapsedMilliseconds / quantidade}ms por item");
    }

    private static async Task InserirTodosTipos(int quantidadePorTipo = 50)
    {
        Console.WriteLine($"Inserindo {quantidadePorTipo} dispositivos de cada tipo...");
        var sw = Stopwatch.StartNew();

        await InserirComputadores(quantidadePorTipo);
        await InserirTablets(quantidadePorTipo);
        await InserirCelulares(quantidadePorTipo);
        await InserirMonitores(quantidadePorTipo);
        await InserirNobreaks(quantidadePorTipo);

        // Adicionar outros tipos
        Console.WriteLine($"Inserindo {quantidadePorTipo} coletores...");
        for (int i = 1; i <= quantidadePorTipo; i++)
        {
            var coletor = new ColetorDevice
            {
                Host = $"COLETOR-{i:D4}",
                SerialNumber = $"SN-COL-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                MacAddress = $"AA:BB:CC:DD:{i / 256:X2}:{i % 256:X2}",
                IpAddress = $"192.168.{(i / 254) + 50}.{(i % 254) + 1}",
                Local = $"Local {i}"
            };
            _store!.AddColetor(coletor);
            if (i % 50 == 0) Console.Write($"\rProgresso: {i}/{quantidadePorTipo}");
        }

        Console.WriteLine($"Inserindo {quantidadePorTipo} impressoras...");
        for (int i = 1; i <= quantidadePorTipo; i++)
        {
            var impressora = new ImpressoraDevice
            {
                Nome = $"PRINTER-{i:D4}",
                TipoModelo = $"HP-{(i % 5) + 1}",
                SerialNumber = $"SN-IMP-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                LocalAtual = $"Andar {(i % 3) + 1}",
                LocalAnterior = ""
            };
            _store!.AddImpressora(impressora);
            if (i % 50 == 0) Console.Write($"\rProgresso: {i}/{quantidadePorTipo}");
        }

        Console.WriteLine($"\nInserindo {quantidadePorTipo} DECTs...");
        for (int i = 1; i <= quantidadePorTipo; i++)
        {
            var dect = new DectPhone
            {
                Responsavel = $"Usuário {i}",
                Ipei = $"IPEI-{i:D10}",
                MacAddress = $"AA:BB:CC:DD:{i / 256:X2}:{i % 256:X2}",
                Numero = $"400{i % 100:D2}",
                Local = $"Sala {i}"
            };
            _store!.AddDect(dect);
            if (i % 50 == 0) Console.Write($"\rProgresso: {i}/{quantidadePorTipo}");
        }

        Console.WriteLine($"\nInserindo {quantidadePorTipo} telefones Cisco...");
        for (int i = 1; i <= quantidadePorTipo; i++)
        {
            var cisco = new CiscoPhone
            {
                Responsavel = $"Usuário {i}",
                MacAddress = $"00:1B:2C:3D:{i / 256:X2}:{i % 256:X2}",
                Numero = $"300{i % 100:D2}",
                Local = $"Sala {i}"
            };
            _store!.AddTelefoneCisco(cisco);
            if (i % 50 == 0) Console.Write($"\rProgresso: {i}/{quantidadePorTipo}");
        }

        Console.WriteLine($"Inserindo {quantidadePorTipo} televisores...");
        for (int i = 1; i <= quantidadePorTipo; i++)
        {
            var tv = new TelevisorDevice
            {
                Local = $"Sala {i}",
                Modelo = $"Samsung-{(i % 3) + 1}",
                SerialNumber = $"SN-TV-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
            };
            _store!.AddTelevisor(tv);
            if (i % 50 == 0) Console.Write($"\rProgresso: {i}/{quantidadePorTipo}");
        }

        Console.WriteLine($"Inserindo {quantidadePorTipo} relógios de ponto...");
        for (int i = 1; i <= quantidadePorTipo; i++)
        {
            var relogio = new RelogioDevice
            {
                Local = $"Entrada {(i % 20) + 1}",
                Ip = $"192.168.10.{(i % 254) + 1}",
                Modelo = $"Henry-{(i % 2) + 1}",
                SerialNumber = $"SN-REL-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
            };
            _store!.AddRelogioPonto(relogio);
            if (i % 50 == 0) Console.Write($"\rProgresso: {i}/{quantidadePorTipo}");
        }

        sw.Stop();
        var total = quantidadePorTipo * 11;
        Console.WriteLine();
        Console.WriteLine($"✅ {total} dispositivos inseridos em {sw.Elapsed.TotalSeconds:F2} segundos");
        Console.WriteLine($"   Média: {(sw.ElapsedMilliseconds / total):F1}ms por item");
    }

    private static async Task TestarLeitura()
    {
        Console.WriteLine("Testando leitura de todos os dispositivos...");
        var sw = Stopwatch.StartNew();

        var devices = await _store!.ListAsync();
        var count = devices.Count;

        sw.Stop();
        Console.WriteLine($"✅ {count} dispositivos lidos em {sw.ElapsedMilliseconds}ms");

        // Contar por tipo
        Console.WriteLine();
        Console.WriteLine("Resumo por tipo:");
        var counts = await _store.CountByTypeAsync();
        foreach (var item in counts)
        {
            Console.WriteLine($"  {item.Key}: {item.Value}");
        }
    }

    private static async Task LimparDados()
    {
        Console.Write("⚠️  Tem certeza que deseja limpar TODOS os dados? (S/N): ");
        var confirmacao = Console.ReadLine()?.ToUpper();

        if (confirmacao != "S")
        {
            Console.WriteLine("Operação cancelada.");
            return;
        }

        Console.WriteLine("Limpando dados...");
        var sw = Stopwatch.StartNew();

        // Listar todos e deletar
        var devices = await _store!.ListAsync();
        int deleted = 0;

        foreach (var device in devices)
        {
            await _store.DeleteAsync((int)device.Id);
            deleted++;

            if (deleted % 10 == 0)
            {
                Console.Write($"\rDeletando: {deleted}/{devices.Count}");
            }
        }

        sw.Stop();
        Console.WriteLine();
        Console.WriteLine($"✅ {deleted} dispositivos removidos em {sw.Elapsed.TotalSeconds:F2} segundos");
    }

    private static async Task CriarUsuarioAdmin()
    {
        Console.WriteLine("Criando usuário admin...");
        
        try
        {
            // Verificar se já existe
            var existingUser = _userStore!.GetUser("admin");
            if (existingUser.HasValue)
            {
                Console.WriteLine("⚠️  Usuário admin já existe no banco de dados!");
                Console.WriteLine($"   ID: {existingUser.Value.Id}");
                Console.WriteLine($"   Nome: {existingUser.Value.FullName}");
                Console.WriteLine($"   Ativo: {existingUser.Value.IsActive}");
                return;
            }

            // Criar usuário admin
            await _userStore!.CreateUserAsync(
                username: "admin",
                password: "L9l337643k#$",
                fullName: "Administrador",
                isActive: true,
                role: "Admin"
            );

            Console.WriteLine("✅ Usuário admin criado com sucesso!");
            Console.WriteLine();
            Console.WriteLine("Credenciais:");
            Console.WriteLine("  Usuário: admin");
            Console.WriteLine("  Senha: L9l337643k#$");
            Console.WriteLine();
            Console.WriteLine("⚠️  Recomenda-se alterar a senha após o primeiro login!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao criar usuário: {ex.Message}");
        }
    }

    private static async Task TestarExportacao()
    {
        Console.WriteLine("Teste de Exportação XLSX");
        Console.WriteLine("=========================");
        Console.WriteLine();

        // Criar diretório de saída
        var outputDir = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "",
            "exports"
        );
        Directory.CreateDirectory(outputDir);

        Console.WriteLine($"Arquivos serão salvos em: {outputDir}");
        Console.WriteLine();

        var deviceTypes = new[]
        {
            (DeviceType.Computer, "Computadores"),
            (DeviceType.Tablet, "Tablets"),
            (DeviceType.ColetorAndroid, "Coletores Android"),
            (DeviceType.Celular, "Celulares"),
            (DeviceType.Impressora, "Impressoras"),
            (DeviceType.Dect, "DECT Phones"),
            (DeviceType.TelefoneCisco, "Telefones Cisco"),
            (DeviceType.Televisor, "Televisores"),
            (DeviceType.RelogioPonto, "Relógios de Ponto"),
            (DeviceType.Monitor, "Monitores"),
            (DeviceType.Nobreak, "Nobreaks")
        };

        var successCount = 0;
        var failureCount = 0;

        foreach (var (type, name) in deviceTypes)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                var filename = $"inventario-{name.ToLower().Replace(" ", "-")}-{timestamp}.xlsx";
                var filepath = Path.Combine(outputDir, filename);

                Console.Write($"Exportando {name,-25} ... ");

                var sw = Stopwatch.StartNew();
                XlsxExporterHelper.ExportToFile(_store!, type, filepath);
                sw.Stop();

                var fileInfo = new FileInfo(filepath);
                Console.WriteLine($"✅ OK ({fileInfo.Length / 1024} KB em {sw.ElapsedMilliseconds}ms)");

                successCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRO: {ex.Message}");
                failureCount++;
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Resumo: {successCount} sucesso, {failureCount} erro");
        Console.WriteLine();
        Console.WriteLine($"Arquivos salvos em: {outputDir}");
    }
}
