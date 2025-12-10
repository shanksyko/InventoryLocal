using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using LegacyDevices = InventarioSistem.Core.Devices;

namespace InventarioSistem.WinForms
{
    public partial class MainForm
    {
        private List<LegacyDevices.Computer> _computersCache = new();
        private List<LegacyDevices.Tablet> _tabletsCache = new();
        private List<LegacyDevices.ColetorAndroid> _coletoresCache = new();
        private List<LegacyDevices.Celular> _celularesCache = new();
        private List<LegacyDevices.Impressora> _impressorasCache = new();
        private List<LegacyDevices.DectPhone> _dectsCache = new();
        private List<LegacyDevices.CiscoPhone> _ciscoCache = new();
        private List<LegacyDevices.Televisor> _tvsCache = new();
        private List<LegacyDevices.RelogioPonto> _relogiosCache = new();
        private List<LegacyDevices.Monitor> _monitoresCache = new();
        private List<LegacyDevices.Nobreak> _nobreaksCache = new();

        private TextBox _txtComputersFilter = null!;
        private TextBox _txtTabletsFilter = null!;
        private TextBox _txtColetoresFilter = null!;
        private TextBox _txtCelularesFilter = null!;
        private TextBox _txtImpressorasFilter = null!;
        private TextBox _txtDectsFilter = null!;
        private TextBox _txtCiscoFilter = null!;
        private TextBox _txtTvsFilter = null!;
        private TextBox _txtRelogiosFilter = null!;
        private TextBox _txtMonitoresFilter = null!;
        private TextBox _txtNobreaksFilter = null!;

        // Método otimizado para busca sem alocar string toda vez
        private bool FastContains(string? text, string? searchTerm)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchTerm))
                return false;
            return text.IndexOf(searchTerm, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ApplyComputersFilter()
        {
            if (_gridComputadores == null)
                return;

            var view = _computersCache ?? new List<LegacyDevices.Computer>();
            var term = _txtComputersFilter?.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(term))
            {
                view = view
                    .Where(c =>
                        FastContains(c.Host, term) ||
                        FastContains(c.SerialNumber, term) ||
                        FastContains(c.Proprietario, term) ||
                        FastContains(c.Departamento, term) ||
                        FastContains(c.Matricula, term))
                    .ToList();
            }

            _gridComputadores.DataSource = new BindingList<LegacyDevices.Computer>(ToList(view));
            HideIdColumn(_gridComputadores);
        }

        private void ApplyTabletsFilter()
        {
            if (_gridTablets == null)
                return;

            var view = _tabletsCache ?? new List<LegacyDevices.Tablet>();
            var term = _txtTabletsFilter?.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(term))
            {
                view = view
                    .Where(t =>
                        FastContains(t.Host, term) ||
                        FastContains(t.SerialNumber, term) ||
                        FastContains(t.Local, term) ||
                        FastContains(t.Responsavel, term) ||
                        (t.Imeis != null && t.Imeis.Any(i => FastContains(i, term))))
                    .ToList();
            }

            _gridTablets.DataSource = new BindingList<LegacyDevices.Tablet>(ToList(view));
            HideIdColumn(_gridTablets);
        }

        private void ApplyColetoresFilter()
        {
            if (_gridColetores == null)
                return;

            var view = _coletoresCache ?? new List<LegacyDevices.ColetorAndroid>();
            var term = _txtColetoresFilter?.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(term))
            {
                view = view
                    .Where(c =>
                        FastContains(c.Host, term) ||
                        FastContains(c.SerialNumber, term) ||
                        FastContains(c.MacAddress, term) ||
                        FastContains(c.IpAddress, term) ||
                        FastContains(c.Local, term))
                    .ToList();
            }

            _gridColetores.DataSource = new BindingList<LegacyDevices.ColetorAndroid>(ToList(view));
            HideIdColumn(_gridColetores);
        }

        private void ApplyCelularesFilter()
        {
            if (_gridCelulares == null)
                return;

            var view = _celularesCache ?? new List<LegacyDevices.Celular>();
            var term = _txtCelularesFilter?.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(term))
            {
                view = view
                    .Where(c =>
                        FastContains(c.CellName, term) ||
                        FastContains(c.Imei1, term) ||
                        FastContains(c.Imei2, term) ||
                        FastContains(c.Modelo, term) ||
                        FastContains(c.Numero, term) ||
                        FastContains(c.Usuario, term) ||
                        FastContains(c.Setor, term))
                    .ToList();
            }

            _gridCelulares.DataSource = new BindingList<LegacyDevices.Celular>(ToList(view));
            HideIdColumn(_gridCelulares);
        }

        private void ApplyImpressorasFilter()
        {
            if (_gridImpressoras == null)
                return;

            var view = _impressorasCache ?? new List<LegacyDevices.Impressora>();
            var term = _txtImpressorasFilter?.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var normalized = term.ToLowerInvariant();
                view = view
                    .Where(i =>
                        (!string.IsNullOrEmpty(i.Nome) && i.Nome.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(i.TipoModelo) && i.TipoModelo.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(i.SerialNumber) && i.SerialNumber.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(i.LocalAtual) && i.LocalAtual.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(i.LocalAnterior) && i.LocalAnterior.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(i.Responsavel) && i.Responsavel.ToLowerInvariant().Contains(normalized)))
                    .ToList();
            }

            _gridImpressoras.DataSource = new BindingList<LegacyDevices.Impressora>(ToList(view));
            HideIdColumn(_gridImpressoras);
        }

        private void ApplyDectsFilter()
        {
            if (_gridDects == null)
                return;

            var view = _dectsCache ?? new List<LegacyDevices.DectPhone>();
            var term = _txtDectsFilter?.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var normalized = term.ToLowerInvariant();
                view = view
                    .Where(d =>
                        (!string.IsNullOrEmpty(d.Responsavel) && d.Responsavel.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(d.Ipei) && d.Ipei.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(d.MacAddress) && d.MacAddress.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(d.Numero) && d.Numero.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(d.Local) && d.Local.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(d.Modelo) && d.Modelo.ToLowerInvariant().Contains(normalized)))
                    .ToList();
            }

            _gridDects.DataSource = new BindingList<LegacyDevices.DectPhone>(ToList(view));
            HideIdColumn(_gridDects);
        }

        private void ApplyCiscoFilter()
        {
            if (_gridTelefonesCisco == null)
                return;

            var view = _ciscoCache ?? new List<LegacyDevices.CiscoPhone>();
            var term = _txtCiscoFilter?.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var normalized = term.ToLowerInvariant();
                view = view
                    .Where(c =>
                        (!string.IsNullOrEmpty(c.Responsavel) && c.Responsavel.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.MacAddress) && c.MacAddress.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.Numero) && c.Numero.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.Local) && c.Local.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.IpAddress) && c.IpAddress.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.Serial) && c.Serial.ToLowerInvariant().Contains(normalized)))
                    .ToList();
            }

            _gridTelefonesCisco.DataSource = new BindingList<LegacyDevices.CiscoPhone>(ToList(view));
            HideIdColumn(_gridTelefonesCisco);
        }

        private void ApplyTvsFilter()
        {
            if (_gridTelevisores == null)
                return;

            var view = _tvsCache ?? new List<LegacyDevices.Televisor>();
            var term = _txtTvsFilter?.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var normalized = term.ToLowerInvariant();
                view = view
                    .Where(t =>
                        (!string.IsNullOrEmpty(t.Modelo) && t.Modelo.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(t.SerialNumber) && t.SerialNumber.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(t.Local) && t.Local.ToLowerInvariant().Contains(normalized)))
                    .ToList();
            }

            _gridTelevisores.DataSource = new BindingList<LegacyDevices.Televisor>(ToList(view));
            HideIdColumn(_gridTelevisores);
        }

        private void ApplyRelogiosFilter()
        {
            if (_gridRelogiosPonto == null)
                return;

            var view = _relogiosCache ?? new List<LegacyDevices.RelogioPonto>();
            var term = _txtRelogiosFilter?.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var normalized = term.ToLowerInvariant();
                view = view
                    .Where(r =>
                        (!string.IsNullOrEmpty(r.Modelo) && r.Modelo.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(r.SerialNumber) && r.SerialNumber.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(r.Local) && r.Local.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(r.Ip) && r.Ip.ToLowerInvariant().Contains(normalized)))
                    .ToList();
            }

            _gridRelogiosPonto.DataSource = new BindingList<LegacyDevices.RelogioPonto>(ToList(view));
            HideIdColumn(_gridRelogiosPonto);
        }

        private void ApplyMonitoresFilter()
        {
            if (_gridMonitores == null)
                return;

            var view = _monitoresCache ?? new List<LegacyDevices.Monitor>();
            var term = _txtMonitoresFilter?.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var normalized = term.ToLowerInvariant();
                view = view
                    .Where(m =>
                        (!string.IsNullOrEmpty(m.Modelo) && m.Modelo.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(m.SerialNumber) && m.SerialNumber.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(m.Local) && m.Local.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(m.Responsavel) && m.Responsavel.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(m.ComputadorVinculado) && m.ComputadorVinculado.ToLowerInvariant().Contains(normalized)))
                    .ToList();
            }

            _gridMonitores.DataSource = new BindingList<LegacyDevices.Monitor>(ToList(view));
            HideIdColumn(_gridMonitores);
        }

        private void ApplyNobreaksFilter()
        {
            if (_gridNobreaks == null)
                return;

            var view = _nobreaksCache ?? new List<LegacyDevices.Nobreak>();
            var term = _txtNobreaksFilter?.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var normalized = term.ToLowerInvariant();
                view = view
                    .Where(n =>
                        (!string.IsNullOrEmpty(n.Hostname) && n.Hostname.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(n.Local) && n.Local.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(n.IpAddress) && n.IpAddress.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(n.Modelo) && n.Modelo.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(n.Status) && n.Status.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(n.SerialNumber) && n.SerialNumber.ToLowerInvariant().Contains(normalized)))
                    .ToList();
            }

            _gridNobreaks.DataSource = new BindingList<LegacyDevices.Nobreak>(ToList(view));
            HideIdColumn(_gridNobreaks);
        }
    }
}
