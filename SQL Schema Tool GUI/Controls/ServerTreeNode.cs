using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Text;

using Lewis.SST.SQLMethods;
namespace Lewis.SST.Controls
{
    /// <summary>
    /// class to store server login settings for SQL server treeview
    /// </summary>
    public class ServerTreeNode : ExtTreeNode
    {
        private string _uID;
        private string _pwd;
        private bool _connected = false;
        private bool _savePWD = false;
        private SecurityType _sType;
        private SqlConnection _sqlConn = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerTreeNode"/> class.
        /// </summary>
        public ServerTreeNode()
        {
            this.ImageIndex = 0;
            this.SelectedImageIndex = 0;
            _sType = SecurityType.Integrated;
            _connected = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerTreeNode"/> class.
        /// </summary>
        /// <param name="servername">The servername.</param>
        public ServerTreeNode(string servername)
        {
            this.Name = servername;
            this.Text = servername;
            this.ImageIndex = 0;
            this.SelectedImageIndex = 0;
            _sType = SecurityType.Integrated;
            _connected = false;
            _savePWD = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerTreeNode"/> class.
        /// </summary>
        /// <param name="servername">The servername.</param>
        /// <param name="UID">The UID.</param>
        /// <param name="PWD">The PWD.</param>
        public ServerTreeNode(string servername, string UID, string PWD, bool savePWD)
        {
            this.Name = servername;
            this.Text = servername;
            this.ImageIndex = 0;
            this.SelectedImageIndex = 0;
            _uID = UID;
            _pwd = PWD;
            _sType = SecurityType.Mixed;
            _connected = false;
            _savePWD = savePWD;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerTreeNode"/> class.
        /// </summary>
        /// <param name="servername">The servername.</param>
        /// <param name="UID">The UID.</param>
        /// <param name="PWD">The PWD.</param>
        /// <param name="imgNdx">The img NDX.</param>
        /// <param name="selectimgNdx">The selectimg NDX.</param>
        /// <param name="children">The children.</param>
        public ServerTreeNode(string servername, string UID, string PWD, bool savePWD, int imgNdx, int selectimgNdx, ExtTreeNode[] children)
        {
            this.Name = servername;
            this.Text = servername;
            this.ImageIndex = imgNdx;
            this.SelectedImageIndex = selectimgNdx;
            this.Nodes.AddRange(children);
            _sType = SecurityType.Mixed;
            _uID = UID;
            _pwd = PWD;
            _connected = false;
            _savePWD = savePWD;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerTreeNode"/> class.
        /// </summary>
        /// <param name="servername">The servername.</param>
        /// <param name="UID">The UID.</param>
        /// <param name="PWD">The PWD.</param>
        /// <param name="imgNdx">The img NDX.</param>
        /// <param name="selectimgNdx">The selectimg NDX.</param>
        public ServerTreeNode(string servername, string UID, string PWD, bool savePWD, int imgNdx, int selectimgNdx)
        {
            this.Name = servername;
            this.Text = servername;
            this.ImageIndex = imgNdx;
            this.SelectedImageIndex = selectimgNdx;
            _sType = SecurityType.Mixed;
            _uID = UID;
            _pwd = PWD;
            _connected = false;
            _savePWD = savePWD;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerTreeNode"/> class.
        /// </summary>
        /// <param name="servername">The servername.</param>
        /// <param name="UID">The UID.</param>
        /// <param name="PWD">The PWD.</param>
        /// <param name="children">The children.</param>
        public ServerTreeNode(string servername, string UID, string PWD, bool savePWD, ServerTreeNode[] children)
        {
            this.Name = servername;
            this.Text = servername;
            this.Nodes.AddRange(children);
            _sType = SecurityType.Mixed;
            _uID = UID;
            _pwd = PWD;
            _connected = false;
            _savePWD = savePWD;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerTreeNode"/> class.
        /// </summary>
        /// <param name="servername">The servername.</param>
        /// <param name="imgNdx">The img NDX.</param>
        /// <param name="selectimgNdx">The selectimg NDX.</param>
        /// <param name="children">The children.</param>
        public ServerTreeNode(string servername, int imgNdx, int selectimgNdx, ExtTreeNode[] children)
        {
            this.Name = servername;
            this.Text = servername;
            this.ImageIndex = imgNdx;
            this.SelectedImageIndex = selectimgNdx;
            this.Nodes.AddRange(children);
            _sType = SecurityType.Integrated;
            _connected = false;
            _savePWD = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerTreeNode"/> class.
        /// </summary>
        /// <param name="servername">The servername.</param>
        /// <param name="imgNdx">The img NDX.</param>
        /// <param name="selectimgNdx">The selectimg NDX.</param>
        public ServerTreeNode(string servername, int imgNdx, int selectimgNdx)
        {
            this.Name = servername;
            this.Text = servername;
            this.ImageIndex = imgNdx;
            this.SelectedImageIndex = selectimgNdx;
            _sType = SecurityType.Integrated;
            _connected = false;
            _savePWD = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerTreeNode"/> class.
        /// </summary>
        /// <param name="servername">The servername.</param>
        /// <param name="children">The children.</param>
        public ServerTreeNode(string servername, ServerTreeNode[] children)
        {
            this.Name = servername;
            this.Text = servername;
            this.Nodes.AddRange(children);
            _sType = SecurityType.Integrated;
            _connected = false;
            _savePWD = false;
        }

        /// <summary>
        /// Gets or sets the security.
        /// </summary>
        /// <value>The security.</value>
        public SecurityType Security
        {
            get
            {
                return _sType;
            }
            set
            {
                _sType = value;
            }
        }

        /// <summary>
        /// Gets or sets the UID.
        /// </summary>
        /// <value>The UID.</value>
        public string UID
        {
            get
            {
                return _uID;
            }
            set
            {
                _uID = value;
            }
        }

        /// <summary>
        /// Gets or sets the PWD.
        /// </summary>
        /// <value>The PWD.</value>
        public string Pwd
        {
            get
            {
                return _pwd;
            }
            set
            {
                _pwd = value;
            }
        }

        public bool Connected
        {
            get { return _connected; }
            set { _connected = value; }
        }

        public SqlConnection SQLServerConnection
        {
            get { return _sqlConn; }
            set { _sqlConn = value; }
        }

        public bool SavePWD
        {
            get { return _savePWD; }
            set { _savePWD = value; }
        }

    }

    public class ServerTreeEventArgs : EventArgs
    {
        public ServerTreeNode ServerTreeNode;
    }
}
