namespace Lewis.SST.Gui
{
    partial class XMLDoc
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XMLDoc));
            this.contextMenuTabPage = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.panelMain = new System.Windows.Forms.Panel();
            this.txtEditCtl = new ICSharpCode.TextEditor.TextEditorControl();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.contextMenuTabPage.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
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
            this.menuItem4.Image = ((System.Drawing.Image)(resources.GetObject("menuItem4.Image")));
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
            // panelMain
            // 
            this.panelMain.Controls.Add(this.txtEditCtl);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 4);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(448, 389);
            this.panelMain.TabIndex = 1;
            // 
            // txtEditCtl
            // 
            this.txtEditCtl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEditCtl.Location = new System.Drawing.Point(0, 0);
            this.txtEditCtl.Name = "txtEditCtl";
            this.txtEditCtl.ShowEOLMarkers = true;
            this.txtEditCtl.ShowSpaces = true;
            this.txtEditCtl.ShowTabs = true;
            this.txtEditCtl.ShowVRuler = true;
            this.txtEditCtl.Size = new System.Drawing.Size(448, 389);
            this.txtEditCtl.TabIndex = 0;
            // 
            // XMLDoc
            // 
            this.ClientSize = new System.Drawing.Size(448, 393);
            this.Controls.Add(this.panelMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "XMLDoc";
            this.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.TabPageContextMenuStrip = this.contextMenuTabPage;
            this.contextMenuTabPage.ResumeLayout(false);
            this.panelMain.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuTabPage;
		private System.Windows.Forms.ToolStripMenuItem menuItem3;
		private System.Windows.Forms.ToolStripMenuItem menuItem4;
        private System.Windows.Forms.ToolStripMenuItem menuItem5;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Panel panelMain;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;

        // this is here only to allow the designer to display properly
//        private ICSharpCode.TextEditor.TextEditorControl txtEditCtl;
    }
}