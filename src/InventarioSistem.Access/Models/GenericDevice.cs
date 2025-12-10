using System;
using InventarioSistem.Core.Entities;

namespace InventarioSistem.Access.Models;

/// <summary>
/// Generic device representation for cross-type queries and listings.
/// Acts as a DTO/view model aggregating all device types.
/// </summary>
public class GenericDevice
{
    public int Id { get; set; }
    public DeviceType Type { get; set; }
    
    // Common fields across all devices
    public string? Patrimonio { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? NumeroSerie { get; set; }
    public string? SerialNumber { get; set; }
    public string? Hostname { get; set; }
    public string? Imei { get; set; }
    public string? Local { get; set; }
    public string? Localizacao { get; set; }
    public string? Responsavel { get; set; }
    public string? Proprietario { get; set; }
    public string? Departamento { get; set; }
    public string? Matricula { get; set; }
    public string? MacAddress { get; set; }
    public DateTime? CreatedAt { get; set; }

    public override string ToString()
    {
        return $"[{Type}] {Patrimonio ?? NumeroSerie ?? SerialNumber ?? Hostname ?? $"ID:{Id}"}";
    }
}
