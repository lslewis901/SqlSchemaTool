using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using Lewis.SST.AsyncMethods;
using Lewis.SST.Controls;
using Lewis.SST.SQLMethods;
using Lewis.Xml;

using WeifenLuo.WinFormsUI.Docking;

namespace Lewis.SST.Gui
{
    // TODO:  lots of stuff still, like hooking up context find/edit menu items
    public partial class XmlNodeExplorer : ToolWindow
    {
        private string xmlDocFileName = string.Empty;
        private FindValueDlg fdlg = new FindValueDlg();

        public XmlNodeExplorer()
        {

            InitializeComponent();            
            treeView1.Nodes.Clear();
            XmlProperties.ToolbarVisible = true;
        }

        public void Clear()
        {
            try
            {
                treeView1.Nodes.Clear();
                xmlDocFileName = null;
                XmlProperties.SelectedObject = null;
            }
            catch { }
        }

        public string Filename
        {
            get { return xmlDocFileName; }
        }

        /// <summary>
        /// Opens the specified file.
        /// </summary>
        public void OpenFile(string fileName)
        {
            xmlDocFileName = fileName;
            if (fileName != null)
            {
                treeView1.Nodes.Clear();

                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    if (File.Exists(fileName))
                    {
                        // populate treeview with xml nodes
                        this.treeView1.Open(fileName);
                        XmlProperties.SelectedObject = ((XPathNavigatorTreeNode)this.treeView1.Nodes[0]).Navigator;
                    }
                }
                catch (XmlException err)
                {
                    MessageBox.Show(err.Message);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private TreeNode CheckNodeForMouseUp(TreeNode tnParent, Point p, MouseButtons mouseButton)
        {
            foreach (TreeNode tn in tnParent.Nodes)
            {
                if (tn.Bounds.Contains(p))
                {
                    if (typeof(ExtTreeNode).IsInstanceOfType(tn))
                    {
                        ((ExtTreeNode)tn).LastMouseButton = mouseButton;
                    }
                    treeView1.SelectedNode = tn;
                    return tn;
                }
                TreeNode tn2 = CheckNodeForMouseUp(tn, p, mouseButton);
                if (tn2 != null)
                {
                    return tn2;
                }
            }
            return null;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (typeof(XPathNavigatorTreeNode).IsInstanceOfType(this.treeView1.SelectedNode))
            {
                XmlProperties.SelectedObject = ((XPathNavigatorTreeNode)this.treeView1.SelectedNode).Navigator;
            }
        }

        private void treeView1_MouseUp(object sender, MouseEventArgs e)
        {
            MouseButtons mouseButton = e.Button;
            Point p = new Point(e.X, e.Y);
            if (treeView1.Nodes.Count == 0 || treeView1.Nodes[0] == null) return;

            TreeNode tn = CheckNodeForMouseUp(treeView1.Nodes[0], p, mouseButton);
            if (tn != null)
            {
                if (mouseButton == MouseButtons.Right)
                {
                    if (tn.ContextMenuStrip != null)
                    {
                        if (typeof(ExtTreeNode).IsInstanceOfType(tn))
                        {
                            ((ExtTreeNode)tn).LastMouseButton = mouseButton;
                        }
                        tn.ContextMenuStrip.Show();
                    }
                }
            }
        }

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            MouseButtons mouseButton = e.Button;
            Point p = new Point(e.X, e.Y);
            if (treeView1.Nodes.Count == 0 || treeView1.Nodes[0] == null) return;

            foreach (TreeNode tn in treeView1.Nodes[0].Nodes)
            {
                if (tn.Bounds.Contains(p))
                {
                    if (typeof(ExtTreeNode).IsInstanceOfType(tn))
                    {
                        ((ExtTreeNode)tn).LastMouseButton = mouseButton;
                    }
                    treeView1.SelectedNode = tn;
                    break;
                }
            }
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fdlg.ReplaceFlag = false;
            fdlg.Text = "Find By XPath...";
            DialogResult dr = fdlg.ShowDialog(this);
            if (fdlg.FindValue.Length == 0) return;

            if (dr == DialogResult.OK)
            {
                string searchPattern = fdlg.FindValue;
                if (!treeView1.FindByXpath(searchPattern))
                {
                    // TODO hook status message to indicate unable to find node
                }
            }
        }
    }
}
