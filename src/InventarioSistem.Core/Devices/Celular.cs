namespace InventarioSistem.Core.Devices;

public class Celular
{
    public int Id { get; set; }

    // Schema solicitado:
    // CellName IMEI1 IMEI2 Modelo Número Roaming Usuário Matricula Cargo Setor E-mail Senha
    public string CellName { get; set; } = string.Empty;
    public string Imei1 { get; set; } = string.Empty;
    public string Imei2 { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Roaming { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string Matricula { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string Setor { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;

    // Aliases para compatibilidade com código antigo (se houver)
    public string Hostname
    {
        get => CellName;
        set => CellName = value;
    }

    public string Proprietario
    {
        get => Usuario;
        set => Usuario = value;
    }

    public override string ToString()
        => $"{CellName} - {Modelo} - {Numero}";
}
