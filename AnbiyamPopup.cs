using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestFat
{
    public partial class AnbiyamPopup : Form
    {
        int selectedAnbiyamId = 0;
        public AnbiyamPopup(int selected = 0)
        {
            InitializeComponent();

            txtPhone.KeyPress += NumberOnlyTextBox_KeyPress;

            this.StartPosition   = FormStartPosition.CenterParent;
            this.MaximizeBox     = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            selectedAnbiyamId    = selected;

            for (int i = 1; i <= 8; i++)
                cmbAnbiyamZone.Items.Add(i.ToString());
            cmbAnbiyamZone.SelectedIndex = 0;

            if (selectedAnbiyamId != 0)
            {
                btnSave.Text = "Update Anbiyam";
                LoadAnbiyamData(selectedAnbiyamId);
            }
            else
            {
                btnSave.Text = "Register Anbiyam";
            }

            ApplyTheme();
        }

        private void ApplyTheme()
        {
            bool isEdit = selectedAnbiyamId != 0;
            string headerTitle = isEdit ? "✝  Edit Anbiyam" : "✝  Register Anbiyam";

            this.BackColor = AppTheme.OffWhite;
            this.Font      = AppTheme.BodyFont;
            this.Text      = isEdit ? "Edit Anbiyam" : "Register Anbiyam";

            // ── Navy header bar ──
            var header = new Panel
            {
                Height    = 44,
                Dock      = DockStyle.Top,
                BackColor = AppTheme.Navy,
            };
            header.Controls.Add(new Label
            {
                Text      = headerTitle,
                Font      = AppTheme.HeaderFont,
                ForeColor = AppTheme.Gold,
                AutoSize  = true,
                Location  = new Point(12, 12),
            });
            this.Controls.Add(header);
            header.BringToFront();

            // ── Content card ──
            panel1.BackColor   = Color.White;
            panel1.BorderStyle = BorderStyle.None;

            // Style all labels in the form navy
            foreach (Control c in panel1.Controls)
            {
                if (c is Label l)
                {
                    l.ForeColor = AppTheme.Navy;
                    l.Font      = AppTheme.BoldSmall;
                    l.BackColor = Color.Transparent;
                }
                else if (c is TextBox tb)
                {
                    tb.BorderStyle = BorderStyle.FixedSingle;
                    tb.BackColor   = AppTheme.OffWhite;
                    tb.ForeColor   = AppTheme.Navy;
                    tb.Font        = AppTheme.BodyFont;
                }
                else if (c is ComboBox cb)
                {
                    cb.BackColor = AppTheme.OffWhite;
                    cb.ForeColor = AppTheme.Navy;
                    cb.Font      = AppTheme.BodyFont;
                }
            }

            // ── Save button ──
            AppTheme.StyleButtonPrimary(btnSave);
            AppTheme.SetIcon(btnSave,
                isEdit ? AppTheme.IconSave() : AppTheme.IconPlus(),
                btnSave.Text);

            // ── Cancel button (add next to Save) ──
            var btnCancel = new Button
            {
                Text     = "Cancel",
                Size     = btnSave.Size,
                Location = new Point(btnSave.Left - btnSave.Width - 8, btnSave.Top),
                Anchor   = btnSave.Anchor,
            };
            AppTheme.StyleButtonSecondary(btnCancel);
            AppTheme.SetIcon(btnCancel, AppTheme.IconClose(), "Cancel", 16);
            btnCancel.Click += (s, e) => this.Close();
            panel1.Controls.Add(btnCancel);
        }

        private void NumberOnlyTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only digits and control keys (e.g., backspace)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedAnbiyamId != 0)
                {
                    //MessageBox.Show("Selected id = ", selectedAnbiyamId.ToString());

                    string anbiyamName = txtAnbiyamName.Text.Trim();
                    int anbiyamZone = int.Parse(cmbAnbiyamZone.SelectedItem.ToString());
                    string coordinatorName = txtCoordinator.Text.Trim();
                    string assistantCoordinatorName = txtAssistantCo.Text.Trim();
                    string email = txtEmail.Text.Trim();
                    string phone = txtPhone.Text.Trim();
                    string code = anbiyamCode.Text.Trim();

                    var parameters = new[]
                    {
                    new SqlParameter("@anbiyam_id", selectedAnbiyamId),
                    new SqlParameter("@anbiyam_name", anbiyamName),
                    new SqlParameter("@anbiyam_zone", anbiyamZone),
                    new SqlParameter("@anbiyam_coordinator_name", coordinatorName),
                    new SqlParameter("@anbiyam_ass_coordinator_name", assistantCoordinatorName),
                    new SqlParameter("@coordinator_email", email),
                    new SqlParameter("@coordinator_phone", phone),
                    new SqlParameter("@anbiyam_code", code)
                };

                    DatabaseHelper.ExecuteStoredProcedure("sp_InsertOrUpdateAnbiyam", parameters);
                    ThemedDialog.Info("Anbiyam updated successfully.", "Saved", this);
                    this.Close();
                }
                else
                {
                    string anbiyamName = txtAnbiyamName.Text.Trim();
                    int anbiyamZone = int.Parse(cmbAnbiyamZone.SelectedItem.ToString());
                    string coordinatorName = txtCoordinator.Text.Trim();
                    string assistantCoordinatorName = txtAssistantCo.Text.Trim();
                    string email = txtEmail.Text.Trim();
                    string phone = txtPhone.Text.Trim();
                    string code = anbiyamCode.Text.Trim();

                    if (anbiyamName == "" || coordinatorName == "" || phone == "")
                    {
                        ThemedDialog.Warn("Please fill in all mandatory fields (marked *).", "Required Fields", this);
                        return;
                    }

                    var parameters = new[]
                    {
                        new SqlParameter("@anbiyam_name", anbiyamName),
                        new SqlParameter("@anbiyam_zone", anbiyamZone),
                        new SqlParameter("@anbiyam_coordinator_name", coordinatorName),
                        new SqlParameter("@anbiyam_ass_coordinator_name", assistantCoordinatorName),
                        new SqlParameter("@coordinator_email", email),
                        new SqlParameter("@coordinator_phone", phone),
                        new SqlParameter("@anbiyam_code", code)
                    };

                    DatabaseHelper.ExecuteStoredProcedure("sp_InsertOrUpdateAnbiyam", parameters);
                    ThemedDialog.Info("Anbiyam registered successfully.", "Saved", this);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                ThemedDialog.Error(ex.Message, "Error", this);
            }
        }

        private int GetMaxAnbiyamId()
        {
            // Get the maximum Anbiyam ID from the database
            return DatabaseHelper.ExecuteScalarStoredProcedure("sp_GetTotalAnbiyamsCount");
        }

        private void LoadAnbiyamData(int anbiyamId)
        {
            txtAnbiyamName.Enabled = false;
            var param = new SqlParameter("@anbiyam_id", anbiyamId);
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetSelectedAnbiyam", param);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                txtAnbiyamName.Text = row["anbiyam_name"].ToString();
                cmbAnbiyamZone.SelectedItem = row["anbiyam_zone"].ToString();
                txtCoordinator.Text = row["anbiyam_coordinator_name"].ToString();
                txtAssistantCo.Text = row["anbiyam_ass_coordinator_name"].ToString();
                txtEmail.Text = row["coordinator_email"].ToString();
                txtPhone.Text = row["coordinator_phone"].ToString();
                anbiyamCode.Text = row["anbiyam_code"].ToString();
            }
        }

        private void txtAnbiyamName_Leave(object sender, EventArgs e)
        {
            if (txtAnbiyamName.Text.Length > 0)
            {
                anbiyamCode.Text = txtAnbiyamName.Text.Trim().ToUpper().Substring(0, 2);
            }
        }
    }
}
