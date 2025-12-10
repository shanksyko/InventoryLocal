using System;

namespace InventarioSistem.Core.Models
{
    public class Device
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;

        // Campos existentes (não remover)
        public string Hostname { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Local { get; set; } = string.Empty;
        public string Responsavel { get; set; } = string.Empty;

        // Campos adicionados (categorias novas)
        public string TipoModelo { get; set; } = string.Empty;
        public string LocalizacaoAtual { get; set; } = string.Empty;
        public string LocalizacaoAnterior { get; set; } = string.Empty;

        public string Numero { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string IPEI { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;

        public string IMEI1 { get; set; } = string.Empty;
        public string IMEI2 { get; set; } = string.Empty;
        public bool Roaming { get; set; }

        public string Usuario { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string Setor { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;

        public string ComputadorVinculado { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
        public DateTime? DataBateria { get; set; }
        public DateTime? DataNobreak { get; set; }
        public DateTime? ProximaVerificacao { get; set; }
    }
}
