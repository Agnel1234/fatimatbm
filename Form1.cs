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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadFamiliesAsync();
            LoadAnbiyam();
            LoadZoneFamilyChart();
            LoadFamilyBasicDetails();
            this.familygrid.SelectionChanged += familygrid_SelectionChanged;
            btncreate.Image = new Bitmap(Properties.Resources.createAnbiyam, new Size(24, 24)); // Assuming you have an add icon in your resources
            btnedit.Image = new Bitmap(Properties.Resources.editAnbiyam, new Size(24, 24)); // Replace with your actual path or resource


            btnFamilyCreate.Image = new Bitmap(Properties.Resources.createAnbiyam, new Size(24, 24)); // Assuming you have an add icon in your resources
            btnFamilyEdit.Image = new Bitmap(Properties.Resources.editAnbiyam, new Size(24, 24)); // Replace with your actual path or resource

            btncreate.ImageAlign = ContentAlignment.MiddleLeft;
            btncreate.TextAlign = ContentAlignment.MiddleCenter;

            btnFamilyCreate.ImageAlign = ContentAlignment.MiddleLeft;
            btnFamilyCreate.TextAlign = ContentAlignment.MiddleCenter;

            btnedit.ImageAlign = ContentAlignment.MiddleLeft;
            btnedit.TextAlign = ContentAlignment.MiddleCenter;

            btnFamilyEdit.ImageAlign = ContentAlignment.MiddleLeft;
            btnFamilyEdit.TextAlign = ContentAlignment.MiddleCenter;
            anbiyamGrid.CellClick += anbiyamGrid_CellClick;
        }

        private void exitMenuItem3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void LoadFamiliesAsync()
        {
            using (var progress = new ProgressForm("Connecting Database..."))
            {
                DataTable dt = null;
                Exception dbException = null;

                var worker = new System.ComponentModel.BackgroundWorker();
                worker.DoWork += (s, e) =>
                {
                    try
                    {
                        dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyWithAnbiyam");
                    }
                    catch (Exception ex)
                    {
                        dbException = ex;
                    }
                };
                worker.RunWorkerCompleted += (s, e) =>
                {
                    progress.Close();
                    if (dbException != null)
                    {
                        MessageBox.Show("Database connection failed:\n" + dbException.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        dataGridView1.DataSource = dt;
                        dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 12, FontStyle.Bold);
                        dataGridView1.DefaultCellStyle.Font = new Font("Tahoma", 9, FontStyle.Regular);
                        dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;
                    }
                };

                worker.RunWorkerAsync();
                progress.ShowDialog(this);
            }
        }

        private void LoadAnbiyam()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetAllAnbiyam");

            // Insert a default "Select" row at the top
            DataRow newRow = dt.NewRow();
            newRow[dt.Columns[0].ColumnName] = "Select";
            newRow[dt.Columns[1].ColumnName] = 200; // or 0 if you prefer
            dt.Rows.InsertAt(newRow, 0);

            anbiyamCombobox.DataSource = dt;
            anbiyamCombobox.DisplayMember = dt.Columns[0].ColumnName;
            anbiyamCombobox.ValueMember = dt.Columns[1].ColumnName;
            anbiyamCombobox.SelectedIndex = 0; // Ensure "Select" is shown by default
        }

        private void LoadFamilyMembers(int familyId)
        {
            var param = new SqlParameter("@family_id", familyId);
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyMembersByFamilyId", param);
            dataGridView1.DataSource = dt;
        }

        private void LoadFamilyBasicDetails()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyBasicDetails");
            familygrid.DataSource = dt;
            familygrid.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 12, FontStyle.Bold);
            familygrid.DefaultCellStyle.Font = new Font("Tahoma", 9, FontStyle.Regular);
            familygrid.Columns["FamilyID"].Visible = false;
            familygrid.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;
        }

        private void searchButton_Click_1(object sender, EventArgs e)
        {
            using (var progress = new ProgressForm("Searching..."))
            {
                // Get selected anbiyam_id (handle "Select" as null)
                object anbiyamIdObj = anbiyamCombobox.SelectedValue;
                int? anbiyamId = null;
                if (anbiyamIdObj != null && int.TryParse(anbiyamIdObj.ToString(), out int parsedId) && parsedId != 200) // 200 is your "Select" value
                    anbiyamId = parsedId;

                // Get coordinator name and head of family from textboxes
                string coordinatorName = coordinatorTetbox.Text.Trim();
                string headOfFamily = familyHeadTextbox.Text.Trim();

                // Use DBNull.Value for empty parameters
                var parameters = new[]
                {
                new SqlParameter("@anbiyam_id", (object)anbiyamId ?? DBNull.Value),
                new SqlParameter("@coordinator_name", string.IsNullOrEmpty(coordinatorName) ? (object)DBNull.Value : coordinatorName),
                new SqlParameter("@head_of_family", string.IsNullOrEmpty(headOfFamily) ? (object)DBNull.Value : headOfFamily)
            };

                DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_SearchFamilyWithAnbiyam", parameters);
                dataGridView1.DataSource = dt;
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadAnbiyamGrid();
        }

        private void LoadZoneFamilyChart()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyCountByZone");

            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisX.Title = "Zone";
            chart1.ChartAreas[0].AxisY.Title = "Number of Families";

            var series = chart1.Series.Add("Families per Zone");
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;

            foreach (DataRow row in dt.Rows)
            {
                string zone = row["Zone"].ToString();
                int count = Convert.ToInt32(row["FamilyCount"]); 
                series.Points.AddXY(zone, count);
            }

            chart2.Series.Clear();
            chart2.ChartAreas[0].AxisX.Title = "Zone";
            chart2.ChartAreas[0].AxisY.Title = "Number of Families";

            var series2 = chart2.Series.Add("Families per Zone");
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;

            foreach (DataRow row in dt.Rows)
            {
                string zone = row["Zone"].ToString();
                int count = Convert.ToInt32(row["FamilyCount"]);
                series2.Points.AddXY(zone, count);
            }

            chart3.Series.Clear();
            chart3.ChartAreas[0].AxisX.Title = "Zone";
            chart3.ChartAreas[0].AxisY.Title = "Number of Families";

            var series3 = chart3.Series.Add("Families per Zone");
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area;

            foreach (DataRow row in dt.Rows)
            {
                string zone = row["Zone"].ToString();
                int count = Convert.ToInt32(row["FamilyCount"]);
                series3.Points.AddXY(zone, count);
            }

        }

        private void ShowPopup(string action)
        {
            if (action == "create")
            {
                using (var popup = new AnbiyamPopup())
                {
                    popup.ShowDialog(); // Shows as a modal dialog
                }
                LoadAnbiyamGrid();
            }
            else if (action == "edit")
            {
                if (anbiyamGrid.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select an Anbiyam to edit.");
                    return;
                }
                var selectedId = anbiyamGrid.SelectedRows[0];
                if (selectedId == null || selectedId.Cells["anbiyamID"].Value == null || selectedId.Cells["anbiyamID"].Value == "")
                {
                    MessageBox.Show("Please select an Anbiyam to edit.");
                    return;
                }
                using (var popup = new AnbiyamPopup((int)selectedId.Cells["anbiyamID"].Value))
                {
                    popup.ShowDialog(); // Shows as a modal dialog
                }
                LoadAnbiyamGrid();
            }
            else
            {
                MessageBox.Show("Invalid action specified.");
                return;
            }
        }

        private void btncreate_Click(object sender, EventArgs e)
        {
            ShowPopup("create");

        }

        private void btnedit_Click(object sender, EventArgs e)
        {
            ShowPopup("edit");
        }

        private void LoadAnbiyamGrid()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetAllAnbiyams");
            anbiyamGrid.DataSource = dt;
            anbiyamGrid.Columns["anbiyamID"].Visible = false;
            anbiyamGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 12, FontStyle.Bold);
            anbiyamGrid.DefaultCellStyle.Font = new Font("Tahoma", 9, FontStyle.Regular);
            anbiyamGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;

            //// Check if the icon column already exists to avoid duplicates
            //if (anbiyamGrid.Columns["delete"] == null)
            //{
            //    DataGridViewImageColumn iconColumn = new DataGridViewImageColumn();
            //    iconColumn.Name = "delete";
            //    iconColumn.HeaderText = "";
            //    iconColumn.ImageLayout = DataGridViewImageCellLayout.Normal;
            //    iconColumn.Image = new Bitmap(Properties.Resources.delete3, new Size(15, 15));
                
            //    anbiyamGrid.Columns.Insert(7, iconColumn); // Insert at the first position, or use Add() for last

            //}


            // Remove existing delete column if present to avoid duplicates
            if (anbiyamGrid.Columns["delete"] != null)
                anbiyamGrid.Columns.Remove("delete");

            // Add a button column for delete
            DataGridViewButtonColumn btnCol = new DataGridViewButtonColumn();
            btnCol.Name = "delete";
            btnCol.HeaderText = "";
            btnCol.Text = "Delete";
            btnCol.UseColumnTextForButtonValue = true;
            btnCol.Width = 60;
            anbiyamGrid.Columns.Insert(7, btnCol); // Adjust index as needed
        }

        private void familygrid_SelectionChanged(object sender, EventArgs e)
        {
            if (familygrid.SelectedRows.Count > 0)
            {
                var selectedRow = familygrid.SelectedRows[0];
                // Assuming you have a hidden column or a way to get family_id
                int familyId = GetFamilyIdFromSelectedRow(selectedRow);
                LoadFamilyMembersForGrid(familyId);
            }
        }


        private void anbiyamGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && anbiyamGrid.Columns[e.ColumnIndex].Name == "delete")
            {
                var result = MessageBox.Show("Are you sure you want to delete this row?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    int id = Convert.ToInt32(anbiyamGrid.Rows[e.RowIndex].Cells["anbiyamID"].Value);
                    // Call your delete logic here (e.g., delete from database)
                    DatabaseHelper.ExecuteStoredProcedure("sp_DeleteAnbiyam", new SqlParameter("@anbiyamID", id));
                    // Refresh grid
                    LoadAnbiyamGrid();
                }
            }
        }

        private int GetFamilyIdFromSelectedRow(DataGridViewRow row)
        {
            // If you have family_id as a hidden column:
            return Convert.ToInt32(row.Cells["FamilyID"].Value);

            // If not, you need to fetch it based on unique fields (not recommended).
            // Best practice: include family_id as a hidden column in your SELECT.
            throw new NotImplementedException();
        }

        private void LoadFamilyMembersForGrid(int familyId)
        {
            var param = new SqlParameter("@family_id", familyId);
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyMembersByFamilyId", param);
            familyMembersGrid.DataSource = dt;
            familyMembersGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 10, FontStyle.Bold);
            familyMembersGrid.DefaultCellStyle.Font = new Font("Tahoma", 9, FontStyle.Regular);
            familyMembersGrid.Columns["memberID"].Visible = false; // Hide the ID column if needed
            familyMembersGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;
        }

        private void btnFamilyCreate_Click(object sender, EventArgs e)
        {
            using (var popup = new FamilyPopup())
            {
                popup.ShowDialog(); // Shows as a modal dialog
            }
        }
    }
}