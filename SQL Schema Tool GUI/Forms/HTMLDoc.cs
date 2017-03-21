using Lewis.SST.AsyncMethods;
using Lewis.SST.Controls;
using Lewis.SST;
using Lewis.Xml;

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
    public class HTMLDoc : Document
	{
        private System.Windows.Forms.ContextMenuStrip contextMenuTabPage;
        private System.Windows.Forms.ToolStripMenuItem menuItem3;
        private System.Windows.Forms.ToolStripMenuItem menuItem4;
        private System.Windows.Forms.ToolStripMenuItem menuItem5;

        private IContainer components;
        private WebBrowser wb1;
        private string _Name;
        private bool _loaded = false;
        private bool _default = false;

        public event EventHandler<WebBrowserDocumentCompletedEventArgs> DocumentLoaded;

        public HTMLDoc(String name)
		{
            _Name = name;
			InitializeComponent();
            this.TabText = _Name;
            this.Text = _Name;
            this.ToolTipText = _Name;
            WireEvents();
            SetupMenuIemSelectCompare();
		}

        private void SetupMenuIemSelectCompare()
        {
            //menuItem3.Name = "generateDTSXml";
            //menuItem3.Text = "Generate DTS XML Package File From Selected...";
            //menuItem3.Click += new EventHandler(generateDTS_Click);
            //menuItem4.Name = "SyncXMLTree";
            //menuItem4.Text = "Synchronize in XML Node Explorer";
            //menuItem4.Visible = false;
            //menuItem5.Name = "Close";
            //menuItem5.Text = "Close This";
            //menuItem5.Click += new EventHandler(closeThis_Click);
        }

        private void closeThis_Click(object sender, EventArgs e)
        {
            this.Close();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HTMLDoc));
            this.contextMenuTabPage = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.wb1 = new System.Windows.Forms.WebBrowser();
            this.contextMenuTabPage.SuspendLayout();
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
            // wb1
            // 
            this.wb1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wb1.Location = new System.Drawing.Point(0, 4);
            this.wb1.MinimumSize = new System.Drawing.Size(20, 20);
            this.wb1.Name = "wb1";
            this.wb1.Size = new System.Drawing.Size(640, 353);
            this.wb1.TabIndex = 1;
            // 
            // HTMLDoc
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(640, 357);
            this.Controls.Add(this.wb1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "HTMLDoc";
            this.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.TabPageContextMenuStrip = this.contextMenuTabPage;
            this.TabText = "(Local)";
            this.Text = "Report";
            this.contextMenuTabPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

		#endregion

        protected override void WireEvents()
        {
            base.WireEvents();
            this.wb1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(wb1_DocumentCompleted);
        }

        private void wb1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Cursor.Current = Cursors.Default;
            //wb1.EndInvoke(this);
            _loaded = true;
            EventHandler<WebBrowserDocumentCompletedEventArgs> handler = DocumentLoaded;
            if (handler != null)
            {
                // raises the event. 
                handler(this, e);
            }
        }

        public string HTMLText
        {
            get { return this.wb1.DocumentText; }
            set { this.wb1.DocumentText = value; }
        }

        public bool HTMLLoaded
        {
            get { return _loaded; }
        }

        public bool DefaultView
        {
            set { _default = value; }
        }

        public override bool Open(DockPanel dockPanel)
        {
            m_DialogTitle = "Open HTML Report...";
            m_DialogTypeFilter = "HTML files (*.html)|*.html|All files (*.*)|*.*";
            bool result = base.Open(dockPanel);
            FileName = m_fileName;
            if (result)
            {
                Show(dockPanel);
            }
            return result;
        }

        public override string FileName
        {
            get { return m_fileName; }
            set
            {
                if (value != null && value != string.Empty)
                {
                    m_fileName = value;
                    if (File.Exists(m_fileName))
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        FileInfo fi = new FileInfo(m_fileName);
                        this.TabText = fi.Name.Replace(fi.Extension, ".html");
                        this.Text = fi.Name.Replace(fi.Extension, ".html");
                        this.ToolTipText = fi.Name.Replace(fi.Extension, ".html");
                        try
                        {
                            if (_default)
                            {
                                asyncHTML_CompleteTransformHTML(fi.FullName);
                                //AsyncTransformHTML asyncHTML = new AsyncTransformHTML(this, m_fileName, fi.Name.Replace(fi.Extension, ".html"), AsyncTransformHTML.TransformType.Standard);
                                //asyncHTML.CompleteTransformHTML += new AsyncTransformHTML.CompleteTransformDelegate(asyncHTML_CompleteTransformHTML);
                                //asyncHTML.Start();
                                //HTMLText = XsltHelper.Transform(File.ReadAllText(m_fileName), XsltHelper.DEFAULTXSLT);
                            }
                            else
                            {
                                AsyncTransformHTML asyncHTML = new AsyncTransformHTML(this, m_fileName, fi.Name.Replace(fi.Extension, ".html"), AsyncTransformHTML.TransformType.Report);
                                asyncHTML.CompleteTransformHTML += new AsyncTransformHTML.CompleteTransformDelegate(asyncHTML_CompleteTransformHTML);
                                m_fileName = fi.Name.Replace(fi.Extension, ".html");
                                asyncHTML.Start();
                                //HTMLText = XsltHelper.Transform(File.ReadAllText(m_fileName), XsltHelper.HTMLXSLT);
                            }
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show(ex.Message, "TRANSFORMATION ERROR", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            logger.Error(SQLSchemaTool.ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
                        }
                    }
                    else
                    {
                        m_fileName = null;
                    }
                }
            }
        }

        private void asyncHTML_CompleteTransformHTML(string fileName)
        {
            this.wb1.Navigate(string.Format("file:///{0}", fileName));
            m_fileName = fileName;
        }
	}
}