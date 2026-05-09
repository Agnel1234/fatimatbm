# Modern PDF Report Generation Guide

## Overview
The `ModernPdfGenerator` class creates professional, multi-page PDF reports using your app's color theme (Navy, Teal, Gold). Perfect for exporting family records, cemetery data, and subscriptions.

---

## PDF Structure

### Page 1: Cover Page
```
╔════════════════════════════════════════════════════════════════╗
║                                                                ║
║                    ═══════════════════════════                 ║
║                    ✝ (Large Gold Cross)                        ║
║                                                                ║
║          FAMILY & CEMETERY RECORDS                            ║
║    Our Lady of Fatima Church, Tambaram                        ║
║                                                                ║
║               Total Records: 12 Families                      ║
║                                                                ║
║          Generated: 24 April 2026, 02:30 PM                   ║
║                                                                ║
╚════════════════════════════════════════════════════════════════╝
```

**Features:**
- Professional title in Gold
- Church name and location
- Total record count
- Generation timestamp
- Navy background with Gold decorative bars

---

### Page 2: Summary Overview
```
╔════════════════════════════════════════════════════════════════╗
║ SUMMARY OVERVIEW                                               ║
├──────────────────┬─────────────────┬──────────────────────────┤
│  Total Families  │Active Subscribers│Outstanding Dues          │
│       12         │       45         │    ₹47,500               │
├──────────────────┼─────────────────┼──────────────────────────┤
│  Active Subscr.  │  Pending Payments│                          │
│       32         │        8         │                          │
└──────────────────┴─────────────────┴──────────────────────────┘

STATISTICS
┌────────────────────────┬──────────────────────────────────────┐
│ Metric                 │ Value                                │
├────────────────────────┼──────────────────────────────────────┤
│ Total Members          │ 45                                   │
│ Burials This Year      │ 2                                    │
│ Total Revenue          │ ₹1,92,000                            │
│ Collection Rate        │ 94.2%                                │
│ Cemetery Plots Used    │ 18                                   │
└────────────────────────┴──────────────────────────────────────┘
```

**Features:**
- Summary metrics in Teal boxes with Gold borders
- Key statistics in styled tables
- Navy headers with Gold text
- Alternating row colors (light Teal)

---

### Pages 3-N: Family Detail Pages
```
╔════════════════════════════════════════════════════════════════╗
║ ═══════════════════════════════════════════════════════════   ║
║ 👥 FAMILY: KHAN FAMILY                                         ║
║
║ 📋 FAMILY INFORMATION
│ Registered:    15 Jan 2020
│ Contact:       +91-9876543210
│ Email:         khan@email.com
│ Address:       123 Main Street, Tambaram
│
│ 👥 MEMBERS (5 Total)
│ ┌──────────────┬──────────────┬──────────┬──────────────┐
│ │ Name         │ DOB          │ Status   │ Relationship │
│ ├──────────────┼──────────────┼──────────┼──────────────┤
│ │ Mohammed Khan│ 15-Mar-1960  │ ✓ Active │ Patriarch    │
│ │ Fatima Khan  │ 22-Jul-1962  │ ✓ Active │ Matriarch    │
│ │ Ahmed Khan   │ 10-May-1985  │ ✓ Active │ Son          │
│ └──────────────┴──────────────┴──────────┴──────────────┘
│
│ 🪦 CEMETERY SUBSCRIPTIONS
│ ┌──────┬──────────┬──────────────┬──────────────────┐
│ │ Year │ Amount   │ Status       │ Paid Date        │
│ ├──────┼──────────┼──────────────┼──────────────────┤
│ │ 2024 │ ₹5,000   │ ✓ Paid       │ 15-Jan-2024      │
│ │ 2025 │ ₹5,500   │ ✓ Paid       │ 10-Jan-2025      │
│ │ 2026 │ ₹6,000   │ ⚠ Pending    │ -                │
│ └──────┴──────────┴──────────────┴──────────────────┘
│
│ ┌──────────────────────────────────────────────────┐
│ │ 📝 REMARKS                                        │
│ │ Regular subscriber since 2020. Maintains good    │
│ │ payment record. Plot location: Section A, Row 3  │
│ └──────────────────────────────────────────────────┘
╚════════════════════════════════════════════════════════════════╝
```

**Features:**
- Teal divider line under family name
- Color-coded sections with icons
- Family information box
- Members table with status
- Subscriptions table with payment colors
- Remarks in styled callout box
- Navy text with Teal/Gold accents

---

### Last Page: Statistical Summary
```
╔════════════════════════════════════════════════════════════════╗
║ STATISTICAL SUMMARY                                            ║
│
│ Payment Status Breakdown
│ ┌──────────────┬────────┬──────────────┐
│ │ Status       │ Count  │ Percentage   │
│ ├──────────────┼────────┼──────────────┤
│ │ ✓ Paid       │   32   │    80.3%     │
│ │ ⚠ Pending    │    8   │    17.6%     │
│ │ ✗ Overdue    │    2   │     5.2%     │
│ └──────────────┴────────┴──────────────┘
│
│ Top Outstanding Dues
│ ┌──────────────────────┬──────────────┐
│ │ Family Name          │ Amount       │
│ ├──────────────────────┼──────────────┤
│ │ Sheikh Family        │ ₹11,700      │
│ │ Hassan Merchants Grp │ ₹9,800       │
│ │ Al-Noor Community    │ ₹8,500       │
│ │ Hussain Family       │ ₹7,200       │
│ │ Others               │ ₹10,300      │
│ └──────────────────────┴──────────────┘
╚════════════════════════════════════════════════════════════════╝
```

**Features:**
- Payment status breakdown table
- Top outstanding families list
- Color-coded status indicators
- Navy headers with Gold text
- Percentage calculations

---

## Header & Footer (Every Page)

### Header
```
╔════════════════════════════════════════════════════════════════╗
║ ✝ Our Lady of Fatima Church - Family & Cemetery Records       ║
║   Tambaram, Chennai                                            ║
║ ═══════════════════════════════════════════════════════════   ║
```

### Footer
```
╠════════════════════════════════════════════════════════════════╣
║ © Our Lady of Fatima Church, Tambaram, Chennai  Page [PAGE_NUMBER]║
╚════════════════════════════════════════════════════════════════╝
```

---

## Color Scheme

| Element | Color Code | RGB | Usage |
|---------|-----------|-----|-------|
| **Navy (Primary)** | #1A2D42 | 26, 45, 66 | Headers, titles, borders |
| **Teal (Secondary)** | #2C6E7A | 44, 110, 122 | Accents, dividers, boxes |
| **Gold (Highlight)** | #C9A84C | 201, 168, 76 | Text on Navy, borders, icons |
| **OffWhite (Background)** | #F0F4F7 | 240, 244, 247 | Page background |
| **Row Alt (Striping)** | #F0F7FA | 240, 247, 250 | Alternating table rows |
| **Paid Badge** | #D4EDDA | 212, 237, 218 | Success/Paid status |
| **Pending Badge** | #FFF3CD | 255, 243, 205 | Warning/Pending status |
| **Overdue Badge** | #F8D7DA | 248, 215, 218 | Error/Overdue status |
| **White** | #FFFFFF | 255, 255, 255 | Text on colored backgrounds |
| **Light Gray** | #C8D2DC | 200, 210, 220 | Table borders |

---

## Implementation in Form1.cs

### Step 1: Add Button to Export Modern PDF
```csharp
// In your Form1_Load or theme initialization:
Button btnExportModernPDF = new Button 
{ 
    Text = "Export Modern PDF",
    Location = new Point(x, y),
    Width = 120,
    Height = 32
};
btnExportModernPDF.Click += BtnExportModernPDF_Click;
AppTheme.StyleButtonPrimary(btnExportModernPDF);
AppTheme.SetIcon(btnExportModernPDF, AppTheme.IconDownload(), "Export PDF");
this.Controls.Add(btnExportModernPDF);
```

### Step 2: Implement Click Handler
```csharp
private void BtnExportModernPDF_Click(object sender, EventArgs e)
{
    try
    {
        // Get all families from database
        DataTable familiesTable = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyBasicDetailsForExport");
        List<FamilyReportData> families = ConvertToFamilyReportData(familiesTable);

        // Generate PDF
        string filename = "family_report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";
        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), filename);

        ModernPdfGenerator.GenerateModernFamilyReport(families, filePath);

        ThemedDialog.Info($"Report exported successfully.\n\nSaved to: {filePath}", "Export Complete", this);
        System.Diagnostics.Process.Start(filePath); // Open in PDF viewer
    }
    catch (Exception ex)
    {
        ThemedDialog.Error($"Export failed: {ex.Message}", "Export Error", this);
    }
}
```

### Step 3: Convert DataTable to Report Format
```csharp
private List<FamilyReportData> ConvertToFamilyReportData(DataTable familiesTable)
{
    var families = new List<FamilyReportData>();

    foreach (DataRow row in familiesTable.Rows)
    {
        int familyId = Convert.ToInt32(row["FamilyID"]);
        var family = new FamilyReportData
        {
            FamilyName = row["FamilyName"]?.ToString() ?? "",
            RegistrationDate = Convert.ToDateTime(row["RegistrationDate"]).ToString("dd MMM yyyy"),
            Contact = row["Phone"]?.ToString() ?? "",
            Email = row["Email"]?.ToString() ?? "",
            Address = row["Address"]?.ToString() ?? "",
            OutstandingDues = Convert.ToInt32(row["OutstandingDues"] ?? 0),
            ActiveSubscriptions = Convert.ToInt32(row["ActiveSubscriptions"] ?? 0),
            TotalRevenue = Convert.ToInt32(row["TotalRevenue"] ?? 0),
            CemeteryPlotsUsed = Convert.ToInt32(row["CemeteryPlotsUsed"] ?? 0),
            BurialsThisYear = Convert.ToInt32(row["BurialsThisYear"] ?? 0),
            Remarks = row["Remarks"]?.ToString() ?? "",
            Members = LoadFamilyMembers(familyId),
            Subscriptions = LoadFamilySubscriptions(familyId)
        };

        families.Add(family);
    }

    return families;
}

private List<MemberData> LoadFamilyMembers(int familyId)
{
    var members = new List<MemberData>();
    var param = new SqlParameter("@family_id", familyId);
    DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyMembers", param);

    foreach (DataRow row in dt.Rows)
    {
        members.Add(new MemberData
        {
            Name = row["Name"]?.ToString() ?? "",
            DateOfBirth = Convert.ToDateTime(row["DOB"]).ToString("dd-MMM-yyyy"),
            Status = row["Status"]?.ToString() ?? "Active",
            Relationship = row["Relationship"]?.ToString() ?? ""
        });
    }

    return members;
}

private List<SubscriptionData> LoadFamilySubscriptions(int familyId)
{
    var subscriptions = new List<SubscriptionData>();
    var param = new SqlParameter("@family_id", familyId);
    DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilySubscriptions", param);

    foreach (DataRow row in dt.Rows)
    {
        subscriptions.Add(new SubscriptionData
        {
            Year = Convert.ToInt32(row["Year"]).ToString(),
            Amount = Convert.ToInt32(row["Amount"]),
            Status = row["Status"]?.ToString() ?? "Pending",
            PaidDate = row["PaidDate"] != DBNull.Value
                ? Convert.ToDateTime(row["PaidDate"]).ToString("dd-MMM-yyyy")
                : "-"
        });
    }

    return subscriptions;
}
```

---

## Features Included

✅ **Visual Design**
- Professional Navy/Teal/Gold color scheme
- Consistent headers and footers on every page
- Clean, modern typography
- Proper spacing and alignment
- Decorative dividers and borders

✅ **Data Presentation**
- Summary overview with key metrics
- Family detail pages with all information
- Color-coded status badges
- Icons and symbols for visual scanning
- Alternating row colors for readability
- Professional tables with proper borders

✅ **Multi-Page Reports**
- Cover page with document info
- Summary page with metrics
- Individual family pages (one per family)
- Statistics and summary page
- Automatic pagination
- Professional headers/footers
- Page numbering

✅ **Additional Features**
- Remarks/notes sections
- Cemetery subscription tracking
- Member information
- Payment status tracking
- Collection rate calculations
- Top outstanding families list
- Church branding and information

---

## Database Requirements

Your stored procedures should return:

**sp_GetFamilyBasicDetailsForExport**
```sql
SELECT 
    FamilyID, FamilyName, RegistrationDate, Phone, Email, Address,
    OutstandingDues, ActiveSubscriptions, TotalRevenue, 
    CemeteryPlotsUsed, BurialsThisYear, Remarks
```

**sp_GetFamilyMembers**
```sql
SELECT Name, DOB, Status, Relationship WHERE family_id = @family_id
```

**sp_GetFamilySubscriptions**
```sql
SELECT Year, Amount, Status, PaidDate WHERE family_id = @family_id
```

---

## Tips & Customization

### To modify colors:
Edit the color definitions at the top of ModernPdfGenerator.cs:
```csharp
private static readonly PdfColor ColorNavy = new PdfColor(26, 45, 66);
// Change RGB values as needed
```

### To add more data sections:
1. Add properties to `FamilyReportData` class
2. Create new `Draw*Section()` method in ModernPdfGenerator
3. Call it from `CreateFamilyDetailPage()`

### To customize page layout:
- Modify `CreateFamilyDetailPage()` method
- Adjust X/Y coordinates for different layouts
- Add or remove sections as needed

### To change fonts:
Edit font definitions at the top:
```csharp
private static readonly PdfStandardFont FontHeaderBold 
    = new PdfStandardFont(PdfFontFamily.Helvetica, 12f, PdfFontStyle.Bold);
```

---

## Output Sample Filenames

- `family_report_20260424_023045.pdf` — Full family report
- `family_report_5_20260424_023045.pdf` — Single family (ID 5)
- `anbiyam_export_20260424_023045.pdf` — Anbiyam (members) export

All files are saved to: `Documents\` folder

---

## Troubleshooting

**PDF looks empty or has no data:**
- Check that DataTable/List has data before calling generator
- Verify database stored procedures return results
- Check that column names match property names

**Formatting looks off:**
- Verify page margins are correct
- Check font names match PDF font families
- Ensure colors are valid PdfColor objects

**Performance issues with large reports:**
- For 100+ families, consider generating in batches
- Use async/await to prevent UI blocking
- Show progress dialog while generating

---

## Example Output File Size

- Single family page: ~30-40 KB
- 10 families with summary: ~200-300 KB
- 50 families with all pages: ~800 KB - 1.2 MB

Files are optimized and compressed by Syncfusion PDF library.
