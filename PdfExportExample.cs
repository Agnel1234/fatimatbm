using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace TestFat
{
    /// USAGE EXAMPLES - How to use ModernPdfGenerator in your Form1.cs
    ///
    /// Example 1: Export all families with modern multi-page report
    /// ────────────────────────────────────────────────────────────
    ///
    /// private void btnExportModernPDF_Click(object sender, EventArgs e)
    /// {
    ///     try
    ///     {
    ///         // Fetch all family data from database
    ///         DataTable familiesTable = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyBasicDetailsForExport");
    ///
    ///         // Convert to list of FamilyReportData
    ///         List<FamilyReportData> families = ConvertToFamilyReportData(familiesTable);
    ///
    ///         // Generate PDF
    ///         string filename = "family_report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";
    ///         string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), filename);
    ///
    ///         ModernPdfGenerator.GenerateModernFamilyReport(families, filePath);
    ///
    ///         ThemedDialog.Info($"Report exported successfully.\n\nSaved to: {filePath}", "Export Complete", this);
    ///         System.Diagnostics.Process.Start(filePath); // Open PDF
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         ThemedDialog.Error($"Export failed: {ex.Message}", "Export Error", this);
    ///     }
    /// }
    ///
    ///
    /// Example 2: Convert DataTable to FamilyReportData list
    /// ──────────────────────────────────────────────────────
    ///
    /// private List<FamilyReportData> ConvertToFamilyReportData(DataTable familiesTable)
    /// {
    ///     var families = new List<FamilyReportData>();
    ///
    ///     foreach (DataRow row in familiesTable.Rows)
    ///     {
    ///         int familyId = Convert.ToInt32(row["FamilyID"]);
    ///         var family = new FamilyReportData
    ///         {
    ///             FamilyName = row["FamilyName"]?.ToString() ?? "",
    ///             RegistrationDate = Convert.ToDateTime(row["RegistrationDate"]).ToString("dd MMM yyyy"),
    ///             Contact = row["Phone"]?.ToString() ?? "",
    ///             Email = row["Email"]?.ToString() ?? "",
    ///             Address = row["Address"]?.ToString() ?? "",
    ///             OutstandingDues = Convert.ToInt32(row["OutstandingDues"] ?? 0),
    ///             ActiveSubscriptions = Convert.ToInt32(row["ActiveSubscriptions"] ?? 0),
    ///             TotalRevenue = Convert.ToInt32(row["TotalRevenue"] ?? 0),
    ///             CemeteryPlotsUsed = Convert.ToInt32(row["CemeteryPlotsUsed"] ?? 0),
    ///             BurialsThisYear = Convert.ToInt32(row["BurialsThisYear"] ?? 0),
    ///             Remarks = row["Remarks"]?.ToString() ?? ""
    ///         };
    ///
    ///         // Load members
    ///         family.Members = LoadFamilyMembers(familyId);
    ///
    ///         // Load subscriptions
    ///         family.Subscriptions = LoadFamilySubscriptions(familyId);
    ///
    ///         families.Add(family);
    ///     }
    ///
    ///     return families;
    /// }
    ///
    ///
    /// Example 3: Load family members for the report
    /// ────────────────────────────────────────────
    ///
    /// private List<MemberData> LoadFamilyMembers(int familyId)
    /// {
    ///     var members = new List<MemberData>();
    ///     var param = new SqlParameter("@family_id", familyId);
    ///     DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyMembers", param);
    ///
    ///     foreach (DataRow row in dt.Rows)
    ///     {
    ///         members.Add(new MemberData
    ///         {
    ///             Name = row["Name"]?.ToString() ?? "",
    ///             DateOfBirth = Convert.ToDateTime(row["DOB"]).ToString("dd-MMM-yyyy"),
    ///             Status = row["Status"]?.ToString() ?? "Active",
    ///             Relationship = row["Relationship"]?.ToString() ?? ""
    ///         });
    ///     }
    ///
    ///     return members;
    /// }
    ///
    ///
    /// Example 4: Load cemetery subscriptions for the report
    /// ─────────────────────────────────────────────────────
    ///
    /// private List<SubscriptionData> LoadFamilySubscriptions(int familyId)
    /// {
    ///     var subscriptions = new List<SubscriptionData>();
    ///     var param = new SqlParameter("@family_id", familyId);
    ///     DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilySubscriptions", param);
    ///
    ///     foreach (DataRow row in dt.Rows)
    ///     {
    ///         subscriptions.Add(new SubscriptionData
    ///         {
    ///             Year = Convert.ToInt32(row["Year"]).ToString(),
    ///             Amount = Convert.ToInt32(row["Amount"]),
    ///             Status = row["Status"]?.ToString() ?? "Pending",
    ///             PaidDate = row["PaidDate"] != DBNull.Value
    ///                 ? Convert.ToDateTime(row["PaidDate"]).ToString("dd-MMM-yyyy")
    ///                 : "-"
    ///         });
    ///     }
    ///
    ///     return subscriptions;
    /// }
    ///
    ///
    /// Example 5: Alternative - Export single family with modern layout
    /// ──────────────────────────────────────────────────────────────
    ///
    /// private void btnExportSingleFamilyPDF_Click(object sender, EventArgs e)
    /// {
    ///     if (familygrid.SelectedRows.Count != 1)
    ///     {
    ///         ThemedDialog.Warn("Please select one family to export.", "Selection Required", this);
    ///         return;
    ///     }
    ///
    ///     try
    ///     {
    ///         int familyId = Convert.ToInt32(familygrid.SelectedRows[0].Cells["FamilyID"].Value);
    ///
    ///         // Create single-family list and generate report
    ///         var families = new List<FamilyReportData> { LoadFamilyReportData(familyId) };
    ///
    ///         string filename = $"family_report_{familyId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
    ///         string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), filename);
    ///
    ///         ModernPdfGenerator.GenerateModernFamilyReport(families, filePath);
    ///
    ///         ThemedDialog.Info($"Report exported successfully.\n\nSaved to: {filePath}", "Export Complete", this);
    ///         System.Diagnostics.Process.Start(filePath);
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         ThemedDialog.Error($"Export failed: {ex.Message}", "Export Error", this);
    ///     }
    /// }
    ///
    /// private FamilyReportData LoadFamilyReportData(int familyId)
    /// {
    ///     var param = new SqlParameter("@family_id", familyId);
    ///     DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyBasicDetails", param);
    ///
    ///     if (dt.Rows.Count == 0) return null;
    ///
    ///     DataRow row = dt.Rows[0];
    ///     return new FamilyReportData
    ///     {
    ///         FamilyName = row["FamilyName"]?.ToString() ?? "",
    ///         RegistrationDate = Convert.ToDateTime(row["RegistrationDate"]).ToString("dd MMM yyyy"),
    ///         Contact = row["Phone"]?.ToString() ?? "",
    ///         Email = row["Email"]?.ToString() ?? "",
    ///         Address = row["Address"]?.ToString() ?? "",
    ///         Remarks = row["Remarks"]?.ToString() ?? "",
    ///         OutstandingDues = Convert.ToInt32(row["OutstandingDues"] ?? 0),
    ///         ActiveSubscriptions = Convert.ToInt32(row["ActiveSubscriptions"] ?? 0),
    ///         TotalRevenue = Convert.ToInt32(row["TotalRevenue"] ?? 0),
    ///         CemeteryPlotsUsed = Convert.ToInt32(row["CemeteryPlotsUsed"] ?? 0),
    ///         BurialsThisYear = Convert.ToInt32(row["BurialsThisYear"] ?? 0),
    ///         Members = LoadFamilyMembers(familyId),
    ///         Subscriptions = LoadFamilySubscriptions(familyId)
    ///     };
    /// }
    ///
    ///
    /// FEATURES INCLUDED:
    /// ══════════════════
    /// ✓ Modern color scheme using AppTheme (Navy, Teal, Gold)
    /// ✓ Professional header and footer on every page
    /// ✓ Cover page with document title and metadata
    /// ✓ Summary overview with key metrics in styled boxes
    /// ✓ Detailed family pages with:
    ///   - Family information
    ///   - Members list with status
    ///   - Cemetery subscriptions with payment status
    ///   - Remarks/notes section
    /// ✓ Statistical summary page with:
    ///   - Payment status breakdown
    ///   - Top outstanding families
    ///   - Collection rate calculations
    /// ✓ Color-coded status badges (Paid/Pending/Overdue)
    /// ✓ Alternating row colors for better readability
    /// ✓ Professional typography and spacing
    /// ✓ Multi-page support with automatic pagination
    /// ✓ Icons and symbols (✓, ✗, ⚠, ✝, 👥, 🪦, 📋, 📝)
    ///
    /// REQUIRED STORED PROCEDURES:
    /// ══════════════════════════
    /// - sp_GetFamilyBasicDetailsForExport  (returns all families with summary data)
    /// - sp_GetFamilyBasicDetails           (returns single family with summary data)
    /// - sp_GetFamilyMembers                (returns family members)
    /// - sp_GetFamilySubscriptions          (returns cemetery subscriptions)
    ///
    /// INTEGRATION STEPS:
    /// ══════════════════
    /// 1. Add button to Form1.Designer.cs and set click event
    /// 2. Copy the example methods above into Form1.cs
    /// 3. Update stored procedure names to match your database
    /// 4. Wire up the button click event handler
    /// 5. Call the export method when user clicks the button
}
