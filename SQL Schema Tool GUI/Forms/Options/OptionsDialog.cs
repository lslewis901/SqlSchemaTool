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

namespace Lewis.OptionsDialog
{
    public partial class OptionsDialog : Form
    {
        private TreeNode topLevel = new TreeNode();
        public EventHandler<OptionsClosingEvent> DialogClosing;
        private string _values = string.Empty;
        private ToolTip toolTip = new ToolTip();

        public OptionsDialog()
        {
            // Set up the delays for the ToolTip.
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip.ShowAlways = true;

            InitializeComponent();
            this.splitContainer1.Panel2.Controls.Clear();
            this.Load += new EventHandler(OptionsDialog_Load);
            this.FormClosing += new FormClosingEventHandler(OptionsDialog_FormClosing);
            this.treeView1.AfterSelect += new TreeViewEventHandler(treeView1_AfterSelect);
            CreateDynamicControls();
        }

        public OptionsDialog(string values)
        {
            // Set up the delays for the ToolTip.
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip.ShowAlways = true;
            toolTip.UseFading = true;
            toolTip.IsBalloon = true;

            _values = values;
            InitializeComponent();
            this.splitContainer1.Panel2.Controls.Clear();
            this.Load += new EventHandler(OptionsDialog_Load);
            this.FormClosing += new FormClosingEventHandler(OptionsDialog_FormClosing);
            this.treeView1.AfterSelect += new TreeViewEventHandler(treeView1_AfterSelect);
            CreateDynamicControls();
            SetControlsProperties();
        }

        public string ControlsValues
        {
            get { return _values; }
            set 
            { 
                _values = value;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // change panels to current node;
            if (e.Node.Tag != null && typeof(TableLayoutPanel).IsInstanceOfType(e.Node.Tag))
            {
                this.splitContainer1.Panel2.Controls.Clear();
                Panel p = (Panel)e.Node.Tag;
                this.splitContainer1.Panel2.Controls.Add(p);
                p.Dock = DockStyle.Fill;
                p.Visible = true;
                toolTip.Active = true;
            }
        }

        private void OptionsDialog_Load(object sender, EventArgs e)
        {
            // Sanity check 521, 325
            if (this.Width < 200 || this.Height < 100)
            {
                this.Height = 325;
                this.Width = 521;
            }
        }

        private void OptionsDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // serialize the child controls values and save if OK button pressed
            string values = string.Empty;
            if (this.DialogResult == DialogResult.OK)
            {
                values = ParseOptionsControls();
            }
            OptionsClosingEvent args = new OptionsClosingEvent(this.DialogResult, values);
            EventHandler<OptionsClosingEvent> handler = DialogClosing;
            if (handler != null)
            {
                // raises the event. 
                handler(this, args);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CreateDynamicControls()
        {
            string xmlControls = ResourceReader.ReadFromResource("Lewis.SST.Forms.Options.optionsControls.xml");
            if (!xmlControls.Equals(string.Empty))
            {
                XmlDocument xControls = new XmlDocument();
                xControls.LoadXml(xmlControls);
                string name = xControls.SelectSingleNode("/options/settings").Attributes["name"].Value;
                topLevel.Text = name;
                treeView1.Nodes.Add(topLevel);
                XmlNodeList xnl = xControls.SelectNodes("/options/settings/panel");
                if (xnl != null)
                {
                    foreach (XmlNode node in xnl)
                    {
                        // add new panel for each new tree node
                        ColumnStyle cstyle = new ColumnStyle(SizeType.Percent);
                        cstyle.Width = 50;  // 50%
                        TreeNode tn = new TreeNode(node.Attributes["name"].Value);
                        TableLayoutPanel p = new TableLayoutPanel();
                        p.AutoSize = true;
                        p.AutoScroll = true;
                        p.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
                        p.Padding = new Padding(1, 1, 4, 5);
                        p.Name = tn.Text;
                        p.Anchor = AnchorStyles.None;
                        p.ColumnStyles.Add(cstyle);
                        p.ColumnCount = 2;
                        p.Visible = false;
                        
                        XmlNodeList xnlControls = node.SelectNodes("./controls/control");
                        for (int ii = 0; ii < xnlControls.Count; ii++)
                        {
                            XmlNode xnCtrl = (XmlNode)xnlControls[ii];
                            string type = xnCtrl.Attributes["type"].Value;
                            string label = xnCtrl.Attributes["label"].Value;
                            string ctrlName = xnCtrl.Attributes["name"].Value;
                            string tooTip = xnCtrl.Attributes["tooltip"].Value;
                            Type t = OptionValues.GetTypeFromAppDomain(type, "System.Windows.Forms");
                            Object obj = Activator.CreateInstance(t);
                            ((Control)obj).Name = ctrlName;
                            if (type.ToLower().Equals("textbox"))
                            {
                                ((TextBox)obj).MaxLength = 255;
                                ((TextBox)obj).ScrollBars = ScrollBars.Horizontal;
                            }
                            toolTip.SetToolTip((Control)obj, tooTip);
                            Label lbl = new Label();
                            lbl.Padding = new Padding(2, 6, 0, 0);
                            lbl.AutoSize = true;
                            lbl.Text = label;
                            p.Controls.Add((Control)lbl, 0, ii);
                            p.Controls.Add((Control)obj, 1, ii);
                        }
                        tn.Tag = p;
                        topLevel.Nodes.Add(tn);
                    }
                }
                treeView1.ExpandAll();
            }
        }

        private void SetPropertyValue(object obj, string propName, object val)
        {
            PropertyDescriptor prop = TypeDescriptor.GetProperties(obj.GetType())[propName];
            if (null != prop)
            {
                // Check for string type
                if (prop.PropertyType.IsAssignableFrom(val.GetType()))
                {
                    // Just set the value
                    prop.SetValue(obj, val);
                }
                else
                {
                    // Need to do type conversion - use a type converter
                    TypeConverter tc = TypeDescriptor.GetConverter(prop.PropertyType);
                    object newVal = null;

                    if ((null != tc) && (tc.CanConvertFrom(typeof(string))))
                    {
                        newVal = tc.ConvertFrom(val);
                        if (null != newVal)
                        {
                            // Conversion worked, set value
                            prop.SetValue(obj, newVal);
                        }
                    }
                }
            }
            else if (val.GetType() == typeof(string))
            {
                // Maybe an event?
                //SetEventValue(obj, propName, val as string);
            }
        }

        private void SetControlsProperties()
        {
            if (_values != null && _values.Length > 0)
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(_values);
                XmlNodeList xnl = xDoc.SelectNodes("/options/settings/panel");
                if (xnl != null)
                {
                    foreach (XmlNode xn in xnl)
                    {
                        foreach (TreeNode tn in topLevel.Nodes)
                        {
                            if (tn.Text.ToLower().Equals(xn.Attributes["name"].Value.ToLower()))
                            {
                                if (tn.Tag != null && typeof(TableLayoutPanel).IsInstanceOfType(tn.Tag))
                                {
                                    foreach (Control c in ((TableLayoutPanel)tn.Tag).Controls)
                                    {
                                        if (!typeof(Label).IsInstanceOfType(c))
                                        {
                                            XmlNode ctrl = xn.SelectSingleNode("/options/settings/panel/controls/control[@name='" + c.Name + "']");
                                            if (ctrl != null)
                                            {
                                                if (ctrl.InnerText != null && ctrl.InnerText.Length > 0)
                                                {
                                                    string[] ctrlItems = ctrl.InnerText.Split(':');
                                                    string property = ctrlItems.Length > 0 ? ctrlItems[0].Trim() : null;
                                                    string value = null;
                                                    if ( ctrlItems.Length > 1 )
                                                    {
                                                        for (int ii = 1; ii < ctrlItems.Length ; ii++)
                                                        {
                                                            bool addColon = ii < ctrlItems.Length - 1;
                                                            value += ctrlItems[ii].Trim() + (addColon ? ":" : "");
                                                        }
                                                    }
                                                    SetPropertyValue(c, property, value);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private string ParseOptionsControls()
        {
            XmlDocument xDoc = new XmlDocument();
            XmlElement options = xDoc.CreateElement("options");
            xDoc.AppendChild(options);
            XmlElement settings = xDoc.CreateElement("settings");
            options.AppendChild(settings);
            foreach (TreeNode tn in topLevel.Nodes)
            {
                if (tn.Tag != null && typeof(TableLayoutPanel).IsInstanceOfType(tn.Tag))
                {
                    XmlElement panel = xDoc.CreateElement("panel");
                    panel.SetAttribute("name", tn.Text);
                    settings.AppendChild(panel);
                    XmlElement controls = xDoc.CreateElement("controls");
                    panel.AppendChild(controls);
                    foreach (Control c in ((TableLayoutPanel)tn.Tag).Controls)
                    {
                        if (!typeof(Label).IsInstanceOfType(c))
                        {
                            int firstPos = c.ToString().IndexOf(',');
                            int lastPos = c.ToString().Substring(0, firstPos).LastIndexOf('.');
                            string ctrlType = c.ToString().Substring(0, firstPos).Substring(lastPos + 1).Trim();
                            string ctrlValue = string.Empty;
                            if (ctrlType.ToLower().Equals("textbox"))
                            {
                                ctrlValue = "Text: " + c.Text;
                            }
                            else
                            {
                                ctrlValue = c.ToString().Substring(firstPos + 1).Trim();
                            }
                            XmlElement ctrl = xDoc.CreateElement("control");
                            ctrl.SetAttribute("name", c.Name);
                            ctrl.SetAttribute("type", ctrlType);
                            ctrl.InnerText = ctrlValue;
                            controls.AppendChild(ctrl);
                        }
                    }
                }
            }
            XmlDeclaration declare = xDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xDoc.PrependChild(declare);
            _values = xDoc.OuterXml;
            return _values;
        }
    }

    public class OptionsClosingEvent : EventArgs
    {
        private DialogResult _dr;
        private string _values;
        public OptionsClosingEvent(DialogResult dr, string values)
        {
            _dr = dr;
            _values = values;
        }

        public DialogResult Response
        {
            get { return _dr; }
        }

        public string Values
        {
            get { return _values; }
        }
    }

    public static class OptionValues
    {
        public static Type GetTypeFromAppDomain(string name, string ns)
        {
            Type type = null;
            string fullName = ns + "." + name;
            Assembly system = null;

            // Check if loaded in AppDomain
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // See if the assembly is already loaded
                type = assembly.GetType(fullName);

                // If found, then done
                if (type != null)
                    break;

                // Check for System assembly
                if ("mscorlib" == assembly.GetName().Name.ToLower())
                    system = assembly;
            }

            // If not found, then check System
            if ((null == type) && (null != system))
            {
                type = system.GetType("System." + name);
            }

            return type;
        }

        public static object GetValue(string Name, string settings)
        {
            string value = null;
            object retVal = null;
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(settings);
            XmlNode ctrl = xDoc.SelectSingleNode("/options/settings/panel/controls/control[@name='" + Name + "']");
            if (ctrl != null)
            {
                string[] ctrlItems = ctrl.InnerText.Split(':');
                if (ctrlItems != null)
                {
                    string type = ctrl.Attributes["type"].Value;
                    if (type != null)
                    {
                        string property = ctrlItems.Length > 0 ? ctrlItems[0].Trim() : null;
                        if (ctrlItems.Length > 1)
                        {
                            for (int ii = 1; ii < ctrlItems.Length; ii++)
                            {
                                bool addColon = ii < ctrlItems.Length - 1;
                                value += ctrlItems[ii].Trim() + (addColon ? ":" : "");
                            }
                        }
                        Type t = OptionValues.GetTypeFromAppDomain(type, "System.Windows.Forms");
                        if (t != null)
                        {
                            TypeConverter tc = TypeDescriptor.GetConverter(t.GetProperty(property).PropertyType);
                            if (tc != null && tc.CanConvertFrom(typeof(string)))
                            {
                                retVal = tc.ConvertFrom(value);
                            }
                        }
                    }
                }
            }
            return retVal == null ? value : retVal;
        }
    }
}