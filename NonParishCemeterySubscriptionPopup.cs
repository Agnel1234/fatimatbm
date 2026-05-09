using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace TestFat
{
    public class NonParishCemeterySubscriptionPopup : Form
    {
        private readonly int _cemeteryId;

        private Label  lblInfo;
        private Label  lblYear, lblAmount, lblPaidOn, lblStatus, lblRemarks;
        private NumericUpDown nudYear;
        private TextBox       txtAmount, txtRemarks;
        private DateTimePicker dtpPaidOn;
        private ComboBox      cmbStatus;
        private Button        btnSave, btnClose;
        private DataGridView  grdHistory;

        public NonParishCemeterySubscriptionPopup(int cemeteryId)
        {
            _cemeteryId = cemeteryId;
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.MinimizeBox     = true;
            this.Font            = new Font("Georgia", 11F, FontStyle.Regular);
            this.StartPosition   = FormStartPosition.CenterParent;
            ApplyTheme();
            this.Load += OnLoad;
        }

        private void ApplyTheme()
        {
            this.BackColor = AppTheme.OffWhite;

            var header = new Panel
            {
                Height    = 44,
                BackColor = AppTheme.Navy,
                Location  = new Point(0, 0),
                Width     = this.ClientSize.Width,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            header.Controls.Add(new Label
            {
                Text      = "✝  Outside Parish Cemetery Subscription",
                Font      = AppTheme.HeaderFont,
                ForeColor = AppTheme.Gold,
                AutoSize  = true,
                Location  = new Point(12, 12),
            });
            this.Controls.Add(header);
            header.BringToFront();

            foreach (Control c in this.Controls)
                if (c is Label l) { l.ForeColor = AppTheme.Navy; l.BackColor = Color.Transparent; }

            AppTheme.StyleButtonPrimary(btnSave);
            AppTheme.SetIcon(btnSave, AppTheme.IconSave(), "Save");
            AppTheme.StyleButtonSecondary(btnClose);
            AppTheme.SetIcon(btnClose, AppTheme.IconClose(), "Close", 16);
            AppTheme.StyleGrid(grdHistory);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(560, 560);
            this.Text = "Outside Parish Cemetery Subscription";

            int labelX = 14, fieldX = 160, top = 55;

            lblInfo = new Label { AutoSize = false, Size = new Size(530, 22), Location = new Point(labelX, top),
                Font = new Font("Georgia", 10F, FontStyle.Bold), ForeColor = AppTheme.Navy };
            top += 32;

            lblYear = new Label { Text = "Year:", AutoSize = true, Location = new Point(labelX, top + 4) };
            nudYear = new NumericUpDown { Minimum = 2020, Maximum = 2100, Value = DateTime.Today.Year,
                Location = new Point(fieldX, top), Width = 100, Font = new Font("Georgia", 11F) };
            top += 40;

            lblAmount = new Label { Text = "Amount (₹):", AutoSize = true, Location = new Point(labelX, top + 4) };
            txtAmount = new TextBox { Location = new Point(fieldX, top), Width = 160, Font = new Font("Georgia", 11F) };
            top += 40;

            lblPaidOn = new Label { Text = "Paid On:", AutoSize = true, Location = new Point(labelX, top + 4) };
            dtpPaidOn = new DateTimePicker { Location = new Point(fieldX, top), Width = 220,
                Format = DateTimePickerFormat.Short, Font = new Font("Georgia", 11F) };
            top += 40;

            lblStatus = new Label { Text = "Status:", AutoSize = true, Location = new Point(labelX, top + 4) };
            cmbStatus = new ComboBox { Location = new Point(fieldX, top), Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Georgia", 11F) };
            cmbStatus.Items.AddRange(new object[] { "Paid", "Pending", "Overdue" });
            cmbStatus.SelectedIndex = 0;
            top += 40;

            lblRemarks = new Label { Text = "Remarks:", AutoSize = true, Location = new Point(labelX, top + 4) };
            txtRemarks = new TextBox { Location = new Point(fieldX, top), Width = 340, Height = 46,
                Multiline = true, Font = new Font("Georgia", 10F) };
            top += 56;

            btnSave = new Button { Text = "Save", Location = new Point(fieldX, top), Width = 100, Height = 32,
                Font = new Font("Georgia", 11F, FontStyle.Bold) };
            btnSave.Click += BtnSave_Click;

            btnClose = new Button { Text = "Close", Location = new Point(fieldX + 114, top), Width = 100, Height = 32,
                Font = new Font("Georgia", 11F) };
            btnClose.Click += (s, e) => this.Close();
            top += 46;

            var lblHistory = new Label { Text = "Subscription History", AutoSize = true,
                Location = new Point(labelX, top), Font = new Font("Georgia", 10F, FontStyle.Bold) };
            top += 24;

            grdHistory = new DataGridView
            {
                Location = new Point(labelX, top),
                Size     = new Size(530, 140),
                Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows   = false,
                ColumnHeadersDefaultCellStyle = { Font = new Font("Georgia", 10F, FontStyle.Bold) },
            };

            this.Controls.AddRange(new Control[] {
                lblInfo,
                lblYear, nudYear,
                lblAmount, txtAmount,
                lblPaidOn, dtpPaidOn,
                lblStatus, cmbStatus,
                lblRemarks, txtRemarks,
                btnSave, btnClose,
                lblHistory, grdHistory,
            });

            this.ResumeLayout(false);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            try
            {
                DataTable dtCem = DatabaseHelper.ExecuteStoredProcedure("sp_GetNonParishCemeteryList");
                // Find this entry
                foreach (DataRow row in dtCem.Rows)
                {
                    if (Convert.ToInt32(row["CemeteryId"]) == _cemeteryId)
                    {
                        string code = row["cemeterycode"]?.ToString() ?? "";
                        string name = row["Name"]?.ToString() ?? "";
                        lblInfo.Text = $"Cemetery: {code} — {name}";
                        this.Text    = $"Cemetery Subscription — {code}";
                        break;
                    }
                }
            }
            catch { /* leave default label text */ }

            LoadExistingSubscription((int)nudYear.Value);
            RefreshHistory();
            nudYear.ValueChanged += (s, ev) => LoadExistingSubscription((int)nudYear.Value);
        }

        private void LoadExistingSubscription(int year)
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetNonParishCemeterySubscriptions",
                    new SqlParameter("@nonparish_cemetery_id", _cemeteryId));

                DataRow found = null;
                foreach (DataRow row in dt.Rows)
                {
                    if (Convert.ToInt32(row["subscription_year"]) == year) { found = row; break; }
                }

                if (found != null)
                {
                    txtAmount.Text = found["amount"]?.ToString() ?? "";
                    if (found["payment_date"] != DBNull.Value)
                        dtpPaidOn.Value = Convert.ToDateTime(found["payment_date"]);
                    string status = found["payment_status"]?.ToString() ?? "Pending";
                    int idx = cmbStatus.Items.IndexOf(status);
                    cmbStatus.SelectedIndex = idx >= 0 ? idx : 0;
                    txtRemarks.Text = found["remarks"]?.ToString() ?? "";
                }
                else
                {
                    txtAmount.Text  = "";
                    dtpPaidOn.Value = DateTime.Today;
                    cmbStatus.SelectedIndex = 0;
                    txtRemarks.Text = "";
                }
            }
            catch { txtAmount.Text = ""; }
        }

        private void RefreshHistory()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetNonParishCemeterySubscriptions",
                    new SqlParameter("@nonparish_cemetery_id", _cemeteryId));
                grdHistory.DataSource = dt;
                if (grdHistory.Columns.Contains("subscription_id"))     grdHistory.Columns["subscription_id"].Visible     = false;
                if (grdHistory.Columns.Contains("nonparish_cemetery_id")) grdHistory.Columns["nonparish_cemetery_id"].Visible = false;
                if (grdHistory.Columns.Contains("created_at"))           grdHistory.Columns["created_at"].Visible           = false;
                if (grdHistory.Columns.Contains("modified"))             grdHistory.Columns["modified"].Visible             = false;
            }
            catch { }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtAmount.Text.Trim(), out decimal amount))
            {
                MessageBox.Show("Enter a valid amount.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@nonparish_cemetery_id", _cemeteryId),
                    new SqlParameter("@subscription_year",     (int)nudYear.Value),
                    new SqlParameter("@amount",                amount),
                    new SqlParameter("@payment_date",          (object)dtpPaidOn.Value.Date),
                    new SqlParameter("@payment_status",        cmbStatus.SelectedItem?.ToString() ?? "Paid"),
                    new SqlParameter("@remarks",               (object)txtRemarks.Text.Trim() ?? DBNull.Value),
                };
                DatabaseHelper.ExecuteStoredProcedure("sp_SetNonParishCemeterySubscription", parameters);
                MessageBox.Show("Saved successfully.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
