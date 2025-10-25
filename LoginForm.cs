using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TestFat
{
    public partial class LoginForm : Form
    {
        public string LoggedInUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();

            this.MaximizeBox = false;
            this.MinimizeBox = true;

            // Make password field a sensitive (masked) field
            // Ensure the control name matches the designer (txtPassword)
            // Use explicit asterisk (*) as the masking character
            if (this.Controls.ContainsKey("txtPassword") || txtPassword != null)
            {
                // Ensure system masking not used so PasswordChar takes effect
                txtPassword.UseSystemPasswordChar = false;
                txtPassword.PasswordChar = '*';
            }
        }

        private bool AuthenticateUser(string username, string password)
        {
            // Use the same hash format stored in DB (hex string produced by CONVERT(..., 2))
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.[users] " +
                "WHERE username = @username " +
                "AND password = CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', @password), 2)", conn))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password); // plain text input; SQL hashes it
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