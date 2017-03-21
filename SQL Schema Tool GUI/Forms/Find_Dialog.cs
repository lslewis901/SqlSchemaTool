using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Lewis.SST.Gui
{
	/// <summary>
	/// Summary description for Find.
	/// </summary>
	public class FindValueDlg : System.Windows.Forms.Form
	{
		private string _Find = string.Empty;
		private bool _replaceFlag = false;
		private string _Replace = string.Empty;
		private ArrayList ar_prevFind = new ArrayList();
		private ArrayList ar_prevReplace = new ArrayList();

		private System.Windows.Forms.Button btn_GO;
		private System.Windows.Forms.ComboBox cbo_FindValue;
		private System.Windows.Forms.Button btn_Cancel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cbo_ReplaceValue;
		private System.Windows.Forms.Label lbl_replace;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="Find_Dialog"/> class.
		/// </summary>
		public FindValueDlg()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FindValueDlg));
            this.btn_GO = new System.Windows.Forms.Button();
            this.cbo_FindValue = new System.Windows.Forms.ComboBox();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lbl_replace = new System.Windows.Forms.Label();
            this.cbo_ReplaceValue = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // btn_GO
            // 
            this.btn_GO.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_GO.BackColor = System.Drawing.Color.Transparent;
            this.btn_GO.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_GO.Location = new System.Drawing.Point(168, 103);
            this.btn_GO.Name = "btn_GO";
            this.btn_GO.Size = new System.Drawing.Size(64, 24);
            this.btn_GO.TabIndex = 2;
            this.btn_GO.Text = "Ok";
            this.btn_GO.UseVisualStyleBackColor = false;
            this.btn_GO.Click += new System.EventHandler(this.btn_GO_Click);
            // 
            // cbo_FindValue
            // 
            this.cbo_FindValue.Location = new System.Drawing.Point(16, 24);
            this.cbo_FindValue.Name = "cbo_FindValue";
            this.cbo_FindValue.Size = new System.Drawing.Size(288, 21);
            this.cbo_FindValue.TabIndex = 0;
            this.cbo_FindValue.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cbo_FindValue_KeyUp);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Location = new System.Drawing.Point(240, 103);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(64, 24);
            this.btn_Cancel.TabIndex = 3;
            this.btn_Cancel.Text = "Cancel";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(288, 16);
            this.label1.TabIndex = 3;
            this.label1.Text = "Enter phrase or word to search for:";
            // 
            // lbl_replace
            // 
            this.lbl_replace.Location = new System.Drawing.Point(16, 56);
            this.lbl_replace.Name = "lbl_replace";
            this.lbl_replace.Size = new System.Drawing.Size(288, 16);
            this.lbl_replace.TabIndex = 5;
            this.lbl_replace.Text = "Enter phrase or word to replace with:";
            this.lbl_replace.Visible = false;
            // 
            // cbo_ReplaceValue
            // 
            this.cbo_ReplaceValue.Location = new System.Drawing.Point(16, 72);
            this.cbo_ReplaceValue.Name = "cbo_ReplaceValue";
            this.cbo_ReplaceValue.Size = new System.Drawing.Size(288, 21);
            this.cbo_ReplaceValue.TabIndex = 1;
            this.cbo_ReplaceValue.Visible = false;
            // 
            // FindValueDlg
            // 
            this.AcceptButton = this.btn_GO;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(312, 134);
            this.Controls.Add(this.lbl_replace);
            this.Controls.Add(this.cbo_ReplaceValue);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.cbo_FindValue);
            this.Controls.Add(this.btn_GO);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FindValueDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Find";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Find_Dialog_Load);
            this.ResumeLayout(false);

		}
		#endregion

		private void btn_GO_Click(object sender, System.EventArgs e)
		{
			_Find = this.cbo_FindValue.Text;
			if (!ar_prevFind.Contains(_Find))
			{
				ar_prevFind.Add(_Find);
			}
			_Replace = this.cbo_ReplaceValue.Text;
			if (!ar_prevReplace.Contains(_Find))
			{
				ar_prevReplace.Add(_Find);
			}
		}

		private void cbo_FindValue_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				btn_GO.PerformClick();
			}
		}

		private void Find_Dialog_Load(object sender, System.EventArgs e)
		{
            this.cbo_FindValue.Items.Clear();
            if (_Find != null && _Find != string.Empty)
            {
                if (!ar_prevFind.Contains(_Find))
                {
                    ar_prevFind.Add(_Find);
                    this.cbo_FindValue.Items.AddRange(ar_prevFind.ToArray());
                    this.cbo_FindValue.SelectedIndex = 0;
                }
            }
            else
            {
                this.cbo_FindValue.Items.Add(string.Empty);
                this.cbo_FindValue.Items.AddRange(ar_prevFind.ToArray());
                this.cbo_FindValue.SelectedIndex = 0;
            }
			this.cbo_ReplaceValue.Items.AddRange( ar_prevReplace.ToArray() );
			this.lbl_replace.Visible = false;
			this.cbo_ReplaceValue.Visible = false;
			this.Height = 112;
			if (_replaceFlag)
			{
				this.Text += " and Replace";
				this.Height = 160;
				this.lbl_replace.Visible = true;
				this.cbo_ReplaceValue.Visible = true;
			}
		}

		/// <summary>
		/// Sets a value indicating whether [replace flag].
		/// </summary>
		/// <value><c>true</c> if [replace flag]; otherwise, <c>false</c>.</value>
		public bool ReplaceFlag
		{
			set {_replaceFlag = value;}
		}

		/// <summary>
		/// Gets the replace value.
		/// </summary>
		/// <value>The replace value.</value>
		public string ReplaceValue
		{
			get{return _Replace;}
		}

		/// <summary>
		/// Gets the find value.
		/// </summary>
		/// <value>The find value.</value>
		public string FindValue
		{
			get
			{
				return _Find;
			}
            set
            {
                _Find = value;
            }
		}
	}
}
