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
        private int childCount = 0;
        private int otherRelationCount = 0;
        public FamilyPopup()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            child1Groupbox.Visible = false;
            child2Groupbox.Visible = false;
            child3Groupbox.Visible = false;
            child4Groupbox.Visible = false;
            child5Groupbox.Visible = false;
            otherRelation1Groupbox.Visible = false;
            otherRelation2Groupbox.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (childCount == 0)
            {
                child1Groupbox.Visible = true;
                childCount++;
            }
            else if (childCount == 1)
            {
                child2Groupbox.Visible = true;
                childCount++;
            }
             else if (childCount == 2)
            {
                child3Groupbox.Visible = true;
                childCount++;
            }
            else if (childCount == 3)
            {
                child4Groupbox.Visible = true;
                childCount++;
            }
            else if (childCount == 4)
            {
                child5Groupbox.Visible = true;
                childCount++;
            }
            else
            {
                MessageBox.Show("Information","You can only add five children.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(otherRelationCount == 0)
            {
                otherRelation1Groupbox.Visible = true;
                otherRelationCount++;
            }
            else if (otherRelationCount == 1)
            {
                otherRelation2Groupbox.Visible = true;
                otherRelationCount++;
            }
            else
            {
                MessageBox.Show("Information", "You Can Only Add Two Relations.");
            }
        }
    }
}
