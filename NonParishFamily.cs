using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestFat
{
    public partial class NonParishFamily : Form
    {
        public NonParishFamily()
        {
            InitializeComponent();

            tempZipcodetxt.KeyPress += NumberOnlyTextBox_KeyPress;
            tempZipcodetxt.KeyPress += NumberOnlyTextBox_KeyPress;
            txtHeadPhone.KeyPress += NumberOnlyTextBox_KeyPress;
            this.MaximizeBox = false;
        }

        private void btnAddCemetery_Click(object sender, EventArgs e)
        {
            try
            {
                string tempAddress = tempAddresstxt.Text.Trim();
                string tempCity = tempCitytxt.Text.Trim();
                string tempState = tempStatetxt.Text.Trim();
                string tempZip = tempZipcodetxt.Text.Trim();
                string familynotes = txtFamilyNotes.Text.Trim();
                DateTime? deceaseddate = dtdeceased.Checked ? dtdeceased.Value : (DateTime?)null;
                DateTime? burrieddate = dtburried.Checked ? dtburried.Value : (DateTime?)null;
                string headName = txtHeadName.Text.Trim();
                DateTime? headDob = dtheadDOB.Checked ? dtheadDOB.Value : (DateTime?)null;
                string contactgender = txtGender.Text.Trim();
                string headPhone = txtHeadPhone.Text.Trim();
                string contact = txtContact.Text.Trim();
                string cemeterycode = txtCemeterycode.Text.Trim();

                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    conn.Open();
                    try
                    {
                        var familyCmd = new SqlCommand("sp_SaveNonParishCemetery", conn)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        familyCmd.Parameters.AddWithValue("@Name", headName);
                        familyCmd.Parameters.AddWithValue("@gender", contactgender);
                        familyCmd.Parameters.AddWithValue("@DOB", headDob);
                        familyCmd.Parameters.AddWithValue("@Address", tempAddress);
                        familyCmd.Parameters.AddWithValue("@City", tempCity);
                        familyCmd.Parameters.AddWithValue("@State", tempState);
                        familyCmd.Parameters.AddWithValue("@ZipCode", tempZip);
                        familyCmd.Parameters.AddWithValue("@Remarks", familynotes);
                        familyCmd.Parameters.AddWithValue("@ContactPerson", contact);
                        familyCmd.Parameters.AddWithValue("@ContactPhone", headPhone);
                        familyCmd.Parameters.AddWithValue("@DeceasedDate", deceaseddate);
                        familyCmd.Parameters.AddWithValue("@BuriedDate", burrieddate);
                        familyCmd.Parameters.AddWithValue("@cemeterycode", cemeterycode);

                        familyCmd.ExecuteNonQuery();
                        MessageBox.Show("Non-Parish Cemetery saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving family: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NumberOnlyTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only digits and control keys (e.g., backspace)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
