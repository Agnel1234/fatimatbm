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
using System.Windows.Forms.DataVisualization.Charting;

namespace TestFat
{
    public partial class FamilyPopup : Form
    {
        private int childCount = 0;
        private int otherRelationCount = 0;
        public FamilyPopup(int familyID)
        {
            if (familyID == 0)
            {
                InitializeComponent();
                this.StartPosition = FormStartPosition.CenterScreen;
                child1Groupbox.Visible = false;
                child2Groupbox.Visible = false;
                child3Groupbox.Visible = false;
                child4Groupbox.Visible = false;
                child5Groupbox.Visible = false;
                relation1Groupbox.Visible = false;
                otherRelation2Groupbox.Visible = false;
                LoadAnbiyam();
            }
            else
            {
                GetFamilyInfoById(familyID);
            }

        }

        private void btnAddChildren_Click(object sender, EventArgs e)
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
                MessageBox.Show("Family can add maximum of five children", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private int GetMaxFamilyId()
        {
            // Get the maximum Anbiyam ID from the database
            return DatabaseHelper.ExecuteScalarStoredProcedure("sp_GetTotalFamilyCount");
        }

        private void btnAddOtherRelation_Click(object sender, EventArgs e)
        {
            if (otherRelationCount == 0)
            {
                relation1Groupbox.Visible = true;
                otherRelationCount++;
            }
            else if (otherRelationCount == 1)
            {
                otherRelation2Groupbox.Visible = true;
                otherRelationCount++;
            }
            else
            {
                MessageBox.Show("Family can add maximum of two older relations", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadAnbiyam()
        {
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetAllAnbiyam");

            // Insert a default "Select" row at the top
            DataRow newRow = dt.NewRow();
            newRow[dt.Columns[0].ColumnName] = "Select Anbiyam";
            newRow[dt.Columns[1].ColumnName] = 200; // or 0 if you prefer
            dt.Rows.InsertAt(newRow, 0);

            anbiyamCombobox.DataSource = dt;
            anbiyamCombobox.DisplayMember = dt.Columns[0].ColumnName;
            anbiyamCombobox.ValueMember = dt.Columns[1].ColumnName;
            anbiyamCombobox.SelectedIndex = 0; // Ensure "Select" is shown by default
        }

        private void anbiyamCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nextFamilyID = GetMaxFamilyId() + 1;
            string code = GetAnbiyamCodeById((anbiyamCombobox.SelectedIndex));
            familyCodetxt.Text = code + nextFamilyID.ToString();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            int familyId = 0; // Initialize familyId to 0
            try
            {
                // 1. Collect family details
                string familyCode = familyCodetxt.Text.Trim();
                string zone = familyZonetxt.Text.Trim();
                string subscription = familySubscriptiontxt.Text.Trim();
                string permAddress = permAddresstxt.Text.Trim();
                string permCity = permCitytxt.Text.Trim();
                string permState = permStatetxt.Text.Trim();
                string permZip = permZipcodetxt.Text.Trim();
                string tempAddress = tempAddresstxt.Text.Trim();
                string tempCity = tempCitytxt.Text.Trim();
                string tempState = tempStatetxt.Text.Trim();
                string tempZip = tempZipcodetxt.Text.Trim();
                int anbiyamId = (anbiyamCombobox.SelectedValue != null && anbiyamCombobox.SelectedValue.ToString() != "200")
                    ? Convert.ToInt32(anbiyamCombobox.SelectedValue)
                    : 0;

                // 2. Collect head of family (husband) details
                string headName = txtHeadName.Text.Trim();
                DateTime? headDob = dtheadDOB.Checked ? dtheadDOB.Value : (DateTime?)null;
                string headPhone = txtHeadPhone.Text.Trim();
                string headEmail = txtHeadEmail.Text.Trim();
                string headBlood = txtHeadBG.Text.Trim();
                string headOccupation = txtHeadOccupation.Text.Trim();
                string headQualification = txtHeadQualification.Text.Trim();

                // 3. Collect spouse (wife) details
                string spouseName = txtSpouseName.Text.Trim();
                DateTime? spouseDob = dtSpouseDOB.Checked ? dtSpouseDOB.Value : (DateTime?)null;
                string spousePhone = txtSpousePhone.Text.Trim();
                string spouseEmail = txtSpouseEmail.Text.Trim();
                string spouseBlood = txtSpoueBG.Text.Trim();
                string spouseOccupation = txtSpouseOccupation.Text.Trim();
                string spouseQualification = txtSpouseQualification.Text.Trim();

                // 4. Collect children and other relations (repeat for each visible groupbox)
                var members = new List<FamilyMemberDto>();

                // Head
                members.Add(new FamilyMemberDto
                {
                    Name = headName,
                    Relationship = "Head",
                    DOB = headDob,
                    Phone = headPhone,
                    Email = headEmail,
                    BloodGroup = headBlood,
                    Occupation = headOccupation,
                    Qualification = headQualification,
                    IsAdmin = headAdminCouncil.Checked,
                    IsChoir = headChoir.Checked,
                    IsWomenAssoc = headWomenAssc.Checked,
                    IsCatechismTeacher = headCatechismTeacher.Checked,
                    IsLitergyCouncil = headLiturgyCouncil.Checked,
                    IsLOM = headLOM.Checked,
                    IsVDPaul = headStVDPaul.Checked,
                    Baptismdate = headBaptismDate.Checked ? headBaptismDate.Value : (DateTime?)null,
                    Marriagedate = headMarriageDate.Checked ? headMarriageDate.Value : (DateTime?)null,
                    Gender = "Male",
                    MemberGroup = "Family"

                });

                // Spouse
                members.Add(new FamilyMemberDto
                {
                    Name = spouseName,
                    Relationship = "Spouse",
                    DOB = spouseDob,
                    Phone = spousePhone,
                    Email = spouseEmail,
                    BloodGroup = spouseBlood,
                    Occupation = spouseOccupation,
                    Qualification = spouseQualification,
                    IsAdmin = spouseAdminCouncil.Checked,
                    IsChoir = spouseChoir.Checked,
                    IsWomenAssoc = spouseWomenAssc.Checked,
                    IsCatechismTeacher = spouseCatechismTeacher.Checked,
                    IsLitergyCouncil = spouseLiturgyCouncil.Checked,
                    IsLOM = spouseLOM.Checked,
                    IsVDPaul = spouseStVDPaul.Checked,
                    Baptismdate = spouseBaptismDate.Checked ? spouseBaptismDate.Value : (DateTime?)null,
                    Marriagedate = spouseMarriageDate.Checked ? spouseMarriageDate.Value : (DateTime?)null,
                    Gender = "Female",
                    MemberGroup = "Family"
                });

                // Children (example for child1Groupbox)
                if (child1Groupbox.Visible)
                {
                    members.Add(new FamilyMemberDto
                    {
                        Name = txtChild1Name.Text.Trim(),
                        Relationship = comboBoxChild1Relation.Text,
                        DOB = dtChild1DOB.Checked ? dtChild1DOB.Value : (DateTime?)null,
                        Phone = txtChild1Whatsapp.Text.Trim(),
                        Email = "",
                        BloodGroup = txtChild1BG.Text.Trim(),
                        Occupation = txtChild1Occupation.Text.Trim(),
                        Qualification = txtChild1Qualification.Text.Trim(),
                        IsAdmin = checkBoxChild1IsAdmin.Checked,
                        IsChoir = checkBoxChild1IsChoir.Checked,
                        IsWomenAssoc = false,
                        IsCatechismTeacher = checkBoxChild1IsTeacher.Checked,
                        IsLitergyCouncil = false,
                        IsLOM = checkBoxChild1IsLOM.Checked,
                        IsVDPaul = checkBoxChild1VincentDePaul.Checked,
                        IsAlterServices = checkBoxChild1IsAlter.Checked,
                        IsYouth = checkBoxChild1IsYouth.Checked,
                        Baptismdate = dtChild1Baptism.Checked ? dtChild1Baptism.Value : (DateTime?)null,
                        Marriagedate = dtChild1Marriage.Checked ? dtChild1Marriage.Value : (DateTime?)null,
                        confirmation = dtChild1Confirmation.Checked ? dtChild1Confirmation.Value : (DateTime?)null,
                        Communion = dtChild1Communion.Checked ? dtChild1Communion.Value : (DateTime?)null,
                        Priesthood = dtChild1Priest.Checked ? dtChild1Priest.Value : (DateTime?)null,
                        Gender = comboBoxChild1Relation.Text == "Son" ? "Male" : "Female",
                        MemberGroup = "Child1"
                    });
                }
                if (child2Groupbox.Visible)
                {
                    members.Add(new FamilyMemberDto
                    {
                        Name = txtChild2Name.Text.Trim(),
                        Relationship = comboBoxChild2Relation.Text,
                        DOB = dtChild2DOB.Checked ? dtChild2DOB.Value : (DateTime?)null,
                        Phone = txtChild2Whatsapp.Text.Trim(),
                        Email = "",
                        BloodGroup = txtChild2BG.Text.Trim(),
                        Occupation = txtChild2Occupation.Text.Trim(),
                        Qualification = txtChild2Qualification.Text.Trim(),
                        IsAdmin = checkBoxChild2IsAdmin.Checked,
                        IsChoir = checkBoxChild2IsChoir.Checked,
                        IsWomenAssoc = false,
                        IsCatechismTeacher = checkBoxChild2IsTeacher.Checked,
                        IsLitergyCouncil = false,
                        IsLOM = checkBoxChild2IsLOM.Checked,
                        IsVDPaul = checkBoxChild2VincentDePaul.Checked,
                        IsAlterServices = checkBoxChild2IsAlter.Checked,
                        IsYouth = checkBoxChild2IsYouth.Checked,
                        Baptismdate = dtChild2Baptism.Checked ? dtChild2Baptism.Value : (DateTime?)null,
                        Marriagedate = dtChild2Marriage.Checked ? dtChild2Marriage.Value : (DateTime?)null,
                        confirmation = dtChild2Confirmation.Checked ? dtChild2Confirmation.Value : (DateTime?)null,
                        Communion = dtChild2Commuion.Checked ? dtChild2Commuion.Value : (DateTime?)null,
                        Priesthood = dtChild2Priest.Checked ? dtChild2Priest.Value : (DateTime?)null,
                        Gender = comboBoxChild2Relation.Text == "Son" ? "Male" : "Female",
                        MemberGroup = "Child2"
                    });
                }
                if (child3Groupbox.Visible)
                {
                    members.Add(new FamilyMemberDto
                    {
                        Name = txtChild3Name.Text.Trim(),
                        Relationship = comboBoxChild3Relation.Text,
                        DOB = dtChild3DOB.Checked ? dtChild3DOB.Value : (DateTime?)null,
                        Phone = txtChild3Whatsapp.Text.Trim(),
                        Email = "",
                        BloodGroup = txtChild3BG.Text.Trim(),
                        Occupation = txtChild3Occupation.Text.Trim(),
                        Qualification = txtChild3Qualification.Text.Trim(),
                        IsAdmin = checkBoxChild3IsAdmin.Checked,
                        IsChoir = checkBoxChild3IsChoir.Checked,
                        IsWomenAssoc = false,
                        IsCatechismTeacher = checkBoxChild3IsTeacher.Checked,
                        IsLitergyCouncil = false,
                        IsLOM = checkBoxChild3IsLOM.Checked,
                        IsVDPaul = checkBoxChild3VincentDePaul.Checked,
                        IsAlterServices = checkBoxChild3IsAlter.Checked,
                        IsYouth = checkBoxChild3IsYOuth.Checked,
                        Baptismdate = dtChild3Baptism.Checked ? dtChild3Baptism.Value : (DateTime?)null,
                        Marriagedate = dtChild3Marriage.Checked ? dtChild3Marriage.Value : (DateTime?)null,
                        confirmation = dtChild3Confirmation.Checked ? dtChild3Confirmation.Value : (DateTime?)null,
                        Communion = dtChild3Communion.Checked ? dtChild3Communion.Value : (DateTime?)null,
                        Priesthood = dtChild3Priest.Checked ? dtChild3Priest.Value : (DateTime?)null,
                        Gender = comboBoxChild3Relation.Text == "Son" ? "Male" : "Female",
                        MemberGroup = "Child3"
                    });
                }
                if (child4Groupbox.Visible)
                {
                    members.Add(new FamilyMemberDto
                    {
                        Name = txtChild4Name.Text.Trim(),
                        Relationship = comboBoxChild4Relation.Text,
                        DOB = dtChild4DOB.Checked ? dtChild4DOB.Value : (DateTime?)null,
                        Phone = txtChild4Whatsapp.Text.Trim(),
                        Email = "",
                        BloodGroup = txtChild4BG.Text.Trim(),
                        Occupation = txtChild4Occupation.Text.Trim(),
                        Qualification = txtChild4Qualification.Text.Trim(),
                        IsAdmin = checkBoxChild4IsAdmin.Checked,
                        IsChoir = checkBoxChild4IsChoir.Checked,
                        IsWomenAssoc = false,
                        IsCatechismTeacher = checkBoxChild4IsTeacher.Checked,
                        IsLitergyCouncil = false,
                        IsLOM = checkBoxChild4IsLOM.Checked,
                        IsVDPaul = checkBoxChild4VincentDePual.Checked,
                        IsAlterServices = checkBoxChild4IsAlter.Checked,
                        IsYouth = checkBoxChild4IsYOuth.Checked,
                        Baptismdate = dtChild4Baptism.Checked ? dtChild4Baptism.Value : (DateTime?)null,
                        Marriagedate = dtChild4Marriage.Checked ? dtChild4Marriage.Value : (DateTime?)null,
                        confirmation = dtChild4Confirmation.Checked ? dtChild4Confirmation.Value : (DateTime?)null,
                        Communion = dtChild4Communion.Checked ? dtChild4Communion.Value : (DateTime?)null,
                        Priesthood = dtChild4Priest.Checked ? dtChild4Priest.Value : (DateTime?)null,
                        Gender = comboBoxChild4Relation.Text == "Son" ? "Male" : "Female",
                        MemberGroup = "Child4"
                    });
                }
                if (child5Groupbox.Visible)
                {
                    members.Add(new FamilyMemberDto
                    {
                        Name = txtChild5Name.Text.Trim(),
                        Relationship = child5Relation.Text,
                        DOB = child5DOB.Checked ? child5DOB.Value : (DateTime?)null,
                        Phone = txtChild5Whatsapp.Text.Trim(),
                        Email = "",
                        BloodGroup = txtChild5BG.Text.Trim(),
                        Occupation = txtChild5Occupation.Text.Trim(),
                        Qualification = txtChild5Qualification.Text.Trim(),
                        IsAdmin = checkBoxChild5IsAdmin.Checked,
                        IsChoir = checkBoxChild5IsChoir.Checked,
                        IsWomenAssoc = false,
                        IsCatechismTeacher = checkBoxChild5IsTeacher.Checked,
                        IsLitergyCouncil = false,
                        IsLOM = checkBoxChild5IsLOM.Checked,
                        IsVDPaul = checkBoxChild5VincentDePaul.Checked,
                        IsAlterServices = checkBoxChild5IsAlter.Checked,
                        IsYouth = checkBoxChild5IsYOuth.Checked,
                        Baptismdate = child5Baptism.Checked ? child5Baptism.Value : (DateTime?)null,
                        Marriagedate = child5Marriage.Checked ? child5Marriage.Value : (DateTime?)null,
                        confirmation = child5Confirmation.Checked ? child5Confirmation.Value : (DateTime?)null,
                        Communion = child5Communion.Checked ? child5Communion.Value : (DateTime?)null,
                        Priesthood = child5Priest.Checked ? child5Priest.Value : (DateTime?)null,
                        Gender = child5Relation.Text == "Son" ? "Male" : "Female",
                        MemberGroup = "Child5"
                    });
                }

                // Other Relations
                if (relation1Groupbox.Visible)
                {
                    members.Add(new FamilyMemberDto
                    {
                        Name = txtOther1Name.Text.Trim(),
                        Relationship = relation1Combobox.Text, // Use a ComboBox for relationship type if available
                        DOB = dtOther1dob.Checked ? dtOther1dob.Value : (DateTime?)null,
                        Phone = txtOther1Whatsapp.Text.Trim(),
                        Email = "", // Add if you have a field
                        BloodGroup = txtOther1Bloodgroup.Text.Trim(),
                        Occupation = txtOther1Occupation.Text.Trim(),
                        Qualification = txtOther1Qualification.Text.Trim(),
                        IsAdmin = checkBoxOther1IsAdmin.Checked,
                        IsChoir = checkBoxOther1IsChoir.Checked,
                        IsWomenAssoc = false,
                        IsCatechismTeacher = checkBoxOther1IsTeacher.Checked,
                        IsLitergyCouncil = false,
                        IsLOM = checkBoxOther1IsLOM.Checked,
                        IsVDPaul = checkBoxOther1IsVencntDePaul.Checked,
                        IsAlterServices = checkBoxOther1IsAlter.Checked,
                        IsYouth = checkBoxOther1IsYouth.Checked,
                        Baptismdate = dtOther1Baptism.Checked ? dtOther1Baptism.Value : (DateTime?)null,
                        Marriagedate = dtOther1Matrimony.Checked ? dtOther1Matrimony.Value : (DateTime?)null,
                        confirmation = dtOther1Confirmation.Checked ? dtOther1Confirmation.Value : (DateTime?)null,
                        Communion = dtOther1Communion.Checked ? dtOther1Communion.Value : (DateTime?)null,
                        Priesthood = null, // Add if you have a field
                        Gender = getGender(relation1Combobox.Text),
                        MemberGroup = "Other1"
                    });
                }
                if (otherRelation2Groupbox.Visible)
                {
                    members.Add(new FamilyMemberDto
                    {
                        Name = txtOther2Name.Text.Trim(),
                        Relationship = other2Combobox.Text,
                        DOB = dtother2dob.Checked ? dtother2dob.Value : (DateTime?)null,
                        Phone = txtOther2Whatsapp.Text.Trim(),
                        Email = "", // Add if you have a field
                        BloodGroup = txtOther2bloodgroup.Text.Trim(),
                        Occupation = txtOther2Occupation.Text.Trim(),
                        Qualification = txtOther2Qualification.Text.Trim(),
                        IsAdmin = checkBoxOther2IsAdmin.Checked,
                        IsChoir = checkBoxOther2IsChoir.Checked,
                        IsWomenAssoc = false,
                        IsCatechismTeacher = checkBoxOther2IsTeacher.Checked,
                        IsLitergyCouncil = false,
                        IsLOM = checkBoxOther2IsLOM.Checked,
                        IsVDPaul = checkBoxOther2IsVincentDePaul.Checked,
                        IsAlterServices = checkBoxOther2IsAlter.Checked,
                        IsYouth = checkBoxOther2IsYouth.Checked,
                        Baptismdate = dtOther2Baptism.Checked ? dtOther2Baptism.Value : (DateTime?)null,
                        Marriagedate = dtOther2Marriage.Checked ? dtOther2Marriage.Value : (DateTime?)null,
                        confirmation = dtOther2Confirmation.Checked ? dtOther2Confirmation.Value : (DateTime?)null,
                        Communion = dtOther2Communion.Checked ? dtOther2Communion.Value : (DateTime?)null,
                        Priesthood = null, // Add if you have a field
                        Gender = getGender(other2Combobox.Text),
                        MemberGroup = "Other2"
                    });
                }

                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            // Save family
                            var familyCmd = new SqlCommand("sp_SaveFamily", conn, tran)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            familyCmd.Parameters.AddWithValue("@family_code", familyCode);
                            familyCmd.Parameters.AddWithValue("@head_of_family", headName);
                            familyCmd.Parameters.AddWithValue("@gender", "Male"); // or get from UI
                            familyCmd.Parameters.AddWithValue("@family_permanant_address", permAddress);
                            familyCmd.Parameters.AddWithValue("@family_temp_address", tempAddress);
                            familyCmd.Parameters.AddWithValue("@phone", headPhone);
                            familyCmd.Parameters.AddWithValue("@email", headEmail);
                            familyCmd.Parameters.AddWithValue("@anbiyam_id", anbiyamId);
                            familyCmd.Parameters.AddWithValue("@family_perm_city", permCitytxt.Text.Trim());
                            familyCmd.Parameters.AddWithValue("@family_perm_state", permStatetxt.Text.Trim());
                            familyCmd.Parameters.AddWithValue("@family_perm_zipcode", permZipcodetxt.Text.Trim());
                            familyCmd.Parameters.AddWithValue("@family_temp_city", tempCitytxt.Text.Trim());
                            familyCmd.Parameters.AddWithValue("@family_temp_state", tempStatetxt.Text.Trim());
                            familyCmd.Parameters.AddWithValue("@family_temp_zipcode", tempZipcodetxt.Text.Trim());
                            familyCmd.Parameters.AddWithValue("@monthly_subscription", subscription);

                            var familyIdParam = new SqlParameter("@family_id", SqlDbType.Int) { Direction = ParameterDirection.Output };
                            familyCmd.Parameters.Add(familyIdParam);

                            if (spouseName != "" && spousePhone != null && spouseMarriageDate != null)
                            {
                                familyCmd.ExecuteNonQuery();
                                familyId = (int)familyIdParam.Value;
                            }
                            else
                            {
                                MessageBox.Show("Please enter spouse details", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            if (familyId == 0)
                            {
                                MessageBox.Show("Failed to save family. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            else
                            {
                                // Save members
                                foreach (var m in members)
                                {
                                    var memberCmd = new SqlCommand("sp_SaveFamilyMember", conn, tran)
                                    {
                                        CommandType = CommandType.StoredProcedure
                                    };
                                    memberCmd.Parameters.AddWithValue("@family_id", familyId);
                                    memberCmd.Parameters.AddWithValue("@first_name", m.Name);
                                    memberCmd.Parameters.AddWithValue("@relationship", m.Relationship);
                                    memberCmd.Parameters.AddWithValue("@gender", m.Gender);
                                    memberCmd.Parameters.AddWithValue("@dob", m.DOB);
                                    memberCmd.Parameters.AddWithValue("@member_status", "Active");
                                    memberCmd.Parameters.AddWithValue("@phone", m.Phone ?? "00000000");
                                    memberCmd.Parameters.AddWithValue("@occupation", m.Occupation);
                                    memberCmd.Parameters.AddWithValue("@qualification", m.Qualification);
                                    memberCmd.Parameters.AddWithValue("@blood_group", m.BloodGroup);
                                    memberCmd.Parameters.AddWithValue("@email", m.Phone ?? "samplemeil@email.com");
                                    memberCmd.Parameters.AddWithValue("@baptized_date", m.Baptismdate);
                                    memberCmd.Parameters.AddWithValue("@marriage_date", m.Marriagedate);
                                    memberCmd.Parameters.AddWithValue("@isadmin", m.IsAdmin);
                                    memberCmd.Parameters.AddWithValue("@legionofmary", m.IsLOM);
                                    memberCmd.Parameters.AddWithValue("@isvencentdepaul", m.IsVDPaul);
                                    memberCmd.Parameters.AddWithValue("@ischoir", m.IsChoir);
                                    memberCmd.Parameters.AddWithValue("@iscatechismteacher", m.IsCatechismTeacher);
                                    memberCmd.ExecuteNonQuery();
                                }
                            }

                            tran.Commit();
                            MessageBox.Show("Family saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Close();

                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            MessageBox.Show("Error saving family: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string GetAnbiyamCodeById(int anbiyamId)
        {
            string anbiyamCode = null;
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("sp_GetAnbiyamCodeById", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@anbiyam_id", anbiyamId);

                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    anbiyamCode = result.ToString();
            }
            return anbiyamCode;
        }

        public void GetFamilyInfoById(int familyID)
        {
            var param = new SqlParameter("@family_id", familyID);
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyDetailsById", param);
            if (dt.Rows.Count > 0)
            {
                //foreach (DataRow row in dt.Rows)
                //{
                //    string zone = row["Zone"].ToString();
                //    int count = Convert.ToInt32(row["FamilyCount"]);

                //}

                DataRow row = dt.Rows[0];
                familyCodetxt.Text = row["FamilyCode"].ToString();
                familyZonetxt.Text = row["Zone"].ToString();
                familySubscriptiontxt.Text = row["MonthlySubscription"].ToString();
                permAddresstxt.Text = row["PermAddress"].ToString();
                permCitytxt.Text = row["PermCity"].ToString();
                permStatetxt.Text = row["PermState"].ToString();
                permZipcodetxt.Text = row["PermZip"].ToString();
                tempAddresstxt.Text = row["TempAddress"].ToString();
                tempCitytxt.Text = row["TempCity"].ToString();
                tempStatetxt.Text = row["TempState"].ToString();
                tempZipcodetxt.Text = row["TempZip"].ToString();
                txtHeadName.Text = row["HeadOfFamily"].ToString();
                dtheadDOB.Value = Convert.ToDateTime(row["HeadDOB"]);
                txtHeadPhone.Text = row["HeadPhone"].ToString();
                txtHeadEmail.Text = row["HeadEmail"].ToString();
                txtHeadBG.Text = row["HeadBloodGroup"].ToString();
                txtHeadOccupation.Text = row["HeadOccupation"].ToString();
                txtHeadQualification.Text = row["HeadQualification"].ToString();

            }
            else
            {
                MessageBox.Show("Something went wrong while trying to Edit this Family", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private string getGender(string relationship)
        {
            if (relationship == "Brother" || relationship == "Brother-In-Law" || relationship == "Father" || relationship == "Father-In-Law")
            {
                return "Male";
            }
            else
            {
                return "Female";
            }
        }
    }
    // Change the access modifier of the FamilyMemberDto class from private to internal  
    internal class FamilyMemberDto
    {
        public string Name { get; set; }
        public string Relationship { get; set; }
        public DateTime? DOB { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string BloodGroup { get; set; }
        public string Occupation { get; set; }
        public string Qualification { get; set; }
        public DateTime? Marriagedate { get; set; }
        public DateTime? Baptismdate { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsLOM { get; set; }
        public bool IsChoir { get; set; }
        public bool IsVDPaul { get; set; }
        public bool IsWomenAssoc { get; set; }
        public bool IsYouth { get; set; }
        public bool IsAlterServices { get; set; }
        public bool IsCatechismTeacher { get; set; }
        public bool IsLitergyCouncil { get; set; }
        public string Gender { get; set; }
        public string MemberGroup { get; set; }
        public DateTime? confirmation { get; set; }
        public DateTime? Communion { get; set; }
        public DateTime? Priesthood { get; set; }
    }
}