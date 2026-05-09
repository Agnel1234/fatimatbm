/// QUICK START - Copy & Paste These Methods Into Form1.cs
/// ═══════════════════════════════════════════════════════════════════════════

namespace TestFat
{
    /// Step 1: Add this button to your Form1 buttons panel
    /// ────────────────────────────────────────────────────
    /// Inside your ApplyTheme() or button initialization method, add:
    ///
    /// Button btnExportModernFamilyPDF = new Button
    /// {
    ///     Text = "Export Modern PDF",
    ///     Name = "btnExportModernFamilyPDF",
    ///     Location = new Point(620, 10),
    ///     Width = 140,
    ///     Height = 32,
    ///     Font = new Font("Georgia", 9F)
    /// };
    /// btnExportModernFamilyPDF.Click += BtnExportModernFamilyPDF_Click;
    /// AppTheme.StyleButtonPrimary(btnExportModernFamilyPDF);
    /// AppTheme.SetIcon(btnExportModernFamilyPDF, AppTheme.IconDownload(), "Export PDF");
    /// this.topButtonPanel.Controls.Add(btnExportModernFamilyPDF);


    /// Step 2: Copy ALL these methods into Form1.cs
    /// ──────────────────────────────────────────────

    partial class Form1 // Add these methods to your Form1 class
    {
        // ── EVENT HANDLER ──────────────────────────────────────────────────────

        /// <summary>
        /// Export all families to a modern multi-page PDF report
        /// </summary>
        private void BtnExportModernFamilyPDF_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult choice = MessageBox.Show(
                    "Export all families?\n\n[Yes] Export all families in the system\n[No] Export current selection only",
                    "Export PDF Report",
                    MessageBoxButtons.YesNoCancel);

                if (choice == DialogResult.Cancel)
                    return;

                List<FamilyReportData> families = new List<FamilyReportData>();

                if (choice == DialogResult.Yes)
                {
                    // Export all families
                    DataTable allFamilies = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyBasicDetailsForExport");
                    families = ConvertToFamilyReportData(allFamilies);
                }
                else if (choice == DialogResult.No)
                {
                    // Export selected families from grid
                    if (familygrid.SelectedRows.Count == 0)
                    {
                        MessageBox.Show("Please select at least one family.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    foreach (DataGridViewRow row in familygrid.SelectedRows)
                    {
                        int familyId = Convert.ToInt32(row.Cells["FamilyID"].Value);
                        var family = LoadFamilyReportData(familyId);
                        if (family != null)
                            families.Add(family);
                    }
                }

                if (families.Count == 0)
                {
                    MessageBox.Show("No families to export.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Generate PDF
                string filename = "family_report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), filename);

                ModernPdfGenerator.GenerateModernFamilyReport(families, filePath);

                ThemedDialog.Info($"Report exported successfully ({families.Count} families).\n\nSaved to:\n{filePath}", "Export Complete", this);

                // Open PDF in default viewer
                System.Diagnostics.Process.Start(filePath);
            }
            catch (Exception ex)
            {
                ThemedDialog.Error($"Export failed: {ex.Message}", "Export Error", this);
            }
        }


        // ── DATA CONVERSION ────────────────────────────────────────────────────

        /// <summary>
        /// Convert all families DataTable to FamilyReportData list
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

                    // Load related data
                    family.Members = LoadFamilyMembers(familyId);
                    family.Subscriptions = LoadFamilySubscriptions(familyId);

                    families.Add(family);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error converting family row: {ex.Message}");
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
                var param = new System.Data.SqlClient.SqlParameter("@family_id", familyId);
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

                return family;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading family: {ex.Message}");
                return null;
            }
        }


        // ── DATA LOADERS ───────────────────────────────────────────────────────

        /// <summary>
        /// Load family members for the report
        /// </summary>
        private List<MemberData> LoadFamilyMembers(int familyId)
        {
            var members = new List<MemberData>();

            try
            {
                var param = new System.Data.SqlClient.SqlParameter("@family_id", familyId);
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
                System.Diagnostics.Debug.WriteLine($"Error loading family members: {ex.Message}");
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
                var param = new System.Data.SqlClient.SqlParameter("@family_id", familyId);
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
                System.Diagnostics.Debug.WriteLine($"Error loading subscriptions: {ex.Message}");
            }

            return subscriptions;
        }
    }
}


/// INTEGRATION CHECKLIST
/// ═══════════════════════════════════════════════════════════════════════════
///
/// ☐ 1. Copy ModernPdfGenerator.cs to your project folder
/// ☐ 2. Add using statements if needed (already included in ModernPdfGenerator)
/// ☐ 3. Copy all methods above into Form1.cs
/// ☐ 4. Add button to your form (in ApplyTheme() method)
/// ☐ 5. Build the project to check for compilation errors
/// ☐ 6. Verify your database has these stored procedures:
///      - sp_GetFamilyBasicDetailsForExport
///      - sp_GetFamilyMembers
///      - sp_GetFamilySubscriptions
/// ☐ 7. Adjust stored procedure names/parameters if different
/// ☐ 8. Test exporting a small family first
/// ☐ 9. Check the generated PDF in Documents folder
/// ☐ 10. Customize colors/layout in ModernPdfGenerator.cs if needed
///
/// EXPECTED DATABASE COLUMNS
/// ═══════════════════════════════════════════════════════════════════════════
///
/// sp_GetFamilyBasicDetailsForExport should return:
///   - FamilyID (int)
///   - FamilyName (varchar)
///   - RegistrationDate (datetime)
///   - Phone (varchar)
///   - Email (varchar)
///   - Address (varchar)
///   - OutstandingDues (int)
///   - ActiveSubscriptions (int)
///   - TotalRevenue (int)
///   - CemeteryPlotsUsed (int)
///   - BurialsThisYear (int)
///   - Remarks (varchar)
///
/// sp_GetFamilyMembers should return:
///   - Name (varchar)
///   - DOB (datetime)
///   - Status (varchar)
///   - Relationship (varchar)
///
/// sp_GetFamilySubscriptions should return:
///   - Year (int)
///   - Amount (int)
///   - Status (varchar - "Paid", "Pending", "Overdue")
///   - PaidDate (datetime, nullable)
///
/// TROUBLESHOOTING
/// ═══════════════════════════════════════════════════════════════════════════
///
/// Q: "Object reference not set to an instance of an object"
/// A: Check that DataTable from stored procedure has data and column names match
///
/// Q: "File not found" or file won't open
/// A: Verify Documents folder path is correct, check file permissions
///
/// Q: PDF is blank or missing data
/// A: Check that lists/tables have data before passing to ModernPdfGenerator
///
/// Q: Layout looks different than expected
/// A: Verify page margins (40pt on all sides), check font sizes match
///
/// Q: Colors are wrong
/// A: Verify PdfColor RGB values match AppTheme.cs definitions
///
/// PERFORMANCE TIPS
/// ═══════════════════════════════════════════════════════════════════════════
///
/// For large exports (100+ families):
/// - Wrap in Task.Run() for async execution
/// - Show progress bar or status message
/// - Consider exporting in batches
///
/// Example async wrapper:
///
/// var progress = new ProgressForm();
/// progress.Show(this);
///
/// await Task.Run(() => {
///     ModernPdfGenerator.GenerateModernFamilyReport(families, filePath);
/// });
///
/// progress.Close();
/// ThemedDialog.Info("Export complete!", "Success", this);
