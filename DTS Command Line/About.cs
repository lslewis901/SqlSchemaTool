using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;


namespace Lewis.SST
{
	/// <summary>
	/// Class to display About/error message form.
	/// </summary>
	public class About : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.TextBox txtCRT;
		private System.Windows.Forms.TextBox txtCompany;
		private System.Windows.Forms.TextBox txtComments;
		private System.Windows.Forms.TextBox txtDescription;
		private System.Windows.Forms.TextBox txtVer;
		private System.Windows.Forms.TextBox txtProductName;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panelAbout;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.RichTextBox rtxtHelp;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.PictureBox Logo;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
        private string _message;

		/// <summary>
		/// Initializes a new instance of the <see cref="About"/> class.
		/// </summary>
		public About()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			initHelpMsg();
		}


		/// <summary>
		/// The about class constructor
		/// </summary>
		/// <param name="a">assembly</param>
		public About(Assembly a)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			if (a != null)
			{
				this.txtCompany.Text = System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location).CompanyName;
				this.txtCRT.Text = System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location).LegalCopyright;
				this.txtProductName.Text = System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location).ProductName;
				this.txtDescription.Text = System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location).FileDescription;
				this.txtComments.Text = System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location).Comments;
				this.txtVer.Text = System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location).FileVersion;
			}
			initHelpMsg();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            this.panel1 = new System.Windows.Forms.Panel();
            this.Logo = new System.Windows.Forms.PictureBox();
            this.panelAbout = new System.Windows.Forms.Panel();
            this.txtCRT = new System.Windows.Forms.TextBox();
            this.txtCompany = new System.Windows.Forms.TextBox();
            this.txtComments = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.txtVer = new System.Windows.Forms.TextBox();
            this.txtProductName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.rtxtHelp = new System.Windows.Forms.RichTextBox();
            this.btnOk = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.Logo)).BeginInit();
            this.panelAbout.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(434, 16);
            this.panel1.TabIndex = 0;
            // 
            // Logo
            // 
            this.Logo.BackColor = System.Drawing.Color.White;
            this.Logo.Dock = System.Windows.Forms.DockStyle.Top;
            this.Logo.Image = ((System.Drawing.Image)(resources.GetObject("Logo.Image")));
            this.Logo.Location = new System.Drawing.Point(0, 16);
            this.Logo.Name = "Logo";
            this.Logo.Size = new System.Drawing.Size(434, 55);
            this.Logo.TabIndex = 16;
            this.Logo.TabStop = false;
            // 
            // panelAbout
            // 
            this.panelAbout.Controls.Add(this.txtCRT);
            this.panelAbout.Controls.Add(this.txtCompany);
            this.panelAbout.Controls.Add(this.txtComments);
            this.panelAbout.Controls.Add(this.txtDescription);
            this.panelAbout.Controls.Add(this.txtVer);
            this.panelAbout.Controls.Add(this.txtProductName);
            this.panelAbout.Controls.Add(this.label6);
            this.panelAbout.Controls.Add(this.label5);
            this.panelAbout.Controls.Add(this.label4);
            this.panelAbout.Controls.Add(this.label3);
            this.panelAbout.Controls.Add(this.label2);
            this.panelAbout.Controls.Add(this.label1);
            this.panelAbout.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelAbout.Location = new System.Drawing.Point(0, 71);
            this.panelAbout.Name = "panelAbout";
            this.panelAbout.Size = new System.Drawing.Size(434, 146);
            this.panelAbout.TabIndex = 1;
            // 
            // txtCRT
            // 
            this.txtCRT.Location = new System.Drawing.Point(81, 24);
            this.txtCRT.Name = "txtCRT";
            this.txtCRT.ReadOnly = true;
            this.txtCRT.Size = new System.Drawing.Size(346, 20);
            this.txtCRT.TabIndex = 3;
            this.txtCRT.WordWrap = false;
            // 
            // txtCompany
            // 
            this.txtCompany.Location = new System.Drawing.Point(81, 3);
            this.txtCompany.Name = "txtCompany";
            this.txtCompany.ReadOnly = true;
            this.txtCompany.Size = new System.Drawing.Size(346, 20);
            this.txtCompany.TabIndex = 1;
            this.txtCompany.WordWrap = false;
            // 
            // txtComments
            // 
            this.txtComments.Location = new System.Drawing.Point(81, 121);
            this.txtComments.Name = "txtComments";
            this.txtComments.ReadOnly = true;
            this.txtComments.Size = new System.Drawing.Size(346, 20);
            this.txtComments.TabIndex = 11;
            this.txtComments.WordWrap = false;
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(81, 101);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.Size = new System.Drawing.Size(346, 20);
            this.txtDescription.TabIndex = 9;
            this.txtDescription.WordWrap = false;
            // 
            // txtVer
            // 
            this.txtVer.Location = new System.Drawing.Point(81, 73);
            this.txtVer.Name = "txtVer";
            this.txtVer.ReadOnly = true;
            this.txtVer.Size = new System.Drawing.Size(346, 20);
            this.txtVer.TabIndex = 7;
            this.txtVer.WordWrap = false;
            // 
            // txtProductName
            // 
            this.txtProductName.Location = new System.Drawing.Point(81, 52);
            this.txtProductName.Name = "txtProductName";
            this.txtProductName.ReadOnly = true;
            this.txtProductName.Size = new System.Drawing.Size(346, 20);
            this.txtProductName.TabIndex = 5;
            this.txtProductName.WordWrap = false;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(7, 24);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 21);
            this.label6.TabIndex = 2;
            this.label6.Text = "Copyright:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(7, 3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(67, 21);
            this.label5.TabIndex = 0;
            this.label5.Text = "Company:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(7, 121);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 21);
            this.label4.TabIndex = 10;
            this.label4.Text = "Description:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(7, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 20);
            this.label3.TabIndex = 8;
            this.label3.Text = "Title:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(21, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 21);
            this.label2.TabIndex = 6;
            this.label2.Text = "Version:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(21, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 21);
            this.label1.TabIndex = 4;
            this.label1.Text = "Product:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.rtxtHelp);
            this.panel2.Controls.Add(this.btnOk);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 217);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(434, 210);
            this.panel2.TabIndex = 2;
            // 
            // rtxtHelp
            // 
            this.rtxtHelp.AcceptsTab = true;
            this.rtxtHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtxtHelp.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtxtHelp.Location = new System.Drawing.Point(7, 4);
            this.rtxtHelp.Name = "rtxtHelp";
            this.rtxtHelp.ReadOnly = true;
            this.rtxtHelp.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.rtxtHelp.Size = new System.Drawing.Size(420, 162);
            this.rtxtHelp.TabIndex = 0;
            this.rtxtHelp.Text = "Command Line Options:";
            this.rtxtHelp.WordWrap = false;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(344, 176);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(60, 20);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "Okay";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // About
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(434, 427);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panelAbout);
            this.Controls.Add(this.Logo);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "About";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About the DTS Command Line Tool";
            ((System.ComponentModel.ISupportInitialize)(this.Logo)).EndInit();
            this.panelAbout.ResumeLayout(false);
            this.panelAbout.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void btnOk_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// Shows the text method shows the current about information.
		/// </summary>
		/// <param name="text">The text.</param>
		public void ShowText(string text)
		{
			this.panelAbout.Visible = false;
			rtxtHelp.Text = text;
			this.Text = "DTS Package Information:";
			this.Height = this.panel1.Height + this.panel2.Height + this.Logo.Height + this.btnOk.Height + 20;
			this.ShowDialog();
		}

        /// <summary>
        /// Allows change of display message
        /// </summary>
        public string Message
        {
            set { _message = value; }
        }

        private void initHelpMsg(string textMsg)
        {
            rtxtHelp.Rtf = textMsg;
        }

		private void initHelpMsg()
		{
            String strHlpMsg = null;
            this.panelAbout.Visible = true;
            if (_message != null)
            {
                initHelpMsg(_message);
            }
            else
            {
                strHlpMsg = strHlpMsg + @"{\rtf1\ansi\b ";
                strHlpMsg = strHlpMsg + @"Command Line Parameters:\b0 \par[/?] or [help]\tab\tab - display this window.\par ";
                strHlpMsg = strHlpMsg + @"[/d]\tab\tab\tab - display debug messages, [/h] overrides this.\par ";
                strHlpMsg = strHlpMsg + @"[/f logfile name]\tab\tab - set the package logging file name.\par ";
                strHlpMsg = strHlpMsg + @"[/e password]\tab\tab - set the package password.\par ";
                strHlpMsg = strHlpMsg + @"[/h]\tab\tab\tab - hide all debug and error message windows.\par ";
                strHlpMsg = strHlpMsg + @"[/i \{packageID\}]\tab\tab - sets the package ID to use.\par ";
                strHlpMsg = strHlpMsg + @"\tab\tab\tab   ...use with [/n] option.\par ";
                strHlpMsg = strHlpMsg + @"[/l DTS/XML file name]\tab - load package from DTS/XML file.\par ";
                strHlpMsg = strHlpMsg + @"\tab\tab\tab   ...use with [/n] option.\par ";
                strHlpMsg = strHlpMsg + @"\tab\tab\tab   ...automatically adds the .XML extension.\par ";
                strHlpMsg = strHlpMsg + @"[/n DTSPackageName]\tab - sets the DTS Package Name.\par ";
                strHlpMsg = strHlpMsg + @"\tab\tab\tab   ...this is a \b REQUIRED\b0  parameter.\par ";
                strHlpMsg = strHlpMsg + @"[/p password]\tab\tab - change the default admin password.\par ";
                strHlpMsg = strHlpMsg + @"[/r]\tab\tab\tab - remove the existing DTS package by name.\par ";
                strHlpMsg = strHlpMsg + @"\tab\tab\tab   ...use with [/n] option.\par ";
                strHlpMsg = strHlpMsg + @"[/pi]\tab\tab\tab - return package ID and version information.\par ";
                strHlpMsg = strHlpMsg + @"\tab\tab\tab   ...use with [/n] option.\par ";
                strHlpMsg = strHlpMsg + @"[/s sqlservername]\tab\tab - change the default server.\par ";
                strHlpMsg = strHlpMsg + @"\tab\tab\tab   ...this defaults to the local machine name.\par ";
                strHlpMsg = strHlpMsg + @"[/t]\tab\tab\tab - tests the SQL server connection, then exits.\par ";
                strHlpMsg = strHlpMsg + @"[/u username]\tab\tab - change the default admin user.\par ";
                strHlpMsg = strHlpMsg + @"[/v \{versionID\}]\tab\tab - sets the version ID to use.\par ";
                strHlpMsg = strHlpMsg + @"\tab\tab\tab   ...use with [/n] option.\par ";
                strHlpMsg = strHlpMsg + @"[/w]\tab\tab\tab - use Windows authentication for SQL server.\par ";
                strHlpMsg = strHlpMsg + @"\tab\tab\tab   ...overrides SQL mixed mode settings.\par ";
                strHlpMsg = strHlpMsg + @"\tab\tab\tab   ...use with load, remove and save operations.\par ";
                strHlpMsg = strHlpMsg + @"[/x XML file name]\tab\tab - output package to XML file.\par ";
                strHlpMsg = strHlpMsg + @"\tab\tab\tab   ...automatically adds the .XML extension.\par ";
                strHlpMsg = strHlpMsg + @"\tab\tab\tab   ...use with [/n] option.\par ";
                strHlpMsg = strHlpMsg + "}";
            }
			initHelpMsg(strHlpMsg);
		}
	}
}
