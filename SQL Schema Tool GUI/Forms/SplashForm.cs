
#region using directives
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Threading;

//using SSK.SSKClient.UIHelper;
//using SSK.SSKClient.ClientStorage;
#endregion

namespace Lewis.SST.Gui
{
    /// <summary>
    /// Summary description for SplashForm.
    /// </summary>
    public class SplashForm : System.Windows.Forms.Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        //the timer used to close the splash form and turn over control to the main form on event
        private System.Windows.Forms.Timer timer;

        //the timer used to fade in and out the splash form
        private System.Windows.Forms.Timer fadetimer;

        //this is the variable that determines the fade increment for opacity
        static double _fade;

        //this value will be used as the timer tick for the fadetimer
        static int _fadetimerinterval;

        //the holder for status text that's to be displayed at the bottom of the splash screen
        static string statusText = null;

        //how long to keep the window open for
        static int _timeout;

        //a reference to the form itself
        static SplashForm _splashForm = null;
        private PictureBox pictureBox1;

        //label for the starter kit text

        //used for multi threading
        static Thread _splashthread = null;
        private Label label1;
        private Label label2;

        //a flag to determine if the form has a dialog box showing
        static bool _showDialogBox = false;



        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout">
        /// the number of ms before the form automatically closes.  Pass in 0 to disable this.
        /// </param>
        public SplashForm(int timeout)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            timer = new System.Windows.Forms.Timer();


            //don't start the timer is 0 passed in
            if (timeout > 0)
            {
                timer.Start();
                timer.Interval = timeout;
                timer.Tick += new EventHandler(closeSplash);
            }

            fadetimer = new System.Windows.Forms.Timer();

            fadetimer.Interval = SplashForm._fadetimerinterval;


            fadetimer.Tick += new EventHandler(fadeTimerHandler);
            fadetimer.Start();


            this.Opacity = 0;

            this.TopLevel = true;

            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
        }

        /// <summary>
        /// This is used to show the splash form using a seperate thread.  
        /// This makes it possible to load in the background on the main thread
        /// and update your user with status on the splash screen
        /// </summary>
        /// <param name="timeout">The number of ms before the form automatically closes.  Pass in 0 to disable this and close manually.</param>
        /// <param name="fade">A  value representing the change in opacity to make for each tick of the faderTimer.</param>
        /// <param name="fadertimerinterval">The interval in ms for the fader timer.  Each tick of the fader timer changes the opacity by the amount specified in "fade"</param>
        static public void Showasyncsplash(int timeout, double fade, int fadertimerinterval)
        {
            SplashForm._timeout = timeout;
            SplashForm._fade = fade;
            SplashForm._fadetimerinterval = fadertimerinterval;
            
            //make sure we only launch the form once
            if (_splashForm == null)
            {
                _splashthread = new Thread(new ThreadStart(SplashForm.ShowSplash));
                _splashthread.IsBackground = true;
                _splashthread.SetApartmentState(ApartmentState.STA);
                _splashthread.Start();
            }
            else
            {
                return;
            }
        }

        static public void ShowDialogBox(string message)
        {
            _showDialogBox = true;
            MessageBox.Show(message);
            _showDialogBox = false;
            closeAsyncSplash();
        }

        static public void closeAsyncSplash()
        {
            if (!_showDialogBox)
            {
                if (_splashForm != null && _splashForm.IsDisposed == false && SplashForm._fade > 0)
                {
                    //if the timer expires then reverse the fade constant
                    SplashForm._fade = -SplashForm._fade;
                }

                _splashthread = null;
                _splashForm = null;
            }
        }

        /// <summary>
        /// show the splash form on the new thread
        /// </summary>
        static public void ShowSplash()
        {
            _splashForm = new SplashForm(SplashForm._timeout);

            Application.Run(_splashForm);
        }

        /// <summary>
        /// set the private variable to the status text
        /// </summary>
        /// <param name="text">The status text to be displayed.</param>
        static public string StatusText
        {
            set
            {
                SplashForm.statusText = value;
            }
            get
            {
                return SplashForm.statusText;
            }
        }

        /// <summary>
        /// return a reference of the splash form
        /// </summary>
        static public Form FormRef
        {
            get
            {
                return _splashForm;
            }
        }

        /// <summary>
        /// Override the OnPaint event to draw the background
        /// </summary>
        /// <param name="pea">The OnPaint event arguments</param>
        protected override void OnPaint(PaintEventArgs pea)
        {

            DrawSplashFooterText(statusText, pea.Graphics, ClientSize.Width, ClientSize.Height, this.pictureBox1.Height);
        }

        /// <summary>
        /// draws a gradient text string based on the arguments from the OnPaint event
        /// </summary>
        /// <param name="grfx">grfx from the onpaint event</param>
        /// <param name="cx">form width</param>
        /// <param name="cy">form height</param>
        private static void DrawSplashFooterText(string text, Graphics grfx, int cx, int cy, int height)
        {
            Font font = new Font("Tahoma", 12, FontStyle.Bold);
            SizeF sizef = grfx.MeasureString(text, font);
            PointF ptf = new PointF((cx - sizef.Width) / 2,
                (cy + height) / 2);

            RectangleF rectf = new RectangleF(ptf, sizef);

            LinearGradientBrush lgbrush = new LinearGradientBrush(rectf,
                Color.Red, Color.Orange,
                LinearGradientMode.ForwardDiagonal);

            grfx.DrawString(text, font, lgbrush, ptf);
            font.Dispose();

        }





        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashForm));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(300, 224);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Consolas", 20.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Firebrick;
            this.label1.Location = new System.Drawing.Point(318, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(240, 32);
            this.label1.TabIndex = 1;
            this.label1.Text = "SQL Schema Tool";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Consolas", 20.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.IndianRed;
            this.label2.Location = new System.Drawing.Point(318, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(210, 32);
            this.label2.TabIndex = 2;
            this.label2.Text = "Lewis && Lewis";
            // 
            // SplashForm
            // 
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(567, 441);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.DoubleBuffered = true;
            this.Name = "SplashForm";
            this.ShowInTaskbar = false;
            this.Text = "SQL Schema Tool";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.SplashForm_Closing);
            this.Load += new System.EventHandler(this.SplashForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion



        /// <summary>
        /// This is the event handler for the timer.
        /// It closes the splash form and stops the event timer.
        /// </summary>
        /// <param name="myObject">event object</param>
        /// <param name="myEventArgs">Event args</param>
        private void closeSplash(Object myObject, EventArgs myEventArgs)
        {

            timer.Stop();
            this.Close();

        }

        /// <summary>
        /// This is the event handler for the Fadetimer.
        /// It changes the opacity of the form
        /// </summary>
        /// <param name="myObject">event object</param>
        /// <param name="myEventArgs">Event args</param>
        private void fadeTimerHandler(Object myObject, EventArgs myEventArgs)
        {

            this.Opacity += _fade;

            if (this.Opacity <= 0)
            {
                this.Close();
            }


        }

        /// <summary>
        /// This event catches the splash form closing event, and changes the sign
        /// of the fade constant and cancels the close event.  When the fade constant reaches 0 (completely transparent), the form will
        /// be allowed to close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SplashForm_Closing(object sender, CancelEventArgs e)
        {
            //don't close the form if there is a dialog box showing
            if (_showDialogBox)
            {
                
                e.Cancel = true;
                return;
            }

            //if the timer expires then reverse the fade constant
            if (_fade > 0)
            {
                _fade = -_fade;
                e.Cancel = true;
            }
        }



        private void SplashForm_Load(object sender, System.EventArgs e)
        {

        }


    }
}
