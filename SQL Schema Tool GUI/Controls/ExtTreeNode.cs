using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text;

namespace Lewis.SST.Controls
{
    #region Helper class for Extended treeview nodes

    /// <summary>
    /// added this class to allow forward searches thru the tree view nodes 
    /// used in the find method
    /// </summary>
    public class ExtTreeNode : TreeNode
    {
        private int _level;
        private MouseButtons _mouseButton;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtTreeNode"/> class.
        /// </summary>
        public ExtTreeNode()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtTreeNode"/> class.
        /// </summary>
        /// <param name="Text">The text.</param>
        public ExtTreeNode(string Text)
        {
            this.Text = Text;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtTreeNode"/> class.
        /// </summary>
        /// <param name="Text">The text.</param>
        /// <param name="Level">The level.</param>
        public ExtTreeNode(string Text, int Level)
        {
            this.Text = Text;
            _level = Level;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtTreeNode"/> class.
        /// </summary>
        /// <param name="Text">The text.</param>
        /// <param name="imgNdx">The img NDX.</param>
        /// <param name="selectimgNdx">The selectimg NDX.</param>
        /// <param name="children">The children.</param>
        public ExtTreeNode(string Text, int imgNdx, int selectimgNdx, ExtTreeNode[] children)
        {
            this.Text = Text;
            this.ImageIndex = imgNdx;
            this.SelectedImageIndex = selectimgNdx;
            this.Nodes.AddRange(children);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtTreeNode"/> class.
        /// </summary>
        /// <param name="Text">The text.</param>
        /// <param name="imgNdx">The img NDX.</param>
        /// <param name="selectimgNdx">The selectimg NDX.</param>
        public ExtTreeNode(string Text, int imgNdx, int selectimgNdx)
        {
            this.Text = Text;
            this.ImageIndex = imgNdx;
            this.SelectedImageIndex = selectimgNdx;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtTreeNode"/> class.
        /// </summary>
        /// <param name="Text">The text.</param>
        /// <param name="children">The children.</param>
        public ExtTreeNode(string Text, ExtTreeNode[] children)
        {
            this.Text = Text;
            this.Nodes.AddRange(children);
        }

        /// <summary>
        /// Gets or sets the current tree node level.
        /// </summary>
        /// <value>The level.</value>
        public new int Level
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value;
            }
        }

        public MouseButtons LastMouseButton
        {
            get
            {
                return _mouseButton;
            }
            set
            {
                _mouseButton = value;
            }
        }

    }

    #endregion
}
