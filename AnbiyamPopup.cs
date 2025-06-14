using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestFat
{
    public partial class AnbiyamPopup : Form
    {
        public AnbiyamPopup()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;

            // Example: Populate with zones 1-10
            for (int i = 1; i <= 10; i++)
                cmbAnbiyamZone.Items.Add(i.ToString());
            cmbAnbiyamZone.SelectedIndex = 0;
        }
    }
}
