namespace InventarioSistem.Core.Devices;

public class Computer
{
    public int Id { get; set; }

    public string Host { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Proprietario { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string Matricula { get; set; } = string.Empty;

    /// <summary>
    /// Descrição dos monitores conectados (ex.: "Dell 24 + LG 27").
    /// </summary>
    public string Monitores { get; set; } = string.Empty;

    public override string ToString()
        => $"[PC] Id={Id}, Host={Host}, NS={SerialNumber}, Prop={Proprietario}, Dept={Departamento}, Mat={Matricula}, Monitores={Monitores}";
}
