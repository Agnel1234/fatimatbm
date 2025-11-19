using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace TestFat
{
    public partial class SubscriptionPopup : Form
    {
        private readonly int _familyId;
        private int _year;
        private readonly Button[] _monthButtons = new Button[12];

        // UI controls (created by InitializeComponent)
        private Label lblFamily;
        private Label lblTitle;
        private NumericUpDown nudYear;
        private FlowLayoutPanel monthsPanel;
        private Panel paymentPanel;
        private Label lblAmount;
        private TextBox txtAmount;
        private Label lblPaidOn;
        private DateTimePicker dtpPaidOn;
        private Button btnSaveMonth;
        private Button btnClose;

        public SubscriptionPopup(int familyId, int? year = null)
        {
            _familyId = familyId;
            _year = year ?? DateTime.Today.Year;

            // Build the UI and wire events
            InitializeComponent();

            // Match other forms' aesthetics
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.BackColor = Color.WhiteSmoke;
            this.Font = new Font("Georgia", 11F, FontStyle.Regular);
            this.ShowIcon = true;
            this.StartPosition = FormStartPosition.CenterParent;

            Load += SubscriptionPopup_Load;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form
            this.ClientSize = new Size(620, 485);
            this.Text = "Subscription";
            this.Font = new Font("Georgia", 11F);
            this.BackColor = Color.WhiteSmoke;

            // Title label
            lblTitle = new Label
            {
                Text = "",
                Font = new Font("Georgia", 16F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(12, 8)
            };

            // Family label (populated on load)
            lblFamily = new Label
            {
                Text = "Family: ",
                AutoSize = true,
                Location = new Point(14, 46)
            };

            // Year controls
            Label lblYear = new Label
            {
                Text = "Year:",
                AutoSize = true,
                Location = new Point(460, 46)
            };
            nudYear = new NumericUpDown
            {
                Minimum = 2000,
                Maximum = 2100,
                Value = _year,
                Location = new Point(500, 42),
                Width = 90
            };
            nudYear.ValueChanged += NudYear_ValueChanged;

            // Months panel
            monthsPanel = new FlowLayoutPanel
            {
                Location = new Point(14, 84),
                Size = new Size(592, 260),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                WrapContents = true
            };

            // Payment editor panel
            paymentPanel = new Panel
            {
                Location = new Point(14, 356),
                Size = new Size(592, 80),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblAmount = new Label { Text = "Amount:", Location = new Point(8, 12), AutoSize = true };
            txtAmount = new TextBox { Location = new Point(90, 8), Width = 140 };
            lblPaidOn = new Label { Text = "Paid On:", Location = new Point(250, 12), AutoSize = true };
            dtpPaidOn = new DateTimePicker { Format = DateTimePickerFormat.Short, Location = new Point(320, 8), Width = 140 };

            btnSaveMonth = new Button
            {
                Text = "Save",
                Location = new Point(480, 6),
                Width = 90,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.RoyalBlue,
                ForeColor = Color.White
            };
            btnSaveMonth.FlatAppearance.BorderColor = Color.DarkGray;
            btnSaveMonth.Click += BtnSaveMonth_Click;

            paymentPanel.Controls.Add(lblAmount);
            paymentPanel.Controls.Add(txtAmount);
            paymentPanel.Controls.Add(lblPaidOn);
            paymentPanel.Controls.Add(dtpPaidOn);
            paymentPanel.Controls.Add(btnSaveMonth);

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(516, 440),
                Size = new Size(90, 30),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.FlatAppearance.BorderColor = Color.DarkGray;
            btnClose.Click += (s, e) => this.Close();

            // Add to form
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblFamily);
            this.Controls.Add(lblYear);
            this.Controls.Add(nudYear);
            this.Controls.Add(monthsPanel);
            this.Controls.Add(paymentPanel);
            this.Controls.Add(btnClose);

            // Build month buttons now (they rely on monthsPanel)
            BuildMonthButtons();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void SubscriptionPopup_Load(object sender, EventArgs e)
        {
            try { lblFamily.Text = GetFamilyDisplayName(_familyId); } catch { /* ignore */ }
            nudYear.Value = _year;
            LoadSubscriptionYear(_year);
        }

        private void NudYear_ValueChanged(object sender, EventArgs e)
        {
            _year = (int)nudYear.Value;
            LoadSubscriptionYear(_year);
        }

        private void BuildMonthButtons()
        {
            // Ensure monthsPanel exists (InitializeComponent ensures it)
            if (monthsPanel == null) return;

            monthsPanel.Controls.Clear();
            string[] monthNames = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames.Take(12).ToArray();

            for (int i = 0; i < 12; i++)
            {
                var b = new Button
                {
                    Tag = i + 1, // month number
                    Text = $"{i + 1}. {monthNames[i]}",
                    Width = 180,
                    Height = 44,
                    Margin = new Padding(6),
                    BackColor = Color.LightGray,
                    FlatStyle = FlatStyle.Flat
                };
                b.FlatAppearance.BorderColor = Color.DarkGray;
                b.Click += MonthButton_Click;
                _monthButtons[i] = b;
                monthsPanel.Controls.Add(b);
            }
        }

        private void MonthButton_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            int month = (int)btn.Tag;
            // Populate payment panel with month data
            var row = GetMonthDataRow(_year, month);
            if (row != null)
            {
                decimal amount = row.Field<decimal?>($"{GetMonthColumnPrefix(month)}_amount") ?? 0m;
                DateTime? paidOn = row.Field<DateTime?>($"{GetMonthColumnPrefix(month)}_paid_date");
                txtAmount.Text = amount == 0m ? "" : amount.ToString("0.##");
                dtpPaidOn.Value = paidOn ?? DateTime.Today;
            }
            else
            {
                txtAmount.Text = "";
                dtpPaidOn.Value = DateTime.Today;
            }

            // Store selected month in Save button Tag for later
            btnSaveMonth.Tag = month;
        }

        private void BtnSaveMonth_Click(object sender, EventArgs e)
        {
            if (btnSaveMonth.Tag == null) return;
            int month = (int)btnSaveMonth.Tag;

            if (!decimal.TryParse(txtAmount.Text.Trim(), out decimal amount))
            {
                MessageBox.Show("Enter valid amount.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime paidOn = dtpPaidOn.Value.Date;
            var parameters = new[]
            {
                new SqlParameter("@family_id", _familyId),
                new SqlParameter("@subscription_year", _year),
                new SqlParameter("@month", month),
                new SqlParameter("@amount", amount),
                new SqlParameter("@paid_date", (object)paidOn ?? DBNull.Value),
                new SqlParameter("@status", "Paid")
            };

            try
            {
                DatabaseHelper.ExecuteStoredProcedure("sp_SetFamilySubscriptionMonth", parameters);
                MessageBox.Show("Saved successfully.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadSubscriptionYear(_year); // refresh UI
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSubscriptionYear(int year)
        {
            // Reset UI
            foreach (var b in _monthButtons) if (b != null) b.BackColor = Color.LightGray;

            var parameters = new[] {
                new SqlParameter("@family_id", _familyId),
                new SqlParameter("@subscription_year", year)
            };

            DataTable dt = null;
            try
            {
                dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilySubscriptionYear", parameters);
            }
            catch
            {
                dt = null;
            }

            if (dt == null || dt.Rows.Count == 0)
            {
                // no row yet - treat as all pending
                return;
            }

            var row = dt.Rows[0];
            for (int m = 1; m <= 12; m++)
            {
                var btn = _monthButtons[m - 1];
                string prefix = GetMonthColumnPrefix(m);
                var status = row.Table.Columns.Contains(prefix + "_status") ? row[prefix + "_status"] as string : null;
                var paidDateObj = row.Table.Columns.Contains(prefix + "_paid_date") ? row[prefix + "_paid_date"] : DBNull.Value;
                var amountObj = row.Table.Columns.Contains(prefix + "_amount") ? row[prefix + "_amount"] : DBNull.Value;

                if (status != null && status.Equals("Paid", StringComparison.OrdinalIgnoreCase) && paidDateObj != DBNull.Value)
                {
                    btn.BackColor = Color.LightGreen;
                    btn.Text = $"{m}. {CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[m - 1]} • Paid";
                }
                else if (amountObj != DBNull.Value)
                {
                    btn.BackColor = Color.Khaki;
                    btn.Text = $"{m}. {CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[m - 1]} • Due";
                }
                else
                {
                    btn.BackColor = Color.LightGray;
                    btn.Text = $"{m}. {CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[m - 1]}";
                }
            }
        }

        private DataRow GetMonthDataRow(int year, int month)
        {
            var parameters = new[] {
                new SqlParameter("@family_id", _familyId),
                new SqlParameter("@subscription_year", year)
            };
            var dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilySubscriptionYear", parameters);
            if (dt == null || dt.Rows.Count == 0) return null;
            return dt.Rows[0];
        }

        private static string GetMonthColumnPrefix(int month)
        {
            switch (month)
            {
                case 1: return "jan";
                case 2: return "feb";
                case 3: return "mar";
                case 4: return "apr";
                case 5: return "may";
                case 6: return "jun";
                case 7: return "jul";
                case 8: return "aug";
                case 9: return "sep";
                case 10: return "oct";
                case 11: return "nov";
                case 12: return "dec";
                default: throw new ArgumentOutOfRangeException(nameof(month));
            }
        }

        private string GetFamilyDisplayName(int familyId)
        {
            try
            {
                var dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyDetailsById", new SqlParameter("@family_id", familyId));
                if (dt != null && dt.Rows.Count > 0)
                {
                    var r = dt.Rows[0];
                    return $"Family: {r["family_code"]} - {r["head_of_family"]}";
                }
            }
            catch { }
            return $"FamilyId: {familyId}";
        }
    }
}