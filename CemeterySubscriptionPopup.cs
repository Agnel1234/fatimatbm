using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace TestFat
{
    public partial class CemeterySubscriptionPopup : Form
    {
        private readonly int _familyId;

        private Label lblFamilyInfo;
        private Label lblTitle;
        private NumericUpDown nudYear;
        private Label lblYear;
        private TextBox txtAmount;
        private Label lblAmount;
        private DateTimePicker dtpPaidOn;
        private Label lblPaidOn;
        private ComboBox cmbStatus;
        private Label lblStatus;
        private Button btnSave;
        private Button btnClose;

        public CemeterySubscriptionPopup(int familyId)
        {
            _familyId = familyId;
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.Font = new Font("Georgia", 11F, FontStyle.Regular);
            this.StartPosition = FormStartPosition.CenterParent;
            ApplyTheme();
            Load += CemeterySubscriptionPopup_Load;
        }

        private void ApplyTheme()
        {
            this.BackColor = AppTheme.OffWhite;

            // Navy header bar
            var header = new Panel { Height = 44, BackColor = AppTheme.Navy,
                Location = new Point(0, 0), Width = this.ClientSize.Width };
            var headerLbl = new Label
            {
                Text = "✝  Cemetery Subscription",
                Font = AppTheme.HeaderFont,
                ForeColor = AppTheme.Gold,
                AutoSize = true,
                Location = new Point(12, 12)
            };
            header.Controls.Add(headerLbl);
            this.Controls.Add(header);
            header.BringToFront();

            // Shift existing controls below the header
            int shift = 44;
            foreach (Control c in this.Controls)
            {
                if (c == header) continue;
                c.Location = new Point(c.Left, c.Top + shift);
            }
            this.ClientSize = new Size(this.ClientSize.Width, this.ClientSize.Height + shift);

            // Hide old title label (header bar replaces it)
            lblTitle.Visible = false;

            // Label colours
            foreach (Control c in this.Controls)
                if (c is Label l) { l.ForeColor = AppTheme.Navy; l.BackColor = Color.Transparent; }

            // Buttons
            AppTheme.StyleButtonPrimary(btnSave);
            AppTheme.SetIcon(btnSave, AppTheme.IconSave(), "Save");
            AppTheme.StyleButtonSecondary(btnClose);
            AppTheme.SetIcon(btnClose, AppTheme.IconClose(), "Close", 16);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.ClientSize = new Size(480, 340);
            this.Text = "Cemetery Subscription";
            this.Font = new Font("Georgia", 11F);
            this.BackColor = Color.WhiteSmoke;

            lblTitle = new Label
            {
                Text = "Cemetery Subscription",
                Font = new Font("Georgia", 14F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(12, 10)
            };

            lblFamilyInfo = new Label
            {
                Text = "Family: ",
                AutoSize = true,
                Location = new Point(14, 50)
            };

            lblYear = new Label { Text = "Year:", AutoSize = true, Location = new Point(14, 95) };
            nudYear = new NumericUpDown
            {
                Minimum = 2020,
                Maximum = 2100,
                Value = DateTime.Today.Year,
                Location = new Point(130, 91),
                Width = 100,
                Font = new Font("Georgia", 11F)
            };

            lblAmount = new Label { Text = "Amount (₹):", AutoSize = true, Location = new Point(14, 140) };
            txtAmount = new TextBox
            {
                Location = new Point(130, 136),
                Width = 160,
                Font = new Font("Georgia", 11F)
            };

            lblPaidOn = new Label { Text = "Paid On:", AutoSize = true, Location = new Point(14, 185) };
            dtpPaidOn = new DateTimePicker
            {
                Location = new Point(130, 181),
                Width = 220,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Georgia", 11F)
            };

            lblStatus = new Label { Text = "Status:", AutoSize = true, Location = new Point(14, 230) };
            cmbStatus = new ComboBox
            {
                Location = new Point(130, 226),
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Georgia", 11F)
            };
            cmbStatus.Items.AddRange(new object[] { "Paid", "Pending", "Overdue" });
            cmbStatus.SelectedIndex = 0;

            btnSave = new Button
            {
                Text = "Save",
                Location = new Point(130, 280),
                Width = 100,
                Height = 32,
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                Font = new Font("Georgia", 11F, FontStyle.Bold)
            };
            btnSave.Click += BtnSave_Click;

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(250, 280),
                Width = 100,
                Height = 32,
                Font = new Font("Georgia", 11F)
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblTitle, lblFamilyInfo,
                lblYear, nudYear,
                lblAmount, txtAmount,
                lblPaidOn, dtpPaidOn,
                lblStatus, cmbStatus,
                btnSave, btnClose
            });

            this.ResumeLayout(false);
        }

        private void CemeterySubscriptionPopup_Load(object sender, EventArgs e)
        {
            // Load family display name
            try
            {
                DataTable dtFamily = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyById",
                    new SqlParameter("@family_id", _familyId));
                if (dtFamily.Rows.Count > 0)
                {
                    string code = dtFamily.Rows[0]["family_code"]?.ToString() ?? "";
                    string head = dtFamily.Rows[0]["head_of_family"]?.ToString() ?? "";
                    lblFamilyInfo.Text = $"Family: {code} – {head}";
                    this.Text = $"Cemetery Subscription – {code}";
                }

                // Load existing subscription for the current year if available
                LoadExistingSubscription((int)nudYear.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading family info: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            nudYear.ValueChanged += (s, ev) => LoadExistingSubscription((int)nudYear.Value);
        }

        private void LoadExistingSubscription(int year)
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetCemeterySubscription",
                    new SqlParameter("@family_id", _familyId),
                    new SqlParameter("@subscription_year", year));

                if (dt.Rows.Count > 0)
                {
                    txtAmount.Text = dt.Rows[0]["amount"]?.ToString() ?? "";
                    if (dt.Rows[0]["payment_date"] != DBNull.Value)
                        dtpPaidOn.Value = Convert.ToDateTime(dt.Rows[0]["payment_date"]);
                    string status = dt.Rows[0]["payment_status"]?.ToString() ?? "Pending";
                    int idx = cmbStatus.Items.IndexOf(status);
                    cmbStatus.SelectedIndex = idx >= 0 ? idx : 0;
                }
                else
                {
                    txtAmount.Text = "";
                    dtpPaidOn.Value = DateTime.Today;
                    cmbStatus.SelectedIndex = 0;
                }
            }
            catch
            {
                // No existing record is fine
                txtAmount.Text = "";
            }
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
                    new SqlParameter("@family_id",        _familyId),
                    new SqlParameter("@subscription_year",(int)nudYear.Value),
                    new SqlParameter("@amount",           amount),
                    new SqlParameter("@payment_date",     (object)dtpPaidOn.Value.Date),
                    new SqlParameter("@payment_status",   cmbStatus.SelectedItem?.ToString() ?? "Paid")
                };
                DatabaseHelper.ExecuteStoredProcedure("sp_SetCemeterySubscription", parameters);
                MessageBox.Show("Saved successfully.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadExistingSubscription((int)nudYear.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
