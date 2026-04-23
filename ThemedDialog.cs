using System;
using System.Drawing;
using System.Windows.Forms;

namespace TestFat
{
    internal static class ThemedDialog
    {
        public static bool Confirm(string message, string title, Form owner = null)
            => Show(message, title, DialogKind.Confirm, owner) == DialogResult.Yes;

        public static DialogResult ConfirmYesNoCancel(string message, string title, Form owner = null)
            => Show(message, title, DialogKind.YesNoCancel, owner);

        public static void Info(string message, string title, Form owner = null)
            => Show(message, title, DialogKind.Info, owner);

        public static void Warn(string message, string title, Form owner = null)
            => Show(message, title, DialogKind.Warn, owner);

        public static void Error(string message, string title, Form owner = null)
            => Show(message, title, DialogKind.Error, owner);

        private enum DialogKind { Confirm, YesNoCancel, Info, Warn, Error }

        private static DialogResult Show(string message, string title, DialogKind kind, Form owner)
        {
            var result = DialogResult.None;

            // Accent colour per kind
            Color accent;
            if (kind == DialogKind.Error)              accent = AppTheme.OverdueFg;
            else if (kind == DialogKind.Warn)          accent = AppTheme.PendingFg;
            else if (kind == DialogKind.Confirm ||
                     kind == DialogKind.YesNoCancel)   accent = AppTheme.Teal;
            else                                       accent = AppTheme.Navy;

            string icon;
            if (kind == DialogKind.Error)              icon = "✕";
            else if (kind == DialogKind.Warn)          icon = "⚠";
            else if (kind == DialogKind.Confirm ||
                     kind == DialogKind.YesNoCancel)   icon = "?";
            else                                       icon = "✔";

            using (var dlg = new Form())
            {
                dlg.FormBorderStyle = FormBorderStyle.None;
                dlg.StartPosition   = owner != null ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;
                dlg.BackColor       = AppTheme.OffWhite;
                dlg.ClientSize      = new Size(420, 200);
                dlg.MaximizeBox     = false;
                dlg.MinimizeBox     = false;
                dlg.ShowInTaskbar   = false;
                dlg.Font            = AppTheme.BodyFont;

                // Drop-shadow border via Paint
                dlg.Paint += (s, e) =>
                    e.Graphics.DrawRectangle(new Pen(AppTheme.GridBorder, 1), 0, 0, dlg.Width - 1, dlg.Height - 1);

                // Header bar
                var header = new Panel
                {
                    Height    = 44,
                    Dock      = DockStyle.Top,
                    BackColor = AppTheme.Navy
                };
                header.Controls.Add(new Label
                {
                    Text      = title,
                    Font      = AppTheme.HeaderFont,
                    ForeColor = AppTheme.Gold,
                    AutoSize  = true,
                    Location  = new Point(12, 12)
                });

                // Accent strip + icon badge
                var badge = new Label
                {
                    Text      = icon,
                    Font      = new Font("Segoe UI", 14F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = accent,
                    Size      = new Size(40, 44),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location  = new Point(dlg.Width - 40, 0)
                };
                header.Controls.Add(badge);

                // Message
                var lbl = new Label
                {
                    Text      = message,
                    Font      = AppTheme.BodyFont,
                    ForeColor = AppTheme.Navy,
                    AutoSize  = false,
                    Size      = new Size(dlg.Width - 32, 80),
                    Location  = new Point(16, 58),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // Button row
                var btnPanel = new Panel
                {
                    Height    = 50,
                    Dock      = DockStyle.Bottom,
                    BackColor = AppTheme.OffWhite
                };

                void AddBtn(string text, DialogResult dr, bool primary, int rightOffset)
                {
                    var btn = new Button
                    {
                        Text     = text,
                        Size     = new Size(110, 34),
                        Location = new Point(dlg.Width - rightOffset, 8),
                        Cursor   = Cursors.Hand,
                        FlatStyle = FlatStyle.Flat
                    };
                    if (primary)
                    {
                        btn.BackColor = accent;
                        btn.ForeColor = Color.White;
                        btn.FlatAppearance.BorderColor = accent;
                    }
                    else
                    {
                        btn.BackColor = AppTheme.OffWhite;
                        btn.ForeColor = AppTheme.Navy;
                        btn.FlatAppearance.BorderColor = AppTheme.GridBorder;
                    }
                    btn.FlatAppearance.BorderSize = 1;
                    btn.Font = AppTheme.BoldSmall;
                    var captured = dr;
                    btn.Click += (s, e) => { result = captured; dlg.Close(); };
                    btnPanel.Controls.Add(btn);
                }

                switch (kind)
                {
                    case DialogKind.Confirm:
                        AddBtn("Yes, Delete", DialogResult.Yes, true,  126);
                        AddBtn("Cancel",      DialogResult.No,  false, 246);
                        break;
                    case DialogKind.YesNoCancel:
                        AddBtn("All Families",      DialogResult.Yes,    true,  126);
                        AddBtn("Filtered Only",     DialogResult.No,     true,  246);
                        AddBtn("Cancel",            DialogResult.Cancel, false, 366);
                        lbl.Size = new Size(dlg.Width - 32, 70);
                        dlg.ClientSize = new Size(450, 200);
                        break;
                    case DialogKind.Info:
                        result = DialogResult.OK;
                        AddBtn("OK", DialogResult.OK, true, 126);
                        break;
                    case DialogKind.Warn:
                        result = DialogResult.OK;
                        AddBtn("OK", DialogResult.OK, true, 126);
                        break;
                    case DialogKind.Error:
                        result = DialogResult.OK;
                        AddBtn("Close", DialogResult.OK, true, 126);
                        break;
                }

                dlg.Controls.Add(header);
                dlg.Controls.Add(lbl);
                dlg.Controls.Add(btnPanel);

                if (owner != null)
                    dlg.ShowDialog(owner);
                else
                    dlg.ShowDialog();
            }

            return result;
        }
    }
}
