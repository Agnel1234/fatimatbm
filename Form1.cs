﻿using System;
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
            LoadFamilies();
            LoadAnbiyam();
            LoadZoneFamilyChart();
        }

        private void searchButton_Click(object sender, EventArgs e)
        {

        }

        private void exitMenuItem3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void LoadFamilies()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyWithAnbiyam");
            dataGridView1.DataSource = dt;
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

        private void searchButton_Click_1(object sender, EventArgs e)
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

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetAllAnbiyams");
            anbiyamGrid.DataSource = dt;

            // Check if the icon column already exists to avoid duplicates
            if (anbiyamGrid.Columns["Icon"] == null)
            {
                DataGridViewImageColumn iconColumn = new DataGridViewImageColumn();
                iconColumn.Name = "Icon";
                iconColumn.HeaderText = "Delete";
                iconColumn.ImageLayout = DataGridViewImageCellLayout.Normal;
                anbiyamGrid.Columns.Insert(6, iconColumn); // Insert at the first position, or use Add() for last

                //iconColumn = new DataGridViewImageColumn();
                //iconColumn.Name = "Icon1";
                //iconColumn.HeaderText = "Delete";
                //iconColumn.ImageLayout = DataGridViewImageCellLayout.Normal;
                //anbiyamGrid.Columns.Insert(7, iconColumn); // Insert at the first position, or use Add() for last
            }
        }

        private void LoadZoneFamilyChart()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyCountByZone");

            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisX.Title = "Zone";
            chart1.ChartAreas[0].AxisY.Title = "Number of Families";

            var series = chart1.Series.Add("Families per Zone");
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;

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
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;

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
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;

            foreach (DataRow row in dt.Rows)
            {
                string zone = row["Zone"].ToString();
                int count = Convert.ToInt32(row["FamilyCount"]);
                series3.Points.AddXY(zone, count);
            }

        }

        private void ShowPopup(string action)
        {
            using (var popup = new AnbiyamPopup())
            {
                if( action == "create")
                {
                    popup.ShowDialog(this); // Shows as a modal dialog
                }
                else if (action == "edit")
                {
                    this.anbiyamGrid.SelectedRows.Cast<DataGridViewRow>().ToList().ForEach(row =>
                    {
                        // Assuming the first cell contains the ID or unique identifier
                        string selectedId = row.Cells[0].Value.ToString();
                        popup.Tag = selectedId; // Store the ID in the popup for later use
                    });
                }
                else
                {
                    MessageBox.Show("Invalid action specified.");
                    return;
                }

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

        private void chart3_Click(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }
    }
}
