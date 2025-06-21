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
    public partial class FamilyPopup : Form
    {
        public FamilyPopup()
        {
            InitializeComponent();
           // AddChildrenGroupBox();
           // AddOtherRelationGroupBox();
            child1Groupbox.Visible = false;
        }

        private int childCount = 0;
        private int otherRelationCount = 0;

        //private void AddChildrenGroupBox()
        //{
        //    // Create GroupBox
        //    GroupBox groupBox = new GroupBox();
        //    groupBox.Text = $"Member {childCount + 1}";
        //    groupBox.Size = new Size(300, 120);
        //    groupBox.Location = new Point(10, 10 + childCount * 130);

        //    // Name Label and TextBox
        //    Label lblName = new Label() { Text = "Name:", Location = new Point(10, 25), AutoSize = true };
        //    TextBox txtName = new TextBox() { Location = new Point(80, 22), Width = 180 };

        //    // Age Label and TextBox
        //    Label lblAge = new Label() { Text = "Age:", Location = new Point(10, 55), AutoSize = true };
        //    TextBox txtAge = new TextBox() { Location = new Point(80, 52), Width = 50 };

        //    // Gender Label and ComboBox
        //    Label lblGender = new Label() { Text = "Gender:", Location = new Point(10, 85), AutoSize = true };
        //    ComboBox cmbGender = new ComboBox() { Location = new Point(80, 82), Width = 100 };
        //    cmbGender.Items.AddRange(new string[] { "Male", "Female", "Other" });

        //    // Add controls to GroupBox
        //    groupBox.Controls.Add(lblName);
        //    groupBox.Controls.Add(txtName);
        //    groupBox.Controls.Add(lblAge);
        //    groupBox.Controls.Add(txtAge);
        //    groupBox.Controls.Add(lblGender);
        //    groupBox.Controls.Add(cmbGender);

        //    // Add GroupBox to panel
        //    flowLayoutPanel1.Controls.Add(groupBox);

        //    childCount++;
        //}

        //private void AddOtherRelationGroupBox()
        //{
        //    // Create GroupBox
        //    GroupBox groupBox = new GroupBox();
        //    groupBox.Text = $"Member {otherRelationCount + 1}";
        //    groupBox.Size = new Size(300, 120);
        //    groupBox.Location = new Point(10, 10 + otherRelationCount * 130);

        //    // Name Label and TextBox
        //    Label lblName = new Label() { Text = "Name:", Location = new Point(10, 25), AutoSize = true };
        //    TextBox txtName = new TextBox() { Location = new Point(80, 22), Width = 180 };

        //    // Age Label and TextBox
        //    Label lblAge = new Label() { Text = "Age:", Location = new Point(10, 55), AutoSize = true };
        //    TextBox txtAge = new TextBox() { Location = new Point(80, 52), Width = 50 };

        //    // Gender Label and ComboBox
        //    Label lblGender = new Label() { Text = "Gender:", Location = new Point(10, 85), AutoSize = true };
        //    ComboBox cmbGender = new ComboBox() { Location = new Point(80, 82), Width = 100 };
        //    cmbGender.Items.AddRange(new string[] { "Male", "Female", "Other" });

        //    // Add controls to GroupBox
        //    groupBox.Controls.Add(lblName);
        //    groupBox.Controls.Add(txtName);
        //    groupBox.Controls.Add(lblAge);
        //    groupBox.Controls.Add(txtAge);
        //    groupBox.Controls.Add(lblGender);
        //    groupBox.Controls.Add(cmbGender);

        //    // Add GroupBox to panel
        //    flowLayoutPanel1.Controls.Add(groupBox);

        //    otherRelationCount++;
        //}

        private void button1_Click(object sender, EventArgs e)
        {
            child1Groupbox.Visible = true;
            childCount++;
        }
    }
}
