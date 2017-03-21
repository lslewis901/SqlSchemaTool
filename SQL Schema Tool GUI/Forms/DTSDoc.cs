using Lewis.SST.Controls;
using Lewis.SST.DTSPackageClass;
using Lewis.SST.SQLObjects;

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

using WeifenLuo.WinFormsUI.Docking;

namespace Lewis.SST.Gui
{
	/// <summary>
	/// Dialog class to display SQL server DTS packages and allow loading and saving of those DTS packages from or to XML files.
	/// </summary>
    public class DTSDoc : Document
	{
		private string _UID;
		private string _PWD;

		private DTSPackages m_dPack = new DTSPackages();

		private System.Windows.Forms.Panel panelButtons;
		private System.Windows.Forms.Panel panelGrid;
		private System.Windows.Forms.DataGrid dataGrid1;
        private System.Windows.Forms.Button btn_GenXML;
        private System.Windows.Forms.ContextMenuStrip contextMenuTabPage;
        private System.Windows.Forms.ToolStripMenuItem menuItem3;
        private System.Windows.Forms.ToolStripMenuItem menuItem4;
        private System.Windows.Forms.ToolStripMenuItem menuItem5;

        private IContainer components;
        private string _Name;

		/// <summary>
		/// Initializes a new instance of the <see cref="DTS_Serializer"/> class.
		/// </summary>
		public DTSDoc(String name)
		{
            _Name = name;
			InitializeComponent();
            this.TabText = _Name;
            this.Text = _Name;
            this.ToolTipText = _Name;
            btn_GenXML.FlatAppearance.BorderSize = 0;
            WireEvents();
            SetupMenuIemSelectCompare();
		}

        private void SetupMenuIemSelectCompare()
        {
            menuItem3.Name = "generateDTSXml";
            menuItem3.Text = "Generate DTS XML Package File From Selected...";
            menuItem3.Click += new EventHandler(generateDTS_Click);
            menuItem4.Name = "SyncXMLTree";
            menuItem4.Text = "Synchronize in XML Node Explorer";
            menuItem4.Visible = false;
            menuItem5.Name = "Close";
            menuItem5.Text = "Close This";
            menuItem5.Click += new EventHandler(closeThis_Click);
        }

        private void closeThis_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void generateDTS_Click(object sender, EventArgs e)
        {
            GenerateXMLPackage();
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DTSDoc));
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btn_GenXML = new System.Windows.Forms.Button();
            this.panelGrid = new System.Windows.Forms.Panel();
            this.dataGrid1 = new System.Windows.Forms.DataGrid();
            this.contextMenuTabPage = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.panelButtons.SuspendLayout();
            this.panelGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
            this.contextMenuTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.btn_GenXML);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 325);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Padding = new System.Windows.Forms.Padding(2);
            this.panelButtons.Size = new System.Drawing.Size(640, 32);
            this.panelButtons.TabIndex = 3;
            // 
            // btn_GenXML
            // 
            this.btn_GenXML.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_GenXML.BackColor = System.Drawing.Color.Transparent;
            this.btn_GenXML.FlatAppearance.BorderColor = System.Drawing.Color.RoyalBlue;
            this.btn_GenXML.FlatAppearance.BorderSize = 0;
            this.btn_GenXML.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightSteelBlue;
            this.btn_GenXML.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_GenXML.Image = ((System.Drawing.Image)(resources.GetObject("btn_GenXML.Image")));
            this.btn_GenXML.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_GenXML.Location = new System.Drawing.Point(519, 5);
            this.btn_GenXML.Name = "btn_GenXML";
            this.btn_GenXML.Size = new System.Drawing.Size(116, 23);
            this.btn_GenXML.TabIndex = 0;
            this.btn_GenXML.Text = "Generate XML";
            this.btn_GenXML.UseVisualStyleBackColor = false;
            this.btn_GenXML.MouseLeave += new System.EventHandler(this.btn_GenXML_MouseLeave);
            this.btn_GenXML.Click += new System.EventHandler(this.btn_GenXML_Click);
            this.btn_GenXML.MouseMove += new System.Windows.Forms.MouseEventHandler(this.btn_GenXML_MouseMove);
            // 
            // panelGrid
            // 
            this.panelGrid.Controls.Add(this.dataGrid1);
            this.panelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGrid.Location = new System.Drawing.Point(0, 4);
            this.panelGrid.Name = "panelGrid";
            this.panelGrid.Padding = new System.Windows.Forms.Padding(2);
            this.panelGrid.Size = new System.Drawing.Size(640, 321);
            this.panelGrid.TabIndex = 4;
            // 
            // dataGrid1
            // 
            this.dataGrid1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGrid1.CaptionText = "DTS Packages";
            this.dataGrid1.DataMember = "";
            this.dataGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGrid1.FlatMode = true;
            this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGrid1.Location = new System.Drawing.Point(2, 2);
            this.dataGrid1.Name = "dataGrid1";
            this.dataGrid1.ReadOnly = true;
            this.dataGrid1.RowHeaderWidth = 5;
            this.dataGrid1.Size = new System.Drawing.Size(636, 317);
            this.dataGrid1.TabIndex = 1;
            // 
            // contextMenuTabPage
            // 
            this.contextMenuTabPage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem3,
            this.menuItem4,
            this.menuItem5});
            this.contextMenuTabPage.Name = "contextMenuTabPage";
            this.contextMenuTabPage.Size = new System.Drawing.Size(127, 70);
            // 
            // menuItem3
            // 
            this.menuItem3.Name = "menuItem3";
            this.menuItem3.Size = new System.Drawing.Size(126, 22);
            this.menuItem3.Text = "Option &1";
            // 
            // menuItem4
            // 
            this.menuItem4.Name = "menuItem4";
            this.menuItem4.Size = new System.Drawing.Size(126, 22);
            this.menuItem4.Text = "Option &2";
            // 
            // menuItem5
            // 
            this.menuItem5.Name = "menuItem5";
            this.menuItem5.Size = new System.Drawing.Size(126, 22);
            this.menuItem5.Text = "Option &3";
            // 
            // DTS_Serializer
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(640, 357);
            this.Controls.Add(this.panelGrid);
            this.Controls.Add(this.panelButtons);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DTS_Serializer";
            this.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.TabPageContextMenuStrip = this.contextMenuTabPage;
            this.TabText = "(Local)";
            this.Text = "(Local)";
            this.panelButtons.ResumeLayout(false);
            this.panelGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
            this.contextMenuTabPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

		#endregion

		/// <summary>
		/// Refreshes the grid.
		/// </summary>
        public void RefreshGrid(SqlConnection sqlConnection)
		{
            m_dPack.GetObject<DTSPackages>(sqlConnection, false);
            if (m_dPack.Tables.Count > 0)
			{
                this.dataGrid1.DataSource = m_dPack.Tables[0];
			}
			else
			{
				this.dataGrid1.DataSource = null;
			}
            dataGrid1.Focus();
		}

		/// <summary>
		/// Refreshes the grid.
		/// </summary>
		/// <param name="MostRecentOnly">if set to <c>true</c> [most recent only].</param>
		public void RefreshGrid(SqlConnection sqlConnection, bool MostRecentOnly)
		{
            m_dPack.GetObject<DTSPackages>(sqlConnection, MostRecentOnly);
            if (m_dPack.Tables.Count > 0)
			{
                this.dataGrid1.DataSource = m_dPack.Tables[0];
			}
			else
			{
				this.dataGrid1.DataSource = null;
			}
            dataGrid1.Focus();
        }

		/// <summary>
		/// Refreshes the grid.
		/// </summary>
		/// <param name="UserID">The user ID.</param>
		/// <param name="Password">The password.</param>
		public void RefreshGrid(string UserID, string Password)
		{
			_UID = UserID;
			_PWD = Password;

            m_dPack.GetObject<DTSPackages>(null, this.Text, false, UserID, Password, false);
            if (m_dPack.Tables.Count > 0)
			{
                this.dataGrid1.DataSource = m_dPack.Tables[0];
			}
			else
			{
				this.dataGrid1.DataSource = null;
			}
            dataGrid1.Focus();
        }

		/// <summary>
		/// Refreshes the grid.
		/// </summary>
		/// <param name="UserID">The user ID.</param>
		/// <param name="Password">The password.</param>
		/// <param name="MostRecentOnly">if set to <c>true</c> [most recent only].</param>
		public void RefreshGrid(string UserID, string Password, bool MostRecentOnly)
		{
			_UID = UserID;
			_PWD = Password;

            m_dPack.GetObject<DTSPackages>(null, this.Text, false, UserID, Password, MostRecentOnly);
            if (m_dPack.Tables.Count > 0)
			{
                this.dataGrid1.DataSource = m_dPack.Tables[0];
			}
			else
			{
				this.dataGrid1.DataSource = null;
			}
            dataGrid1.Focus();
        }

		private void SerializePackageAsXML(string PackageName, string LogFileName, string Description, string packGUID, string verGUID)
		{
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				DTSPackage2 oPackage = new DTSPackage2(PackageName, LogFileName, Description);

				// set the DTS package authentication type
				if (_UID != null && _PWD != null)
				{
					oPackage.Authentication = DTSPackage2.authTypeFlags.Default;
				}
				else
				{
					oPackage.Authentication = DTSPackage2.authTypeFlags.Trusted;
				}

				string SQLserver = this.TabText;

				// load the package from SQL server into the persisted object
				oPackage.Load(SQLserver, _UID, _PWD, null, packGUID, verGUID);
                m_DialogTitle = "Save DTS XML Package File as...";
                m_DialogTypeFilter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                ArrayList arl = ShowSaveFileDialog(PackageName, m_DialogTitle, m_DialogTypeFilter);
                DialogResult dr = (DialogResult)arl[0];
				if (dr == DialogResult.OK)
				{
					// save package from persisted object onto the HD as a file
                    FileName = (string)arl[1];
                    // TODO:  may want to do this async for bigger DTS packages
                    oPackage.Save(FileName);
                    // show xml
                    XMLDoc xmlDoc = new XMLDoc();
                    xmlDoc.RightToLeftLayout = RightToLeftLayout;
                    FileInfo fi = new FileInfo(FileName);
                    xmlDoc.TabText = fi.Name;
                    xmlDoc.Text = fi.Name;
                    xmlDoc.ToolTipText = fi.Name;
                    xmlDoc.FileName = FileName;
                    xmlDoc.Show(this.DockPanel);
                }
			}
			catch(Exception ex)
			{
                Cursor.Current = Cursors.Default;
                MessageBox.Show("XML Serializer Error is: " + ex.Message, "SERIALIZER ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
                Cursor.Current = Cursors.Default;
			}
		}

        public void GenerateXMLPackage()
        {
            DataTable dt = (DataTable)this.dataGrid1.DataSource;  
            BindingManagerBase bmGrid;
            if (dt != null)
            {
                bmGrid = BindingContext[dt];
                //int row = bmGrid.Position;
                if (bmGrid != null && bmGrid.Count > 0 && bmGrid.Current != null)
                {
                    DataRowView drv = (DataRowView)bmGrid.Current;
                    DataRow dr = drv.Row;
                    string PackGUID = "{" + dr["id"].ToString().ToUpper() + "}";
                    string VerGUID = "{" + dr["versionid"].ToString().ToUpper() + "}";
                    string msgstr = string.Format("Serialize the following DTS package as XML?\n\nSQL Server: {0}\nPackage Name: {1}\nPackage ID: {2}\nVersion ID: {3}", this.TabText, dr["name"], PackGUID, VerGUID);
                    DialogResult res = MessageBox.Show(this, msgstr, "Serialize DTS?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (res == DialogResult.Yes)
                    {
                        SerializePackageAsXML(dr["name"].ToString(), "", dr["description"].ToString(), PackGUID, VerGUID);
                    }
                }
            }
        }

        public DataTable gridDataSource
        {
            get { return (DataTable)this.dataGrid1.DataSource; }
        }

		private void btn_GenXML_Click(object sender, EventArgs e)
		{
            GenerateXMLPackage();
		}

        private void btn_GenXML_MouseMove(object sender, MouseEventArgs e)
        {
            btn_GenXML.FlatAppearance.BorderSize = 1; // about 2 pixels wide it seems, TODO: make a button who's style is like the tool bar menu buttons in VS2005
        }

        private void btn_GenXML_MouseLeave(object sender, EventArgs e)
        {
            btn_GenXML.FlatAppearance.BorderSize = 0;
        }
	}
}