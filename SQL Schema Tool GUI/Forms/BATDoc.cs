using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Util;

using Lewis.SST.Controls;
using Lewis.Xml;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using WeifenLuo.WinFormsUI.Docking;

namespace Lewis.SST.Gui
{
    public partial class BATDoc : Document
    {
        public BATDoc()
        { 
            InitializeComponent();
            m_fileName = "SSTcmdline.bat";

            SetTextEditorDefaultProperties();
            SetupMenuIemSelectCompare();

            BATToolResourceSyntaxModeProvider provider = new BATToolResourceSyntaxModeProvider();
            ICSharpCode.TextEditor.Document.HighlightingManager.Manager.AddSyntaxModeFileProvider(provider);
            txtEditCtl.Document.HighlightingStrategy = HighlightingManager.Manager.FindHighlighter("BAT");

            // TODO: replace with BAT folding strategy
            XmlFoldingStrategy folding = new XmlFoldingStrategy();

            txtEditCtl.Document.FoldingManager.FoldingStrategy = (IFoldingStrategy)folding;

            WireEvents();
        }

        private void SetupMenuIemSelectCompare()
        {
            menuItem3.Name = "RUNTHIS";
            menuItem3.Text = "Run This";
            menuItem3.Visible = false;
            menuItem4.Name = "";
            menuItem4.Text = "";
            menuItem4.Visible = false;
            menuItem5.Name = "Close";
            menuItem5.Text = "Close This";
            menuItem5.Click += new EventHandler(closeThis_Click);
        }

        private void closeThis_Click(object sender, EventArgs e)
        {
            // Close doc
            Close();
        }

        public string BatchText
        {
            get
            {
                return txtEditCtl != null ? txtEditCtl.Text : string.Empty;
            }
            set
            {
                if (txtEditCtl != null)
                {
                    txtEditCtl.Text = value;
                    base.TabText = m_fileName;
                }
            }
        }

        public override bool Open(DockPanel dockPanel)
        {
            m_DialogTitle = "Open BAT File...";
            m_DialogTypeFilter = "BAT files (*.bat)|*.bat|All files (*.*)|*.*";
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
			get	{ return m_fileName; }
			set
			{
                if (value != null && value != string.Empty)
				{
                    m_fileName = value;
                    if (File.Exists(m_fileName))
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        FileInfo fi = new FileInfo(m_fileName);
                        this.TabText = fi.Name;
                        this.Text = fi.Name;
                        this.ToolTipText = fi.Name;
                        txtEditCtl.LoadFile(m_fileName, true, true);
                        ForceFoldingUpdate(m_fileName);
                        Cursor.Current = Cursors.Default;
                    }
                    else
                    {
                        m_fileName = string.Empty;
                    }
                }
			}
		}

        public override void Save(bool showDialog)
        {
            m_DialogTitle = "Save BAT File As...";
            m_DialogTypeFilter = "BAT files (*.bat)|*.bat|All files (*.*)|*.*";
            m_fileName = m_fileName == string.Empty ? "SSTcmdline.bat" : m_fileName; // default filename
            base.Save(showDialog);
        }
    }

    public class BATToolResourceSyntaxModeProvider : ISyntaxModeFileProvider
    {
        List<SyntaxMode> syntaxModes = null;

        public ICollection<SyntaxMode> SyntaxModes
        {
            get
            {
                return syntaxModes;
            }
        }

        public BATToolResourceSyntaxModeProvider()
        {
            Assembly assembly = this.GetType().Assembly;
            Stream syntaxModeStream = assembly.GetManifestResourceStream("Lewis.SST.Resources.SyntaxModes.xml");
            if (syntaxModeStream != null)
            {
                syntaxModes = SyntaxMode.GetSyntaxModes(syntaxModeStream);
            }
            else
            {
                syntaxModes = new List<SyntaxMode>();
            }
        }

        public XmlTextReader GetSyntaxModeFile(SyntaxMode syntaxMode)
        {
            Assembly assembly = this.GetType().Assembly;
            return new XmlTextReader(assembly.GetManifestResourceStream("Lewis.SST.Resources." + syntaxMode.FileName));
        }

        public void UpdateSyntaxModeList()
        {
            // resources don't change during runtime
        }
    }
}