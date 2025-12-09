using System.Collections.Generic;
using InventarioSistem.Core.Models;

namespace InventarioSistem.Core.Helpers
{
    public static class DeviceFieldMap
    {
        public static readonly Dictionary<string, string[]> FieldsByType =
            new()
            {
                {
                    DeviceType.Impressora,
                    new[]
                    {
                        "Nome", "TipoModelo", "SerialNumber", "LocalizacaoAtual", "LocalizacaoAnterior"
                    }
                },
                {
                    DeviceType.Dect,
                    new[]
                    {
                        "Responsavel", "IPEI", "MacAddress", "Numero", "Local"
                    }
                },
                {
                    DeviceType.Cisco,
                    new[]
                    {
                        "Responsavel", "MacAddress", "Numero", "Local"
                    }
                },
                {
                    DeviceType.Televisor,
                    new[]
                    {
                        "Modelo", "SerialNumber", "Local"
                    }
                },
                {
                    DeviceType.RelogioPonto,
                    new[]
                    {
                        "Modelo", "SerialNumber", "Local", "IP",
                        "DataBateria", "DataNobreak", "ProximaVerificacao"
                    }
                },
                {
                    DeviceType.Monitor,
                    new[]
                    {
                        "Modelo", "SerialNumber", "Local",
                        "Responsavel", "ComputadorVinculado"
                    }
                },
                {
                    DeviceType.Nobreak,
                    new[]
                    {
                        "Hostname", "Local", "IP", "Modelo", "Status", "SerialNumber"
                    }
                },
                {
                    DeviceType.Celular,
                    new[]
                    {
                        "Hostname","IMEI1","IMEI2","Modelo","Numero",
                        "Roaming","Usuario","Matricula","Cargo","Setor","Email","Senha"
                    }
                }
            };
    }
}
