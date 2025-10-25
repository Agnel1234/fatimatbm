using System;
using System.Windows.Forms;

namespace TestFat
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var login = new LoginForm())
            {
                // Ensure login is shown centered
                login.StartPosition = FormStartPosition.CenterScreen;

                if (login.ShowDialog() == DialogResult.OK)
                {
                    // Pass successful username into Form1
                    var main = new Form1(login.LoggedInUser);
                    main.StartPosition = FormStartPosition.CenterScreen;
                    Application.Run(main);
                }
            }
        }
    }
}