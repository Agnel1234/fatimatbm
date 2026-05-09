/// ═══════════════════════════════════════════════════════════════════════════════
/// FORM1.CS INTEGRATION FOR MODERN PDF EXPORT
///
/// INSTRUCTIONS:
/// 1. Copy this entire file content into Form1.cs (as a partial class continuation)
/// 2. Add the button creation code to your ApplyTheme() method
/// 3. Update the stored procedure calls if your database procedure names differ
/// 4. Build and test
/// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace TestFat
{
    public partial class Form1 : Form
    {
        // ───────────────────────────────────────────────────────────────────────
        // STEP 1: Add this to ApplyTheme() method (around line 450-480)
        // Find the section with other buttons and add this:
        // ───────────────────────────────────────────────────────────────────────

        /*
        // Modern PDF Export Button
        Button btnExportModernPDF = new Button
        {
            Name = "btnExportModernPDF",
            Text = "Export Modern PDF Report",
            Location = new Point(730, 10),  // Adjust X position as needed
            Width = 180,
            Height = 32,
            Font = new Font("Georgia", 9F)
        };
        btnExportModernPDF.Click += BtnExportModernPDF_Click;
        AppTheme.StyleButtonPrimary(btnExportModernPDF);
        AppTheme.SetIcon(btnExportModernPDF, AppTheme.IconDownload(), "PDF Report");
        // Add to your button panel:
        rightFlow.Controls.Add(btnExportModernPDF);  // or your panel name
        */

        // ───────────────────────────────────────────────────────────────────────
        // STEP 2: Copy ALL these methods into Form1.cs class
        // ───────────────────────────────────────────────────────────────────────

        // ═════════════════════════════════════════════════════════════════════════
        // EVENT HANDLER - Export Modern PDF Report
        // ═════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Export families to a professional multi-page PDF report
        /// </summary>
        private void BtnExportModernPDF_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult choice = MessageBox.Show(
                    "Choose export scope:\n\n[Yes] Export ALL families in the system\n[No] Export selected families only\n[Cancel] Cancel",
                    "Export Modern PDF Report",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (choice == DialogResult.Cancel)
                    return;

                List<FamilyReportData> families = new List<FamilyReportData>();
                string reportTitle = "";

                if (choice == DialogResult.Yes)
                {
                    // Export all families from database
                    reportTitle = "All Families Export";
                    DataTable allFamilies = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyBasicDetailsForExport");

                    if (allFamilies == null || allFamilies.Rows.Count == 0)
                    {
                        ThemedDialog.Warn("No family data found in database.", "No Data", this);
                        return;
                    }

                    families = ConvertToFamilyReportData(allFamilies);
                }
                else if (choice == DialogResult.No)
                {
                    // Export selected families from grid
                    if (familygrid.SelectedRows.Count == 0)
                    {
                        ThemedDialog.Warn("Please select at least one family to export.", "No Selection", this);
                        return;
                    }

                    reportTitle = $"Selected Families Export ({familygrid.SelectedRows.Count} families)";

                    foreach (DataGridViewRow row in familygrid.SelectedRows)
                    {
                        try
                        {
                            int familyId = Convert.ToInt32(row.Cells["family_id"].Value);
                            var family = LoadFamilyReportData(familyId);
                            if (family != null)
                                families.Add(family);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error loading family row: {ex.Message}");
                        }
                    }
                }

                if (families.Count == 0)
                {
                    ThemedDialog.Warn("No families could be loaded for export.", "Error", this);
                    return;
                }

                // Show progress
                var progressForm = new ProgressForm("Generating PDF Report", $"Processing {families.Count} families...");
                progressForm.Show(this);
                Application.DoEvents();

                try
                {
                    // Generate PDF
                    string filename = "family_report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";
                    string filePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        filename);

                    ModernPdfGenerator.GenerateModernFamilyReport(families, filePath);

                    progressForm.Close();

                    // Show success message
                    DialogResult openPdf = MessageBox.Show(
                        $"Report generated successfully!\n\nFamilies: {families.Count}\nPages: {families.Count + 4}\nFile: {Path.GetFileName(filePath)}\n\nOpen the PDF now?",
                        "Export Complete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (openPdf == DialogResult.Yes)
                    {
                        Process.Start(filePath);
                    }
                }
                catch (Exception ex)
                {
                    progressForm.Close();
                    ThemedDialog.Error($"PDF generation failed:\n\n{ex.Message}", "Export Error", this);
                }
            }
            catch (Exception ex)
            {
                ThemedDialog.Error($"Export failed:\n\n{ex.Message}", "Error", this);
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // DATA CONVERSION - Convert DataTable to Report Format
        // ═════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Convert all families DataTable to FamilyReportData list for PDF
        /// </summary>
        private List<FamilyReportData> ConvertToFamilyReportData(DataTable familiesTable)
        {
            var families = new List<FamilyReportData>();

            foreach (DataRow row in familiesTable.Rows)
            {
                try
                {
                    int familyId = Convert.ToInt32(row["FamilyID"]);
                    var family = new FamilyReportData
                    {
                        FamilyName = row["FamilyName"]?.ToString() ?? "Unknown",
                        RegistrationDate = row["RegistrationDate"] != DBNull.Value
                            ? Convert.ToDateTime(row["RegistrationDate"]).ToString("dd MMM yyyy")
                            : "-",
                        Contact = row["Phone"]?.ToString() ?? "",
                        Email = row["Email"]?.ToString() ?? "",
                        Address = row["Address"]?.ToString() ?? "",
                        OutstandingDues = row["OutstandingDues"] != DBNull.Value
                            ? Convert.ToInt32(row["OutstandingDues"])
                            : 0,
                        ActiveSubscriptions = row["ActiveSubscriptions"] != DBNull.Value
                            ? Convert.ToInt32(row["ActiveSubscriptions"])
                            : 0,
                        TotalRevenue = row["TotalRevenue"] != DBNull.Value
                            ? Convert.ToInt32(row["TotalRevenue"])
                            : 0,
                        CemeteryPlotsUsed = row["CemeteryPlotsUsed"] != DBNull.Value
                            ? Convert.ToInt32(row["CemeteryPlotsUsed"])
                            : 0,
                        BurialsThisYear = row["BurialsThisYear"] != DBNull.Value
                            ? Convert.ToInt32(row["BurialsThisYear"])
                            : 0,
                        Remarks = row["Remarks"]?.ToString() ?? ""
                    };

                    // Load related data from database
                    family.Members = LoadFamilyMembers(familyId);
                    family.Subscriptions = LoadFamilySubscriptions(familyId);
                    family.CemeteryDetails = LoadFamilyCemeteryDetails(familyId);
                    family.Timeline = LoadFamilyTimeline(familyId);

                    families.Add(family);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error converting family row: {ex.Message}");
                }
            }

            return families;
        }

        /// <summary>
        /// Load single family report data by ID
        /// </summary>
        private FamilyReportData LoadFamilyReportData(int familyId)
        {
            try
            {
                var param = new SqlParameter("@family_id", familyId);
                DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyBasicDetailsForExport", param);

                if (dt.Rows.Count == 0)
                    return null;

                DataRow row = dt.Rows[0];
                var family = new FamilyReportData
                {
                    FamilyName = row["FamilyName"]?.ToString() ?? "Unknown",
                    RegistrationDate = row["RegistrationDate"] != DBNull.Value
                        ? Convert.ToDateTime(row["RegistrationDate"]).ToString("dd MMM yyyy")
                        : "-",
                    Contact = row["Phone"]?.ToString() ?? "",
                    Email = row["Email"]?.ToString() ?? "",
                    Address = row["Address"]?.ToString() ?? "",
                    OutstandingDues = row["OutstandingDues"] != DBNull.Value
                        ? Convert.ToInt32(row["OutstandingDues"])
                        : 0,
                    ActiveSubscriptions = row["ActiveSubscriptions"] != DBNull.Value
                        ? Convert.ToInt32(row["ActiveSubscriptions"])
                        : 0,
                    TotalRevenue = row["TotalRevenue"] != DBNull.Value
                        ? Convert.ToInt32(row["TotalRevenue"])
                        : 0,
                    CemeteryPlotsUsed = row["CemeteryPlotsUsed"] != DBNull.Value
                        ? Convert.ToInt32(row["CemeteryPlotsUsed"])
                        : 0,
                    BurialsThisYear = row["BurialsThisYear"] != DBNull.Value
                        ? Convert.ToInt32(row["BurialsThisYear"])
                        : 0,
                    Remarks = row["Remarks"]?.ToString() ?? ""
                };

                family.Members = LoadFamilyMembers(familyId);
                family.Subscriptions = LoadFamilySubscriptions(familyId);
                family.CemeteryDetails = LoadFamilyCemeteryDetails(familyId);
                family.Timeline = LoadFamilyTimeline(familyId);

                return family;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading family: {ex.Message}");
                return null;
            }
        }

        // ═════════════════════════════════════════════════════════════════════════
        // DATA LOADERS - Load related data from database
        // ═════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Load family members for the report
        /// </summary>
        private List<MemberData> LoadFamilyMembers(int familyId)
        {
            var members = new List<MemberData>();

            try
            {
                var param = new SqlParameter("@family_id", familyId);
                DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyMembers", param);

                foreach (DataRow row in dt.Rows)
                {
                    members.Add(new MemberData
                    {
                        Name = row["Name"]?.ToString() ?? "",
                        DateOfBirth = row["DOB"] != DBNull.Value
                            ? Convert.ToDateTime(row["DOB"]).ToString("dd-MMM-yyyy")
                            : "-",
                        Status = row["Status"]?.ToString() ?? "Active",
                        Relationship = row["Relationship"]?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading family members: {ex.Message}");
            }

            return members;
        }

        /// <summary>
        /// Load cemetery subscriptions for the report
        /// </summary>
        private List<SubscriptionData> LoadFamilySubscriptions(int familyId)
        {
            var subscriptions = new List<SubscriptionData>();

            try
            {
                var param = new SqlParameter("@family_id", familyId);
                DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilySubscriptions", param);

                foreach (DataRow row in dt.Rows)
                {
                    subscriptions.Add(new SubscriptionData
                    {
                        Year = row["Year"] != DBNull.Value
                            ? Convert.ToInt32(row["Year"]).ToString()
                            : "-",
                        Amount = row["Amount"] != DBNull.Value
                            ? Convert.ToInt32(row["Amount"])
                            : 0,
                        Status = row["Status"]?.ToString() ?? "Pending",
                        PaidDate = row["PaidDate"] != DBNull.Value
                            ? Convert.ToDateTime(row["PaidDate"]).ToString("dd-MMM-yyyy")
                            : "-"
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading subscriptions: {ex.Message}");
            }

            return subscriptions;
        }

        /// <summary>
        /// Load cemetery details for the report
        /// </summary>
        private List<CemeteryDetailData> LoadFamilyCemeteryDetails(int familyId)
        {
            var details = new List<CemeteryDetailData>();

            try
            {
                var param = new SqlParameter("@family_id", familyId);
                DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyCemeteryDetails", param);

                foreach (DataRow row in dt.Rows)
                {
                    details.Add(new CemeteryDetailData
                    {
                        DeceasedName = row["DeceasedName"]?.ToString() ?? "",
                        DateOfDeath = row["DateOfDeath"] != DBNull.Value
                            ? Convert.ToDateTime(row["DateOfDeath"]).ToString("dd-MMM-yyyy")
                            : "-",
                        BurialDate = row["BurialDate"] != DBNull.Value
                            ? Convert.ToDateTime(row["BurialDate"]).ToString("dd-MMM-yyyy")
                            : "-",
                        BurialPlace = row["BurialPlace"]?.ToString() ?? "",
                        GraveNumber = row["GraveNumber"]?.ToString() ?? "-"
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading cemetery details: {ex.Message}");
            }

            return details;
        }

        /// <summary>
        /// Load family timeline (important events and dates)
        /// </summary>
        private List<TimelineEventData> LoadFamilyTimeline(int familyId)
        {
            var events = new List<TimelineEventData>();

            try
            {
                var param = new SqlParameter("@family_id", familyId);
                DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyTimeline", param);

                foreach (DataRow row in dt.Rows)
                {
                    events.Add(new TimelineEventData
                    {
                        EventType = row["EventType"]?.ToString() ?? "",
                        EventDate = row["EventDate"] != DBNull.Value
                            ? Convert.ToDateTime(row["EventDate"]).ToString("dd-MMM-yyyy")
                            : "-",
                        Description = row["EventDescription"]?.ToString() ?? "",
                        Category = row["Category"]?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading timeline: {ex.Message}");
            }

            return events;
        }
    }

    // ═════════════════════════════════════════════════════════════════════════════
    // HELPER CLASSES - Additional data models for timeline and cemetery details
    // Add these to ModernPdfGenerator.cs
    // ═════════════════════════════════════════════════════════════════════════════

    /*
    internal class CemeteryDetailData
    {
        public string DeceasedName { get; set; }
        public string DateOfDeath { get; set; }
        public string BurialDate { get; set; }
        public string BurialPlace { get; set; }
        public string GraveNumber { get; set; }
    }

    internal class TimelineEventData
    {
        public string EventType { get; set; }
        public string EventDate { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
    }

    // Add these properties to FamilyReportData class:
    public List<CemeteryDetailData> CemeteryDetails { get; set; } = new List<CemeteryDetailData>();
    public List<TimelineEventData> Timeline { get; set; } = new List<TimelineEventData>();
    */
}
