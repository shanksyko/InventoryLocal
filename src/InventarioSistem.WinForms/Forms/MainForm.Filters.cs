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

        private TextBox _txtComputersFilter = null!;
        private TextBox _txtTabletsFilter = null!;
        private TextBox _txtColetoresFilter = null!;
        private TextBox _txtCelularesFilter = null!;
        private TextBox _txtImpressorasFilter = null!;
        private TextBox _txtDectsFilter = null!;
        private TextBox _txtCiscoFilter = null!;
        private TextBox _txtTvsFilter = null!;
        private TextBox _txtRelogiosFilter = null!;

        private void ApplyComputersFilter()
        {
            if (_gridComputadores == null)
                return;

            var view = _computersCache ?? new List<LegacyDevices.Computer>();
            var term = _txtComputersFilter?.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var normalized = term.ToLowerInvariant();
                view = view
                    .Where(c =>
                        (!string.IsNullOrEmpty(c.Host) && c.Host.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.SerialNumber) && c.SerialNumber.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.Proprietario) && c.Proprietario.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.Departamento) && c.Departamento.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.Matricula) && c.Matricula.ToLowerInvariant().Contains(normalized)))
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
                var normalized = term.ToLowerInvariant();
                view = view
                    .Where(t =>
                        (!string.IsNullOrEmpty(t.Host) && t.Host.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(t.SerialNumber) && t.SerialNumber.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(t.Local) && t.Local.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(t.Responsavel) && t.Responsavel.ToLowerInvariant().Contains(normalized)) ||
                        (t.Imeis != null && t.Imeis.Any(i => i != null && i.ToLowerInvariant().Contains(normalized))))
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
                var normalized = term.ToLowerInvariant();
                view = view
                    .Where(c =>
                        (!string.IsNullOrEmpty(c.Host) && c.Host.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.SerialNumber) && c.SerialNumber.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.MacAddress) && c.MacAddress.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.IpAddress) && c.IpAddress.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.Local) && c.Local.ToLowerInvariant().Contains(normalized)))
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
                var normalized = term.ToLowerInvariant();
                view = view
                    .Where(c =>
                        (!string.IsNullOrEmpty(c.CellName) && c.CellName.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.Imei1) && c.Imei1.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.Imei2) && c.Imei2.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.Modelo) && c.Modelo.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.Numero) && c.Numero.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.Usuario) && c.Usuario.ToLowerInvariant().Contains(normalized)) ||
                        (!string.IsNullOrEmpty(c.Setor) && c.Setor.ToLowerInvariant().Contains(normalized)))
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
    }
}
