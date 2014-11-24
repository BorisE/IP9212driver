namespace ASCOM.IP9212_v2
{
    partial class SetupDialogForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupDialogForm));
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.picASCOM = new System.Windows.Forms.PictureBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dataGridOutputSwitch = new System.Windows.Forms.DataGridView();
            this.SwitchId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SwitchName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SwitchDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.dataGridInputSwitch = new System.Windows.Forms.DataGridView();
            this.InputSwitchId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InputSwitchName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.InputSwitchDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.myToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.chkTrace = new System.Windows.Forms.CheckBox();
            this.ipaddr = new System.Windows.Forms.TextBox();
            this.login = new System.Windows.Forms.TextBox();
            this.pass = new System.Windows.Forms.TextBox();
            this.port = new System.Windows.Forms.TextBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridOutputSwitch)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridInputSwitch)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOK.Location = new System.Drawing.Point(464, 87);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(69, 24);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(464, 117);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(69, 25);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // picASCOM
            // 
            this.picASCOM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picASCOM.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picASCOM.Image = global::ASCOM.IP9212.Properties.Resources.ASCOM;
            this.picASCOM.Location = new System.Drawing.Point(475, 25);
            this.picASCOM.Name = "picASCOM";
            this.picASCOM.Size = new System.Drawing.Size(48, 56);
            this.picASCOM.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picASCOM.TabIndex = 3;
            this.picASCOM.TabStop = false;
            this.picASCOM.Click += new System.EventHandler(this.BrowseToAscom);
            this.picASCOM.DoubleClick += new System.EventHandler(this.BrowseToAscom);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.textBox1);
            this.groupBox3.Controls.Add(this.chkTrace);
            this.groupBox3.Location = new System.Drawing.Point(222, 9);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(236, 149);
            this.groupBox3.TabIndex = 33;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Advanced";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(149, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(10, 13);
            this.label3.TabIndex = 27;
            this.label3.Text = ":";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 73);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(30, 13);
            this.label5.TabIndex = 28;
            this.label5.Text = "Pass";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 29;
            this.label4.Text = "Login";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 30;
            this.label2.Text = "IP addr";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dataGridOutputSwitch);
            this.groupBox2.Location = new System.Drawing.Point(3, 164);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(530, 230);
            this.groupBox2.TabIndex = 31;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Output switch settings";
            // 
            // dataGridOutputSwitch
            // 
            this.dataGridOutputSwitch.AllowUserToAddRows = false;
            this.dataGridOutputSwitch.AllowUserToDeleteRows = false;
            this.dataGridOutputSwitch.AllowUserToResizeRows = false;
            this.dataGridOutputSwitch.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridOutputSwitch.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SwitchId,
            this.SwitchName,
            this.SwitchDescription});
            this.dataGridOutputSwitch.Location = new System.Drawing.Point(13, 20);
            this.dataGridOutputSwitch.Name = "dataGridOutputSwitch";
            this.dataGridOutputSwitch.RowHeadersVisible = false;
            this.dataGridOutputSwitch.RowHeadersWidth = 30;
            this.dataGridOutputSwitch.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.dataGridOutputSwitch.ShowCellToolTips = false;
            this.dataGridOutputSwitch.Size = new System.Drawing.Size(503, 199);
            this.dataGridOutputSwitch.TabIndex = 0;
            this.myToolTip.SetToolTip(this.dataGridOutputSwitch, "Aviosys IP9212 has different predefined statuses:\r\nOUT1 - OUT4: Normally closed r" +
        "elay (NÑ)\r\nOUT5 - OUT8: Normally opened relay (NO)\r\nThis cannot be changed");
            // 
            // SwitchId
            // 
            this.SwitchId.HeaderText = "#";
            this.SwitchId.MinimumWidth = 10;
            this.SwitchId.Name = "SwitchId";
            this.SwitchId.ReadOnly = true;
            this.SwitchId.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.SwitchId.Width = 40;
            // 
            // SwitchName
            // 
            this.SwitchName.HeaderText = "Name";
            this.SwitchName.MinimumWidth = 50;
            this.SwitchName.Name = "SwitchName";
            this.SwitchName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.SwitchName.Width = 180;
            // 
            // SwitchDescription
            // 
            this.SwitchDescription.HeaderText = "Description";
            this.SwitchDescription.MinimumWidth = 100;
            this.SwitchDescription.Name = "SwitchDescription";
            this.SwitchDescription.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.SwitchDescription.Width = 280;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.ipaddr);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.login);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.pass);
            this.groupBox1.Controls.Add(this.port);
            this.groupBox1.Location = new System.Drawing.Point(3, 9);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(213, 149);
            this.groupBox1.TabIndex = 32;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "IP9212 address";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.dataGridInputSwitch);
            this.groupBox4.Location = new System.Drawing.Point(3, 400);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(530, 230);
            this.groupBox4.TabIndex = 31;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Input switch settings";
            // 
            // dataGridInputSwitch
            // 
            this.dataGridInputSwitch.AllowUserToAddRows = false;
            this.dataGridInputSwitch.AllowUserToDeleteRows = false;
            this.dataGridInputSwitch.AllowUserToResizeRows = false;
            this.dataGridInputSwitch.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridInputSwitch.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.InputSwitchId,
            this.InputSwitchName,
            this.InputSwitchDescription});
            this.dataGridInputSwitch.Location = new System.Drawing.Point(13, 19);
            this.dataGridInputSwitch.Name = "dataGridInputSwitch";
            this.dataGridInputSwitch.RowHeadersVisible = false;
            this.dataGridInputSwitch.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.dataGridInputSwitch.ShowCellToolTips = false;
            this.dataGridInputSwitch.Size = new System.Drawing.Size(503, 199);
            this.dataGridInputSwitch.TabIndex = 0;
            this.myToolTip.SetToolTip(this.dataGridInputSwitch, resources.GetString("dataGridInputSwitch.ToolTip"));
            // 
            // InputSwitchId
            // 
            this.InputSwitchId.HeaderText = "#";
            this.InputSwitchId.Name = "InputSwitchId";
            this.InputSwitchId.ReadOnly = true;
            this.InputSwitchId.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.InputSwitchId.Width = 40;
            // 
            // InputSwitchName
            // 
            this.InputSwitchName.HeaderText = "Name";
            this.InputSwitchName.Name = "InputSwitchName";
            this.InputSwitchName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.InputSwitchName.Width = 180;
            // 
            // InputSwitchDescription
            // 
            this.InputSwitchDescription.HeaderText = "Description";
            this.InputSwitchDescription.Name = "InputSwitchDescription";
            this.InputSwitchDescription.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.InputSwitchDescription.Width = 280;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(127, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Caching check results for";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(206, 48);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(24, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "sec";
            // 
            // textBox1
            // 
            this.textBox1.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::ASCOM.IP9212.Properties.Settings.Default, "CheckCacheTimeout", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBox1.Location = new System.Drawing.Point(139, 45);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(61, 20);
            this.textBox1.TabIndex = 7;
            this.textBox1.Text = global::ASCOM.IP9212.Properties.Settings.Default.CheckCacheTimeout;
            this.myToolTip.SetToolTip(this.textBox1, resources.GetString("textBox1.ToolTip"));
            // 
            // chkTrace
            // 
            this.chkTrace.AutoSize = true;
            this.chkTrace.Checked = global::ASCOM.IP9212.Properties.Settings.Default.trace;
            this.chkTrace.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTrace.Location = new System.Drawing.Point(8, 19);
            this.chkTrace.Name = "chkTrace";
            this.chkTrace.Size = new System.Drawing.Size(69, 17);
            this.chkTrace.TabIndex = 6;
            this.chkTrace.Text = "Trace on";
            this.myToolTip.SetToolTip(this.chkTrace, "Driver will write ASCOM logs. Usefull for debugging problems");
            this.chkTrace.UseVisualStyleBackColor = true;
            // 
            // ipaddr
            // 
            this.ipaddr.Location = new System.Drawing.Point(54, 19);
            this.ipaddr.Name = "ipaddr";
            this.ipaddr.Size = new System.Drawing.Size(92, 20);
            this.ipaddr.TabIndex = 26;
            this.ipaddr.Text = global::ASCOM.IP9212.Properties.Settings.Default.ipaddr;
            this.myToolTip.SetToolTip(this.ipaddr, "IP addres of the device. \r\nUse \"ipEdit.exe\" to locate its IP addres");
            // 
            // login
            // 
            this.login.Location = new System.Drawing.Point(54, 45);
            this.login.Name = "login";
            this.login.Size = new System.Drawing.Size(92, 20);
            this.login.TabIndex = 25;
            this.login.Text = global::ASCOM.IP9212.Properties.Settings.Default.iplogin;
            this.myToolTip.SetToolTip(this.login, "Login name. \r\nFactory default for the device is \"admin\"");
            // 
            // pass
            // 
            this.pass.Location = new System.Drawing.Point(54, 70);
            this.pass.MaxLength = 14;
            this.pass.Multiline = true;
            this.pass.Name = "pass";
            this.pass.PasswordChar = '*';
            this.pass.Size = new System.Drawing.Size(92, 20);
            this.pass.TabIndex = 24;
            this.pass.Text = global::ASCOM.IP9212.Properties.Settings.Default.ippass;
            this.myToolTip.SetToolTip(this.pass, "Device password. \r\nFactory default for the device is \"12345678\"\r\n");
            // 
            // port
            // 
            this.port.Location = new System.Drawing.Point(165, 19);
            this.port.Name = "port";
            this.port.Size = new System.Drawing.Size(38, 20);
            this.port.TabIndex = 23;
            this.port.Text = global::ASCOM.IP9212.Properties.Settings.Default.ipport;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(472, 9);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(59, 13);
            this.linkLabel1.TabIndex = 10;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Astromania";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // SetupDialogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(535, 631);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.picASCOM);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupDialogForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Aviosus IP9212 Switch Driver Setup";
            this.Load += new System.EventHandler(this.SetupDialogForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridOutputSwitch)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridInputSwitch)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.PictureBox picASCOM;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox chkTrace;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox port;
        private System.Windows.Forms.TextBox pass;
        private System.Windows.Forms.TextBox login;
        private System.Windows.Forms.TextBox ipaddr;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dataGridOutputSwitch;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.DataGridView dataGridInputSwitch;
        private System.Windows.Forms.DataGridViewTextBoxColumn InputSwitchId;
        private System.Windows.Forms.DataGridViewTextBoxColumn InputSwitchName;
        private System.Windows.Forms.DataGridViewTextBoxColumn InputSwitchDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn SwitchId;
        private System.Windows.Forms.DataGridViewTextBoxColumn SwitchName;
        private System.Windows.Forms.DataGridViewTextBoxColumn SwitchDescription;
        private System.Windows.Forms.ToolTip myToolTip;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}