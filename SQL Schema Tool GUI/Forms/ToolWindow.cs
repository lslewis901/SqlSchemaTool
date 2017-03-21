using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Lewis.SST.Gui
{
    public partial class ToolWindow : DockContent
    {
        public ToolWindow()
        {
            InitializeComponent();
            option1ToolStripMenuItem.Click += new EventHandler(option1ToolStripMenuItem_Click);
            option2ToolStripMenuItem.Click += new EventHandler(option2ToolStripMenuItem_Click);
            option3ToolStripMenuItem.Click += new EventHandler(option3ToolStripMenuItem_Click);
            option4ToolStripMenuItem.Click += new EventHandler(option4ToolStripMenuItem_Click);
            // TODO: add dropdown menu options for window, such as floating, dockable, tabbed document, auto hide, and hide
            //contextMenuStrip1.Enabled = false;
        }

        void option4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            option4ToolStripMenuItem.Checked = false; //Hide
            if (this.IsDockStateValid(DockState.Hidden))
            {
                this.DockState = DockState.Hidden; 
            }
        }

        void option3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            option3ToolStripMenuItem.Checked = false; //Auto Hide
            if (this.DockState == DockState.DockLeft)
            {
                if (this.IsDockStateValid(DockState.DockLeftAutoHide))
                {
                    this.DockState = DockState.DockLeftAutoHide;
                }
            }
            if (this.DockState == DockState.DockRight)
            {
                if (this.IsDockStateValid(DockState.DockRightAutoHide))
                {
                    this.DockState = DockState.DockRightAutoHide;
                }
            }
            if (this.DockState == DockState.DockTop)
            {
                if (this.IsDockStateValid(DockState.DockTopAutoHide))
                {
                    this.DockState = DockState.DockTopAutoHide;
                }
            }
            if (this.DockState == DockState.DockBottom)
            {
                if (this.IsDockStateValid(DockState.DockBottomAutoHide))
                {
                    this.DockState = DockState.DockBottomAutoHide;
                }
            }
        }

        void option2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!option2ToolStripMenuItem.Checked) //Dockable            
            {
                if (this.IsDockStateValid(DockState.Document))
                {
                    this.DockState = DockState.Document;
                }
                else { option2ToolStripMenuItem.Checked = true; };
            }
        }

        void option1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            option1ToolStripMenuItem.Checked = false; //Floating
            if (this.IsDockStateValid(DockState.Float))
            {
                this.DockState = DockState.Float; 
            }
        }

    }
}