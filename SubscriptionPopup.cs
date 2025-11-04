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

        public SubscriptionPopup(int familyId, int? year = null)
        {
            _familyId = familyId;
            _year = year ?? DateTime.Today.Year;
            InitializeComponent();

            // Match other forms' aesthetics
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.BackColor = Color.WhiteSmoke;
            this.Font = new Font("Georgia", 11F, FontStyle.Regular);
            this.ShowIcon = true;

            // Keep dialog centered over parent form (Form1 should be centered on screen)
            this.StartPosition = FormStartPosition.CenterParent;

            Load += SubscriptionPopup_Load;
        }

        private void SubscriptionPopup_Load(object sender, EventArgs e)
        {
            lblFamily.Text = GetFamilyDisplayName(_familyId);
            nudYear.Value = _year;
            BuildMonthButtons();
            LoadSubscriptionYear(_year);
        }

        // UI controls (simple code-based layout so designer file not required)
        private Label lblTitle;
        private Label lblFamily;
        private NumericUpDown nudYear;
        private FlowLayoutPanel monthsPanel;
        private Panel paymentPanel;
        private Label lblAmount;
        private TextBox txtAmount;
        private Label lblPaidOn;
        private DateTimePicker dtpPaidOn;
        private Button btnSaveMonth;
        private Button btnClose;
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SubscriptionPopup));
            this.SuspendLayout();
            // 
            // SubscriptionPopup
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SubscriptionPopup";
            this.ResumeLayout(false);

        }

        private void BuildMonthButtons()
        {
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
                // show existing values
                decimal amount = row.Field<decimal?>($"{GetMonthColumnPrefix(month)}_amount") ?? 0m;
                DateTime? paidOn = row.Field<DateTime?>($"{GetMonthColumnPrefix(month)}_paid_date");
                string status = row.Field<string>($"{GetMonthColumnPrefix(month)}_status");

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
            // call stored proc sp_SetFamilySubscriptionMonth
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
            foreach (var b in _monthButtons) b.BackColor = Color.LightGray;

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
                else if (amountObj != DBNull.Value && amountObj != DBNull.Value)
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