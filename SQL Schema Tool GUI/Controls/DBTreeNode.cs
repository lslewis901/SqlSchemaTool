using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text;

namespace Lewis.SST.Controls
{
    public class DBTreeNode : ExtTreeNode
    {
        private bool _compare = false;
        private string _server = null;
        // TODO: add DB image here.
        // TODO: add context menu here.
        public DBTreeNode()
        {
            // default ctor
        }

        public DBTreeNode(string Text)
        {
            this.Text = Text;
        }

        public string FullDBPath
        {
            get { return _server + "\\" + this.Text; }
        }

        public string Server
        {
            get { return _server; }
            set { _server = value; }
        }

        public bool SelectedForCompare
        {
            get { return _compare; }
            set { _compare = value; }
        }
    }

    public class DBTreeEventArgs : EventArgs
    {
        public DBTreeNode DBTreeNode;
    }
}
