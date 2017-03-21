namespace Lewis.SST.Gui
{
    partial class SQLServerExplorer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SQLServerExplorer));
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.ServerMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.securityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshTreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.getDTSPackagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addSQLServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.refreshDatabasesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DBMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectedDBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deselectDBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateSQLScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.getDataTablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideListOfDataTablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnRefreshServers = new System.Windows.Forms.Button();
            this.btnAddServer = new System.Windows.Forms.Button();
            this.TableMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.compareSelectedMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectTableMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deselectTableMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deselectAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.getDataAsXLSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.excelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cvsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.xMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.generateTableSchemaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.ServerMenuStrip.SuspendLayout();
            this.DBMenuStrip.SuspendLayout();
            this.TableMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.ImageIndex = 10;
            this.treeView1.ImageList = this.imageList1;
            this.treeView1.Indent = 19;
            this.treeView1.Location = new System.Drawing.Point(0, 24);
            this.treeView1.Name = "treeView1";
            this.treeView1.SelectedImageIndex = 10;
            this.treeView1.Size = new System.Drawing.Size(245, 297);
            this.treeView1.TabIndex = 0;
            this.treeView1.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView1_BeforeExpand);
            this.treeView1.DoubleClick += new System.EventHandler(this.treeView1_DoubleClick);
            this.treeView1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeView1_MouseUp);
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.treeView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView1_MouseDown);
            this.treeView1.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView1_BeforeSelect);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList1.Images.SetKeyName(0, "");
            this.imageList1.Images.SetKeyName(1, "");
            this.imageList1.Images.SetKeyName(2, "");
            this.imageList1.Images.SetKeyName(3, "");
            this.imageList1.Images.SetKeyName(4, "");
            this.imageList1.Images.SetKeyName(5, "");
            this.imageList1.Images.SetKeyName(6, "");
            this.imageList1.Images.SetKeyName(7, "");
            this.imageList1.Images.SetKeyName(8, "");
            this.imageList1.Images.SetKeyName(9, "");
            this.imageList1.Images.SetKeyName(10, "EnterpriseManager.gif");
            this.imageList1.Images.SetKeyName(11, "connected.gif");
            this.imageList1.Images.SetKeyName(12, "unconnected.gif");
            this.imageList1.Images.SetKeyName(13, "Database.gif");
            this.imageList1.Images.SetKeyName(14, "Database2.gif");
            this.imageList1.Images.SetKeyName(15, "DTSPackage.gif");
            this.imageList1.Images.SetKeyName(16, "lan disconnect.ico");
            this.imageList1.Images.SetKeyName(17, "db.ico");
            this.imageList1.Images.SetKeyName(18, "dbs.ico");
            this.imageList1.Images.SetKeyName(19, "refresh.gif");
            this.imageList1.Images.SetKeyName(20, "database.bmp");
            this.imageList1.Images.SetKeyName(21, "database_read-only.bmp");
            this.imageList1.Images.SetKeyName(22, "database_pipes_24bit.bmp");
            this.imageList1.Images.SetKeyName(23, "TableHS.png");
            this.imageList1.Images.SetKeyName(24, "ckdb.PNG");
            this.imageList1.Images.SetKeyName(25, "database checked.png");
            this.imageList1.Images.SetKeyName(26, "table checked.png");
            // 
            // ServerMenuStrip
            // 
            this.ServerMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.securityToolStripMenuItem,
            this.removeServerToolStripMenuItem,
            this.refreshTreeToolStripMenuItem,
            this.getDTSPackagesToolStripMenuItem,
            this.addSQLServerToolStripMenuItem,
            this.toolStripMenuItem3,
            this.refreshDatabasesMenuItem});
            this.ServerMenuStrip.Name = "contextMenuStrip1";
            this.ServerMenuStrip.Size = new System.Drawing.Size(179, 142);
            this.ServerMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.ServerMenuStrip_Opening);
            // 
            // securityToolStripMenuItem
            // 
            this.securityToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("securityToolStripMenuItem.Image")));
            this.securityToolStripMenuItem.Name = "securityToolStripMenuItem";
            this.securityToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.securityToolStripMenuItem.Text = "&Set Server Security";
            this.securityToolStripMenuItem.Click += new System.EventHandler(this.mi_Security_Click);
            // 
            // removeServerToolStripMenuItem
            // 
            this.removeServerToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("removeServerToolStripMenuItem.Image")));
            this.removeServerToolStripMenuItem.Name = "removeServerToolStripMenuItem";
            this.removeServerToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.removeServerToolStripMenuItem.Text = "Re&move Server";
            this.removeServerToolStripMenuItem.Click += new System.EventHandler(this.removeServerToolStripMenuItem_Click);
            // 
            // refreshTreeToolStripMenuItem
            // 
            this.refreshTreeToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("refreshTreeToolStripMenuItem.Image")));
            this.refreshTreeToolStripMenuItem.Name = "refreshTreeToolStripMenuItem";
            this.refreshTreeToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.refreshTreeToolStripMenuItem.Text = "&Refresh Servers";
            this.refreshTreeToolStripMenuItem.Click += new System.EventHandler(this.mi_Refresh_Click);
            // 
            // getDTSPackagesToolStripMenuItem
            // 
            this.getDTSPackagesToolStripMenuItem.Enabled = false;
            this.getDTSPackagesToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("getDTSPackagesToolStripMenuItem.Image")));
            this.getDTSPackagesToolStripMenuItem.Name = "getDTSPackagesToolStripMenuItem";
            this.getDTSPackagesToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.getDTSPackagesToolStripMenuItem.Text = "Get &DTS Packages";
            this.getDTSPackagesToolStripMenuItem.Click += new System.EventHandler(this.getDTSPackagesToolStripMenuItem_Click);
            // 
            // addSQLServerToolStripMenuItem
            // 
            this.addSQLServerToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("addSQLServerToolStripMenuItem.Image")));
            this.addSQLServerToolStripMenuItem.Name = "addSQLServerToolStripMenuItem";
            this.addSQLServerToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.addSQLServerToolStripMenuItem.Text = "&Add SQL Server";
            this.addSQLServerToolStripMenuItem.Click += new System.EventHandler(this.addSQLServerToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(175, 6);
            // 
            // refreshDatabasesMenuItem
            // 
            this.refreshDatabasesMenuItem.Name = "refreshDatabasesMenuItem";
            this.refreshDatabasesMenuItem.Size = new System.Drawing.Size(178, 22);
            this.refreshDatabasesMenuItem.Text = "Refresh &Databases";
            this.refreshDatabasesMenuItem.Click += new System.EventHandler(this.refreshDatabasesMenuItem_Click);
            // 
            // DBMenuStrip
            // 
            this.DBMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectedDBToolStripMenuItem,
            this.deselectDBToolStripMenuItem,
            this.generateSQLScriptToolStripMenuItem,
            this.toolStripMenuItem1,
            this.getDataTablesToolStripMenuItem,
            this.hideListOfDataTablesToolStripMenuItem});
            this.DBMenuStrip.Name = "DBMenuStrip";
            this.DBMenuStrip.Size = new System.Drawing.Size(217, 120);
            this.DBMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.DBMenuStrip_Opening);
            // 
            // selectedDBToolStripMenuItem
            // 
            this.selectedDBToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("selectedDBToolStripMenuItem.Image")));
            this.selectedDBToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.selectedDBToolStripMenuItem.Name = "selectedDBToolStripMenuItem";
            this.selectedDBToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.selectedDBToolStripMenuItem.Text = "Select DB";
            this.selectedDBToolStripMenuItem.Click += new System.EventHandler(this.selectForCompareToolStripMenuItem_Click);
            // 
            // deselectDBToolStripMenuItem
            // 
            this.deselectDBToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("deselectDBToolStripMenuItem.Image")));
            this.deselectDBToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
            this.deselectDBToolStripMenuItem.Name = "deselectDBToolStripMenuItem";
            this.deselectDBToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.deselectDBToolStripMenuItem.Text = "Deselect DB";
            this.deselectDBToolStripMenuItem.Click += new System.EventHandler(this.deselectDBToolStripMenuItem_Click);
            // 
            // generateSQLScriptToolStripMenuItem
            // 
            this.generateSQLScriptToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("generateSQLScriptToolStripMenuItem.Image")));
            this.generateSQLScriptToolStripMenuItem.Name = "generateSQLScriptToolStripMenuItem";
            this.generateSQLScriptToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.generateSQLScriptToolStripMenuItem.Text = "&Generate SQL Schema";
            this.generateSQLScriptToolStripMenuItem.Click += new System.EventHandler(this.generateSQLScriptToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(213, 6);
            // 
            // getDataTablesToolStripMenuItem
            // 
            this.getDataTablesToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("getDataTablesToolStripMenuItem.Image")));
            this.getDataTablesToolStripMenuItem.Name = "getDataTablesToolStripMenuItem";
            this.getDataTablesToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.getDataTablesToolStripMenuItem.Text = "Get &List of Data Tables";
            this.getDataTablesToolStripMenuItem.Click += new System.EventHandler(this.getDataTablesToolStripMenuItem_Click);
            // 
            // hideListOfDataTablesToolStripMenuItem
            // 
            this.hideListOfDataTablesToolStripMenuItem.Name = "hideListOfDataTablesToolStripMenuItem";
            this.hideListOfDataTablesToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.hideListOfDataTablesToolStripMenuItem.Text = "&Remove List of Data Tables";
            this.hideListOfDataTablesToolStripMenuItem.Click += new System.EventHandler(this.hideListOfDataTablesToolStripMenuItem_Click);
            // 
            // btnRefreshServers
            // 
            this.btnRefreshServers.BackColor = System.Drawing.Color.Transparent;
            this.btnRefreshServers.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.btnRefreshServers.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnRefreshServers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefreshServers.Image = ((System.Drawing.Image)(resources.GetObject("btnRefreshServers.Image")));
            this.btnRefreshServers.Location = new System.Drawing.Point(0, 0);
            this.btnRefreshServers.Name = "btnRefreshServers";
            this.btnRefreshServers.Size = new System.Drawing.Size(28, 24);
            this.btnRefreshServers.TabIndex = 2;
            this.btnRefreshServers.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.btnRefreshServers, "Refresh SQL server list from the available local and networked SQL servers");
            this.btnRefreshServers.UseVisualStyleBackColor = false;
            this.btnRefreshServers.Click += new System.EventHandler(this.btnRefreshServers_Click);
            // 
            // btnAddServer
            // 
            this.btnAddServer.BackColor = System.Drawing.Color.Transparent;
            this.btnAddServer.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.btnAddServer.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnAddServer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddServer.Image = ((System.Drawing.Image)(resources.GetObject("btnAddServer.Image")));
            this.btnAddServer.Location = new System.Drawing.Point(34, 0);
            this.btnAddServer.Name = "btnAddServer";
            this.btnAddServer.Size = new System.Drawing.Size(28, 24);
            this.btnAddServer.TabIndex = 3;
            this.btnAddServer.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.btnAddServer, "Add a new SQL server connection");
            this.btnAddServer.UseVisualStyleBackColor = false;
            this.btnAddServer.Click += new System.EventHandler(this.btnAddServer_Click);
            // 
            // TableMenuStrip
            // 
            this.TableMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.compareSelectedMenuItem,
            this.selectTableMenuItem,
            this.deselectTableMenuItem,
            this.deselectAllMenuItem,
            this.toolStripMenuItem2,
            this.getDataAsXLSToolStripMenuItem,
            this.toolStripMenuItem4,
            this.generateTableSchemaToolStripMenuItem});
            this.TableMenuStrip.Name = "TableMenuStrip";
            this.TableMenuStrip.Size = new System.Drawing.Size(237, 170);
            this.TableMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.DBMenuStrip_Opening);
            // 
            // compareSelectedMenuItem
            // 
            this.compareSelectedMenuItem.Name = "compareSelectedMenuItem";
            this.compareSelectedMenuItem.Size = new System.Drawing.Size(236, 22);
            this.compareSelectedMenuItem.Text = "Compare Selected...";
            this.compareSelectedMenuItem.Visible = false;
            this.compareSelectedMenuItem.Click += new System.EventHandler(this.compareSelectedMenuItem_Click);
            // 
            // selectTableMenuItem
            // 
            this.selectTableMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("selectTableMenuItem.Image")));
            this.selectTableMenuItem.Name = "selectTableMenuItem";
            this.selectTableMenuItem.Size = new System.Drawing.Size(236, 22);
            this.selectTableMenuItem.Text = "Select Table";
            this.selectTableMenuItem.Click += new System.EventHandler(this.selectTableMenuItem_Click);
            // 
            // deselectTableMenuItem
            // 
            this.deselectTableMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("deselectTableMenuItem.Image")));
            this.deselectTableMenuItem.Name = "deselectTableMenuItem";
            this.deselectTableMenuItem.Size = new System.Drawing.Size(236, 22);
            this.deselectTableMenuItem.Text = "Deselect Table";
            this.deselectTableMenuItem.Visible = false;
            this.deselectTableMenuItem.Click += new System.EventHandler(this.deselectTableMenuItem_Click);
            // 
            // deselectAllMenuItem
            // 
            this.deselectAllMenuItem.Name = "deselectAllMenuItem";
            this.deselectAllMenuItem.Size = new System.Drawing.Size(236, 22);
            this.deselectAllMenuItem.Text = "Deselect All";
            this.deselectAllMenuItem.Visible = false;
            this.deselectAllMenuItem.Click += new System.EventHandler(this.deselectAllMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(233, 6);
            // 
            // getDataAsXLSToolStripMenuItem
            // 
            this.getDataAsXLSToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.excelMenuItem,
            this.cvsMenuItem,
            this.xMLToolStripMenuItem});
            this.getDataAsXLSToolStripMenuItem.Name = "getDataAsXLSToolStripMenuItem";
            this.getDataAsXLSToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.getDataAsXLSToolStripMenuItem.Text = "Export Table Data as ...";
            // 
            // excelMenuItem
            // 
            this.excelMenuItem.Name = "excelMenuItem";
            this.excelMenuItem.Size = new System.Drawing.Size(110, 22);
            this.excelMenuItem.Text = "Excel";
            this.excelMenuItem.Click += new System.EventHandler(this.excelMenuItem_Click);
            // 
            // cvsMenuItem
            // 
            this.cvsMenuItem.Name = "cvsMenuItem";
            this.cvsMenuItem.Size = new System.Drawing.Size(110, 22);
            this.cvsMenuItem.Text = "CVS";
            this.cvsMenuItem.Click += new System.EventHandler(this.cvsMenuItem_Click);
            // 
            // xMLToolStripMenuItem
            // 
            this.xMLToolStripMenuItem.Name = "xMLToolStripMenuItem";
            this.xMLToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.xMLToolStripMenuItem.Text = "XML";
            this.xMLToolStripMenuItem.Click += new System.EventHandler(this.xMLToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(233, 6);
            // 
            // generateTableSchemaToolStripMenuItem
            // 
            this.generateTableSchemaToolStripMenuItem.Name = "generateTableSchemaToolStripMenuItem";
            this.generateTableSchemaToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.generateTableSchemaToolStripMenuItem.Text = "Generate Table SQL Schema ...";
            this.generateTableSchemaToolStripMenuItem.Click += new System.EventHandler(this.generateTableSchemaToolStripMenuItem_Click);
            // 
            // SQLServerExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(245, 322);
            this.Controls.Add(this.btnAddServer);
            this.Controls.Add(this.btnRefreshServers);
            this.Controls.Add(this.treeView1);
            this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)((((WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft | WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight)
                        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop)
                        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom)));
            this.DoubleBuffered = true;
            this.HideOnClose = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SQLServerExplorer";
            this.Padding = new System.Windows.Forms.Padding(0, 24, 0, 1);
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockLeft;
            this.ShowInTaskbar = false;
            this.TabText = "SQL Server Explorer";
            this.Text = "SQL Server Explorer";
            this.ServerMenuStrip.ResumeLayout(false);
            this.DBMenuStrip.ResumeLayout(false);
            this.TableMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ContextMenuStrip ServerMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem securityToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshTreeToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip DBMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem selectedDBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deselectDBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem getDTSPackagesToolStripMenuItem;
        private System.Windows.Forms.Button btnRefreshServers;
        private System.Windows.Forms.ToolStripMenuItem generateSQLScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSQLServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeServerToolStripMenuItem;
        private System.Windows.Forms.Button btnAddServer;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem getDataTablesToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip TableMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem selectTableMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deselectTableMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem hideListOfDataTablesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem getDataAsXLSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem excelMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cvsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem xMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshDatabasesMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem compareSelectedMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deselectAllMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem generateTableSchemaToolStripMenuItem;


    }
}