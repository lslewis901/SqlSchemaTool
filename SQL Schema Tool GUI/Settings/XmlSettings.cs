using Lewis.SST.Gui;
using Lewis.SST.SQLMethods;

using NLog;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Lewis.SST.Settings
{
    public static class XmlSettings
    {
        private static Logger logger = LogManager.GetLogger("Lewis.SST.Settings");

        public static void WriteXml(string fileName, CtrlSettings settings)
        {
            if (fileName != null && fileName != string.Empty && settings != null)
            {
                try
                {
                    TextWriter writer = new StreamWriter(fileName, false);
                    XmlSerializer ser = new XmlSerializer(typeof(CtrlSettings));
                    ser.Serialize(writer, settings);
                    ser = null;
                    writer.Close();
                    writer.Dispose();
                }
                catch (Exception ex)
                {
                    logger.Error(SQLSchemaTool.ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
                }
            }
        }

        public static CtrlSettings ReadXml(string fileName)
        {
            // Create a TextReader to read the file if it exists. 
            CtrlSettings cs = new CtrlSettings();
            if (fileName != null && fileName != string.Empty)
            {
                try
                {
                    FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);
                    TextReader reader = new StreamReader(fs);
                    XmlSerializer ser = new XmlSerializer(typeof(CtrlSettings));
                    cs = (CtrlSettings)ser.Deserialize(reader);

                    ser = null;
                    fs.Close();
                    reader.Close();
                    fs.Dispose();
                    reader.Dispose();
                }
                catch (Exception ex)
                {
                    logger.Error(SQLSchemaTool.ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
                }
            }
            return cs;
        }

        public static CtrlSettings GetSettings(Form form)
        {
            Lewis.SST.Settings.CtrlSettings cs = null;
            if (form != null)
            {
                try
                {
                    cs = new Lewis.SST.Settings.CtrlSettings(form.GetType().ToString());
                    if (typeof(Lewis.SST.Gui.Main).IsInstanceOfType(form))
                    {
                        cs.LastDirectory = ((Lewis.SST.Gui.Main)form).LastDirectory;
                    }
                    cs.Name = form.Name;
                    cs.Location = form.Location;
                    cs.Size = form.Size;
                    ArrayList arl = WalkControls(form.Controls);
                    if (arl != null && arl.Count > 0)
                    {
                        cs.ChildCtrlsToPersist = (Lewis.SST.Settings.CtrlSettings[])arl.ToArray(typeof(Lewis.SST.Settings.CtrlSettings));
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(SQLSchemaTool.ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
                }
            }
            return cs;
        }

        public static CtrlSettings GetSettings(optionsFormProperties ofp)
        {
            Lewis.SST.Settings.CtrlSettings cs = new Lewis.SST.Settings.CtrlSettings(typeof(optionsFormProperties).ToString());
            if (ofp != null)
            {
                try
                {
                    cs.Name = ofp.Name;
                    cs.Location = ofp.Location;
                    cs.Size = ofp.Size;
                }
                catch (Exception ex)
                {
                    logger.Error(SQLSchemaTool.ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
                }
                
            }
            return cs;
        }

        public static void SetSettings(ref Form form, CtrlSettings cs)
        {
            if (form != null && cs != null && form.GetType().ToString().Equals(cs.Type))
            {
                if (form.Name.Equals(cs.Name))
                {
                    if (typeof(Lewis.SST.Gui.Main).IsInstanceOfType(form))
                    {
                        ((Lewis.SST.Gui.Main)form).LastDirectory = cs.LastDirectory;
                    }
                    form.Location = cs.Location;
                    form.Size = cs.Size;
                    if (cs.ChildCtrlsToPersist.Length > 0)
                    {
                        SetControls(form.Controls, cs);
                    }
                }
            }
        }

        private static void SetControls(Control.ControlCollection cc, CtrlSettings cs)
        {
            if (cc != null && cs != null)
            {
                foreach (Control c1 in cc)
                {
                    foreach (CtrlSettings c in cs.ChildCtrlsToPersist)
                    {
                        if (typeof(ToolStripPanel).IsInstanceOfType(c1))
                        {
                            ToolStripPanel ts = (ToolStripPanel)c1;
                            foreach (Control c2 in ts.Controls)
                            {
                                if (c2.GetType().ToString().Equals(c.Type))
                                {
                                    if (c2.Name.Equals(c.Name))
                                    {
                                        if (ts.Dock.ToString().Equals(c.CtrlValue))
                                        {
                                            c2.Location = c.Location;
                                            c2.Size = c.Size;
                                        }
                                        else
                                        {
                                            foreach (Control c3 in ts.Parent.Controls)
                                            {
                                                if (typeof(ToolStripPanel).IsInstanceOfType(c3))
                                                {
                                                    ToolStripPanel ts2 = (ToolStripPanel)c3;
                                                    if (!ts2.Equals(ts))
                                                    {
                                                        if (ts2.Dock.ToString().Equals(c.CtrlValue))
                                                        {
                                                            ts.Controls.Remove(c2);
                                                            c2.Location = c.Location;
                                                            c2.Size = c.Size;
                                                            ts2.Controls.Add(c2);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (typeof(SQLServerExplorer).IsInstanceOfType(c1))
                    {
                        SQLServerExplorer sse = (SQLServerExplorer)c1;
                        foreach (CtrlSettings c in cs.ChildCtrlsToPersist)
                        {
                            if (typeof(SQLConnection).ToString().Equals(c.Type))
                            {
                                string[] ss = c.CtrlValue.Split(new char[] { '\\' });
                                bool savePWD = false;
                                if (ss.Length > 1)
                                {
                                    savePWD = Convert.ToBoolean(ss[1]);
                                }
                                sse.SQLConnections.AddEncrypted(ss[0], "LLEWIS55", savePWD);
                            }
                        }
                    }
                    SetControls(c1.Controls, cs);
                }
            }
        }

        private static ArrayList WalkControls(Control.ControlCollection cc)
        {
            ArrayList arl = new ArrayList();
            if (cc != null)
            {
                foreach (Control c1 in cc)
                {
                    if (typeof(ToolStripPanel).IsInstanceOfType(c1))
                    {
                        ToolStripPanel ts = (ToolStripPanel)c1;
                        foreach (Control c in ts.Controls)
                        {
                            arl.Add(new Lewis.SST.Settings.CtrlSettings(c.Name, c.Location, c.Size, ts.Dock.ToString(), c.GetType().ToString()));
                        }
                    }
                    if (typeof(SQLServerExplorer).IsInstanceOfType(c1))
                    {
                        SQLServerExplorer sse = (SQLServerExplorer)c1;
                        foreach (SQLConnection sql in sse.SQLConnections)
                        {
                            arl.Add(new Lewis.SST.Settings.CtrlSettings(sql.Server, sql.EncryptedConnectionString + "\\" + sql.SavePassword.ToString(), sql.GetType().ToString()));
                        }
                    }
                    arl.AddRange(WalkControls(c1.Controls));
                }
            }
            return arl;
        }
    }

    [Serializable]
    public class CtrlSettings
    {
        private Point _location;
        private Size _size;
        private string _name;
        private string _ctrlValue;
        private CtrlSettings[] _childCtrlsToPersist;
        private string _type;
        private string _lastDirectory;

        public CtrlSettings()
        {
        }

        public CtrlSettings(string type)
        {
            _type = type;
        }

        public CtrlSettings(String name, Point location, Size size, string type)
        {
            _name = name;
            _location = location;
            _size = size;
            _type = type;
        }

        public CtrlSettings(String name, string value, string type)
        {
            _name = name;
            _ctrlValue = value;
            _type = type;
        }

        public CtrlSettings(String name, Point location, Size size, string value, string type)
        {
            _name = name;
            _location = location;
            _size = size;
            _ctrlValue = value;
            _type = type;
        }

        public CtrlSettings(String name, Point location, Size size, string value, CtrlSettings[] childCtrls)
        {
            _name = name;
            _location = location;
            _size = size;
            _ctrlValue = value;
            _childCtrlsToPersist = childCtrls;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string LastDirectory
        {
            get { return _lastDirectory; }
            set { _lastDirectory = value; }
        }

        public Point Location
        {
            get { return _location; }
            set { _location = value; }
        }

        public Size Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public CtrlSettings[] ChildCtrlsToPersist
        {
            get { return _childCtrlsToPersist; }
            set { _childCtrlsToPersist = value; }
        }

        public string CtrlValue
        {
            get { return _ctrlValue; }
            set { _ctrlValue = value; }
        }

    }
}
