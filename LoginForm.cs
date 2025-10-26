using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace TestFat
{
    public partial class LoginForm : Form
    {
        public string LoggedInUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();

            // Aesthetic: Set form background and border style
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;
            this.Font = new Font("Georgia", 11, FontStyle.Regular);

            // Aesthetic: Set title and icon
            this.Text = "User Login";
            // this.Icon = Properties.Resources.YourAppIcon; // Uncomment if you have an icon

            // Aesthetic: Style controls if they exist
            if (this.Controls.ContainsKey("txtUsername"))
            {
                txtUsername.Font = new Font("Georgia", 11, FontStyle.Regular);
                txtUsername.BackColor = Color.White;
                txtUsername.ForeColor = Color.Black;
            }
            if (this.Controls.ContainsKey("txtPassword") || txtPassword != null)
            {
                txtPassword.Font = new Font("Georgia", 11, FontStyle.Regular);
                txtPassword.BackColor = Color.White;
                txtPassword.ForeColor = Color.Black;
                txtPassword.UseSystemPasswordChar = false;
                txtPassword.PasswordChar = '*';
            }
            if (this.Controls.ContainsKey("btnLogin"))
            {
                btnLogin.Font = new Font("Georgia", 12, FontStyle.Bold);
                btnLogin.BackColor = Color.RoyalBlue;
                btnLogin.ForeColor = Color.White;
                btnLogin.FlatStyle = FlatStyle.Flat;
                btnLogin.FlatAppearance.BorderColor = Color.DarkSlateGray;
            }
            if (this.Controls.ContainsKey("lblUsername"))
            {
                lblUsername.Font = new Font("Georgia", 12, FontStyle.Bold);
                lblUsername.ForeColor = Color.DarkSlateGray;
            }
            if (this.Controls.ContainsKey("lblPassword"))
            {
                lblPassword.Font = new Font("Georgia", 12, FontStyle.Bold);
                lblPassword.ForeColor = Color.DarkSlateGray;
            }
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
                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}