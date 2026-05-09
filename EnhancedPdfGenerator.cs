using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;

namespace TestFat
{
    internal static class EnhancedPdfGenerator
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
        private static readonly PdfColor ColorGreen      = new PdfColor(76, 175, 80);
        private static readonly PdfColor ColorWhite      = new PdfColor(255, 255, 255);
        private static readonly PdfColor ColorLightGray  = new PdfColor(200, 210, 220);

        private static readonly PdfStandardFont FontTitleBold   = new PdfStandardFont(PdfFontFamily.Helvetica, 18f, PdfFontStyle.Bold);
        private static readonly PdfStandardFont FontHeaderBold  = new PdfStandardFont(PdfFontFamily.Helvetica, 12f, PdfFontStyle.Bold);
        private static readonly PdfStandardFont FontSectionBold = new PdfStandardFont(PdfFontFamily.Helvetica, 11f, PdfFontStyle.Bold);
        private static readonly PdfStandardFont FontBody         = new PdfStandardFont(PdfFontFamily.Helvetica, 9f);
        private static readonly PdfStandardFont FontSmall        = new PdfStandardFont(PdfFontFamily.Helvetica, 8f);
        private static readonly PdfStandardFont FontSmallBold    = new PdfStandardFont(PdfFontFamily.Helvetica, 8f, PdfFontStyle.Bold);

        /// <summary>
        /// Generate enhanced family report with charts, timelines, and analytics
        /// </summary>
        public static void GenerateEnhancedFamilyReport(List<FamilyReportData> families, string filepath)
        {
            var pdfDocument = new PdfDocument();
            pdfDocument.PageSettings.Orientation = PdfPageOrientation.Portrait;
            pdfDocument.PageSettings.Margins.All = 40;

            ModernPdfGenerator.ApplyHeaderFooter(pdfDocument);

            // Cover page
            ModernPdfGenerator.CreateCoverPage(pdfDocument, families.Count);

            // Executive summary with charts
            CreateExecutiveSummaryPage(pdfDocument, families);

            // Family detail pages with timelines
            foreach (var family in families)
            {
                CreateEnhancedFamilyPage(pdfDocument, family);
            }

            // Analytics and insights
            CreateAnalyticsPage(pdfDocument, families);

            // Cemetery plot visualization
            if (families.Any(f => f.CemeteryDetails.Count > 0))
                CreateCemeteryPage(pdfDocument, families);

            pdfDocument.Save(filepath);
            pdfDocument.Close(true);
        }

        private static void CreateExecutiveSummaryPage(PdfDocument doc, List<FamilyReportData> families)
        {
            PdfPage page = doc.Pages.Add();
            var g = page.Graphics;
            float x = 30, y = 50;

            g.DrawString("EXECUTIVE SUMMARY", FontHeaderBold, new PdfSolidBrush(ColorNavy), new PointF(x, y));
            y += 30;

            // Key metrics with mini-charts
            DrawMetricsWithBars(g, x, y, families);
            y += 120;

            // Payment distribution pie chart
            g.DrawString("Payment Status Distribution", FontSectionBold, new PdfSolidBrush(ColorNavy), new PointF(x, y));
            y += 20;

            DrawPaymentDistributionChart(g, x, y, families);
            y += 100;

            // Member demographics
            g.DrawString("Family Demographics", FontSectionBold, new PdfSolidBrush(ColorNavy), new PointF(x, y));
            y += 20;

            DrawDemographicsChart(g, x, y, families);
        }

        private static void CreateEnhancedFamilyPage(PdfDocument doc, FamilyReportData family)
        {
            PdfPage page = doc.Pages.Add();
            var g = page.Graphics;
            float x = 30, y = 50;

            // Family header
            g.DrawRectangle(new PdfSolidBrush(ColorTeal), new RectangleF(x, y, page.GetClientSize().Width - 60, 5));
            y += 15;

            g.DrawString($"👥 {family.FamilyName}", FontSectionBold, new PdfSolidBrush(ColorNavy), new PointF(x, y));
            y += 20;

            // Family quick stats box
            DrawFamilyStatsBox(g, x, y, family);
            y += 70;

            // Basic information
            var familyInfo = new string[,]
            {
                { "Registered", family.RegistrationDate },
                { "Contact", family.Contact },
                { "Email", family.Email }
            };

            y = ModernPdfGenerator.DrawInfoSection(g, x, y, "Family Information", familyInfo) + 10;

            // Members with timeline
            if (family.Members.Count > 0)
                y = DrawMembersTimeline(g, x, y, family.Members) + 10;

            // Subscriptions with trend
            if (family.Subscriptions.Count > 0)
                y = DrawSubscriptionsTrend(g, x, y, family.Subscriptions) + 10;

            // Cemetery details if available
            if (family.CemeteryDetails.Count > 0)
                DrawCemeteryDetails(g, x, y, family.CemeteryDetails);
        }

        private static void CreateAnalyticsPage(PdfDocument doc, List<FamilyReportData> families)
        {
            PdfPage page = doc.Pages.Add();
            var g = page.Graphics;
            float x = 30, y = 50;

            g.DrawString("ANALYTICS & INSIGHTS", FontHeaderBold, new PdfSolidBrush(ColorNavy), new PointF(x, y));
            y += 35;

            // Collection rate gauge
            g.DrawString("Collection Performance", FontSectionBold, new PdfSolidBrush(ColorNavy), new PointF(x, y));
            y += 20;

            double collectionRate = CalculateCollectionRate(families);
            DrawCollectionGauge(g, x, y, collectionRate);
            y += 80;

            // Top contributors
            g.DrawString("Top Contributing Families", FontSectionBold, new PdfSolidBrush(ColorNavy), new PointF(x, y));
            y += 20;

            var topFamilies = families.OrderByDescending(f => f.TotalRevenue).Take(5).ToList();
            var topData = new List<string[]> { new[] { "Family", "Revenue", "Active Subs" } };

            foreach (var f in topFamilies)
                topData.Add(new[] { f.FamilyName, $"₹{f.TotalRevenue}", f.ActiveSubscriptions.ToString() });

            ModernPdfGenerator.DrawStatsTable(g, x, y, topData.ToArray());
            y += 90;

            // Subscription trend
            g.DrawString("Subscription Trend (Last 3 Years)", FontSectionBold, new PdfSolidBrush(ColorNavy), new PointF(x, y));
            y += 20;

            DrawSubscriptionTrend(g, x, y, families);
        }

        private static void CreateCemeteryPage(PdfDocument doc, List<FamilyReportData> families)
        {
            PdfPage page = doc.Pages.Add();
            var g = page.Graphics;
            float x = 30, y = 50;

            g.DrawString("🪦 CEMETERY RECORDS", FontHeaderBold, new PdfSolidBrush(ColorNavy), new PointF(x, y));
            y += 35;

            g.DrawString("Burial Summary", FontSectionBold, new PdfSolidBrush(ColorNavy), new PointF(x, y));
            y += 20;

            // Cemetery statistics
            int totalBurials = families.Sum(f => f.CemeteryDetails.Count);
            int plotsUsed = families.Sum(f => f.CemeteryPlotsUsed);
            int burialsThisYear = families.Sum(f => f.BurialsThisYear);

            var stats = new string[,]
            {
                { "Metric", "Count" },
                { "Total Burials", totalBurials.ToString() },
                { "Plots Currently Used", plotsUsed.ToString() },
                { "Burials This Year", burialsThisYear.ToString() },
                { "Families with Burials", families.Count(f => f.CemeteryDetails.Count > 0).ToString() }
            };

            ModernPdfGenerator.DrawStatsTable(g, x, y, stats);
            y += 100;

            // Recent burials
            g.DrawString("Recent Burials (Last 5 Years)", FontSectionBold, new PdfSolidBrush(ColorNavy), new PointF(x, y));
            y += 20;

            var recentBurials = families
                .SelectMany(f => f.CemeteryDetails.Select(c => new { Family = f.FamilyName, Cemetery = c }))
                .OrderByDescending(b => b.Cemetery.BurialDate)
                .Take(10)
                .ToList();

            var burialData = new List<string[]> { new[] { "Deceased", "Family", "Burial Date", "Location" } };

            foreach (var burial in recentBurials)
            {
                burialData.Add(new[]
                {
                    burial.Cemetery.DeceasedName,
                    burial.Family,
                    burial.Cemetery.BurialDate,
                    burial.Cemetery.BurialPlace
                });
            }

            ModernPdfGenerator.DrawDataTable(g, x, y, burialData.ToArray());
        }

        // ─────────────────────────────────────────────────────────────────────────
        // CHART RENDERING METHODS
        // ─────────────────────────────────────────────────────────────────────────

        private static void DrawMetricsWithBars(PdfGraphics g, float x, float y, List<FamilyReportData> families)
        {
            // Draw bar chart for key metrics
            int totalFamilies = families.Count;
            int totalMembers = families.Sum(f => f.Members.Count);
            int activeSubscriptions = families.Sum(f => f.ActiveSubscriptions);
            int revenue = families.Sum(f => f.TotalRevenue);

            var metrics = new[]
            {
                ("Families", totalFamilies, 12),
                ("Members", totalMembers, 45),
                ("Active Subs", activeSubscriptions, 32),
                ("Revenue", revenue / 1000, 192) // Divide for scale
            };

            float barWidth = 80;
            float barHeight = 40;
            float spacing = 95;

            foreach (var (label, value, max) in metrics)
            {
                float barHeightActual = (value * barHeight) / max;

                // Draw bar
                g.DrawRectangle(new PdfSolidBrush(ColorTeal),
                    new RectangleF(x, y + barHeight - barHeightActual, barWidth, barHeightActual));

                // Border
                g.DrawRectangle(new PdfPen(ColorGold, 1f),
                    new RectangleF(x, y, barWidth, barHeight));

                // Label
                g.DrawString(label, FontSmall, new PdfSolidBrush(ColorNavy),
                    new PointF(x + 5, y + barHeight + 5));

                // Value
                g.DrawString(value.ToString(), FontSmallBold, new PdfSolidBrush(ColorNavy),
                    new PointF(x + 10, y + barHeight / 2 - 4));

                x += spacing;
            }
        }

        private static void DrawPaymentDistributionChart(PdfGraphics g, float x, float y, List<FamilyReportData> families)
        {
            int paid = families.Sum(f => f.Subscriptions.Count(s => s.Status == "Paid"));
            int pending = families.Sum(f => f.Subscriptions.Count(s => s.Status == "Pending"));
            int overdue = families.Sum(f => f.Subscriptions.Count(s => s.Status == "Overdue"));
            int total = paid + pending + overdue;

            float pieX = x, pieY = y, pieRadius = 40;

            // Pie segments
            float startAngle = 0;

            // Paid (green)
            float paidAngle = (paid * 360f) / total;
            g.DrawArc(new PdfSolidBrush(ColorPaidBg), pieX, pieY, pieRadius * 2, pieRadius * 2, startAngle, paidAngle);
            g.DrawArc(new PdfPen(ColorNavy, 1f), pieX, pieY, pieRadius * 2, pieRadius * 2, startAngle, paidAngle);

            startAngle += paidAngle;

            // Pending (yellow)
            float pendingAngle = (pending * 360f) / total;
            g.DrawArc(new PdfSolidBrush(ColorPendingBg), pieX, pieY, pieRadius * 2, pieRadius * 2, startAngle, pendingAngle);
            g.DrawArc(new PdfPen(ColorNavy, 1f), pieX, pieY, pieRadius * 2, pieRadius * 2, startAngle, pendingAngle);

            startAngle += pendingAngle;

            // Overdue (red)
            float overdueAngle = (overdue * 360f) / total;
            g.DrawArc(new PdfSolidBrush(ColorOverdueBg), pieX, pieY, pieRadius * 2, pieRadius * 2, startAngle, overdueAngle);
            g.DrawArc(new PdfPen(ColorNavy, 1f), pieX, pieY, pieRadius * 2, pieRadius * 2, startAngle, overdueAngle);

            // Legend
            float legendX = pieX + pieRadius * 2 + 30;
            float legendY = pieY;

            g.DrawRectangle(new PdfSolidBrush(ColorPaidBg), new RectangleF(legendX, legendY, 12, 12));
            g.DrawString($"✓ Paid ({paid})", FontSmall, new PdfSolidBrush(ColorNavy), new PointF(legendX + 18, legendY));

            legendY += 20;
            g.DrawRectangle(new PdfSolidBrush(ColorPendingBg), new RectangleF(legendX, legendY, 12, 12));
            g.DrawString($"⚠ Pending ({pending})", FontSmall, new PdfSolidBrush(ColorNavy), new PointF(legendX + 18, legendY));

            legendY += 20;
            g.DrawRectangle(new PdfSolidBrush(ColorOverdueBg), new RectangleF(legendX, legendY, 12, 12));
            g.DrawString($"✗ Overdue ({overdue})", FontSmall, new PdfSolidBrush(ColorNavy), new PointF(legendX + 18, legendY));
        }

        private static void DrawDemographicsChart(PdfGraphics g, float x, float y, List<FamilyReportData> families)
        {
            int totalMembers = families.Sum(f => f.Members.Count);
            int maleMembers = families.Sum(f => f.Members.Count);  // Placeholder - would need gender field
            int femaleMembers = totalMembers - maleMembers;

            // Simple horizontal bar chart
            float chartWidth = 200;
            float barHeight = 15;

            // Total bar
            g.DrawRectangle(new PdfSolidBrush(ColorTeal), new RectangleF(x, y, chartWidth, barHeight));
            g.DrawString($"Total Members: {totalMembers}", FontSmall, new PdfSolidBrush(ColorWhite), new PointF(x + 5, y + 2));

            // Active bar
            int activeMembers = families.Sum(f => f.Members.Count(m => m.Status == "Active"));
            float activeWidth = (activeMembers * chartWidth) / totalMembers;
            g.DrawRectangle(new PdfSolidBrush(ColorGreen), new RectangleF(x, y + 20, activeWidth, barHeight));
            g.DrawString($"Active: {activeMembers}", FontSmall, new PdfSolidBrush(ColorNavy), new PointF(x + 5, y + 22));
        }

        private static void DrawCollectionGauge(PdfGraphics g, float x, float y, double rate)
        {
            float gaugeX = x + 50, gaugeY = y, gaugeRadius = 35;

            // Gauge background (arc from 180 to 360 degrees)
            g.DrawArc(new PdfPen(ColorLightGray, 5f), gaugeX, gaugeY, gaugeRadius * 2, gaugeRadius * 2, 180, 180);

            // Gauge fill (green-red gradient representation)
            float filledAngle = (float)rate;
            if (filledAngle > 180) filledAngle = 180;

            PdfColor gaugeColor = rate >= 90 ? ColorGreen : (rate >= 70 ? ColorGold : ColorOverdueFg);
            g.DrawArc(new PdfPen(gaugeColor, 5f), gaugeX, gaugeY, gaugeRadius * 2, gaugeRadius * 2, 180, filledAngle);

            // Center text
            g.DrawString($"{rate:F1}%", FontTitleBold, new PdfSolidBrush(gaugeColor),
                new PointF(gaugeX + gaugeRadius - 15, gaugeY + gaugeRadius - 10));
        }

        private static void DrawSubscriptionTrend(PdfGraphics g, float x, float y, List<FamilyReportData> families)
        {
            // Simple trend line visualization
            var years = new List<int>();
            var amounts = new List<int>();

            foreach (var sub in families.SelectMany(f => f.Subscriptions).OrderBy(s => s.Year).Distinct())
            {
                if (int.TryParse(sub.Year, out int year))
                {
                    years.Add(year);
                    amounts.Add(sub.Amount);
                }
            }

            if (years.Count < 2) return;

            float chartX = x, chartY = y, chartWidth = 300, chartHeight = 80;

            // Grid
            g.DrawRectangle(new PdfPen(ColorLightGray, 0.5f), new RectangleF(chartX, chartY, chartWidth, chartHeight));

            // Axis labels
            g.DrawString("₹", FontSmall, new PdfSolidBrush(ColorNavy), new PointF(chartX - 15, chartY));
            g.DrawString("Year", FontSmall, new PdfSolidBrush(ColorNavy), new PointF(chartX + chartWidth - 20, chartY + chartHeight + 5));

            // Data line
            float xStep = chartWidth / (years.Count - 1);
            float yScale = chartHeight / (amounts.Max() > 0 ? amounts.Max() : 1);

            for (int i = 0; i < years.Count - 1; i++)
            {
                float x1 = chartX + (i * xStep);
                float y1 = chartY + chartHeight - (amounts[i] * yScale);
                float x2 = chartX + ((i + 1) * xStep);
                float y2 = chartY + chartHeight - (amounts[i + 1] * yScale);

                g.DrawLine(new PdfPen(ColorTeal, 2f), new PointF(x1, y1), new PointF(x2, y2));
                g.DrawRectangle(new PdfSolidBrush(ColorTeal), new RectangleF(x1 - 2, y1 - 2, 4, 4));
            }

            g.DrawRectangle(new PdfSolidBrush(ColorTeal), new RectangleF(chartX + (chartWidth - 4), chartY + chartHeight - (amounts.Last() * yScale) - 2, 4, 4));
        }

        private static float DrawMembersTimeline(PdfGraphics g, float x, float y, List<MemberData> members)
        {
            g.DrawString("👥 MEMBERS & TIMELINE", new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Bold),
                new PdfSolidBrush(ColorNavy), new PointF(x, y));
            y += 15;

            foreach (var member in members.Take(5))
            {
                // Timeline dot
                g.DrawRectangle(new PdfSolidBrush(ColorTeal), new RectangleF(x, y + 2, 6, 6));

                // Timeline line (connecting to next)
                g.DrawLine(new PdfPen(ColorGold, 1f), new PointF(x + 3, y + 8), new PointF(x + 3, y + 14));

                // Member info
                g.DrawString(member.Name, FontSmall, new PdfSolidBrush(ColorNavy), new PointF(x + 15, y));
                g.DrawString($"{member.Relationship} • {member.DateOfBirth}", FontSmall,
                    new PdfSolidBrush(ColorLightGray), new PointF(x + 15, y + 8));

                y += 20;
            }

            return y;
        }

        private static float DrawSubscriptionsTrend(PdfGraphics g, float x, float y, List<SubscriptionData> subscriptions)
        {
            g.DrawString("🪦 SUBSCRIPTION TREND", new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Bold),
                new PdfSolidBrush(ColorNavy), new PointF(x, y));
            y += 15;

            var sortedSubs = subscriptions.OrderByDescending(s => s.Year).Take(5);

            foreach (var sub in sortedSubs)
            {
                // Status indicator
                PdfColor statusColor = sub.Status == "Paid" ? ColorPaidFg : (sub.Status == "Pending" ? ColorPendingFg : ColorOverdueFg);
                string statusIcon = sub.Status == "Paid" ? "✓" : (sub.Status == "Pending" ? "⚠" : "✗");

                g.DrawString($"{statusIcon} {sub.Year}: ₹{sub.Amount}", FontSmall, new PdfSolidBrush(ColorNavy), new PointF(x, y));
                g.DrawString(sub.Status, FontSmall, new PdfSolidBrush(statusColor), new PointF(x + 120, y));

                y += 12;
            }

            return y;
        }

        private static void DrawFamilyStatsBox(PdfGraphics g, float x, float y, FamilyReportData family)
        {
            // Stats in a grid
            float boxWidth = 100;
            float boxHeight = 50;

            var stats = new[] { ("Members", family.Members.Count), ("Active Subs", family.ActiveSubscriptions), ("Outstanding", family.OutstandingDues) };

            foreach (var (label, value) in stats)
            {
                g.DrawRectangle(new PdfSolidBrush(ColorRowAlt), new RectangleF(x, y, boxWidth, boxHeight));
                g.DrawRectangle(new PdfPen(ColorGold, 1f), new RectangleF(x, y, boxWidth, boxHeight));

                g.DrawString(label, FontSmall, new PdfSolidBrush(ColorNavy), new PointF(x + 5, y + 5));
                g.DrawString(value.ToString(), FontSectionBold, new PdfSolidBrush(ColorTeal), new PointF(x + 5, y + 20));

                x += 105;
            }
        }

        private static void DrawCemeteryDetails(PdfGraphics g, float x, float y, List<CemeteryDetailData> details)
        {
            g.DrawString("🪦 CEMETERY RECORDS", new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Bold),
                new PdfSolidBrush(ColorNavy), new PointF(x, y));
            y += 15;

            foreach (var detail in details)
            {
                g.DrawString(detail.DeceasedName, FontSmallBold, new PdfSolidBrush(ColorNavy), new PointF(x, y));
                g.DrawString($"Buried: {detail.BurialDate} at {detail.BurialPlace}", FontSmall,
                    new PdfSolidBrush(ColorLightGray), new PointF(x, y + 10));
                y += 22;
            }
        }

        private static double CalculateCollectionRate(List<FamilyReportData> families)
        {
            int total = families.Sum(f => f.Subscriptions.Count);
            int paid = families.Sum(f => f.Subscriptions.Count(s => s.Status == "Paid"));
            return total > 0 ? (paid * 100.0 / total) : 0;
        }
    }
}
