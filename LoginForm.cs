using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace TestFat
{
    public partial class LoginForm : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        public string LoggedInUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            ApplyTheme();

            this.KeyPreview = true;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) btnLogin_Click_1(s, e); };
        }

        private void ApplyTheme()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.BackColor       = AppTheme.Navy;
            this.ClientSize      = new Size(460, 420);
            this.Text            = "Login";
            this.Font            = AppTheme.BodyFont;

            // Border on outer form
            this.Paint += (s, pe) =>
                pe.Graphics.DrawRectangle(new Pen(AppTheme.Gold, 1), 0, 0, Width - 1, Height - 1);

            // ── Header panel ──
            var header = new Panel
            {
                Height    = 110,
                Dock      = DockStyle.Top,
                BackColor = AppTheme.Navy,
            };

            // Cross + church name stacked (centered in header)
            var lblCross = new Label
            {
                Text      = "✝",
                Font      = new Font("Georgia", 28F, FontStyle.Bold),
                ForeColor = AppTheme.Gold,
                AutoSize  = true,
                Location  = new Point(header.Width / 2 - 14, 8),
            };
            header.Controls.Add(lblCross);
            header.Layout += (s, e) => lblCross.Left = (header.Width / 2) - 14;

            var lblChurch = new Label
            {
                Text      = "Our Lady of Fatima Church",
                Font      = new Font("Georgia", 13F, FontStyle.Bold),
                ForeColor = AppTheme.Gold,
                AutoSize  = true,
                Location  = new Point(110, 42),
            };
            header.Controls.Add(lblChurch);
            header.Layout += (s, e) => lblChurch.Left = (header.Width - lblChurch.Width) / 2;

            var lblLocation = new Label
            {
                Text      = "Tambaram, Chennai",
                Font      = AppTheme.SmallFont,
                ForeColor = Color.FromArgb(180, 200, 215),
                AutoSize  = true,
                Location  = new Point(165, 68),
            };
            header.Controls.Add(lblLocation);
            header.Layout += (s, e) => lblLocation.Left = (header.Width - lblLocation.Width) / 2;

            // Drag the form by dragging the header
            header.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0); }
            };

            // ── Card panel (white) ──
            var card = new Panel
            {
                BackColor   = Color.White,
                Size        = new Size(340, 240),
                Location    = new Point(60, 115),
                BorderStyle = BorderStyle.None,
            };

            // Center card horizontally on form resize
            this.Resize += (s, e) => {
                card.Left = (this.ClientSize.Width - card.Width) / 2;
            };

            card.Paint += (s, pe) =>
                pe.Graphics.DrawRectangle(new Pen(AppTheme.GridBorder, 1), 0, 0, card.Width - 1, card.Height - 1);

            // Card top accent bar
            card.Controls.Add(new Panel
            {
                Height    = 4,
                Dock      = DockStyle.Top,
                BackColor = AppTheme.Teal,
            });

            // ── Labels and fields inside card ──
            int lx = 24, fx = 130, fw = 180, fy = 28;

            // Username
            card.Controls.Add(new Label
            {
                Text      = "Username",
                Font      = AppTheme.BoldSmall,
                ForeColor = AppTheme.Navy,
                AutoSize  = true,
                Location  = new Point(lx, fy + 4),
            });
            txtUsername.Location    = new Point(lx, fy + 22);
            txtUsername.Size        = new Size(fw, 28);
            txtUsername.Font        = AppTheme.BodyFont;
            txtUsername.BackColor   = AppTheme.OffWhite;
            txtUsername.ForeColor   = AppTheme.Navy;
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            card.Controls.Add(txtUsername);

            // Password
            int py = fy + 65;
            card.Controls.Add(new Label
            {
                Text      = "Password",
                Font      = AppTheme.BoldSmall,
                ForeColor = AppTheme.Navy,
                AutoSize  = true,
                Location  = new Point(lx, py + 4),
            });
            txtPassword.Location      = new Point(lx, py + 22);
            txtPassword.Size          = new Size(fw, 28);
            txtPassword.Font          = AppTheme.BodyFont;
            txtPassword.BackColor     = AppTheme.OffWhite;
            txtPassword.ForeColor     = AppTheme.Navy;
            txtPassword.BorderStyle   = BorderStyle.FixedSingle;
            txtPassword.PasswordChar  = '●';
            card.Controls.Add(txtPassword);

            // Login button
            btnLogin.Location  = new Point(lx, py + 75);
            btnLogin.Size      = new Size(fw, 38);
            btnLogin.Text      = "Sign In";
            AppTheme.StyleButtonPrimary(btnLogin);
            AppTheme.SetIcon(btnLogin, AppTheme.IconChurch(18), "Sign In");
            card.Controls.Add(btnLogin);

            // Hint text
            card.Controls.Add(new Label
            {
                Text      = "Use your assigned church credentials",
                Font      = new Font("Georgia", 8F, FontStyle.Italic),
                ForeColor = Color.FromArgb(140, 160, 175),
                AutoSize  = true,
                Location  = new Point(lx, py + 123),
            });

            // ── Close button top-right ──
            var btnClose = new Button
            {
                Text      = "✕",
                Size      = new Size(36, 36),
                Location  = new Point(this.ClientSize.Width - 40, 4),
                FlatStyle = FlatStyle.Flat,
                BackColor = AppTheme.Navy,
                ForeColor = Color.FromArgb(160, 190, 210),
                Font      = new Font("Segoe UI", 11F),
                Cursor    = Cursors.Hand,
                TabStop   = false,
            };
            this.Resize += (s, e) => btnClose.Left = this.ClientSize.Width - 40;

            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.MouseEnter += (s, e) => ((Button)s).BackColor = Color.FromArgb(192, 57, 43);
            btnClose.MouseLeave += (s, e) => ((Button)s).BackColor = AppTheme.Navy;
            btnClose.Click      += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // ── Footer label ──
            var footer = new Label
            {
                Text      = "© Fatima Church, Tambaram",
                Font      = new Font("Georgia", 8F, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 130, 150),
                AutoSize  = true,
                Location  = new Point(155, 365),
            };
            this.Resize += (s, e) => footer.Left = (this.ClientSize.Width - footer.Width) / 2;

            // Remove old panel1 controls from form and add new ones
            this.Controls.Clear();
            this.Controls.Add(header);
            this.Controls.Add(card);
            this.Controls.Add(btnClose);
            this.Controls.Add(footer);

            // Initial centering
            this.Load += (s, e) => {
                card.Left = (this.ClientSize.Width - card.Width) / 2;
                footer.Left = (this.ClientSize.Width - footer.Width) / 2;
            };
        }

        private bool AuthenticateUser(string username, string password)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.[users] " +
                "WHERE username = @username " +
                "AND password = CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', @password), 2)", conn))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return (result != null && Convert.ToInt32(result) > 0);
            }
        }

        private void btnLogin_Click_1(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (AuthenticateUser(username, password))
            {
                LoggedInUser = username.ToUpper();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                ThemedDialog.Error("Invalid username or password.\nPlease try again.", "Login Failed", this);
            }
        }
    }
}