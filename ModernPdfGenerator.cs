using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using Syncfusion.Pdf.Tables;

namespace TestFat
{
    internal static class ModernPdfGenerator
    {
        private static readonly PdfColor ColorNavy       = new PdfColor(26, 45, 66);
        private static readonly PdfColor ColorTeal       = new PdfColor(44, 110, 122);
        private static readonly PdfColor ColorGold       = new PdfColor(201, 168, 76);
        private static readonly PdfColor ColorOffWhite   = new PdfColor(240, 244, 247);
        private static readonly PdfColor ColorRowAlt     = new PdfColor(240, 247, 250);
        private static readonly PdfColor ColorPaidBg     = new PdfColor(212, 237, 218);
        private static readonly PdfColor ColorPaidFg     = new PdfColor(21, 87, 36);
        private static readonly PdfColor ColorPendingBg  = new PdfColor(255, 243, 205);
        private static readonly PdfColor ColorPendingFg  = new PdfColor(133, 100, 4);
        private static readonly PdfColor ColorOverdueBg  = new PdfColor(248, 215, 218);
        private static readonly PdfColor ColorOverdueFg  = new PdfColor(132, 32, 41);
        private static readonly PdfColor ColorWhite      = new PdfColor(255, 255, 255);
        private static readonly PdfColor ColorLightGray  = new PdfColor(200, 210, 220);

        // Font definitions
        private static readonly PdfStandardFont FontTitleBold   = new PdfStandardFont(PdfFontFamily.Helvetica, 18f, PdfFontStyle.Bold);
        private static readonly PdfStandardFont FontHeaderBold  = new PdfStandardFont(PdfFontFamily.Helvetica, 12f, PdfFontStyle.Bold);
        private static readonly PdfStandardFont FontSectionBold = new PdfStandardFont(PdfFontFamily.Helvetica, 11f, PdfFontStyle.Bold);
        private static readonly PdfStandardFont FontBody         = new PdfStandardFont(PdfFontFamily.Helvetica, 9f);
        private static readonly PdfStandardFont FontSmall        = new PdfStandardFont(PdfFontFamily.Helvetica, 8f);
        private static readonly PdfStandardFont FontSmallBold    = new PdfStandardFont(PdfFontFamily.Helvetica, 8f, PdfFontStyle.Bold);

        public static void GenerateModernFamilyReport(List<FamilyReportData> families, string filepath)
        {
            var pdfDocument = new PdfDocument();
            pdfDocument.PageSettings.Orientation = PdfPageOrientation.Portrait;
            pdfDocument.PageSettings.Margins.All = 40;

            // Apply header and footer templates
            ApplyHeaderTemplate(pdfDocument);
            ApplyFooterTemplate(pdfDocument);

            // Create cover page
            CreateCoverPage(pdfDocument, families.Count);

            // Create summary page
            CreateSummaryPage(pdfDocument, families);

            // Create detailed family pages
            foreach (var family in families)
                CreateFamilyDetailPage(pdfDocument, family);

            // Create statistics page
            CreateStatisticsPage(pdfDocument, families);

            pdfDocument.Save(filepath);
            pdfDocument.Close(true);
        }

        public static void ApplyHeaderFooter(PdfDocument doc)
        {
            ApplyHeaderTemplate(doc);
            ApplyFooterTemplate(doc);
        }

        private static void ApplyHeaderTemplate(PdfDocument doc)
        {
            float headerHeight = 60f;
            var headerBounds = new RectangleF(0, 0, doc.PageSettings.Width, headerHeight);
            var headerTemplate = new PdfPageTemplateElement(headerBounds);
            var g = headerTemplate.Graphics;

            // Navy background
            g.DrawRectangle(new PdfSolidBrush(ColorNavy),
                new RectangleF(0, 0, headerBounds.Width, headerHeight));

            // Teal left accent
            g.DrawRectangle(new PdfSolidBrush(ColorTeal),
                new RectangleF(0, 0, 4, headerHeight));

            // Gold right accent
            g.DrawRectangle(new PdfSolidBrush(ColorGold),
                new RectangleF(headerBounds.Width - 4, 0, 4, headerHeight));

            // Cross icon
            g.DrawString("✝", new PdfStandardFont(PdfFontFamily.Helvetica, 20f, PdfFontStyle.Bold),
                new PdfSolidBrush(ColorGold), new PointF(15, 8));

            // Title
            g.DrawString("Our Lady of Fatima Church - Family & Cemetery Records",
                FontSectionBold, new PdfSolidBrush(ColorGold), new PointF(45, 12));

            // Subtitle
            g.DrawString("Tambaram, Chennai",
                FontSmall, new PdfSolidBrush(ColorOffWhite), new PointF(45, 32));

            // Gold separator line
            g.DrawRectangle(new PdfSolidBrush(ColorGold),
                new RectangleF(0, headerHeight - 2, headerBounds.Width, 2));

            doc.Template.Top = headerTemplate;
        }

        private static void ApplyFooterTemplate(PdfDocument doc)
        {
            float footerHeight = 30f;
            var footerBounds = new RectangleF(0, doc.PageSettings.Height - footerHeight,
                doc.PageSettings.Width, footerHeight);
            var footerTemplate = new PdfPageTemplateElement(footerBounds);
            var g = footerTemplate.Graphics;

            // Navy background
            g.DrawRectangle(new PdfSolidBrush(ColorNavy),
                new RectangleF(0, 0, footerBounds.Width, footerHeight));

            // Left text
            g.DrawString("© Our Lady of Fatima Church, Tambaram, Chennai",
                FontSmall, new PdfSolidBrush(ColorOffWhite), new PointF(15, 8));

            // Right text - page number placeholder
            g.DrawString("Page [PAGE_NUMBER]",
                FontSmall, new PdfSolidBrush(ColorOffWhite),
                new PointF(footerBounds.Width - 80, 8));

            // Gold top separator
            g.DrawRectangle(new PdfSolidBrush(ColorGold),
                new RectangleF(0, 0, footerBounds.Width, 2));

            doc.Template.Bottom = footerTemplate;
        }

        private static void CreateCoverPage(PdfDocument doc, int familyCount)
        {
            PdfPage page = doc.Pages.Add();
            var g = page.Graphics;

            float pageWidth = page.GetClientSize().Width;
            float pageHeight = page.GetClientSize().Height;
            float centerX = pageWidth / 2;
            float centerY = pageHeight / 2;

            // Navy background panel
            g.DrawRectangle(new PdfSolidBrush(ColorNavy),
                new RectangleF(0, 0, pageWidth, pageHeight));

            // Gold decorative top bar
            g.DrawRectangle(new PdfSolidBrush(ColorGold),
                new RectangleF(0, 0, pageWidth, 8));

            // Gold decorative bottom bar
            g.DrawRectangle(new PdfSolidBrush(ColorGold),
                new RectangleF(0, pageHeight - 8, pageWidth, 8));

            // Large cross icon
            g.DrawString("✝", new PdfStandardFont(PdfFontFamily.Helvetica, 60f, PdfFontStyle.Bold),
                new PdfSolidBrush(ColorGold), new PointF(centerX - 30, centerY - 100));

            // Main title
            var titleRect = new RectangleF(0, centerY - 20, pageWidth, 60);
            g.DrawString("FAMILY & CEMETERY RECORDS",
                FontTitleBold, new PdfSolidBrush(ColorGold), titleRect,
                new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle));

            // Subtitle
            g.DrawString("Our Lady of Fatima Church, Tambaram",
                FontSectionBold, new PdfSolidBrush(ColorOffWhite),
                new PointF(centerX, centerY + 50),
                new PdfStringFormat(PdfTextAlignment.Center));

            // Record count
            var infoFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10f);
            g.DrawString($"Total Records: {familyCount} Families",
                infoFont, new PdfSolidBrush(ColorOffWhite),
                new PointF(centerX, centerY + 90),
                new PdfStringFormat(PdfTextAlignment.Center));

            // Date generated
            g.DrawString($"Generated: {DateTime.Now:dd MMMM yyyy, hh:mm tt}",
                FontSmall, new PdfSolidBrush(ColorOffWhite),
                new PointF(centerX, pageHeight - 40),
                new PdfStringFormat(PdfTextAlignment.Center));
        }

        private static void CreateSummaryPage(PdfDocument doc, List<FamilyReportData> families)
        {
            PdfPage page = doc.Pages.Add();
            var g = page.Graphics;
            float x = page.GetClientSize().Width / 2 - 120;
            float y = 50;

            // Title
            g.DrawString("SUMMARY OVERVIEW",
                FontHeaderBold, new PdfSolidBrush(ColorNavy),
                new PointF(x, y));

            y += 30;

            // Summary boxes
            DrawSummaryBox(g, x, y, "Total Families", families.Count.ToString(), ColorTeal);
            DrawSummaryBox(g, x + 150, y, "Total Members", families.Sum(f => f.Members.Count).ToString(), ColorTeal);

            y += 80;

            DrawSummaryBox(g, x, y, "Active Subscriptions", families.Sum(f => f.ActiveSubscriptions).ToString(), ColorTeal);
            DrawSummaryBox(g, x + 150, y, "Outstanding Dues", $"₹{families.Sum(f => f.OutstandingDues)}", ColorTeal);

            y += 100;

            // Statistics table
            g.DrawString("STATISTICS",
                FontSectionBold, new PdfSolidBrush(ColorNavy),
                new PointF(x, y));

            y += 20;

            var statsData = new string[,]
            {
                { "Metric", "Value" },
                { "Total Members", families.Sum(f => f.Members.Count).ToString() },
                { "Burials This Year", families.Sum(f => f.BurialsThisYear).ToString() },
                { "Total Revenue", $"₹{families.Sum(f => f.TotalRevenue)}" },
                { "Collection Rate", $"{CalculateCollectionRate(families):F1}%" },
                { "Cemetery Plots Used", families.Sum(f => f.CemeteryPlotsUsed).ToString() },
            };

            DrawStatsTable(g, x, y, statsData);
        }

        private static void CreateFamilyDetailPage(PdfDocument doc, FamilyReportData family)
        {
            PdfPage page = doc.Pages.Add();
            var g = page.Graphics;
            float pageWidth = page.GetClientSize().Width;
            float y = 50;
            const float leftMargin = 30;

            // Family header
            g.DrawRectangle(new PdfSolidBrush(ColorTeal),
                new RectangleF(leftMargin, y, pageWidth - 60, 5));

            y += 15;
            g.DrawString($"👥 FAMILY: {family.FamilyName}",
                FontSectionBold, new PdfSolidBrush(ColorNavy),
                new PointF(leftMargin, y));

            y += 20;

            // Family info table
            var familyInfoData = new string[,]
            {
                { "Registered", family.RegistrationDate },
                { "Contact", family.Contact },
                { "Email", family.Email },
                { "Address", family.Address }
            };

            y = DrawInfoSection(g, leftMargin, y, "Family Information", familyInfoData);

            y += 15;

            // Members table
            y = DrawMembersSection(g, leftMargin, y, family.Members);

            y += 15;

            // Cemetery subscriptions
            y = DrawSubscriptionsSection(g, leftMargin, y, family.Subscriptions);

            y += 15;

            // Remarks
            if (!string.IsNullOrEmpty(family.Remarks))
            {
                DrawRemarksBox(g, leftMargin, y, family.Remarks);
            }
        }

        private static void CreateStatisticsPage(PdfDocument doc, List<FamilyReportData> families)
        {
            PdfPage page = doc.Pages.Add();
            var g = page.Graphics;
            float x = 30;
            float y = 50;

            // Title
            g.DrawString("STATISTICAL SUMMARY",
                FontHeaderBold, new PdfSolidBrush(ColorNavy),
                new PointF(x, y));

            y += 35;

            // Payment status breakdown
            g.DrawString("Payment Status Breakdown",
                FontSectionBold, new PdfSolidBrush(ColorNavy),
                new PointF(x, y));

            y += 20;

            var statusData = new string[,]
            {
                { "Status", "Count", "Percentage" },
                { "✓ Paid", CountByStatus(families, "Paid").ToString(), $"{PercentageByStatus(families, "Paid"):F1}%" },
                { "⚠ Pending", CountByStatus(families, "Pending").ToString(), $"{PercentageByStatus(families, "Pending"):F1}%" },
                { "✗ Overdue", CountByStatus(families, "Overdue").ToString(), $"{PercentageByStatus(families, "Overdue"):F1}%" }
            };

            DrawStatsTable(g, x, y, statusData);

            y += 100;

            // Top outstanding families
            g.DrawString("Top Outstanding Dues",
                FontSectionBold, new PdfSolidBrush(ColorNavy),
                new PointF(x, y));

            y += 20;

            var topFamilies = families.OrderByDescending(f => f.OutstandingDues).Take(5).ToList();
            var topData = new List<string[]> { new[] { "Family Name", "Outstanding Amount" } };

            foreach (var family in topFamilies)
                topData.Add(new[] { family.FamilyName, $"₹{family.OutstandingDues}" });

            DrawStatsTable(g, x, y, topData.ToArray());
        }

        private static void DrawSummaryBox(PdfGraphics g, float x, float y, string label, string value, PdfColor bgColor)
        {
            // Box background
            g.DrawRectangle(new PdfSolidBrush(bgColor),
                new RectangleF(x, y, 120, 60));

            // Box border
            g.DrawRectangle(new PdfPen(ColorGold, 1.5f),
                new RectangleF(x, y, 120, 60));

            // Label
            g.DrawString(label, FontSmall, new PdfSolidBrush(ColorWhite),
                new PointF(x + 10, y + 8));

            // Value
            g.DrawString(value, new PdfStandardFont(PdfFontFamily.Helvetica, 16f, PdfFontStyle.Bold),
                new PdfSolidBrush(ColorGold),
                new PointF(x + 10, y + 28));
        }

        private static float DrawInfoSection(PdfGraphics g, float x, float y, string title, string[,] data)
        {
            g.DrawString($"📋 {title}",
                FontSectionBold, new PdfSolidBrush(ColorNavy),
                new PointF(x, y));
            y += 18;

            for (int i = 0; i < data.GetLength(0); i++)
            {
                g.DrawString(data[i, 0] + ":", FontSmallBold, new PdfSolidBrush(ColorNavy),
                    new PointF(x, y));
                g.DrawString(data[i, 1], FontSmall, new PdfSolidBrush(ColorNavy),
                    new PointF(x + 80, y));
                y += 14;
            }

            return y;
        }

        private static float DrawMembersSection(PdfGraphics g, float x, float y, List<MemberData> members)
        {
            g.DrawString($"👥 MEMBERS ({members.Count} Total)",
                FontSectionBold, new PdfSolidBrush(ColorNavy),
                new PointF(x, y));
            y += 18;

            var memberData = new List<string[]> { new[] { "Name", "DOB", "Status", "Relationship" } };
            foreach (var m in members)
                memberData.Add(new[] { m.Name, m.DateOfBirth, m.Status, m.Relationship });

            return DrawDataTable(g, x, y, memberData.ToArray()) + 10;
        }

        private static float DrawSubscriptionsSection(PdfGraphics g, float x, float y, List<SubscriptionData> subscriptions)
        {
            g.DrawString("🪦 CEMETERY SUBSCRIPTIONS",
                FontSectionBold, new PdfSolidBrush(ColorNavy),
                new PointF(x, y));
            y += 18;

            var subData = new List<string[]> { new[] { "Year", "Amount", "Status", "Paid Date" } };
            foreach (var s in subscriptions)
                subData.Add(new[] { s.Year, $"₹{s.Amount}", s.Status, s.PaidDate });

            return DrawDataTable(g, x, y, subData.ToArray()) + 10;
        }

        private static void DrawRemarksBox(PdfGraphics g, float x, float y, string remarks)
        {
            // Box border
            g.DrawRectangle(new PdfPen(ColorGold, 1f),
                new RectangleF(x, y, 400, 50));

            // Label
            g.DrawString("📝 REMARKS",
                FontSmallBold, new PdfSolidBrush(ColorNavy),
                new PointF(x + 5, y + 2));

            // Content
            g.DrawString(remarks, FontSmall, new PdfSolidBrush(ColorNavy),
                new RectangleF(x + 5, y + 16, 390, 32),
                new PdfStringFormat() { WordWrap = true });
        }

        private static float DrawDataTable(PdfGraphics g, float x, float y, string[,] data)
        {
            float colWidth = 90;
            float rowHeight = 14;

            for (int r = 0; r < data.GetLength(0); r++)
            {
                // Header row background
                if (r == 0)
                {
                    g.DrawRectangle(new PdfSolidBrush(ColorNavy),
                        new RectangleF(x, y, colWidth * data.GetLength(1), rowHeight));
                }
                // Alternating row background
                else if (r % 2 == 0)
                {
                    g.DrawRectangle(new PdfSolidBrush(ColorRowAlt),
                        new RectangleF(x, y, colWidth * data.GetLength(1), rowHeight));
                }

                // Draw cells
                for (int c = 0; c < data.GetLength(1); c++)
                {
                    var textColor = (r == 0) ? ColorGold : ColorNavy;
                    var textFont = (r == 0) ? FontSmallBold : FontSmall;
                    var cellX = x + (c * colWidth);

                    g.DrawString(data[r, c], textFont, new PdfSolidBrush(textColor),
                        new PointF(cellX + 3, y + 2));

                    // Cell border
                    g.DrawRectangle(new PdfPen(ColorLightGray, 0.5f),
                        new RectangleF(cellX, y, colWidth, rowHeight));
                }

                y += rowHeight;
            }

            return y;
        }

        private static void DrawStatsTable(PdfGraphics g, float x, float y, string[,] data)
        {
            float colWidth = 100;
            float rowHeight = 16;

            for (int r = 0; r < data.GetLength(0); r++)
            {
                if (r == 0)
                {
                    g.DrawRectangle(new PdfSolidBrush(ColorNavy),
                        new RectangleF(x, y, colWidth * data.GetLength(1), rowHeight));
                }
                else if (r % 2 == 0)
                {
                    g.DrawRectangle(new PdfSolidBrush(ColorRowAlt),
                        new RectangleF(x, y, colWidth * data.GetLength(1), rowHeight));
                }

                for (int c = 0; c < data.GetLength(1); c++)
                {
                    var textColor = (r == 0) ? ColorGold : ColorNavy;
                    var textFont = (r == 0) ? FontSmallBold : FontSmall;
                    var cellX = x + (c * colWidth);

                    g.DrawString(data[r, c], textFont, new PdfSolidBrush(textColor),
                        new PointF(cellX + 5, y + 2));

                    g.DrawRectangle(new PdfPen(ColorLightGray, 0.5f),
                        new RectangleF(cellX, y, colWidth, rowHeight));
                }

                y += rowHeight;
            }
        }

        private static double CalculateCollectionRate(List<FamilyReportData> families)
        {
            int total = families.Sum(f => f.Subscriptions.Count);
            int paid = families.Sum(f => f.Subscriptions.Count(s => s.Status == "Paid"));
            return total > 0 ? (paid * 100.0 / total) : 0;
        }

        private static int CountByStatus(List<FamilyReportData> families, string status)
        {
            return families.Sum(f => f.Subscriptions.Count(s => s.Status == status));
        }

        private static double PercentageByStatus(List<FamilyReportData> families, string status)
        {
            int count = CountByStatus(families, status);
            int total = families.Sum(f => f.Subscriptions.Count);
            return total > 0 ? (count * 100.0 / total) : 0;
        }
    }

    // Data models for PDF generation
    internal class FamilyReportData
    {
        public string FamilyName { get; set; }
        public string RegistrationDate { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public List<MemberData> Members { get; set; } = new List<MemberData>();
        public List<SubscriptionData> Subscriptions { get; set; } = new List<SubscriptionData>();
        public List<CemeteryDetailData> CemeteryDetails { get; set; } = new List<CemeteryDetailData>();
        public List<TimelineEventData> Timeline { get; set; } = new List<TimelineEventData>();
        public int ActiveSubscriptions { get; set; }
        public int OutstandingDues { get; set; }
        public int BurialsThisYear { get; set; }
        public int TotalRevenue { get; set; }
        public int CemeteryPlotsUsed { get; set; }
        public string Remarks { get; set; }
    }

    internal class MemberData
    {
        public string Name { get; set; }
        public string DateOfBirth { get; set; }
        public string Status { get; set; }
        public string Relationship { get; set; }
    }

    internal class SubscriptionData
    {
        public string Year { get; set; }
        public int Amount { get; set; }
        public string Status { get; set; }
        public string PaidDate { get; set; }
    }

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
}
