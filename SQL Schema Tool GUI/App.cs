using NLog;

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Lewis.SST.Gui
{
    static class App
    {
        private static Logger logger = LogManager.GetLogger("Lewis.SST.Gui");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Main());
            }
            catch(Exception ex)
            {
                MessageBox.Show(string.Format("Application Error: {0}\nNeed to halt the application!\nCheck the application log for the exact error.", ex.Message), "APPLICATION ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                logger.Error(string.Format("Application Error: {0}\n{1}", ex.Message, ex.StackTrace));
            }
        }
    }
}