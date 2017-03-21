using System;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Lewis.SST.Controls
{
    public class XPathNavigatorTreeNode : ExtTreeNode
    {
        #region Variables

        private bool _hasExpanded;
        private XPathNavigator _navigator;
        private int _tipLength = 400;

        #endregion

        #region Constructors

        public XPathNavigatorTreeNode()
        {
        }

        public XPathNavigatorTreeNode(XPathNavigator navigator)
        {
            this.Navigator = navigator;
            this.ToolTipText = navigator.InnerXml != null && navigator.InnerXml.Length > _tipLength ? navigator.InnerXml.Substring(0, _tipLength) + "..." : navigator.InnerXml;
        }

        #endregion

        #region Properties

        public int TipLength
        {
            get { return _tipLength; }
            set { _tipLength = value; }
        }

        /// <summary>
        /// Gets or sets whether this XPathNavigatorTreeNode has been expanded and loaded.
        /// </summary>
        public bool HasExpanded
        {
            get
            {
                return _hasExpanded;
            }
            set
            {
                _hasExpanded = value;
            }
        }

        /// <summary>
        /// Gets or sets the XPathNavigator this XPathNavigatorTreeNode represents.
        /// </summary>
        public XPathNavigator Navigator
        {
            get
            {
                return _navigator;
            }
            set
            {
                _navigator = value;
                this.Initialize();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the text used to display this XPathNavigatorTreeNode, formatted using the XPathNavigator it represents.
        /// </summary>
        /// <returns></returns>
        public string GetDisplayText()
        {
            if (_navigator == null)
                return string.Empty;

            StringBuilder builder = new StringBuilder();
            switch (_navigator.NodeType)
            {
                case XPathNodeType.Comment:
                    // comments are easy, just append the value inside <!-- --> tags
                    builder.Append("<!--");
                    builder.Append(this.StripNonPrintableChars(_navigator.Value));
                    builder.Append(" -->");
                    break;

                case XPathNodeType.Root:
                case XPathNodeType.Element:
                    // append the start of the element
                    builder.AppendFormat("<{0}", _navigator.Name);

                    // append any attributes
                    if (_navigator.HasAttributes)
                    {
                        // clone the node's navigator (cursor), so it doesn't lose it's position
                        XPathNavigator attributeNavigator = _navigator.Clone();
                        if (attributeNavigator.MoveToFirstAttribute())
                        {
                            do
                            {
                                builder.Append(" ");

                                builder.AppendFormat("{0}=\"{1}\"", attributeNavigator.Name, attributeNavigator.Value);
                            }
                            while (attributeNavigator.MoveToNextAttribute());
                        }
                    }

                    // if the element has no children, close the node immediately
                    if (!_navigator.HasChildren)
                    {
                        builder.Append("/>");
                    }
                    else
                    {
                        // otherwise, an end tag node will be appended by the XPathNavigatorTreeView after it's expanded
                        builder.Append(">");
                    }
                    break;

                default:
                    // all other node types are easy, just append the value
                    // strings, whitespace, etc.
                    builder.Append(this.StripNonPrintableChars(_navigator.Value));
                    break;
            }
            return builder.ToString();
        }

        /// <summary>
        /// Initializes the node using it's XPathNavigator
        /// </summary>
        public void Initialize()
        {
            // default to an empty string
            string displayText = string.Empty;

            base.Nodes.Clear();
            if (_navigator != null)
            {
                displayText = this.GetDisplayText();

                // if the xml node has children, add a placeholder node,
                // so that it can be expanded.  a treenode with no child 
                // nodes has no expansion indicator , and cannot be expanded
                if (_navigator.HasChildren)
                {
                    base.Nodes.Add(string.Empty);
                }
            }
            base.Text = displayText;
        }

        private string StripNonPrintableChars(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // todo: use a regex instead
            value = value.Trim(new char[] { '\r', '\n', '\t' });
            value = value.Replace("\r", null);
            value = value.Replace("\n", " ");
            value = value.Replace("\t", null);

            return value;
        }

        #endregion

    }
}
