using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace Lewis.SST.Controls
{
    public sealed class TextUtils
    {
        public static void SelectText(TextEditorControl tec, int startPos, int length)
        {
            Point startPoint = tec.ActiveTextAreaControl.TextArea.Document.OffsetToPosition(startPos);
            Point endPoint = tec.ActiveTextAreaControl.TextArea.Document.OffsetToPosition(startPos + length);
            tec.ActiveTextAreaControl.TextArea.SelectionManager.SetSelection(startPoint, endPoint);
        }

        public static void ReplaceAll(TextEditorControl tec, Regex regex, string replaceWith)
        {
            string context = tec.Text;
            tec.Text = regex.Replace(tec.Text, replaceWith);
        }

        public static void SetPosition(TextEditorControl tec, int pos)
        {
            TextArea textArea = tec.ActiveTextAreaControl.TextArea;
            textArea.Caret.Position = tec.Document.OffsetToPosition(pos);
        }

        public static void SetLine(TextEditorControl tec, int line)
        {
            TextArea textArea = tec.ActiveTextAreaControl.TextArea;
            textArea.Caret.Column = 0;
            textArea.Caret.Line = line;
            textArea.Caret.UpdateCaretPosition();
        }
    }
}
