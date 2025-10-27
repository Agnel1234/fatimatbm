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
        int familyId = 0; // Initialize familyId to 0

        int headMemberId;
        int spouseMemberId;
        int other1MemberId;
        int other2MemberId;
        int child1MemberId;
        int child2MemberId;
        int child3MemberId;
        int child4MemberId;
        int child5MemberId;

        public FamilyPopup(int familyID)

        {
            InitializeComponent();

            permZipcodetxt.KeyPress += NumberOnlyTextBox_KeyPress;
            tempZipcodetxt.KeyPress += NumberOnlyTextBox_KeyPress;
            familySubscriptiontxt.KeyPress += NumberOnlyTextBox_KeyPress;
            txtHeadPhone.KeyPress += NumberOnlyTextBox_KeyPress;
            txtSpousePhone.KeyPress += NumberOnlyTextBox_KeyPress;
            txtChild1Whatsapp.KeyPress += NumberOnlyTextBox_KeyPress;
            txtChild2Whatsapp.KeyPress += NumberOnlyTextBox_KeyPress;
            txtChild3Whatsapp.KeyPress += NumberOnlyTextBox_KeyPress;
            txtChild4Whatsapp.KeyPress += NumberOnlyTextBox_KeyPress;
            txtChild5Whatsapp.KeyPress += NumberOnlyTextBox_KeyPress;
            txtOther1Whatsapp.KeyPress += NumberOnlyTextBox_KeyPress;
            txtOther2Whatsapp.KeyPress += NumberOnlyTextBox_KeyPress;

            permZipcodetxt.MaxLength = 6;
            tempZipcodetxt.MaxLength = 6;
            familySubscriptiontxt.MaxLength = 5;
            txtHeadPhone.MaxLength = 10;
            txtSpousePhone.MaxLength = 10;
            txtChild1Whatsapp.MaxLength = 10;
            txtChild2Whatsapp.MaxLength = 10;
            txtChild3Whatsapp.MaxLength = 10;
            txtChild4Whatsapp.MaxLength = 10;
            txtChild5Whatsapp.MaxLength = 10;
            txtOther1Whatsapp.MaxLength = 10;
            txtOther2Whatsapp.MaxLength = 10;

            this.StartPosition = FormStartPosition.CenterScreen;
            child1Groupbox.Visible = false;
            child2Groupbox.Visible = false;
            child3Groupbox.Visible = false;
            child4Groupbox.Visible = false;
            child5Groupbox.Visible = false;
            relation1Groupbox.Visible = false;
            otherRelation2Groupbox.Visible = false;
            LoadAnbiyam();
            if (familyID != 0)
            {
                bool isloadedSuccess = LoadFamilyInfoById(familyID);
                familyId = familyID;

                if (isloadedSuccess)
                    LoadFamilyMemberDetails(familyID);
            }
            else
            {
                headMemberId = 0;
                spouseMemberId = 0;
                other1MemberId = 0;
                other2MemberId = 0;
                child1MemberId = 0;
                child2MemberId = 0;
                child3MemberId = 0;
                child4MemberId = 0;
                child5MemberId = 0;
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

        private int GetMaxFamilyId(int anbiyamiD)
        {
            var param = new SqlParameter("@anbiyam_id", anbiyamiD);
            return DatabaseHelper.ExecuteScalarStoredProcedure("sp_GetTotalFamilyCount", param);
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
            string code = "";
            int nextFamilyID = 0;

            if (anbiyamCombobox.SelectedIndex == 0)
            {
                familyCodetxt.Text = "Select Anbiyam";
                return;
            } else
            {
                int selectedAnbiyamId = int.Parse(anbiyamCombobox.SelectedValue.ToString(), 0);
                nextFamilyID = GetMaxFamilyId(selectedAnbiyamId) + 1;
                code = GetAnbiyamCodeById(int.Parse(anbiyamCombobox.SelectedValue.ToString(), 0));
            }
                
            familyCodetxt.Text = code + nextFamilyID.ToString();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
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
                DateTime? lastSubscriptionDate = dtLastSubscription.Checked ? dtLastSubscription.Value : (DateTime?)null;

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
                if(checkBoxEnableHusband.Checked == true && headName != "")
                {
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
                        MemberGroup = "Family",
                        MemberId = headMemberId
                    });
                }

                // Spouse
                if(checkBoxEnableWife.Checked == true && spouseName != "")
                {
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
                        MemberGroup = "Family",
                        MemberId = spouseMemberId,
                        
                    });
                }

                // Children (example for child1Groupbox)
                if (child1Groupbox.Visible)
                {
                    if(txtChild1Name.Text != null && txtChild1Name.Text != "")
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
                            ChildInstitution = txtChild1Institution.Text.Trim(),
                            ChildClass = txtChild1Standard.Text.Trim(),
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
                            MemberGroup = "Child1",
                            MemberId = child1MemberId
                        });
                    }
                }
                if (child2Groupbox.Visible)
                {
                    if (txtChild2Name.Text != null && txtChild2Name.Text != "")
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
                            ChildInstitution = txtChild2Institution.Text.Trim(),
                            ChildClass = txtChild2Standard.Text.Trim(),
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
                            MemberGroup = "Child2",
                            MemberId = child2MemberId
                        });
                    }
                }
                if (child3Groupbox.Visible)
                {
                    if (txtChild3Name.Text != null && txtChild3Name.Text != "")
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
                            ChildInstitution = txtChild3Institution.Text.Trim(),
                            ChildClass = txtChild3Standard.Text.Trim(),
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
                            MemberGroup = "Child3",
                            MemberId = child3MemberId
                        });
                    }
                }
                if (child4Groupbox.Visible)
                {
                    if (txtChild4Name.Text != null && txtChild4Name.Text != "")
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
                            ChildInstitution = txtChild4Institution.Text.Trim(),
                            ChildClass = txtChild4Standard.Text.Trim(),
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
                            MemberGroup = "Child4",
                            MemberId = child4MemberId
                        });
                    }
                }
                if (child5Groupbox.Visible)
                {
                    if (txtChild5Name.Text != null && txtChild5Name.Text != "")
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
                            ChildInstitution = txtChild5Institution.Text.Trim(),
                            ChildClass = txtChild5Standard.Text.Trim(),
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
                            MemberGroup = "Child5",
                            MemberId = child5MemberId
                        });
                    }
                }

                // Other Relations
                if (relation1Groupbox.Visible)
                {
                    if (txtOther1Name.Text != null && txtOther1Name.Text != "")
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
                            MemberGroup = "Other1",
                            MemberId = other1MemberId
                        });
                    }
                }
                if (otherRelation2Groupbox.Visible)
                {
                    if (txtOther2Name.Text != null && txtOther2Name.Text != "")
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
                            MemberGroup = "Other2",
                            MemberId = other2MemberId
                        });
                    }
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
                            familyCmd.Parameters.AddWithValue("@family_permanant_address", permAddress);
                            familyCmd.Parameters.AddWithValue("@family_temp_address", tempAddress);
                            familyCmd.Parameters.AddWithValue("@anbiyam_id", anbiyamId);
                            familyCmd.Parameters.AddWithValue("@family_perm_city", permCitytxt.Text.Trim());
                            familyCmd.Parameters.AddWithValue("@family_perm_state", permStatetxt.Text.Trim());
                            familyCmd.Parameters.AddWithValue("@family_perm_zipcode", permZipcodetxt.Text.Trim());
                            familyCmd.Parameters.AddWithValue("@family_temp_city", tempCitytxt.Text.Trim());
                            familyCmd.Parameters.AddWithValue("@family_temp_state", tempStatetxt.Text.Trim());
                            familyCmd.Parameters.AddWithValue("@family_temp_zipcode", tempZipcodetxt.Text.Trim());
                            familyCmd.Parameters.AddWithValue("@monthly_subscription", subscription);
                            familyCmd.Parameters.AddWithValue("@is_multiple_cards", multipleCardsCheckbox.Checked);
                            familyCmd.Parameters.AddWithValue("@family_notes", txtFamilyNotes.Text.Trim());
                            familyCmd.Parameters.AddWithValue("@last_subscriptin_date", lastSubscriptionDate);
                            familyCmd.Parameters.AddWithValue("@ishusbandactive", checkBoxEnableHusband.Checked);
                            familyCmd.Parameters.AddWithValue("@iswifeactive", checkBoxEnableWife.Checked);

                            if (checkBoxEnableHusband.Checked)
                            {                                
                                familyCmd.Parameters.AddWithValue("@head_of_family", headName);
                                familyCmd.Parameters.AddWithValue("@gender", "Male");
                                familyCmd.Parameters.AddWithValue("@phone", headPhone);
                                familyCmd.Parameters.AddWithValue("@email", headEmail);
                            }
                            else
                            {
                                familyCmd.Parameters.AddWithValue("@head_of_family", spouseName);
                                familyCmd.Parameters.AddWithValue("@gender", "Female");
                                familyCmd.Parameters.AddWithValue("@phone", spousePhone);
                                familyCmd.Parameters.AddWithValue("@email", spouseEmail);
                            }

                            var familyIdParam = new SqlParameter("@family_id", familyId) { Direction = ParameterDirection.InputOutput };
                            familyCmd.Parameters.Add(familyIdParam);

                            if(checkBoxEnableHusband.Checked && checkBoxEnableWife.Checked)
                            {
                                if (spouseName != "")
                                {
                                    familyCmd.ExecuteNonQuery();
                                    familyId = (int)familyIdParam.Value;
                                }
                                else
                                {
                                    MessageBox.Show("Please enter spouse details", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                            else if (checkBoxEnableHusband.Checked && checkBoxEnableWife.Checked == false)
                            {
                                if (headName != "")
                                {
                                    familyCmd.ExecuteNonQuery();
                                    familyId = (int)familyIdParam.Value;
                                }
                                else
                                {
                                    MessageBox.Show("Please enter Family Head details", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                            else if (checkBoxEnableHusband.Checked == false && checkBoxEnableWife.Checked == true)
                            {
                                if (spouseName != "")
                                {
                                    familyCmd.ExecuteNonQuery();
                                    familyId = (int)familyIdParam.Value;
                                }
                                else
                                {
                                    MessageBox.Show("Please enter Family Head(Wife) details", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Choose either Husband or Wife as Family Head", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                                    memberCmd.Parameters.AddWithValue("@phone", m.Phone ?? "");
                                    memberCmd.Parameters.AddWithValue("@occupation", m.Occupation);
                                    memberCmd.Parameters.AddWithValue("@qualification", m.Qualification);
                                    memberCmd.Parameters.AddWithValue("@blood_group", m.BloodGroup);
                                    memberCmd.Parameters.AddWithValue("@email", m.Email ?? "");
                                    memberCmd.Parameters.AddWithValue("@baptized_date", m.Baptismdate);
                                    memberCmd.Parameters.AddWithValue("@marriage_date", m.Marriagedate);
                                    memberCmd.Parameters.AddWithValue("@isadmin", m.IsAdmin);
                                    memberCmd.Parameters.AddWithValue("@legionofmary", m.IsLOM);
                                    memberCmd.Parameters.AddWithValue("@isvencentdepaul", m.IsVDPaul);
                                    memberCmd.Parameters.AddWithValue("@ischoir", m.IsChoir);
                                    memberCmd.Parameters.AddWithValue("@iscatechismteacher", m.IsCatechismTeacher);
                                    memberCmd.Parameters.AddWithValue("@iswomenassoc", m.IsWomenAssoc);
                                    memberCmd.Parameters.AddWithValue("@islitergycouncil", m.IsLitergyCouncil);
                                    memberCmd.Parameters.AddWithValue("@isalterservices", m.IsAlterServices);
                                    memberCmd.Parameters.AddWithValue("@isyouth", m.IsYouth);
                                    memberCmd.Parameters.AddWithValue("@confirmation_date", m.confirmation);
                                    memberCmd.Parameters.AddWithValue("@first_communion_date", m.Communion);
                                    memberCmd.Parameters.AddWithValue("@priesthood_date", m.Priesthood);
                                    memberCmd.Parameters.AddWithValue("@member_group", m.MemberGroup);
                                    memberCmd.Parameters.AddWithValue("@childclass", m.ChildClass);
                                    memberCmd.Parameters.AddWithValue("@child_institution", m.ChildInstitution);
                                    memberCmd.Parameters.AddWithValue("@member_id", m.MemberId);

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

        public bool LoadFamilyInfoById(int familyID)
        {
            var param = new SqlParameter("@family_id", familyID);
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyDetailsById", param);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                familyCodetxt.Text = row[0] != null ? row[0].ToString(): null  ;
                anbiyamCombobox.SelectedValue = row[1] != DBNull.Value ? Convert.ToInt32(row[1]) : 0;
                familyZonetxt.Text = row[2] != null ? row[2].ToString() : null;
                familySubscriptiontxt.Text = row[11] != null ? row[11].ToString() : null;
                permAddresstxt.Text = row[3] != null ? row[3].ToString() : null;
                permCitytxt.Text = row[5] != null ? row[5].ToString() : null;
                permStatetxt.Text = row[6] != null ? row[6].ToString() : null;
                permZipcodetxt.Text = row[7] != null ? row[7].ToString() : null;
                tempAddresstxt.Text = row[4] != null ? row[4].ToString() : null;
                tempCitytxt.Text = row[8] != null ? row[8].ToString() : null;
                tempStatetxt.Text = row[9] != null ? row[9].ToString() : null;
                tempZipcodetxt.Text = row[10] != null ? row[10].ToString() : null;
                familySubscriptiontxt.Text = row[11] != null ? row[11].ToString() : null;
                multipleCardsCheckbox.Checked = row[12] != DBNull.Value && Convert.ToBoolean(row[12]);
                txtFamilyNotes.Text = row[13] != null ? row[13].ToString() : null;
                dtLastSubscription.Value = row[14] != DBNull.Value ? Convert.ToDateTime(row[14]) : DateTime.Now;
                dtLastSubscription.Checked = true;

                return true;
            }
            else
            {
                MessageBox.Show("Something went wrong while trying to Edit this Family", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        public void LoadFamilyMemberDetails(int familyID)
        {
            int childCount = 1;
            checkBoxEnableHusband.Checked = false;
            checkBoxEnableWife.Checked = false;
            int otherRelationCount = 1;

            var param = new SqlParameter("@family_id", familyID);
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetFamilyMembersDetailsByFamilyId", param);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row[1] != null && row[1].ToString() == "Head")
                    {
                        txtHeadName.Text = row[0] != null ? row[0].ToString() : null;
                        dtheadDOB.Value = row[2] != DBNull.Value ? Convert.ToDateTime(row[2]) : DateTime.Now;
                        dtheadDOB.Checked = row[2] != DBNull.Value ? true : false;
                        txtHeadPhone.Text = row[3] != null ? row[3].ToString() : null;
                        txtHeadQualification.Text = row[7] != null ? row[7].ToString() : null;
                        txtHeadEmail.Text = row[4] != null ? row[4].ToString() : null;
                        txtHeadOccupation.Text = row[6] != null ? row[6].ToString() : null;
                        txtHeadBG.Text = row[8] != null ? row[8].ToString() : null;

                        headAdminCouncil.Checked = row[14] != DBNull.Value && Convert.ToBoolean(row[14]);
                        headChoir.Checked = row[19] != DBNull.Value && Convert.ToBoolean(row[19]);
                        headWomenAssc.Checked = row[22] != DBNull.Value && Convert.ToBoolean(row[22]);
                        headCatechismTeacher.Checked = row[20] != DBNull.Value && Convert.ToBoolean(row[20]);
                        headLiturgyCouncil.Checked = row[24] != DBNull.Value && Convert.ToBoolean(row[24]);
                        headLOM.Checked = row[15] != DBNull.Value && Convert.ToBoolean(row[15]);
                        headStVDPaul.Checked = row[18] != DBNull.Value && Convert.ToBoolean(row[18]);

                        headBaptismDate.Checked = row[10] != DBNull.Value ? true: false ;
                        headBaptismDate.Value = row[10] != DBNull.Value ? Convert.ToDateTime(row[10]) : DateTime.Now;
                        headMarriageDate.Checked = row[9] != DBNull.Value ? true : false;
                        headMarriageDate.Value = row[9] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;
                        checkBoxEnableHusband.Checked = row[26] != DBNull.Value && Convert.ToBoolean(row[26]);

                        headMemberId = row[28] != DBNull.Value ? Convert.ToInt32(row[28]) : 0;
                    }
                    else if (row[1] != null && row[1].ToString() == "Spouse")
                    {
                        txtSpouseName.Text = row[0] != null ? row[0].ToString() : null;
                        dtSpouseDOB.Value = row[2] != DBNull.Value ? Convert.ToDateTime(row[2]) : DateTime.Now;
                        dtSpouseDOB.Checked = row[2] != DBNull.Value ? true : false;
                        txtSpousePhone.Text = row[3] != null ? row[3].ToString() : null;
                        txtSpouseQualification.Text = row[7] != null ? row[7].ToString() : null;
                        txtSpouseEmail.Text = row[4] != null ? row[4].ToString() : null;
                        txtSpouseOccupation.Text = row[6] != null ? row[6].ToString() : null;
                        txtSpoueBG.Text = row[8] != null ? row[8].ToString() : null;

                        spouseAdminCouncil.Checked = row[14] != DBNull.Value && Convert.ToBoolean(row[14]);
                        spouseChoir.Checked = row[19] != DBNull.Value && Convert.ToBoolean(row[19]);
                        spouseWomenAssc.Checked = row[22] != DBNull.Value && Convert.ToBoolean(row[22]);
                        spouseCatechismTeacher.Checked = row[20] != DBNull.Value && Convert.ToBoolean(row[20]);
                        spouseLiturgyCouncil.Checked = row[24] != DBNull.Value && Convert.ToBoolean(row[24]);
                        spouseLOM.Checked = row[15] != DBNull.Value && Convert.ToBoolean(row[15]);
                        spouseStVDPaul.Checked = row[18] != DBNull.Value && Convert.ToBoolean(row[18]);

                        spouseBaptismDate.Checked = row[10] != DBNull.Value ? true : false;
                        spouseBaptismDate.Value = row[10] != DBNull.Value ? Convert.ToDateTime(row[10]) : DateTime.Now;
                        spouseMarriageDate.Checked = row[9] != DBNull.Value ? true : false;
                        spouseMarriageDate.Value = row[9] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;
                        checkBoxEnableWife.Checked = row[27] != DBNull.Value && Convert.ToBoolean(row[27]);

                        spouseMemberId = row[28] != DBNull.Value ? Convert.ToInt32(row[28]) : 0;
                    }
                    else if (row[1] != null && ( row[1].ToString() == "Son" || row[1].ToString() == "Daughter"))
                    {
                        if(childCount == 1)
                        {
                            child1Groupbox.Visible = true;
                            txtChild1Name.Text = row[0] != null ? row[0].ToString() : null;
                            dtChild1DOB.Value = row[2] != DBNull.Value ? Convert.ToDateTime(row[2]) : DateTime.Now;
                            dtChild1DOB.Checked = row[2] != DBNull.Value ? true : false;
                            txtChild1Whatsapp.Text = row[3] != null ? row[3].ToString() : null;
                            txtChild1Qualification.Text = row[7] != null ? row[7].ToString() : null;
                            txtChild1Occupation.Text = row[6] != null ? row[6].ToString() : null;
                            txtChild1BG.Text = row[8] != null ? row[8].ToString() : null;
                            comboBoxChild1Relation.Text = row[1] != null ? row[1].ToString() : null;
                            txtChild1Standard.Text = row[29] != null ? row[29].ToString() : null;
                            txtChild1Institution.Text = row[23] != null ? row[23].ToString() : null;

                            checkBoxChild1IsAdmin.Checked = row[14] != DBNull.Value && Convert.ToBoolean(row[14]);
                            checkBoxChild1IsLOM.Checked = row[15] != DBNull.Value && Convert.ToBoolean(row[15]);
                            checkBoxChild1IsYouth.Checked = row[16] != DBNull.Value && Convert.ToBoolean(row[16]);
                            checkBoxChild1IsAlter.Checked = row[17] != DBNull.Value && Convert.ToBoolean(row[17]);
                            checkBoxChild1VincentDePaul.Checked = row[18] != DBNull.Value && Convert.ToBoolean(row[18]);
                            checkBoxChild1IsChoir.Checked = row[19] != DBNull.Value && Convert.ToBoolean(row[19]);
                            checkBoxChild1IsStudnet.Checked = row[21] != DBNull.Value && Convert.ToBoolean(row[21]);
                            checkBoxChild1IsTeacher.Checked = row[20] != DBNull.Value && Convert.ToBoolean(row[20]);

                            dtChild1Baptism.Checked = row[10] != DBNull.Value ? true : false;
                            dtChild1Baptism.Value = row[10] != DBNull.Value ? Convert.ToDateTime(row[10]) : DateTime.Now;
                            dtChild1Marriage.Checked = row[9] != DBNull.Value ? true : false;
                            dtChild1Marriage.Value = row[9] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;
                            dtChild1Communion.Checked = row[11] != DBNull.Value ? true : false;
                            dtChild1Communion.Value = row[11] != DBNull.Value ? Convert.ToDateTime(row[10]) : DateTime.Now;
                            dtChild1Confirmation.Checked = row[12] != DBNull.Value ? true : false;
                            dtChild1Confirmation.Value = row[12] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;
                            dtChild1Priest.Checked = row[13] != DBNull.Value ? true : false;
                            dtChild1Priest.Value = row[13] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;

                            child1MemberId = row[28] != DBNull.Value ? Convert.ToInt32(row[28]) : 0;
                        }
                        else if (childCount == 2)
                        {
                            child2Groupbox.Visible = true;
                            txtChild2Name.Text = row[0] != null ? row[0].ToString() : null;
                            dtChild2DOB.Value = row[2] != DBNull.Value ? Convert.ToDateTime(row[2]) : DateTime.Now;
                            dtChild2DOB.Checked = row[2] != DBNull.Value ? true : false;
                            txtChild2Whatsapp.Text = row[3] != null ? row[3].ToString() : null;
                            txtChild2Qualification.Text = row[7] != null ? row[7].ToString() : null;
                            txtChild2Occupation.Text = row[6] != null ? row[6].ToString() : null;
                            txtChild2BG.Text = row[8] != null ? row[8].ToString() : null;
                            comboBoxChild2Relation.Text = row[1] != null ? row[1].ToString() : null;
                            txtChild2Standard.Text = row[29] != null ? row[29].ToString() : null;
                            txtChild2Institution.Text = row[23] != null ? row[23].ToString() : null;

                            checkBoxChild2IsAdmin.Checked = row[14] != DBNull.Value && Convert.ToBoolean(row[14]);
                            checkBoxChild2IsLOM.Checked = row[15] != DBNull.Value && Convert.ToBoolean(row[15]);
                            checkBoxChild2IsYouth.Checked = row[16] != DBNull.Value && Convert.ToBoolean(row[16]);
                            checkBoxChild2IsAlter.Checked = row[17] != DBNull.Value && Convert.ToBoolean(row[17]);
                            checkBoxChild2VincentDePaul.Checked = row[18] != DBNull.Value && Convert.ToBoolean(row[18]);
                            checkBoxChild2IsChoir.Checked = row[19] != DBNull.Value && Convert.ToBoolean(row[19]);
                            checkBoxChild2IsStudent.Checked = row[21] != DBNull.Value && Convert.ToBoolean(row[21]);
                            checkBoxChild2IsTeacher.Checked = row[20] != DBNull.Value && Convert.ToBoolean(row[20]);

                            dtChild2Baptism.Checked = row[10] != DBNull.Value ? true : false;
                            dtChild2Baptism.Value = row[10] != DBNull.Value ? Convert.ToDateTime(row[10]) : DateTime.Now;
                            dtChild2Marriage.Checked = row[9] != DBNull.Value ? true : false;
                            dtChild2Marriage.Value = row[9] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;
                            dtChild2Commuion.Checked = row[11] != DBNull.Value ? true : false;
                            dtChild2Commuion.Value = row[11] != DBNull.Value ? Convert.ToDateTime(row[10]) : DateTime.Now;
                            dtChild2Confirmation.Checked = row[12] != DBNull.Value ? true : false;
                            dtChild2Confirmation.Value = row[12] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;
                            dtChild2Priest.Checked = row[13] != DBNull.Value ? true : false;
                            dtChild2Priest.Value = row[13] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;

                            child2MemberId = row[28] != DBNull.Value ? Convert.ToInt32(row[28]) : 0;
                        }
                        else if (childCount == 3)
                        {
                            child3Groupbox.Visible = true;
                            txtChild3Name.Text = row[0] != null ? row[0].ToString() : null;
                            dtChild3DOB.Value = row[2] != DBNull.Value ? Convert.ToDateTime(row[2]) : DateTime.Now;
                            dtChild3DOB.Checked = row[2] != DBNull.Value ? true : false;
                            txtChild3Whatsapp.Text = row[3] != null ? row[3].ToString() : null;
                            txtChild3Qualification.Text = row[7] != null ? row[7].ToString() : null;
                            txtChild3Occupation.Text = row[6] != null ? row[6].ToString() : null;
                            txtChild3BG.Text = row[8] != null ? row[8].ToString() : null;
                            comboBoxChild3Relation.Text = row[1] != null ? row[1].ToString() : null;
                            txtChild3Standard.Text = row[29] != null ? row[29].ToString() : null;
                            txtChild3Institution.Text = row[23] != null ? row[23].ToString() : null;

                            checkBoxChild3IsAdmin.Checked = row[14] != DBNull.Value && Convert.ToBoolean(row[14]);
                            checkBoxChild3IsLOM.Checked = row[15] != DBNull.Value && Convert.ToBoolean(row[15]);
                            checkBoxChild3IsYOuth.Checked = row[16] != DBNull.Value && Convert.ToBoolean(row[16]);
                            checkBoxChild3IsAlter.Checked = row[17] != DBNull.Value && Convert.ToBoolean(row[17]);
                            checkBoxChild3VincentDePaul.Checked = row[18] != DBNull.Value && Convert.ToBoolean(row[18]);
                            checkBoxChild3IsChoir.Checked = row[19] != DBNull.Value && Convert.ToBoolean(row[19]);
                            checkBoxChild3IsStudent.Checked = row[21] != DBNull.Value && Convert.ToBoolean(row[21]);
                            checkBoxChild3IsTeacher.Checked = row[20] != DBNull.Value && Convert.ToBoolean(row[20]);

                            dtChild3Baptism.Checked = row[10] != DBNull.Value ? true : false;
                            dtChild3Baptism.Value = row[10] != DBNull.Value ? Convert.ToDateTime(row[10]) : DateTime.Now;
                            dtChild3Marriage.Checked = row[9] != DBNull.Value ? true : false;
                            dtChild3Marriage.Value = row[9] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;
                            dtChild3Communion.Checked = row[11] != DBNull.Value ? true : false;
                            dtChild3Communion.Value = row[11] != DBNull.Value ? Convert.ToDateTime(row[10]) : DateTime.Now;
                            dtChild3Confirmation.Checked = row[12] != DBNull.Value ? true : false;
                            dtChild3Confirmation.Value = row[12] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;
                            dtChild3Priest.Checked = row[13] != DBNull.Value ? true : false;
                            dtChild3Priest.Value = row[13] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;

                            child3MemberId = row[28] != DBNull.Value ? Convert.ToInt32(row[28]) : 0;
                        }
                        else if (childCount == 4)
                        {
                            child4Groupbox.Visible = true;
                            txtChild4Name.Text = row[0] != null ? row[0].ToString() : null;
                            dtChild4DOB.Value = row[2] != DBNull.Value ? Convert.ToDateTime(row[2]) : DateTime.Now;
                            dtChild4DOB.Checked = row[2] != DBNull.Value ? true : false;
                            txtChild4Whatsapp.Text = row[3] != null ? row[3].ToString() : null;
                            txtChild4Qualification.Text = row[7] != null ? row[7].ToString() : null;
                            txtChild4Occupation.Text = row[6] != null ? row[6].ToString() : null;
                            txtChild4BG.Text = row[8] != null ? row[8].ToString() : null;
                            comboBoxChild4Relation.Text = row[1] != null ? row[1].ToString() : null;
                            txtChild4Standard.Text = row[29] != null ? row[29].ToString() : null;
                            txtChild4Institution.Text = row[23] != null ? row[23].ToString() : null;

                            checkBoxChild4IsAdmin.Checked = row[14] != DBNull.Value && Convert.ToBoolean(row[14]);
                            checkBoxChild4IsLOM.Checked = row[15] != DBNull.Value && Convert.ToBoolean(row[15]);
                            checkBoxChild4IsYOuth.Checked = row[16] != DBNull.Value && Convert.ToBoolean(row[16]);
                            checkBoxChild4IsAlter.Checked = row[17] != DBNull.Value && Convert.ToBoolean(row[17]);
                            checkBoxChild4VincentDePual.Checked = row[18] != DBNull.Value && Convert.ToBoolean(row[18]);
                            checkBoxChild4IsChoir.Checked = row[19] != DBNull.Value && Convert.ToBoolean(row[19]);
                            checkBoxChild4IsStudent.Checked = row[21] != DBNull.Value && Convert.ToBoolean(row[21]);
                            checkBoxChild4IsTeacher.Checked = row[20] != DBNull.Value && Convert.ToBoolean(row[20]);

                            dtChild4Baptism.Checked = row[10] != DBNull.Value ? true : false;
                            dtChild4Baptism.Value = row[10] != DBNull.Value ? Convert.ToDateTime(row[10]) : DateTime.Now;
                            dtChild4Marriage.Checked = row[9] != DBNull.Value ? true : false;
                            dtChild4Marriage.Value = row[9] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;
                            dtChild4Communion.Checked = row[11] != DBNull.Value ? true : false;
                            dtChild4Communion.Value = row[11] != DBNull.Value ? Convert.ToDateTime(row[10]) : DateTime.Now;
                            dtChild4Confirmation.Checked = row[12] != DBNull.Value ? true : false;
                            dtChild4Confirmation.Value = row[12] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;
                            dtChild4Priest.Checked = row[13] != DBNull.Value ? true : false;
                            dtChild4Priest.Value = row[13] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;

                            child4MemberId = row[28] != DBNull.Value ? Convert.ToInt32(row[28]) : 0;
                        }
                        else
                        {
                            child5Groupbox.Visible = true;
                            txtChild5Name.Text = row[0] != null ? row[0].ToString() : null;
                            child5DOB.Value = row[2] != DBNull.Value ? Convert.ToDateTime(row[2]) : DateTime.Now;
                            child5DOB.Checked = row[2] != DBNull.Value ? true : false;
                            txtChild5Whatsapp.Text = row[3] != null ? row[3].ToString() : null;
                            txtChild5Qualification.Text = row[7] != null ? row[7].ToString() : null;
                            txtChild5Occupation.Text = row[6] != null ? row[6].ToString() : null;
                            txtChild5BG.Text = row[8] != null ? row[8].ToString() : null;
                            child5Relation.Text = row[1] != null ? row[1].ToString() : null;
                            txtChild5Standard.Text = row[29] != null ? row[29].ToString() : null;
                            txtChild5Institution.Text = row[23] != null ? row[23].ToString() : null;

                            checkBoxChild5IsAdmin.Checked = row[14] != DBNull.Value && Convert.ToBoolean(row[14]);
                            checkBoxChild5IsLOM.Checked = row[15] != DBNull.Value && Convert.ToBoolean(row[15]);
                            checkBoxChild5IsYOuth.Checked = row[16] != DBNull.Value && Convert.ToBoolean(row[16]);
                            checkBoxChild5IsAlter.Checked = row[17] != DBNull.Value && Convert.ToBoolean(row[17]);
                            checkBoxChild5VincentDePaul.Checked = row[18] != DBNull.Value && Convert.ToBoolean(row[18]);
                            checkBoxChild5IsChoir.Checked = row[19] != DBNull.Value && Convert.ToBoolean(row[19]);
                            checkBoxChild5IsStudent.Checked = row[21] != DBNull.Value && Convert.ToBoolean(row[21]);
                            checkBoxChild5IsTeacher.Checked = row[20] != DBNull.Value && Convert.ToBoolean(row[20]);

                            child5Baptism.Checked = row[10] != DBNull.Value ? true : false;
                            child5Baptism.Value = row[10] != DBNull.Value ? Convert.ToDateTime(row[10]) : DateTime.Now;
                            child5Marriage.Checked = row[9] != DBNull.Value ? true : false;
                            child5Marriage.Value = row[9] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;
                            child5Communion.Checked = row[11] != DBNull.Value ? true : false;
                            child5Communion.Value = row[11] != DBNull.Value ? Convert.ToDateTime(row[10]) : DateTime.Now;
                            child5Confirmation.Checked = row[12] != DBNull.Value ? true : false;
                            child5Confirmation.Value = row[12] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;
                            child5Priest.Checked = row[13] != DBNull.Value ? true : false;
                            child5Priest.Value = row[13] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;

                            child5MemberId = row[28] != DBNull.Value ? Convert.ToInt32(row[28]) : 0;
                        }
                        childCount++;
                    }
                    else
                    {
                        if (otherRelationCount == 1)
                        {
                            relation1Groupbox.Visible = true;
                            txtOther1Name.Text = row[0] != null ? row[0].ToString() : null;
                            dtOther1dob.Value = row[2] != DBNull.Value ? Convert.ToDateTime(row[2]) : DateTime.Now;
                            dtOther1dob.Checked = row[2] != DBNull.Value ? true : false;
                            txtOther1Whatsapp.Text = row[3] != null ? row[3].ToString() : null;
                            txtOther1Qualification.Text = row[7] != null ? row[7].ToString() : null;
                            txtOther1Occupation.Text = row[6] != null ? row[6].ToString() : null;
                            txtOther1Bloodgroup.Text = row[8] != null ? row[8].ToString() : null;
                            relation1Combobox.Text = row[1] != null ? row[1].ToString() : null;

                            checkBoxOther1IsAdmin.Checked = row[14] != DBNull.Value && Convert.ToBoolean(row[14]);
                            checkBoxOther1IsLOM.Checked = row[15] != DBNull.Value && Convert.ToBoolean(row[15]);
                            checkBoxOther1IsYouth.Checked = row[16] != DBNull.Value && Convert.ToBoolean(row[16]);
                            checkBoxOther1IsAlter.Checked = row[17] != DBNull.Value && Convert.ToBoolean(row[17]);
                            checkBoxOther1IsVencntDePaul.Checked = row[18] != DBNull.Value && Convert.ToBoolean(row[18]);
                            checkBoxOther1IsChoir.Checked = row[19] != DBNull.Value && Convert.ToBoolean(row[19]);
                            checkBoxOther1IsTeacher.Checked = row[20] != DBNull.Value && Convert.ToBoolean(row[20]);

                            dtOther1Baptism.Checked = row[10] != DBNull.Value ? true : false;
                            dtOther1Baptism.Value = row[10] != DBNull.Value ? Convert.ToDateTime(row[10]) : DateTime.Now;
                            dtOther1Matrimony.Checked = row[9] != DBNull.Value ? true : false;
                            dtOther1Matrimony.Value = row[9] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;
                            dtOther1Communion.Checked = row[11] != DBNull.Value ? true : false;
                            dtOther1Communion.Value = row[11] != DBNull.Value ? Convert.ToDateTime(row[10]) : DateTime.Now;
                            dtOther1Confirmation.Checked = row[12] != DBNull.Value ? true : false;
                            dtOther1Confirmation.Value = row[12] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;

                            other1MemberId = row[28] != DBNull.Value ? Convert.ToInt32(row[28]) : 0;
                        }
                        else
                        {
                            otherRelation2Groupbox.Visible = true;
                            txtOther2Name.Text = row[0] != null ? row[0].ToString() : null;
                            dtother2dob.Value = row[2] != DBNull.Value ? Convert.ToDateTime(row[2]) : DateTime.Now;
                            dtother2dob.Checked = row[2] != DBNull.Value ? true : false;
                            txtOther2Whatsapp.Text = row[3] != null ? row[3].ToString() : null;
                            txtOther2Qualification.Text = row[7] != null ? row[7].ToString() : null;
                            txtOther2Occupation.Text = row[6] != null ? row[6].ToString() : null;
                            txtOther2bloodgroup.Text = row[8] != null ? row[8].ToString() : null;
                            other2Combobox.Text = row[1] != null ? row[1].ToString() : null;

                            checkBoxOther2IsAdmin.Checked = row[14] != DBNull.Value && Convert.ToBoolean(row[14]);
                            checkBoxOther2IsLOM.Checked = row[15] != DBNull.Value && Convert.ToBoolean(row[15]);
                            checkBoxOther2IsYouth.Checked = row[16] != DBNull.Value && Convert.ToBoolean(row[16]);
                            checkBoxOther2IsAlter.Checked = row[17] != DBNull.Value && Convert.ToBoolean(row[17]);
                            checkBoxOther2IsVincentDePaul.Checked = row[18] != DBNull.Value && Convert.ToBoolean(row[18]);
                            checkBoxOther2IsChoir.Checked = row[19] != DBNull.Value && Convert.ToBoolean(row[19]);
                            checkBoxOther2IsTeacher.Checked = row[20] != DBNull.Value && Convert.ToBoolean(row[20]);

                            dtOther2Baptism.Checked = row[10] != DBNull.Value ? true : false;
                            dtOther2Baptism.Value = row[10] != DBNull.Value ? Convert.ToDateTime(row[10]) : DateTime.Now;
                            dtOther2Marriage.Checked = row[9] != DBNull.Value ? true : false;
                            dtOther2Marriage.Value = row[9] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;
                            dtOther2Communion.Checked = row[11] != DBNull.Value ? true : false;
                            dtOther2Communion.Value = row[11] != DBNull.Value ? Convert.ToDateTime(row[10]) : DateTime.Now;
                            dtOther2Confirmation.Checked = row[12] != DBNull.Value ? true : false;
                            dtOther2Confirmation.Value = row[12] != DBNull.Value ? Convert.ToDateTime(row[9]) : DateTime.Now;

                            other2MemberId = row[28] != DBNull.Value ? Convert.ToInt32(row[28]) : 0;
                        }
                        otherRelationCount++;
                    }
                }
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

        private void sameAddressCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if(sender is CheckBox checkBox)
            {
                if (checkBox.Checked)
                {
                    permAddresstxt.Text = tempAddresstxt.Text.Trim();
                    permCitytxt.Text = tempCitytxt.Text.Trim();
                    permStatetxt.Text = tempStatetxt.Text.Trim();
                    permZipcodetxt.Text = tempZipcodetxt.Text.Trim();
                }
                else
                {
                    permAddresstxt.Text = "";
                    permCitytxt.Text = "";
                    permStatetxt.Text = "";
                    permZipcodetxt.Text = "";
                }
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

        private void btn_disable_Click(object sender, EventArgs e)
        {
            int familyId = 123; // The ID of the row to update
            string newName = "New Family Name";

            DatabaseHelper.ExecuteStoredProcedure(
                "sp_UpdateFamilyName",
                new SqlParameter("@FamilyID", familyId),
                new SqlParameter("@Name", newName)
            );
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
        public string ChildInstitution { get; set; }
        public string ChildClass { get; set; }
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
        public int MemberId { get; set; }
    }
}