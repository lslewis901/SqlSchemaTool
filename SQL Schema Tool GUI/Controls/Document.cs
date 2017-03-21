using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

using Lewis.SST.AsyncMethods;
using Lewis.SST.Controls;
using Lewis.SST.Gui;

using NLog;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;

using WeifenLuo.WinFormsUI.Docking;

namespace Lewis.SST.Controls
{
    public class Document : DockContent
    {
        #region private variable declarations

        private Regex _findNextRegex;
        private int _findNextStartPos = 0;
        private FindValueDlg fdlg = new FindValueDlg();

        #endregion

        #region variable declarations, visible to this and the child classes

        protected static SortedList SelectedCompareDocs = new SortedList();
        protected string m_fileName = string.Empty;
        protected string m_DialogTitle = null;
        protected string m_DialogTypeFilter = null;
        protected bool _textChanged = false;
        protected TextEditorControl txtEditCtl;
        protected string searchPattern = string.Empty;
        protected DialogResult dr = DialogResult.OK;
        protected static Logger logger = LogManager.GetLogger("Lewis.SST.Controls");
        protected string _initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        #endregion

        #region public methods

        public string InitialDirectory
        {
            get { return _initialDirectory; }
            set { _initialDirectory = value != null ? value : _initialDirectory; }
        }

        public string GetText()
        {
            return txtEditCtl.Text;
        }

        public void Undo()
        {
            if (txtEditCtl != null)
            {
                txtEditCtl.Undo();
            }
        }

        public void Redo()
        {
            if (txtEditCtl != null)
            {
                txtEditCtl.Redo();
            }
        }

        public void Paste()
        {
            if (txtEditCtl != null)
            {
                txtEditCtl.ActiveTextAreaControl.TextArea.ClipboardHandler.Paste(null, null);
            }
        }

        public void Copy()
        {
            if (txtEditCtl != null)
            {
                txtEditCtl.ActiveTextAreaControl.TextArea.ClipboardHandler.Copy(null, null);
            }
        }

        public void Cut()
        {
            if (txtEditCtl != null)
            {
                txtEditCtl.ActiveTextAreaControl.TextArea.ClipboardHandler.Cut(null, null);
            }
        }

        public void Find(bool display)
        {
            if (txtEditCtl != null)
            {
                if (txtEditCtl.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText.Length > 0)
                {
                    fdlg.FindValue = txtEditCtl.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText;
                }
                else
                {
                    fdlg.FindValue = fdlg.FindValue == null || fdlg.FindValue.Length == 0 ? string.Empty : fdlg.FindValue;
                }
                fdlg.ReplaceFlag = false;
                fdlg.Text = "Find...";
                
                if (display)
                {
                    dr = fdlg.ShowDialog(this);
                }
                if (fdlg.FindValue.Length == 0) return;

                if (dr == DialogResult.OK)
                {
                    searchPattern = fdlg.FindValue;
                    searchPattern = searchPattern.Replace("[", @"\[").Replace("]", @"\]").Replace("(", @"\(").Replace(")", @"\)");

                    try
                    {
                        _findNextRegex = new Regex(searchPattern, RegexOptions.IgnoreCase);
                        _findNextStartPos = Find(_findNextRegex, _findNextStartPos);
                    }
                    catch (Exception ex)
                    {
                        logger.DebugException("Error in regular expression for Find.", ex);
                    }
                }
            }
        }

        public void Replace()
        {
            if (txtEditCtl != null)
            {
                if (txtEditCtl.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText.Length > 0)
                {
                    fdlg.FindValue = txtEditCtl.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText;
                }
                else
                {
                    fdlg.FindValue = string.Empty;
                }
                
                fdlg.ReplaceFlag = true;
                dr = fdlg.ShowDialog(this);
                if (fdlg.FindValue.Length == 0) return;

                if (dr == DialogResult.OK)
                {
                    string replacement = fdlg.ReplaceValue;
                    string searchPattern = fdlg.FindValue;
                    searchPattern = searchPattern.Replace("[", @"\[").Replace("]", @"\]").Replace("(", @"\(").Replace(")", @"\)");

                    try
                    {
                        _findNextRegex = new Regex(searchPattern, RegexOptions.IgnoreCase);
                        _findNextStartPos = Replace(_findNextRegex, _findNextStartPos, replacement);
                    }
                    catch (Exception ex)
                    {
                        logger.DebugException("Error in regular expression for Find.", ex);
                    }
                    finally
                    {
                        TextChanged = true;
                    }
                }
            }
        }

        public void ReplaceAll()
        {
            if (txtEditCtl != null)
            {
                if (txtEditCtl.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText.Length > 0)
                {
                    fdlg.FindValue = txtEditCtl.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText;
                }
                else
                {
                    fdlg.FindValue = string.Empty;
                }
                fdlg.ReplaceFlag = true;
                DialogResult dr = fdlg.ShowDialog(this);
                if (fdlg.FindValue.Length == 0) return;

                if (dr == DialogResult.OK)
                {
                    string replacement = fdlg.ReplaceValue;
                    string searchPattern = fdlg.FindValue;
                    _findNextRegex = new Regex(searchPattern, RegexOptions.IgnoreCase);
                    TextUtils.ReplaceAll(txtEditCtl, _findNextRegex, replacement);
                    TextChanged = true;
                    _findNextStartPos = 0;
                    TextUtils.SetPosition(txtEditCtl, _findNextStartPos);
                }
            }
        }

        #endregion

        #region custom event handler(s) for this and child classes

        public event EventHandler<EventArgs> DocumentChanged;

        private void OnDocumentChanged()
        {
            if (_textChanged && !TabText.EndsWith("*"))
            {
                m_fileName += "*";
                TabText += "*";
                Text += "*";

                EventHandler<EventArgs> handler = DocumentChanged;
                if (handler != null)
                {
                    // raises the event. 
                    handler(this, new EventArgs());
                }
            }
        }

        #endregion

        #region events to be processed in this class or the child class

        private void txtEditCtl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F && e.Control || e.KeyCode == Keys.F3)
            {
                // fine method call
                if (e.KeyCode == Keys.F && e.Control)
                {
                    Find(true);
                }
                else
                {
                    Find(false);
                }
            }
            else if (e.KeyCode == Keys.H && e.Control)
            {
                // find and replace method
                Replace();
            }
            else if (e.KeyCode == (Keys.Back | Keys.Space) || e.KeyCode == (Keys.LButton | Keys.MButton | Keys.Space) || e.KeyCode == (Keys.LButton | Keys.RButton | Keys.MButton | Keys.Space) || e.KeyCode == (Keys.RButton | Keys.MButton | Keys.Space))
            {
                // NOP, these are arrow keys so ignore
            }
            else if ((!e.Alt && !e.Control && e.KeyValue >= 32 && e.KeyValue < 97) || (e.Control && e.KeyValue == 68) || (e.Control && e.KeyValue == 88) || (!e.Alt && !e.Control && e.KeyCode == Keys.Back))
            {
                TextChanged = true;
            }
        }

        private void Document_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (TextChanged)
            {
                DialogResult dr = MessageBox.Show("There are changes to this document.  Do you want to save the changes?", "Save File?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (dr == DialogResult.Cancel )
                {
                    e.Cancel = true;
                    return;
                }
                else if (dr == DialogResult.Yes)
                {
                    if (SaveDocument(true) == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
            if (txtEditCtl != null)
            {
                txtEditCtl.ActiveTextAreaControl.TextArea.KeyEventHandler -= new ICSharpCode.TextEditor.KeyEventHandler(TextArea_KeyEventHandler);
                txtEditCtl.ActiveTextAreaControl.TextArea.KeyUp -= new System.Windows.Forms.KeyEventHandler(txtEditCtl_KeyUp);
            }
            this.FormClosing -= new FormClosingEventHandler(Document_FormClosing);
        }

        private bool TextArea_KeyEventHandler(char ch)
        {
            TextChanged = true;
            return false;
        }

        #endregion

        #region override methods

        protected override string GetPersistString()
        {
            string retval = string.Empty;
            try
            {
                retval = GetType().ToString() + "," + m_fileName.Replace("*", "") + "," + TabText.Replace("*", "");
            }
            catch { }
            return retval;
        }

        #endregion

        #region protected methods that are common to child classes

        protected void SetTextEditorDefaultProperties()
        {
            if (txtEditCtl != null)
            {
                txtEditCtl.TextEditorProperties.EnableFolding = true;
                txtEditCtl.TextEditorProperties.ShowEOLMarker = false;
                txtEditCtl.TextEditorProperties.ShowTabs = false;
                txtEditCtl.TextEditorProperties.ShowSpaces = false;
                txtEditCtl.TextEditorProperties.ShowVerticalRuler = false;
                txtEditCtl.TextEditorProperties.UseAntiAliasedFont = true;
            }
        }

        protected virtual void WireEvents()
        {
            if (txtEditCtl != null)
            {
                txtEditCtl.ActiveTextAreaControl.TextArea.KeyEventHandler += new ICSharpCode.TextEditor.KeyEventHandler(TextArea_KeyEventHandler);
                txtEditCtl.ActiveTextAreaControl.TextArea.KeyUp += new System.Windows.Forms.KeyEventHandler(txtEditCtl_KeyUp);
            }
            this.FormClosing += new FormClosingEventHandler(Document_FormClosing);
        }

        protected void ForceFoldingUpdate(string fileName)
        {
            if (txtEditCtl != null && txtEditCtl.TextEditorProperties.EnableFolding)
            {
                txtEditCtl.Document.FoldingManager.UpdateFoldings(fileName, null);
            }
        }

        #endregion

        #region private methods

        private int Find(Regex regex, int startPos)
        {
            if (txtEditCtl != null)
            {
                string context = txtEditCtl.Text.Substring(startPos);
                Match m = regex.Match(context);
                if (!m.Success)
                {
                    MessageBox.Show("The specified text was not found.", "Find", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return 0;
                }
                int wordStart = TextUtilities.FindNextWordStart(txtEditCtl.Document, m.Index);
                int line = txtEditCtl.Document.GetLineNumberForOffset(m.Index + startPos);
                txtEditCtl.ActiveTextAreaControl.TextArea.ScrollTo(line);

                TextUtils.SelectText(txtEditCtl, m.Index + startPos, m.Length);
                _findNextRegex = regex;
                _findNextStartPos = m.Index + startPos;

                TextUtils.SetPosition(txtEditCtl, m.Index + m.Length + startPos);
                return m.Index + m.Length + startPos;
            }
            return 0;
        }

        private int Replace(Regex regex, int startPos, string replaceWith)
        {
            if (txtEditCtl != null)
            {
                if (txtEditCtl.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText.Length > 0)
                {
                    int start = txtEditCtl.ActiveTextAreaControl.SelectionManager.SelectionCollection[0].Offset;
                    int length = txtEditCtl.ActiveTextAreaControl.TextArea.SelectionManager.SelectedText.Length;
                    txtEditCtl.Document.Replace(start, length, replaceWith);

                    return Find(regex, length + start);
                }

                string context = txtEditCtl.Text.Substring(startPos);

                Match m = regex.Match(context);

                if (!m.Success)
                {
                    MessageBox.Show("The specified text was not found.", "Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return 0;
                }
                txtEditCtl.Document.Replace(m.Index + startPos, m.Length, replaceWith);
                txtEditCtl.Refresh();
                TextUtils.SetPosition(txtEditCtl, m.Index + replaceWith.Length + startPos);
                return m.Index + replaceWith.Length + startPos;
            }
            return 0;
        }

        private bool LoadDocument()
        {
            bool result = false;
            if (m_fileName == null || m_fileName == string.Empty)
            {
                ArrayList arl = ShowOpenFileDialog(m_fileName, m_DialogTitle, m_DialogTypeFilter, InitialDirectory);
                DialogResult dr = (DialogResult)arl[0];
                if (dr == DialogResult.OK)
                {
                    m_fileName = (string)arl[1];
                    FileInfo fi = new FileInfo(m_fileName);
                    InitialDirectory = fi.DirectoryName;
                    result = true;
                }
            }
            return result;
        }

        private DialogResult SaveDocument(bool showDialog)
        {
            bool isHtml = false;
            if (this is HTMLDoc)
            {
                isHtml = true;
            }
            if (!isHtml && txtEditCtl == null) return DialogResult.Ignore;
            DialogResult dr = DialogResult.OK;
            if (m_fileName == null || m_fileName == string.Empty)
            {
                showDialog = true;
            }
            else
            {
                m_fileName = m_fileName.Replace("*", "");
            }
            if (showDialog)
            {
                ArrayList arl = ShowSaveFileDialog(m_fileName, m_DialogTitle, m_DialogTypeFilter, InitialDirectory);
                dr = (DialogResult)arl[0];
                if (dr == DialogResult.OK)
                {
                    m_fileName = ((string)arl[1]).Replace("*", "");
                }
            }
            if (dr == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                if (!isHtml && txtEditCtl != null)
                {
                    txtEditCtl.SaveFile(m_fileName);
                }
                else if (isHtml)
                {
                    string html = ((HTMLDoc)this).HTMLText;
                    File.WriteAllText(m_fileName, html);
                }

                InitialDirectory = Path.GetDirectoryName(m_fileName);
                this.TabText = Path.GetFileName(m_fileName);
                this.Text = Path.GetFileName(m_fileName);
                TextChanged = false;
                Cursor.Current = Cursors.Default;
            }
            return dr;
        }

        #endregion

        #region public virtual methods that normally would be overridden
        /// <summary>
        /// Normally this method is to be overridden in the inheriting class
        /// The method will cause a open file dialog to appear if the FileName 
        /// property is null or empty.
        /// Override this method to change the open file dialog - title and filter
        /// </summary>
        /// <param name="dockPanel"></param>
        /// <returns></returns>
        public virtual bool Open(DockPanel dockPanel)
        {
            return LoadDocument();
        }

        // provides this method to be overridden in the inheriting class
        public virtual void Save(bool showDialog)
        {
            SaveDocument(showDialog);
        }

        public virtual string FileName { get { return m_fileName; } set { m_fileName = value; } }

        #endregion

        #region new methods/properties that hide existing methods of the same name in the parent class

        public new bool TextChanged
        {
            get { return _textChanged; }
            set
            {
                _textChanged = value;
                OnDocumentChanged();
            }
        }

        #endregion
    }
}
