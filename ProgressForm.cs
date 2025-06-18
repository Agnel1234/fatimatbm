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
    public partial class ProgressForm : Form
    {
        public ProgressForm(string msg)
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.ControlBox = false;
            this.Width = 250;
            this.Height = 100;
            var label = new Label { Text = msg, Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleCenter};
            label.Font = new Font(label.Font, FontStyle.Bold);
            this.Controls.Add(label);
        }
    }
}
