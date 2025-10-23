using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace TestFat
{
    public partial class Form1 : Form
    {
        private int familyIDInContext = 0;
        public string anbiyamAddress = "";
        public Form1()
        {

            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.FixedSingle; // or Sizable if you want resizable window
            this.ControlBox = true;      // enables the whole control box (minimize, maximize, close)
            this.MaximizeBox = false;    // keep maximize disabled (optional)
            this.MinimizeBox = true;     // enable minimize (optional)
            this.ShowIcon = true;

            LoadAnbiyamGrid(null);
            LoadOccupation();
            LoadAnbiyam();

            LoadAgeGroupChart();
            LoadGenderGroupChart();

            LoadFamilyBasicDetails(null);
            LoadAllCemeteryData(null);

            // Make all grids read-only
            familygrid.ReadOnly = true;
            anbiyamGrid.ReadOnly = true;
            familyMembersGrid.ReadOnly = true;
            cemeteryGridView.ReadOnly = true;

            this.familygrid.SelectionChanged += familygrid_SelectionChanged;
            this.familygrid.RowPrePaint += familygrid_RowPrePaint;

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

            string showMap = ConfigurationManager.AppSettings["showMap"];
            if (showMap != null && showMap == "true")
            {
                ShowAnbiyamOnMap(anbiyamAddress);
                webBrowser1.Dock = DockStyle.Fill;
            }
            parishCombobox.SelectedIndex = 0;

            // panel7 - Top Left
            this.panel7.Location = new System.Drawing.Point(10, 10); // 10px from top and left
            this.panel7.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // panel8 - Top Right
            this.panel8.Location = new System.Drawing.Point(this.ClientSize.Width - this.panel8.Width - 10, 10); // 10px from top and right
            this.panel8.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        }

        private void exitMenuItem3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void familytab_DrawItem(object sender, DrawItemEventArgs e)
        {
            for (int i = 0; i < familytab.TabCount; i++)
            {
                Rectangle tabRect = familytab.GetTabRect(i);
                bool isSelected = (i == familytab.SelectedIndex);

                // Draw tab text
                string tabText = familytab.TabPages[i].Text;
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
                    e.Graphics.DrawString(tabText, familytab.Font, textBrush, tabRect, sf);
                }
            }
        }

        private void LoadOccupation()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_AggregateOccupations");
            // Insert a default "Select" row at the top
            DataRow newRow = dt.NewRow();
            newRow[dt.Columns[0].ColumnName] = "Select";
            dt.Rows.InsertAt(newRow, 0);
            familyOccupationComboxbox.DataSource = dt;
            familyOccupationComboxbox.DisplayMember = dt.Columns[0].ColumnName;
            familyOccupationComboxbox.SelectedIndex = 0; // Ensure "Select" is shown by default
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


            familyAnbiyamCombobox.DataSource = dt;
            familyAnbiyamCombobox.DisplayMember = dt.Columns[0].ColumnName;
            familyAnbiyamCombobox.ValueMember = dt.Columns[1].ColumnName;
            familyAnbiyamCombobox.SelectedIndex = 0; // Ensure "Select" is shown by default
        }

        public void LoadFamilyBasicDetails(DataTable dt)
        {
            if (dt == null)
            {
                dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyBasicDetails");
            }

            familygrid.DataSource = dt;
            familygrid.Columns["FamilyID"].Visible = false;
            familygrid.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 12, FontStyle.Bold);
            familygrid.DefaultCellStyle.Font = new Font("Georgia", 11, FontStyle.Regular);
            familygrid.AlternatingRowsDefaultCellStyle.BackColor = Color.LightSlateGray;
            familygrid.BackgroundColor = Color.WhiteSmoke;
            familygrid.DefaultCellStyle.ForeColor = Color.Black;

            // Remove existing delete column if present to avoid duplicates
            if (familygrid.Columns["delete"] != null)
                familygrid.Columns.Remove("delete");

            // Add a button column for delete
            DataGridViewButtonColumn btnCol = new DataGridViewButtonColumn();
            btnCol.Name = "delete";
            btnCol.HeaderText = "";
            btnCol.Text = "Delete";
            btnCol.UseColumnTextForButtonValue = true;
            btnCol.Width = 60;
            btnCol.DefaultCellStyle.BackColor = Color.DarkRed;
            btnCol.DefaultCellStyle.ForeColor = Color.DarkRed;
            familygrid.Columns.Insert(10, btnCol); // Adjust index as needed
        }


        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(((System.Windows.Forms.TabControl)sender).SelectedTab.Name == "dashboardPage")
            {
                LoadAgeGroupChart();
                LoadGenderGroupChart();
            }
            else if(((System.Windows.Forms.TabControl)sender).SelectedTab.Name == "anbiyamPage")
            {
                LoadAnbiyam();
                LoadAnbiyamGrid(null);
            }
            else if (((System.Windows.Forms.TabControl)sender).SelectedTab.Name == "familyPage")
            {
                LoadOccupation();
                LoadFamilyBasicDetails(null);
            }
            else
            {
                LoadAllCemeteryData(null);
            }
        }

        private void LoadAgeGroupChart()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetAgeGroupChartData");

            chart3.Series.Clear();
            chart3.Titles.Clear(); // Clear any previous titles
            chart3.Titles.Add("Age Group Distribution"); // Add chart name/title
            chart3.Titles[0].Font = new Font("Georgia", 14, FontStyle.Bold);
            chart3.Titles[0].ForeColor = Color.DarkSlateGray;

            chart3.ChartAreas[0].AxisX.Title = "Age Group";
            chart3.ChartAreas[0].AxisY.Title = "Members";

            // Aesthetic: Set chart area background and border
            chart3.ChartAreas[0].BackColor = Color.WhiteSmoke;
            chart3.ChartAreas[0].BorderColor = Color.DarkGray;
            chart3.ChartAreas[0].BorderDashStyle = ChartDashStyle.Solid;
            chart3.ChartAreas[0].BorderWidth = 2;
            chart3.ChartAreas[0].ShadowColor = Color.Gray;
            chart3.ChartAreas[0].ShadowOffset = 2;

                // Chart control background
            chart3.BackColor = Color.White;
            chart3.AntiAliasing = AntiAliasingStyles.All;

            // Legend aesthetics
            chart3.Legends.Clear();
            var legend = chart3.Legends.Add("AgeGroups");
            legend.Docking = Docking.Bottom;
            legend.Font = new Font("Georgia", 11, FontStyle.Bold);
            legend.BackColor = Color.Transparent;
            legend.BorderColor = Color.DarkGray;
            legend.BorderWidth = 1;

            // Pie chart series
            var series = chart3.Series.Add("Members Age Group1");
            series.ChartType = SeriesChartType.Pie;
            series["PieDrawingStyle"] = "SoftEdge";
            series["PieLabelStyle"] = "Outside";
            series.Font = new Font("Georgia", 11, FontStyle.Bold);
            series.BorderColor = Color.White;
            series.BorderWidth = 2;
            series.IsValueShownAsLabel = false;

            // Custom colors for slices
            Color[] pieColors = {
                Color.RoyalBlue,         // Youngest group
                Color.MediumVioletRed,   // Teens
                Color.Teal,              // Young adults
                Color.Goldenrod,         // Middle age
                Color.DarkOrange,        // Seniors
                Color.SlateGray          // Elders/Unknown
            };

            int colorIndex = 0;
            int totalCount = dt.AsEnumerable().Sum(r => r.Field<int>("MemberCount"));

            foreach (DataRow row in dt.Rows)
            {
                string ageGroup = row["AgeGroup"].ToString();
                string ageGroupWithDesc = row["AgeGroupWithDesc"].ToString();
                int count = Convert.ToInt32(row["MemberCount"]);
                int pointIndex = series.Points.AddXY(ageGroup, count);

                // Set color for each slice
                series.Points[pointIndex].Color = pieColors[colorIndex % pieColors.Length];

                // Label: value and percentage, formatted
                double percent = totalCount > 0 ? (double)count / totalCount : 0;
                series.Points[pointIndex].Label = $"{ageGroupWithDesc}\n{count} ({percent:P1})";
                series.Points[pointIndex].LegendText = ageGroupWithDesc;

                // Tooltip for interactivity
                series.Points[pointIndex].ToolTip = $"{ageGroupWithDesc}: {count} members ({percent:P1})";

                colorIndex++;
            }

            // Explode largest slice for emphasis
            if (series.Points.Count > 0)
            {
                int maxIndex = series.Points.IndexOf(series.Points.OrderByDescending(p => p.YValues[0]).First());
                //series.Points[maxIndex].Exploded = true;
            }

            // Remove border around the chart control
            chart3.BorderlineDashStyle = ChartDashStyle.Solid;
            chart3.BorderlineColor = Color.Transparent;
        }

        private void LoadGenderGroupChart()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GenderData");

            chart2.Series.Clear();
            chart2.Titles.Clear(); // Clear any previous titles
            chart2.Titles.Add("Gender Distribution"); // Add chart title
            chart2.Titles[0].Font = new Font("Georgia", 14, FontStyle.Bold);
            chart2.Titles[0].ForeColor = Color.DarkSlateGray;

            // Chart area aesthetics
            chart2.ChartAreas[0].BackColor = Color.WhiteSmoke;
            chart2.ChartAreas[0].BorderColor = Color.DarkGray;
            chart2.ChartAreas[0].BorderDashStyle = ChartDashStyle.Solid;
            chart2.ChartAreas[0].BorderWidth = 2;
            chart2.ChartAreas[0].ShadowColor = Color.Gray;
            chart2.ChartAreas[0].ShadowOffset = 2;

            // Chart control background
            chart2.BackColor = Color.White;
            chart2.AntiAliasing = AntiAliasingStyles.All;

            // Legend aesthetics
            chart2.Legends.Clear();
            var legend = chart2.Legends.Add("GenderGroups");
            legend.Docking = Docking.Bottom;
            legend.Font = new Font("Georgia", 11, FontStyle.Bold);
            legend.BackColor = Color.Transparent;
            legend.BorderColor = Color.DarkGray;
            legend.BorderWidth = 1;

            // Column chart with custom colors and value labels
            var series = chart2.Series.Add("Gender Count");
            series.ChartType = SeriesChartType.Column;
            series.Font = new Font("Georgia", 11, FontStyle.Bold);
            series.IsValueShownAsLabel = true;
            series.LabelForeColor = Color.Black;
            series.IsVisibleInLegend = false;
            series.BorderColor = Color.White;
            series.BorderWidth = 2;

            Color[] columnColors = { Color.RoyalBlue, Color.MediumVioletRed };


            // Add Male and Female counts
            if (dt.Rows.Count > 0)
            {
                int maleCount = 0;
                int femaleCount = 0;

                object maleValue = dt.Rows[0]["MaleCount"];
                if (maleValue != DBNull.Value)
                {
                    maleCount = Convert.ToInt32(maleValue);
                }
                object femaleValue = dt.Rows[0]["FemaleCount"];
                if (femaleValue != DBNull.Value)
                {
                    femaleCount = Convert.ToInt32(femaleValue);
                }

                int pointIndexMale = series.Points.AddXY("Male", maleCount);
                series.Points[pointIndexMale].Color = columnColors[0];
                series.Points[pointIndexMale].Label = $"{maleCount}";
                series.Points[pointIndexMale].LegendText = "Male";
                series.Points[pointIndexMale].ToolTip = $"Male: {maleCount}";

                int pointIndexFemale = series.Points.AddXY("Female", femaleCount);
                series.Points[pointIndexFemale].Color = columnColors[1];
                series.Points[pointIndexFemale].Label = $"{femaleCount}";
                series.Points[pointIndexFemale].LegendText = "Female";
                series.Points[pointIndexFemale].ToolTip = $"Female: {femaleCount}";
            }

            // Axis label font and grid lines
            chart2.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Georgia", 10, FontStyle.Bold);
            chart2.ChartAreas[0].AxisY.LabelStyle.Font = new Font("Georgia", 10, FontStyle.Bold);
            chart2.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.White;
            chart2.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.White;

            // Remove border around the chart control
            chart2.BorderlineDashStyle = ChartDashStyle.NotSet;
            chart2.BorderlineColor = Color.Transparent;
        }

        private void ShowPopup(string action)
        {
            if (action == "create")
            {
                using (var popup = new AnbiyamPopup())
                {
                    popup.ShowDialog(); // Shows as a modal dialog
                }
                LoadAnbiyamGrid(null);
            }
            else if (action == "edit")
            {
                if (anbiyamGrid.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select an Anbiyam to edit.");
                    return;
                }
                var selectedId = anbiyamGrid.SelectedRows[0];
                if (selectedId == null || selectedId.Cells["anbiyam_id"].Value == null || selectedId.Cells["anbiyam_id"].Value == "")
                {
                    MessageBox.Show("Please select an Anbiyam to edit.");
                    return;
                }
                using (var popup = new AnbiyamPopup((int)selectedId.Cells["anbiyam_id"].Value))
                {
                    popup.ShowDialog(); // Shows as a modal dialog
                }
                LoadAnbiyamGrid(null);
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

        private void LoadAnbiyamGrid(DataTable dt)
        {
            if(dt == null || (dt != null & dt.Rows.Count == 0))
            {
                anbiyamGrid.DataSource = null;
                dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyWithAnbiyam");
            }
            
            anbiyamGrid.DataSource = dt;
            anbiyamGrid.Columns["anbiyam_id"].Visible = false;
            anbiyamGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 12, FontStyle.Bold);
            anbiyamGrid.DefaultCellStyle.Font = new Font("Georgia", 11, FontStyle.Regular);
            anbiyamGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.LightSlateGray;
            anbiyamGrid.BackgroundColor = Color.WhiteSmoke;
            anbiyamGrid.DefaultCellStyle.ForeColor = Color.Black;

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
            btnCol.DefaultCellStyle.BackColor = Color.DarkRed;
            btnCol.DefaultCellStyle.ForeColor = Color.DarkRed;
            anbiyamGrid.Columns.Insert(10, btnCol); // Adjust index as needed
        }

        private void familygrid_SelectionChanged(object sender, EventArgs e)
        {
            if (familygrid.SelectedRows.Count == 1)
            {
                var selectedRow = familygrid.SelectedRows[0];
                // Assuming you have a hidden column or a way to get family_id
                int familyId = GetFamilyIdFromSelectedRow(selectedRow);
                familyIDInContext = familyId; // Store the selected family ID in context
                LoadFamilyMembersForGrid(familyId);
            }
        }

        private void anbiyamGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && anbiyamGrid.SelectedCells.Count == 1 && anbiyamGrid.SelectedCells[0].ColumnIndex == 10)
            {
                var result = MessageBox.Show("Are you sure you want to delete this Anbiyam?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    int id = Convert.ToInt32(anbiyamGrid.Rows[e.RowIndex].Cells["anbiyam_id"].Value);
                    try
                    {
                        DatabaseHelper.ExecuteStoredProcedure("sp_DeleteAnbiyam", new SqlParameter("@anbiyamID", id));
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message != null && ex.Message.Contains("Cannot delete"))
                        {
                            MessageBox.Show("This Anbiyam cannot be deleted, as families are linked to it", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            MessageBox.Show(ex.InnerException.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    // Refresh grid
                    LoadAnbiyamGrid(null);
                }
            }
        }

        private int GetFamilyIdFromSelectedRow(DataGridViewRow row)
        {
            // If you have family_id as a hidden column:
            return Convert.ToInt32(row.Cells["FamilyID"].Value);
        }

        private void LoadFamilyMembersForGrid(int familyId)
        {
            var param = new SqlParameter("@family_id", familyId);
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyMembersByFamilyId", param);
            if (dt.Rows.Count > 0)
            {
                familyMembersGrid.DataSource = dt;
                familyMembersGrid.Columns["memberID"].Visible = false; // Hide the ID column if needed
                familyMembersGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 11, FontStyle.Bold);
                familyMembersGrid.DefaultCellStyle.Font = new Font("Georgia", 11, FontStyle.Regular);
                familyMembersGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.LightSlateGray;
                familyMembersGrid.BackgroundColor = Color.WhiteSmoke;
                familyMembersGrid.DefaultCellStyle.ForeColor = Color.Black;

            }
            else
            {
                familyMembersGrid.DataSource = null;
            }
        }

        private void btnFamilyCreate_Click(object sender, EventArgs e)
        {
            int familyID = 0;
            using (var popup = new FamilyPopup(familyID))
            {
                popup.ShowDialog();
                // After closing, reload family basic details
                LoadFamilyBasicDetails(null);
            }
        }

        private void btnFamilyEdit_Click(object sender, EventArgs e)
        {
            if (familyIDInContext <= 0)
            {
                MessageBox.Show("Please select a family to edit.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var popup = new FamilyPopup(familyIDInContext))
            {
                popup.ShowDialog();
                // After closing, reload family basic details
                LoadFamilyBasicDetails(null);
            }
        }

        private void familygrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && familygrid.SelectedCells != null && familygrid.SelectedCells.Count == 1 && familygrid.SelectedCells[0].Value == "Delete")
            {
                var result = MessageBox.Show("Are you sure you want to delete this Family?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    int id = Convert.ToInt32(familygrid.Rows[e.RowIndex].Cells["FamilyID"].Value);
                    try
                    {
                        DatabaseHelper.ExecuteStoredProcedure("sp_DeleteFamily", new SqlParameter("@familyID", id));
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message != null && ex.Message.Contains("Cannot delete"))
                        {
                            MessageBox.Show("This Family cannot be deleted", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    // Refresh grid
                    LoadFamilyBasicDetails(null);
                }
            }
        }


        public void ShowAnbiyamOnMap(string anbiyamAddress)
        {
            string url = $"https://www.bing.com/maps?q={Uri.EscapeDataString("Silver spring flats, bethelpuram, East tambaram")}";
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Navigate(url);
            webBrowser1.Dock = DockStyle.Fill;
        }
        private void mapBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            // Optionally handle any actions after navigation
        }

        private void btnCemetery_Click(object sender, EventArgs e)
        {
            if (familyIDInContext <= 0)
            {
                MessageBox.Show("Please select a family to view cemetery details.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var popup = new Cemetery(familyIDInContext))
            {
                popup.ShowDialog();
                LoadFamilyBasicDetails(null);
            }
        }

        private void LoadAllCemeteryData(DataTable dtInput)
        {
            DataTable dt = null;
            if (dtInput != null)
            {
                dt = dtInput;
            }
            else
            {
                dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetAllCemeteries");
            }

            cemeteryGridView.DataSource = dt;
            cemeteryGridView.Columns["cemeteryid"].Visible = false;
            cemeteryGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Georgia", 12, FontStyle.Bold);
            cemeteryGridView.DefaultCellStyle.Font = new Font("Georgia", 10, FontStyle.Regular);
            cemeteryGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.LightSlateGray;
            cemeteryGridView.BackgroundColor = Color.WhiteSmoke;
            cemeteryGridView.DefaultCellStyle.ForeColor = Color.Black;

        }

        private void searchButton_Click(object sender, EventArgs e)
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

                // Use DBNull.Value for empty parameters
                var parameters = new[]
                {
                new SqlParameter("@anbiyam_id", (object)anbiyamId ?? DBNull.Value),
                new SqlParameter("@coordinator_name", string.IsNullOrEmpty(coordinatorName) ? (object)DBNull.Value : coordinatorName),
            };

                DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_SearchFamilyWithAnbiyam", parameters);
                LoadAnbiyamGrid(dt);
            }
        }

        private void searchFamily_Click(object sender, EventArgs e)
        {
            using (var progress = new ProgressForm("Searching..."))
            {
                // Get selected anbiyam_id (handle "Select" as null)
                object anbiyamIdObj = familyAnbiyamCombobox.SelectedValue;
                int anbiyamId = 0;
                bool? isCemeteryAvailable = null; // Default to null for "Not Applicable"

                if (anbiyamIdObj != null && int.TryParse(anbiyamIdObj.ToString(), out int parsedId) && parsedId != 200) // 200 is your "Select" value
                    anbiyamId = parsedId;

                string familyHead = txtFamilyHead.Text == "" ? null : txtFamilyHead.Text.Trim();
                string occupation = familyOccupationComboxbox.SelectedText == "" ? null : familyOccupationComboxbox.SelectedText.Trim(); ;

                switch(cemeteryComboBox.SelectedItem)
                {
                    case "Not Applicable":
                        isCemeteryAvailable = null; // Handle "Select" as null
                        break;
                    case "":
                        isCemeteryAvailable = null; // Handle "Select" as null
                        break;
                    case "Yes":
                        isCemeteryAvailable = true; // Handle "Not Applicable" as null
                        break;
                    default:
                        isCemeteryAvailable = false;
                        break;
                }

                var parameters = new[]
                {
                new SqlParameter("@anbiyam_id", anbiyamId),
                new SqlParameter("@family_head", string.IsNullOrEmpty(familyHead) ? (object)DBNull.Value : familyHead),
                new SqlParameter("@occupation", string.IsNullOrEmpty(occupation) ? (object)DBNull.Value : occupation),
                new SqlParameter("@cemetery_available", isCemeteryAvailable),
            };

                DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyBasicDetails", parameters);
                LoadFamilyBasicDetails(dt);
            }

        }

        private void cemeterySearchBtn_Click(object sender, EventArgs e)
        {
            using (var progress = new ProgressForm("Searching..."))
            {
                bool? isOurParish = null; // Default to null for "Not Applicable"

                switch (parishCombobox.SelectedItem)
                {
                    case "Not Applicable":
                        isOurParish = null; // Handle "Select" as null
                        break;
                    case "":
                        isOurParish = null; // Handle "Select" as null
                        break;
                    case "Fatima Church Parish":
                        isOurParish = true; // Handle "Not Applicable" as null
                        break;
                    default:
                        isOurParish = false;
                        break;
                }

                var parameters = new[]
                {
                new SqlParameter("@burial_date_from", dtBurialStart.Checked ? dtBurialStart.Value :  (DateTime?)null),
                new SqlParameter("@burial_date_to", dtBurialEnd.Checked ? dtBurialEnd.Value :  (DateTime?)null),
                new SqlParameter("@deceased_date_from", dtDeceasedStart.Checked ? dtDeceasedStart.Value :  (DateTime?)null),
                new SqlParameter("@deceased_date_to", dtDeceasedEnd.Checked ? dtDeceasedEnd.Value :  (DateTime?)null),
                new SqlParameter("@IsOurparish", isOurParish),
            };

                DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetAllCemeteries", parameters);
                LoadAllCemeteryData(dt);
            }
        }


        private void familygrid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var grid = sender as DataGridView;
            if (grid.Rows[e.RowIndex].DataBoundItem is DataRowView drv)
            {
                // Replace "IsSpecial" with your actual boolean column name
                if (drv.Row.Table.Columns.Contains("Multiple Family Cards") && drv.Row.Field<bool>("Multiple Family Cards"))
                {
                    grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                }
            }
        }

        private void btnOutsideParsihMember_Click(object sender, EventArgs e)
        {
            using (var popup = new NonParishFamily())
            {
                popup.StartPosition = FormStartPosition.CenterScreen; // Show in the middle of the screen
                popup.ShowDialog();
                // After closing, reload family basic details
                // LoadFamilyBasicDetails(null);
            }
        }
    }

    }