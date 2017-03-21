using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Lewis.SST.Controls;
using Lewis.SST.SQLMethods;

#region change history
/// 08-22-2008: C01: LLEWIS: fix added to resolve local machine name, otherwise sql commands fail
#endregion

namespace Lewis.SST.Gui
{
	/// <summary>
	/// Summary description for SQLSecuritySettings.
	/// </summary>
	public class SQLSecuritySettings : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txt_UID;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txt_PWD;
		private System.Windows.Forms.RadioButton rdb_Integrated;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton rdb_Mixed;
		private System.Windows.Forms.Button btn_OK;
		private System.Windows.Forms.Button btn_Cancel;
		private System.Windows.Forms.ComboBox txt_ServerName;
        private System.Windows.Forms.CheckBox chkSavePwd;

        private SQLConnections _SQLConnections;
        private ComboBox comboBox1;
        private Label label4;
        private Label lblWarning;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="SQLSecuritySettings"/> class.
		/// </summary>
		public SQLSecuritySettings()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
            this.Height = 230;
            btn_OK.Top = 138;
            btn_Cancel.Top = 169;
            comboBox1.Enabled = false;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="SQLSecuritySettings"/> class.
		/// </summary>
		/// <param name="SQLServer">The SQL server.</param>
		public SQLSecuritySettings(string SQLServer)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
            this.Height = 230;
            btn_OK.Top = 138;
            btn_Cancel.Top = 169;
            comboBox1.Enabled = false;

            chkSavePwd.Checked = false;
			rdb_Integrated.Checked = true;
			rdb_Mixed.Checked = false;
			txt_ServerName.Text = SQLServer;
            if (SQLServer == null || SQLServer.Equals("Enter Server Name..."))
            {
                txt_ServerName.Enabled = true;
                txt_ServerName.Focus();
            }
		}
	
		/// <summary>
		/// Initializes a new instance of the <see cref="SQLSecuritySettings"/> class.
		/// </summary>
		/// <param name="SQLServer">The SQL server.</param>
		/// <param name="UID">The UID.</param>
		/// <param name="PWD">The PWD.</param>
		public SQLSecuritySettings(string SQLServer, string UID, string PWD, bool SavePWD)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
            this.Height = 230;
            btn_OK.Top = 138;
            btn_Cancel.Top = 169;
            comboBox1.Enabled = false;

			rdb_Integrated.Checked = false;
			rdb_Mixed.Checked = true;
            SavePwd = SavePWD;
			txt_ServerName.Text = SQLServer;
			txt_UID.Text = UID;
			txt_PWD.Text = PWD;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLSecuritySettings"/> class.
        /// </summary>
        /// <param name="SQLServers"></param>
        public SQLSecuritySettings(string[] SQLServers)
        {
            //
            InitializeComponent();
            this.Height = 230;
            btn_OK.Top = 138;
            btn_Cancel.Top = 169;
            comboBox1.Enabled = false;

            chkSavePwd.Visible = false;
            rdb_Integrated.Checked = true;
            rdb_Mixed.Checked = false;
            txt_ServerName.Enabled = true;
            if (SQLServers != null)
            {
                txt_ServerName.Items.AddRange(SQLServers);
            }
            else
            {
                txt_ServerName.Enabled = true;
                txt_ServerName.Focus();
            }
            if (txt_ServerName.Items.Count > 0)
            {
                txt_ServerName.SelectedIndex = 0;
            }
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="SQLSecuritySettings"/> class.
		/// </summary>
		/// <param name="SQLServers">The SQL servers.</param>
        /// <param name="showSave">shows the save pwd checkbox</param>
		public SQLSecuritySettings(string [] SQLServers, bool showSave)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
            this.Height = 230;
            btn_OK.Top = 138;
            btn_Cancel.Top = 169;
            comboBox1.Enabled = false;

            chkSavePwd.Visible = showSave;
			rdb_Integrated.Checked = true;
			rdb_Mixed.Checked = false;
			txt_ServerName.Enabled = true;
            if (SQLServers != null)
            {
                txt_ServerName.Items.AddRange(SQLServers);
            }
            else
            {
                txt_ServerName.Enabled = true;
                txt_ServerName.Focus();
            }
            if (txt_ServerName.Items.Count > 0)
            {
                txt_ServerName.SelectedIndex = 0;
            }
		}

        public SQLSecuritySettings(SQLServerExplorer sse)
        {
            string[] SQLServers = sse.Servers;
            SQLConnections sqlConnections = sse.SQLConnections;
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            this.Height = 188;
            lblWarning.Visible = true;
            rdb_Integrated.Visible = false;
            rdb_Mixed.Visible = false;
            groupBox1.Visible = false;
            label2.Visible = false;
            label3.Visible = false;
            txt_PWD.Visible = false;
            txt_UID.Visible = false;
            label4.Top = 30;
            comboBox1.Top = 32;
            comboBox1.Enabled = false;
            btn_OK.Top = 88;
            btn_Cancel.Top = 119;
            lblWarning.Top = 88;

            _SQLConnections = sqlConnections;
            chkSavePwd.Visible = false;
            rdb_Integrated.Checked = true;
            rdb_Mixed.Checked = false;
            txt_ServerName.SelectedIndexChanged += new EventHandler(txt_ServerName_SelectedIndexChanged);
            txt_ServerName.Enabled = true;
            if (SQLServers != null)
            {
                txt_ServerName.Items.AddRange(SQLServers);
            }
            else
            {
                txt_ServerName.Enabled = true;
                txt_ServerName.Focus();
            }
            string servername = string.Empty;
            if (typeof(ServerTreeNode).IsInstanceOfType(sse.SelectedTreeNode))
            {
                servername = sse.SelectedTreeNode.Text;
            }
            if (typeof(DBTreeNode).IsInstanceOfType(sse.SelectedTreeNode))
            {
                servername = sse.SelectedTreeNode.Parent.Text;
            }
            txt_ServerName.Text = servername;
        }

        /// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SQLSecuritySettings));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_UID = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txt_PWD = new System.Windows.Forms.TextBox();
            this.rdb_Integrated = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rdb_Mixed = new System.Windows.Forms.RadioButton();
            this.btn_OK = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.txt_ServerName = new System.Windows.Forms.ComboBox();
            this.chkSavePwd = new System.Windows.Forms.CheckBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblWarning = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 24);
            this.label1.TabIndex = 1;
            this.label1.Text = "SQL Server Name:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(16, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 24);
            this.label2.TabIndex = 3;
            this.label2.Text = "User ID:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txt_UID
            // 
            this.txt_UID.Location = new System.Drawing.Point(120, 32);
            this.txt_UID.Name = "txt_UID";
            this.txt_UID.Size = new System.Drawing.Size(264, 20);
            this.txt_UID.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(16, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 24);
            this.label3.TabIndex = 5;
            this.label3.Text = "Password:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txt_PWD
            // 
            this.txt_PWD.Location = new System.Drawing.Point(120, 56);
            this.txt_PWD.Name = "txt_PWD";
            this.txt_PWD.PasswordChar = '*';
            this.txt_PWD.Size = new System.Drawing.Size(264, 20);
            this.txt_PWD.TabIndex = 2;
            // 
            // rdb_Integrated
            // 
            this.rdb_Integrated.Location = new System.Drawing.Point(16, 24);
            this.rdb_Integrated.Name = "rdb_Integrated";
            this.rdb_Integrated.Size = new System.Drawing.Size(96, 24);
            this.rdb_Integrated.TabIndex = 0;
            this.rdb_Integrated.Text = "Integrated";
            this.rdb_Integrated.CheckedChanged += new System.EventHandler(this.rdb_Integrated_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rdb_Mixed);
            this.groupBox1.Controls.Add(this.rdb_Integrated);
            this.groupBox1.Location = new System.Drawing.Point(120, 104);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(120, 88);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Security Mode";
            // 
            // rdb_Mixed
            // 
            this.rdb_Mixed.Location = new System.Drawing.Point(16, 52);
            this.rdb_Mixed.Name = "rdb_Mixed";
            this.rdb_Mixed.Size = new System.Drawing.Size(96, 24);
            this.rdb_Mixed.TabIndex = 7;
            this.rdb_Mixed.Text = "Mixed Mode";
            this.rdb_Mixed.CheckedChanged += new System.EventHandler(this.rdb_Mixed_CheckedChanged);
            // 
            // btn_OK
            // 
            this.btn_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_OK.Location = new System.Drawing.Point(309, 229);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(75, 23);
            this.btn_OK.TabIndex = 4;
            this.btn_OK.Text = "OK";
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Location = new System.Drawing.Point(309, 260);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_Cancel.TabIndex = 5;
            this.btn_Cancel.Text = "Cancel";
            // 
            // txt_ServerName
            // 
            this.txt_ServerName.Enabled = false;
            this.txt_ServerName.Location = new System.Drawing.Point(120, 8);
            this.txt_ServerName.Name = "txt_ServerName";
            this.txt_ServerName.Size = new System.Drawing.Size(264, 21);
            this.txt_ServerName.TabIndex = 0;
            // 
            // chkSavePwd
            // 
            this.chkSavePwd.AutoSize = true;
            this.chkSavePwd.Location = new System.Drawing.Point(120, 82);
            this.chkSavePwd.Name = "chkSavePwd";
            this.chkSavePwd.Size = new System.Drawing.Size(100, 17);
            this.chkSavePwd.TabIndex = 3;
            this.chkSavePwd.Text = "Save Password";
            this.chkSavePwd.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            this.comboBox1.Enabled = false;
            this.comboBox1.Location = new System.Drawing.Point(120, 201);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(264, 21);
            this.comboBox1.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(15, 200);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 24);
            this.label4.TabIndex = 9;
            this.label4.Text = "DB Name:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblWarning
            // 
            this.lblWarning.Image = ((System.Drawing.Image)(resources.GetObject("lblWarning.Image")));
            this.lblWarning.ImageAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblWarning.Location = new System.Drawing.Point(13, 229);
            this.lblWarning.Name = "lblWarning";
            this.lblWarning.Size = new System.Drawing.Size(234, 63);
            this.lblWarning.TabIndex = 10;
            this.lblWarning.Text = "Be sure to make a backup of your database before running this script!";
            this.lblWarning.Visible = false;
            // 
            // SQLSecuritySettings
            // 
            this.AcceptButton = this.btn_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(399, 294);
            this.Controls.Add(this.lblWarning);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.chkSavePwd);
            this.Controls.Add(this.txt_ServerName);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txt_PWD);
            this.Controls.Add(this.txt_UID);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SQLSecuritySettings";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Security Settings";
            this.TopMost = true;
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

        private void txt_ServerName_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] dbs = getDBNames();
            if (dbs != null)
            {
                comboBox1.Items.AddRange(dbs);
                comboBox1.Enabled = true;
            }
        }

        private string[] getDBNames()
        {
            string[] dbNames = null;
            SQLConnection sql = _SQLConnections[SelectedSQLServer];
            if (sql != null)                
            {
                if (sql.sqlConnection.State == System.Data.ConnectionState.Open)
                {
                    dbNames = SQLServers.GetDBNames(sql.sqlConnection);
                }
                else
                {
                    btn_Cancel.PerformClick();
                    this.DialogResult = DialogResult.Abort;
                }
            }
            return dbNames;
        }

        private void rdb_Integrated_CheckedChanged(object sender, System.EventArgs e)
		{
			if (rdb_Integrated.Checked)
			{
				txt_UID.ReadOnly = true;
				txt_PWD.ReadOnly = true;
                chkSavePwd.Enabled = false;
			}
		}

		private void rdb_Mixed_CheckedChanged(object sender, System.EventArgs e)
		{
			if (rdb_Mixed.Checked)
			{
				txt_UID.ReadOnly = false;
				txt_PWD.ReadOnly = false;
                chkSavePwd.Enabled = true;
			}
		}

		/// <summary>
		/// Sets the name of the SQL server. The value is used to connect to that server.
		/// </summary>
		/// <param name="SQLServer">The SQL server.</param>
		public void SetServerName(string SQLServer)
		{
			txt_ServerName.Text = SQLServer;
		}

		/// <summary>
		/// Returns the security type setting in use for the SQL server.
		/// </summary>
		/// <returns>Returns the security type setting in use for the SQL server.</returns>
		public SQLMethods.SecurityType SecurityMode()
		{
			if (rdb_Integrated.Checked)
                return SQLMethods.SecurityType.Integrated;
			else if (rdb_Mixed.Checked)
                return SQLMethods.SecurityType.Mixed;
			else
                return SQLMethods.SecurityType.Integrated;
		}

		/// <summary>
		/// Gets the SQL server name from the settings/connection dialog.
		/// </summary>
		/// <value>The SQL server.</value>
		public string SelectedSQLServer
		{
			get 
            {
                // 08-22-2008: C01: LLEWIS: fix added to resolve local machine name, otherwise sql commands fail
                string serverName = txt_ServerName.Text;
                if (serverName.Trim().ToLower().Contains("(local)") || serverName.Trim().ToLower().Contains("localhost"))
                {
                    string instance = serverName.Split('\\').Length > 1 ? serverName.Split('\\')[1] : string.Empty;
                    serverName = string.Format("{0}\\{1}", Environment.MachineName, instance);
                }
                return serverName; 
            }
		}

		/// <summary>
		/// Gets or sets the user value.
		/// </summary>
		/// <value>The user.</value>
		public string User
		{
			get { return txt_UID.Text; }
            set { txt_UID.Text = value == null ? "" : value; }
		}

		/// <summary>
		/// Gets or sets the password value.
		/// </summary>
		/// <value>The password.</value>
		public string PWD
		{
			get { return txt_PWD.Text; }
            set { txt_PWD.Text = value == null ? "" : value; }
		}

        public bool SavePwd
        {
            get { return chkSavePwd.Checked; }
            set { chkSavePwd.Checked = value; }
        }

        public string SelectedDBName
        {
            get { return comboBox1.Text; }
        }
	}
}
