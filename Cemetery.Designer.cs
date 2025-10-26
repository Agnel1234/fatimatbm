namespace TestFat
{
    partial class Cemetery
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Cemetery));
            this.panel1 = new System.Windows.Forms.Panel();
            this.cemeteryGrid = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.dtBurialDate = new System.Windows.Forms.DateTimePicker();
            this.lblAnbiyamName = new System.Windows.Forms.Label();
            this.btnAddCemetery = new System.Windows.Forms.Button();
            this.dtDeceasedDate = new System.Windows.Forms.DateTimePicker();
            this.comboMemberName = new System.Windows.Forms.ComboBox();
            this.lblPhone = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCemeteryCode = new System.Windows.Forms.TextBox();
            this.txtCemeteryCharge = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cemeteryGrid)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(226)))), ((int)(((byte)(220)))));
            this.panel1.Controls.Add(this.cemeteryGrid);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Location = new System.Drawing.Point(3, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(653, 407);
            this.panel1.TabIndex = 11;
            // 
            // cemeteryGrid
            // 
            this.cemeteryGrid.AllowUserToOrderColumns = true;
            this.cemeteryGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.cemeteryGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.cemeteryGrid.Location = new System.Drawing.Point(9, 9);
            this.cemeteryGrid.Name = "cemeteryGrid";
            this.cemeteryGrid.Size = new System.Drawing.Size(636, 203);
            this.cemeteryGrid.TabIndex = 19;
            this.cemeteryGrid.SelectionChanged += new System.EventHandler(this.cemeteryGrid_SelectionChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(226)))), ((int)(((byte)(220)))));
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.dtBurialDate);
            this.groupBox1.Controls.Add(this.lblAnbiyamName);
            this.groupBox1.Controls.Add(this.btnAddCemetery);
            this.groupBox1.Controls.Add(this.dtDeceasedDate);
            this.groupBox1.Controls.Add(this.comboMemberName);
            this.groupBox1.Controls.Add(this.lblPhone);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtCemeteryCode);
            this.groupBox1.Controls.Add(this.txtCemeteryCharge);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(3, 218);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(647, 189);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Georgia", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(6, 104);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 15);
            this.label3.TabIndex = 19;
            this.label3.Text = "Burial Date *";
            // 
            // dtBurialDate
            // 
            this.dtBurialDate.Checked = false;
            this.dtBurialDate.Font = new System.Drawing.Font("Georgia", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtBurialDate.Location = new System.Drawing.Point(123, 104);
            this.dtBurialDate.Name = "dtBurialDate";
            this.dtBurialDate.Size = new System.Drawing.Size(200, 21);
            this.dtBurialDate.TabIndex = 18;
            // 
            // lblAnbiyamName
            // 
            this.lblAnbiyamName.AutoSize = true;
            this.lblAnbiyamName.Font = new System.Drawing.Font("Georgia", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnbiyamName.Location = new System.Drawing.Point(6, 32);
            this.lblAnbiyamName.Name = "lblAnbiyamName";
            this.lblAnbiyamName.Size = new System.Drawing.Size(102, 15);
            this.lblAnbiyamName.TabIndex = 10;
            this.lblAnbiyamName.Text = "Member Name *";
            // 
            // btnAddCemetery
            // 
            this.btnAddCemetery.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddCemetery.BackColor = System.Drawing.Color.Transparent;
            this.btnAddCemetery.Font = new System.Drawing.Font("Georgia", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddCemetery.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnAddCemetery.Location = new System.Drawing.Point(454, 141);
            this.btnAddCemetery.Name = "btnAddCemetery";
            this.btnAddCemetery.Size = new System.Drawing.Size(172, 29);
            this.btnAddCemetery.TabIndex = 8;
            this.btnAddCemetery.Text = "Register Cemetery";
            this.btnAddCemetery.UseVisualStyleBackColor = false;
            this.btnAddCemetery.Click += new System.EventHandler(this.btnAddCemetery_Click);
            // 
            // dtDeceasedDate
            // 
            this.dtDeceasedDate.Checked = false;
            this.dtDeceasedDate.Font = new System.Drawing.Font("Georgia", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtDeceasedDate.Location = new System.Drawing.Point(123, 67);
            this.dtDeceasedDate.Name = "dtDeceasedDate";
            this.dtDeceasedDate.Size = new System.Drawing.Size(200, 21);
            this.dtDeceasedDate.TabIndex = 17;
            // 
            // comboMemberName
            // 
            this.comboMemberName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboMemberName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMemberName.Font = new System.Drawing.Font("Georgia", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboMemberName.Location = new System.Drawing.Point(123, 32);
            this.comboMemberName.Name = "comboMemberName";
            this.comboMemberName.Size = new System.Drawing.Size(200, 23);
            this.comboMemberName.TabIndex = 2;
            // 
            // lblPhone
            // 
            this.lblPhone.AutoSize = true;
            this.lblPhone.Font = new System.Drawing.Font("Georgia", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPhone.Location = new System.Drawing.Point(347, 40);
            this.lblPhone.Name = "lblPhone";
            this.lblPhone.Size = new System.Drawing.Size(103, 15);
            this.lblPhone.TabIndex = 15;
            this.lblPhone.Text = "Cemetery Code *";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Georgia", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 15);
            this.label2.TabIndex = 13;
            this.label2.Text = "Deceased Date *";
            // 
            // txtCemeteryCode
            // 
            this.txtCemeteryCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCemeteryCode.Font = new System.Drawing.Font("Georgia", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCemeteryCode.Location = new System.Drawing.Point(454, 37);
            this.txtCemeteryCode.MaxLength = 10;
            this.txtCemeteryCode.Name = "txtCemeteryCode";
            this.txtCemeteryCode.Size = new System.Drawing.Size(172, 21);
            this.txtCemeteryCode.TabIndex = 6;
            // 
            // txtCemeteryCharge
            // 
            this.txtCemeteryCharge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCemeteryCharge.Font = new System.Drawing.Font("Georgia", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCemeteryCharge.Location = new System.Drawing.Point(350, 104);
            this.txtCemeteryCharge.Name = "txtCemeteryCharge";
            this.txtCemeteryCharge.Size = new System.Drawing.Size(276, 21);
            this.txtCemeteryCharge.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Georgia", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(347, 73);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 15);
            this.label1.TabIndex = 16;
            this.label1.Text = "Remark";
            // 
            // Cemetery
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(660, 423);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Cemetery";
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cemeteryGrid)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnAddCemetery;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtCemeteryCharge;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblAnbiyamName;
        private System.Windows.Forms.TextBox txtCemeteryCode;
        private System.Windows.Forms.Label lblPhone;
        private System.Windows.Forms.ComboBox comboMemberName;
        private System.Windows.Forms.DateTimePicker dtDeceasedDate;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView cemeteryGrid;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker dtBurialDate;
    }
}