using System;
using System.Drawing;
using System.Windows.Forms;
using InventarioSistem.Core.Devices;

namespace InventarioSistem.WinForms
{
    public class ColetorEditForm : Form
    {
        private TextBox _txtHost = null!;
        private TextBox _txtSerial = null!;
        private TextBox _txtMac = null!;
        private TextBox _txtIp = null!;
        private TextBox _txtLocal = null!;
        
        // Checkboxes para aplicativos
        private CheckBox _chkAppGwsFg = null!;
        private CheckBox _chkAppGwsRm = null!;
        private CheckBox _chkAppInspection = null!;
        private CheckBox _chkAppCuringTbr = null!;
        private CheckBox _chkAppCuringPcr = null!;
        private CheckBox _chkAppInspectionTbr = null!;
        private CheckBox _chkAppQuimico = null!;
        private CheckBox _chkAppBuildingTbr = null!;
        private CheckBox _chkAppBuildingPcr = null!;
        
        // Checkboxes para sistema operacional
        private CheckBox _chkOsWinCe = null!;
        private CheckBox _chkOsAndroid81 = null!;
        private CheckBox _chkOsAndroid10 = null!;
        
        private Button _btnOk = null!;
        private Button _btnCancelar = null!;

        public ColetorAndroid Result { get; private set; }

        public ColetorEditForm(ColetorAndroid? existing = null)
        {
            Text = existing == null ? "Novo Coletor Android" : "Editar Coletor Android";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(550, 600);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            InitializeLayout();

            if (existing != null)
            {
                _txtHost.Text = existing.Host ?? string.Empty;
                _txtSerial.Text = existing.SerialNumber ?? string.Empty;
                _txtMac.Text = existing.MacAddress ?? string.Empty;
                _txtIp.Text = existing.IpAddress ?? string.Empty;
                _txtLocal.Text = existing.Local ?? string.Empty;
                
                // Aplicativos
                _chkAppGwsFg.Checked = existing.AppGwsFg;
                _chkAppGwsRm.Checked = existing.AppGwsRm;
                _chkAppInspection.Checked = existing.AppInspection;
                _chkAppCuringTbr.Checked = existing.AppCuringTbr;
                _chkAppCuringPcr.Checked = existing.AppCuringPcr;
                _chkAppInspectionTbr.Checked = existing.AppInspectionTbr;
                _chkAppQuimico.Checked = existing.AppQuimico;
                _chkAppBuildingTbr.Checked = existing.AppBuildingTbr;
                _chkAppBuildingPcr.Checked = existing.AppBuildingPcr;
                
                // Sistema Operacional
                _chkOsWinCe.Checked = existing.OsWinCe;
                _chkOsAndroid81.Checked = existing.OsAndroid81;
                _chkOsAndroid10.Checked = existing.OsAndroid10;

                Result = new ColetorAndroid
                {
                    Id = existing.Id,
                    Host = existing.Host ?? string.Empty,
                    SerialNumber = existing.SerialNumber ?? string.Empty,
                    MacAddress = existing.MacAddress ?? string.Empty,
                    IpAddress = existing.IpAddress ?? string.Empty,
                    Local = existing.Local ?? string.Empty,
                    AppGwsFg = existing.AppGwsFg,
                    AppGwsRm = existing.AppGwsRm,
                    AppInspection = existing.AppInspection,
                    AppCuringTbr = existing.AppCuringTbr,
                    AppCuringPcr = existing.AppCuringPcr,
                    AppInspectionTbr = existing.AppInspectionTbr,
                    AppQuimico = existing.AppQuimico,
                    AppBuildingTbr = existing.AppBuildingTbr,
                    AppBuildingPcr = existing.AppBuildingPcr,
                    OsWinCe = existing.OsWinCe,
                    OsAndroid81 = existing.OsAndroid81,
                    OsAndroid10 = existing.OsAndroid10
                };
            }
            else
            {
                Result = new ColetorAndroid
                {
                    Host = string.Empty,
                    SerialNumber = string.Empty,
                    MacAddress = string.Empty,
                    IpAddress = string.Empty,
                    Local = string.Empty
                };
            }
        }

        private void InitializeLayout()
        {
            var lblHost = new Label { Text = "Host:", AutoSize = true, Location = new Point(10, 15) };
            _txtHost = new TextBox { Location = new Point(130, 12), Width = 380 };

            var lblSerial = new Label { Text = "NS:", AutoSize = true, Location = new Point(10, 45) };
            _txtSerial = new TextBox { Location = new Point(130, 42), Width = 380 };

            var lblMac = new Label { Text = "MAC:", AutoSize = true, Location = new Point(10, 75) };
            _txtMac = new TextBox { Location = new Point(130, 72), Width = 380 };

            var lblIp = new Label { Text = "IP:", AutoSize = true, Location = new Point(10, 105) };
            _txtIp = new TextBox { Location = new Point(130, 102), Width = 380 };

            var lblLocal = new Label { Text = "Local:", AutoSize = true, Location = new Point(10, 135) };
            _txtLocal = new TextBox { Location = new Point(130, 132), Width = 380 };

            // GroupBox para aplicativos
            var grpApps = new GroupBox 
            { 
                Text = "Aplicativos Instalados", 
                Location = new Point(10, 170), 
                Size = new Size(510, 230) 
            };

            _chkAppGwsFg = new CheckBox { Text = "GWS FG", Location = new Point(10, 25), AutoSize = true };
            _chkAppGwsRm = new CheckBox { Text = "GWS RM", Location = new Point(10, 50), AutoSize = true };
            _chkAppInspection = new CheckBox { Text = "Inspection", Location = new Point(10, 75), AutoSize = true };
            _chkAppCuringTbr = new CheckBox { Text = "Curing TBR", Location = new Point(10, 100), AutoSize = true };
            _chkAppCuringPcr = new CheckBox { Text = "Curing PCR", Location = new Point(10, 125), AutoSize = true };
            _chkAppInspectionTbr = new CheckBox { Text = "Inspection TBR", Location = new Point(10, 150), AutoSize = true };
            _chkAppQuimico = new CheckBox { Text = "Quimico", Location = new Point(10, 175), AutoSize = true };
            _chkAppBuildingTbr = new CheckBox { Text = "Building TBR", Location = new Point(10, 200), AutoSize = true };
            _chkAppBuildingPcr = new CheckBox { Text = "Building PCR", Location = new Point(250, 25), AutoSize = true };

            grpApps.Controls.AddRange(new Control[]
            {
                _chkAppGwsFg, _chkAppGwsRm, _chkAppInspection, _chkAppCuringTbr,
                _chkAppCuringPcr, _chkAppInspectionTbr, _chkAppQuimico,
                _chkAppBuildingTbr, _chkAppBuildingPcr
            });

            // GroupBox para sistema operacional
            var grpOS = new GroupBox 
            { 
                Text = "Sistema Operacional", 
                Location = new Point(10, 410), 
                Size = new Size(510, 100) 
            };

            _chkOsWinCe = new CheckBox { Text = "WinCE", Location = new Point(10, 25), AutoSize = true };
            _chkOsAndroid81 = new CheckBox { Text = "Android 8.1", Location = new Point(10, 50), AutoSize = true };
            _chkOsAndroid10 = new CheckBox { Text = "Android 10.0", Location = new Point(10, 75), AutoSize = true };

            grpOS.Controls.AddRange(new Control[]
            {
                _chkOsWinCe, _chkOsAndroid81, _chkOsAndroid10
            });

            _btnOk = new Button { Text = "OK", Location = new Point(350, 520), DialogResult = DialogResult.OK };
            _btnCancelar = new Button { Text = "Cancelar", Location = new Point(435, 520), DialogResult = DialogResult.Cancel };

            _btnOk.Click += (_, _) =>
            {
                Result.Host = _txtHost.Text.Trim();
                Result.SerialNumber = _txtSerial.Text.Trim();
                Result.MacAddress = _txtMac.Text.Trim();
                Result.IpAddress = _txtIp.Text.Trim();
                Result.Local = _txtLocal.Text.Trim();
                
                // Aplicativos
                Result.AppGwsFg = _chkAppGwsFg.Checked;
                Result.AppGwsRm = _chkAppGwsRm.Checked;
                Result.AppInspection = _chkAppInspection.Checked;
                Result.AppCuringTbr = _chkAppCuringTbr.Checked;
                Result.AppCuringPcr = _chkAppCuringPcr.Checked;
                Result.AppInspectionTbr = _chkAppInspectionTbr.Checked;
                Result.AppQuimico = _chkAppQuimico.Checked;
                Result.AppBuildingTbr = _chkAppBuildingTbr.Checked;
                Result.AppBuildingPcr = _chkAppBuildingPcr.Checked;
                
                // Sistema Operacional
                Result.OsWinCe = _chkOsWinCe.Checked;
                Result.OsAndroid81 = _chkOsAndroid81.Checked;
                Result.OsAndroid10 = _chkOsAndroid10.Checked;
                
                DialogResult = DialogResult.OK;
                Close();
            };

            _btnCancelar.Click += (_, _) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.AddRange(new Control[]
            {
                lblHost, _txtHost,
                lblSerial, _txtSerial,
                lblMac, _txtMac,
                lblIp, _txtIp,
                lblLocal, _txtLocal,
                grpApps, grpOS,
                _btnOk, _btnCancelar
            });
        }
    }
}
