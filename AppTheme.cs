using System.Drawing;
using System.Drawing.Drawing2D;

namespace TestFat
{
    internal static class AppTheme
    {
        // ── Primary palette ──
        public static readonly Color Navy       = Color.FromArgb(26,  45,  66);   // #1A2D42
        public static readonly Color Teal       = Color.FromArgb(44, 110, 122);   // #2C6E7A
        public static readonly Color Gold       = Color.FromArgb(201, 168, 76);   // #C9A84C
        public static readonly Color OffWhite   = Color.FromArgb(240, 244, 247);  // #F0F4F7
        public static readonly Color CardWhite  = Color.White;
        public static readonly Color RowAlt     = Color.FromArgb(240, 247, 250);  // very light teal tint
        public static readonly Color GridBorder = Color.FromArgb(208, 228, 238);

        // ── Status badge colours ──
        public static readonly Color PaidBg     = Color.FromArgb(212, 237, 218);
        public static readonly Color PaidFg     = Color.FromArgb(21,  87,  36);
        public static readonly Color PendingBg  = Color.FromArgb(255, 243, 205);
        public static readonly Color PendingFg  = Color.FromArgb(133, 100,   4);
        public static readonly Color OverdueBg  = Color.FromArgb(248, 215, 218);
        public static readonly Color OverdueFg  = Color.FromArgb(132,  32,  41);

        // ── Accent panels ──
        public static readonly Color NavBar     = Color.FromArgb(22,  40,  60);
        public static readonly Color FilterBar  = Color.FromArgb(232, 240, 244);
        public static readonly Color ActionBar  = Color.FromArgb(224, 232, 238);

        // ── Fonts ──
        public static readonly Font TitleFont   = new Font("Georgia", 14F, FontStyle.Bold);
        public static readonly Font HeaderFont  = new Font("Georgia", 12F, FontStyle.Bold);
        public static readonly Font BodyFont    = new Font("Georgia", 11F, FontStyle.Regular);
        public static readonly Font SmallFont   = new Font("Georgia", 10F, FontStyle.Regular);
        public static readonly Font BoldSmall   = new Font("Georgia", 10F, FontStyle.Bold);

        // ── Chart colours ──
        public static readonly Color[] ChartPalette =
        {
            Color.FromArgb(44, 110, 122),   // Teal
            Color.FromArgb(201, 168, 76),   // Gold
            Color.FromArgb(26,  45,  66),   // Navy
            Color.FromArgb(42, 157,  92),   // Green
            Color.FromArgb(230, 126,  34),  // Amber
            Color.FromArgb(155,  89, 182),  // Purple
        };

        public static readonly Color MaleColor   = Color.FromArgb(44, 110, 122);
        public static readonly Color FemaleColor = Color.FromArgb(201, 168, 76);

        // ── Helper: style a DataGridView with the theme ──
        public static void StyleGrid(System.Windows.Forms.DataGridView grid)
        {
            grid.BackgroundColor = OffWhite;
            grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            grid.GridColor = GridBorder;
            grid.DefaultCellStyle.Font = BodyFont;
            grid.DefaultCellStyle.BackColor = CardWhite;
            grid.DefaultCellStyle.ForeColor = Navy;
            grid.DefaultCellStyle.SelectionBackColor = Teal;
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.AlternatingRowsDefaultCellStyle.BackColor = RowAlt;
            grid.AlternatingRowsDefaultCellStyle.ForeColor = Navy;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Navy;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Gold;
            grid.ColumnHeadersDefaultCellStyle.Font = HeaderFont;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Navy;
            grid.ColumnHeadersDefaultCellStyle.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            grid.ColumnHeadersHeight = 32;
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;
            grid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
        }

        // ── Helper: style a primary action button ──
        public static void StyleButtonPrimary(System.Windows.Forms.Button btn)
        {
            btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn.BackColor = Teal;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderColor = Teal;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = BoldSmall;
            btn.Cursor = System.Windows.Forms.Cursors.Hand;
        }

        // ── Helper: style a secondary action button ──
        public static void StyleButtonSecondary(System.Windows.Forms.Button btn)
        {
            btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn.BackColor = Navy;
            btn.ForeColor = Gold;
            btn.FlatAppearance.BorderColor = Navy;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = BoldSmall;
            btn.Cursor = System.Windows.Forms.Cursors.Hand;
        }

        // ── Helper: style an outline button ──
        public static void StyleButtonOutline(System.Windows.Forms.Button btn)
        {
            btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn.BackColor = OffWhite;
            btn.ForeColor = Teal;
            btn.FlatAppearance.BorderColor = Teal;
            btn.FlatAppearance.BorderSize = 1;
            btn.Font = SmallFont;
            btn.Cursor = System.Windows.Forms.Cursors.Hand;
        }

        // ── Icon factory helpers ──
        // Scale any existing bitmap to a target size
        public static Bitmap ScaleBitmap(Bitmap src, int size)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(src, 0, 0, size, size);
            }
            return bmp;
        }

        // 🔍 Magnifying glass
        public static Bitmap IconSearch(int size = 18)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                int r = size / 2 - 2;
                using (var pen = new Pen(Color.White, 2f))
                {
                    g.DrawEllipse(pen, 1, 1, r * 2 - 2, r * 2 - 2);
                    g.DrawLine(pen, r * 2 - 2, r * 2 - 2, size - 2, size - 2);
                }
            }
            return bmp;
        }

        // 📄 PDF / document
        public static Bitmap IconPdf(int size = 18)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                int m = 2;
                var rect = new Rectangle(m, m, size - m * 2 - 3, size - m * 2);
                using (var pen = new Pen(Color.White, 1.5f))
                using (var fill = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
                {
                    // Dog-ear fold
                    var pts = new PointF[]
                    {
                        new PointF(rect.Left,  rect.Top),
                        new PointF(rect.Right - 4, rect.Top),
                        new PointF(rect.Right, rect.Top + 4),
                        new PointF(rect.Right, rect.Bottom),
                        new PointF(rect.Left,  rect.Bottom),
                    };
                    g.FillPolygon(fill, pts);
                    g.DrawPolygon(pen, pts);
                    g.DrawLine(pen, rect.Right - 4, rect.Top, rect.Right - 4, rect.Top + 4);
                    g.DrawLine(pen, rect.Right - 4, rect.Top + 4, rect.Right, rect.Top + 4);
                    // Lines representing text
                    int lx1 = rect.Left + 2, lx2 = rect.Right - 2;
                    g.DrawLine(pen, lx1, rect.Top + 7, lx2, rect.Top + 7);
                    g.DrawLine(pen, lx1, rect.Top + 10, lx2, rect.Top + 10);
                    g.DrawLine(pen, lx1, rect.Top + 13, lx2 - 3, rect.Top + 13);
                }
            }
            return bmp;
        }

        // ✝ Church cross
        public static Bitmap IconChurch(int size = 18)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                int cx = size / 2, bar = size / 6;
                using (var pen = new Pen(Color.White, 2.5f))
                {
                    g.DrawLine(pen, cx, 1, cx, size - 2);           // vertical
                    g.DrawLine(pen, cx - bar, size / 3, cx + bar, size / 3); // crossbar
                }
            }
            return bmp;
        }

        // 🚫 Disable (circle with horizontal line)
        public static Bitmap IconDisable(int size = 18)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (var pen = new Pen(Color.White, 2f))
                {
                    g.DrawEllipse(pen, 1, 1, size - 3, size - 3);
                    g.DrawLine(pen, 3, size / 2, size - 4, size / 2);
                }
            }
            return bmp;
        }

        // 💾 Save (floppy disk shape)
        public static Bitmap IconSave(int size = 18)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (var pen = new Pen(Color.White, 1.5f))
                using (var fill = new SolidBrush(Color.FromArgb(80, 255, 255, 255)))
                {
                    var body = new Rectangle(1, 1, size - 3, size - 3);
                    g.FillRectangle(fill, body);
                    g.DrawRectangle(pen, body);
                    // Label slot at top
                    g.FillRectangle(new SolidBrush(Color.FromArgb(120, 255, 255, 255)),
                        3, 2, size - 8, 4);
                    // Storage area at bottom
                    g.FillRectangle(new SolidBrush(Color.FromArgb(100, 255, 255, 255)),
                        4, size - 7, size - 9, 5);
                }
            }
            return bmp;
        }

        // ✕ Close / X
        public static Bitmap IconClose(int size = 16)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (var pen = new Pen(Color.White, 2f))
                {
                    g.DrawLine(pen, 3, 3, size - 4, size - 4);
                    g.DrawLine(pen, size - 4, 3, 3, size - 4);
                }
            }
            return bmp;
        }

        // ➕ Outside/Add person
        public static Bitmap IconAddPerson(int size = 18)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (var pen = new Pen(Color.White, 2f))
                {
                    // Person silhouette
                    int cx = size / 2 - 2;
                    g.DrawEllipse(pen, cx - 3, 1, 6, 6);
                    g.DrawArc(pen, cx - 5, 8, 10, 7, 180, 180);
                    // Plus sign
                    int px = size - 5;
                    g.DrawLine(pen, px, size / 2 - 3, px, size / 2 + 3);
                    g.DrawLine(pen, px - 3, size / 2, px + 3, size / 2);
                }
            }
            return bmp;
        }

        // 🪦 Grave/cemetery marker
        public static Bitmap IconCemetery(int size = 18)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (var pen = new Pen(Color.White, 2f))
                {
                    // Tombstone arch
                    int w = size - 6, left = 3;
                    g.DrawArc(pen, left, 1, w, w, 180, 180);
                    g.DrawLine(pen, left, 1 + w / 2, left, size - 2);
                    g.DrawLine(pen, left + w, 1 + w / 2, left + w, size - 2);
                    g.DrawLine(pen, left - 1, size - 2, left + w + 1, size - 2);
                    // Cross on stone
                    int cx = left + w / 2;
                    g.DrawLine(pen, cx, 3, cx, w / 2 + 1);
                    g.DrawLine(pen, cx - 3, 6, cx + 3, 6);
                }
            }
            return bmp;
        }

        // 🗑 Trash / Delete
        public static Bitmap IconTrash(int size = 18)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (var pen = new Pen(Color.White, 1.6f))
                {
                    int m = size / 6;
                    int bx = m, bw = size - m * 2;
                    // Bin body
                    g.DrawRectangle(pen, bx, m + 3, bw, size - m * 2 - 2);
                    // Lid
                    g.DrawLine(pen, bx - 2, m + 2, bx + bw + 2, m + 2);
                    // Lid handle
                    g.DrawRectangle(pen, bx + bw / 2 - 2, m - 1, 5, 3);
                    // Vertical lines inside
                    int third = bw / 3;
                    g.DrawLine(pen, bx + third,     m + 5, bx + third,     size - m - 1);
                    g.DrawLine(pen, bx + third * 2, m + 5, bx + third * 2, size - m - 1);
                }
            }
            return bmp;
        }

        // ℹ Info circle
        public static Bitmap IconInfo(int size = 18)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (var pen = new Pen(Color.White, 1.8f))
                {
                    // Circle
                    g.DrawEllipse(pen, 1, 1, size - 3, size - 3);
                    int cx = size / 2;
                    // Dot
                    g.FillEllipse(Brushes.White, cx - 1, size / 4, 3, 3);
                    // Stem
                    g.DrawLine(pen, cx, size / 4 + 5, cx, size - size / 4 - 1);
                }
            }
            return bmp;
        }

        // ➕ Plus / Create
        public static Bitmap IconPlus(int size = 18)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                int cx = size / 2, arm = size / 2 - 3;
                using (var pen = new Pen(Color.White, 2.5f))
                {
                    g.DrawLine(pen, cx, cx - arm, cx, cx + arm);
                    g.DrawLine(pen, cx - arm, cx, cx + arm, cx);
                }
            }
            return bmp;
        }

        // ✏ Pencil / Edit  — penColor defaults to White (use Teal for outline buttons)
        public static Bitmap IconPencil(int size = 18, Color? penColor = null)
        {
            var c = penColor ?? Color.White;
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (var pen = new Pen(c, 1.8f))
                {
                    var body = new PointF[]
                    {
                        new PointF(4,  size - 5),
                        new PointF(size - 5, 4),
                        new PointF(size - 2, 7),
                        new PointF(7,  size - 2),
                    };
                    g.DrawPolygon(pen, body);
                    g.DrawLine(pen, 4, size - 5, 3, size - 2);
                    g.DrawLine(pen, 7, size - 2, 3, size - 2);
                }
            }
            return bmp;
        }

        // ⬇ Download / Export arrow  — penColor defaults to White
        public static Bitmap IconDownload(int size = 18, Color? penColor = null)
        {
            var c = penColor ?? Color.White;
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                int cx = size / 2;
                using (var pen = new Pen(c, 2f))
                {
                    g.DrawLine(pen, cx, 2, cx, size - 6);
                    g.DrawLine(pen, cx - 4, size - 10, cx, size - 6);
                    g.DrawLine(pen, cx + 4, size - 10, cx, size - 6);
                    g.DrawLine(pen, 3, size - 3, size - 4, size - 3);
                }
            }
            return bmp;
        }

        // Attach icon + text to a button
        public static void SetIcon(System.Windows.Forms.Button btn, Bitmap icon, string text, int iconSize = 18)
        {
            btn.Image = ScaleBitmap(icon, iconSize);
            btn.Text = "  " + text;
            btn.ImageAlign = ContentAlignment.MiddleLeft;
            btn.TextAlign = ContentAlignment.MiddleCenter;
            btn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            btn.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
        }
    }
}
