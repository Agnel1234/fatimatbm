using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestFat
{
    public partial class FamilyPopup : Form
    {
        private int childCount = 0;
        private int otherRelationCount = 0;
        public FamilyPopup()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            child1Groupbox.Visible = false;
            child2Groupbox.Visible = false;
            child3Groupbox.Visible = false;
            child4Groupbox.Visible = false;
            child5Groupbox.Visible = false;
            otherRelation1Groupbox.Visible = false;
            otherRelation2Groupbox.Visible = false;
            LoadAnbiyam();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (childCount == 0)
            {
                child1Groupbox.Visible = true;
                childCount++;
            }
            else if (childCount == 1)
            {
                child2Groupbox.Visible = true;
                childCount++;
            }
             else if (childCount == 2)
            {
                child3Groupbox.Visible = true;
                childCount++;
            }
            else if (childCount == 3)
            {
                child4Groupbox.Visible = true;
                childCount++;
            }
            else if (childCount == 4)
            {
                child5Groupbox.Visible = true;
                childCount++;
            }
            else
            {
                MessageBox.Show("Family can add maximum of five children", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private int GetMaxFamilyId()
        {
            // Get the maximum Anbiyam ID from the database
            return DatabaseHelper.ExecuteScalarStoredProcedure("sp_GetTotalFamilyCount");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(otherRelationCount == 0)
            {
                otherRelation1Groupbox.Visible = true;
                otherRelationCount++;
            }
            else if (otherRelationCount == 1)
            {
                otherRelation2Groupbox.Visible = true;
                otherRelationCount++;
            }
            else
            {
                MessageBox.Show("Family can add maximum of two older relations", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadAnbiyam()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetAllAnbiyam");

            // Insert a default "Select" row at the top
            DataRow newRow = dt.NewRow();
            newRow[dt.Columns[0].ColumnName] = "Select Anbiyam";
            newRow[dt.Columns[1].ColumnName] = 200; // or 0 if you prefer
            dt.Rows.InsertAt(newRow, 0);

            anbiyamCombobox.DataSource = dt;
            anbiyamCombobox.DisplayMember = dt.Columns[0].ColumnName;
            anbiyamCombobox.ValueMember = dt.Columns[1].ColumnName;
            anbiyamCombobox.SelectedIndex = 0; // Ensure "Select" is shown by default
        }

        private void anbiyamCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nextFamilyID = GetMaxFamilyId() + 1;
            string anbiyamName = anbiyamCombobox.SelectedText.Trim();
            string codePrefix = "";
            if (anbiyamName.Split(' ').Length > 1)
            {
                // If the name contains spaces, use the first two characters of the first word
                codePrefix = anbiyamName.Split(' ')[0].Substring(0, Math.Min(1, anbiyamName.Length)).ToUpper() + anbiyamName.Split(' ')[1].Substring(0, Math.Min(1, anbiyamName.Length)).ToUpper();
                familyCodetxt.Text = codePrefix + nextFamilyID;
            }
            else
            {
                familyCodetxt.Text = anbiyamName.Substring(0, Math.Min(2, anbiyamName.Length)).ToUpper() + nextFamilyID.ToString();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
