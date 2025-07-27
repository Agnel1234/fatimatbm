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
    public partial class Cemetery : Form
    {
        int familydIDInContext = 0; // Store the family ID
        int cemeteryIDInContext = 0; // Store the cemetery ID
        public Cemetery(int familydID)
        {
            InitializeComponent();
            familydIDInContext= familydID;
            this.StartPosition = FormStartPosition.CenterScreen;

            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog; // Optional: prevents resizing

            LoadCemeteryData();
            LoadMemberDropdown();
        }

        private void LoadCemeteryData()
        {
            var param = new SqlParameter("@familyID", familydIDInContext);
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyCemetery", param);
            cemeteryGrid.DataSource = dt;
            cemeteryGrid.Columns["cemeteryid"].Visible = false;
        }

        private void LoadMemberDropdown()
        {
            var param = new SqlParameter("@family_id", familydIDInContext);
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyMembersForDropdown", param);

            DataRow newRow = dt.NewRow();
            newRow[dt.Columns[1].ColumnName] = "Select Member";
            newRow[dt.Columns[0].ColumnName] = 200; 
            dt.Rows.InsertAt(newRow, 0);

            comboMemberName.DataSource = dt;
            comboMemberName.DisplayMember = dt.Columns[1].ColumnName;
            comboMemberName.ValueMember = dt.Columns[0].ColumnName;
            comboMemberName.SelectedIndex = 0; 
        }

        private void btnAddCemetery_Click(object sender, EventArgs e)
        {
            if (cemeteryIDInContext != 0)
            {
                DateTime deceasedDate = dtDeceasedDate.Value;
                DateTime burialDate = dtBurialDate.Value;
                string remark = txtCemeteryCharge.Text.Trim();
                string cemeterycode = txtCemeteryCode.Text.Trim();

                var parameters = new[]
                {
                    new SqlParameter("@cemeteryID", cemeteryIDInContext),
                    new SqlParameter("@memberID", ""),
                    new SqlParameter("@burialDate", burialDate),
                    new SqlParameter("@deathDate", deceasedDate),
                    new SqlParameter("@cemeteryCode", cemeterycode),
                    new SqlParameter("@remarks", remark)
                };

                DatabaseHelper.ExecuteStoredProcedure("sp_InsertOrUpdateCemetery", parameters);
                MessageBox.Show("Cemetery Updated successfully!");
                this.Close();
            }
            else
            {
                int memberID = int.Parse(comboMemberName.SelectedValue.ToString());
                DateTime deceasedDate = dtDeceasedDate.Value;
                DateTime burialDate = dtBurialDate.Value;
                string remark = txtCemeteryCharge.Text.Trim();
                string cemeterycode = txtCemeteryCode.Text.Trim();

                if (comboMemberName.SelectedItem == null)
                {
                    MessageBox.Show("Please add all the mandatory fields");
                    return;
                }

                var parameters = new[]
                {
                    new SqlParameter("@cemeteryID", cemeteryIDInContext),
                    new SqlParameter("@memberID", memberID),
                    new SqlParameter("@burialDate", burialDate),
                    new SqlParameter("@deathDate", deceasedDate),
                    new SqlParameter("@cemeteryCode", cemeterycode),
                    new SqlParameter("@remarks", remark)
                };

                DatabaseHelper.ExecuteStoredProcedure("sp_InsertOrUpdateCemetery", parameters);
                MessageBox.Show("Cemetery Registered successfully!");
                this.Close();
            }
        }

        private void cemeteryGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (cemeteryGrid.SelectedRows.Count == 1)
            {
                var selectedRow = cemeteryGrid.SelectedRows[0];
                // Assuming you have a hidden column or a way to get family_id
                int cemeteryid = Convert.ToInt32(selectedRow.Cells["CemeteryID"].Value);
                cemeteryIDInContext = cemeteryid;
            }
        }
    }
}
