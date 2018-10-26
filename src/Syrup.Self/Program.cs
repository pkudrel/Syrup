using System;
using System.Windows.Forms;
using Syrup.Self.Parts.Globals;
using Syrup.Self.Parts.Log;

namespace Syrup.Self
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Logger log;
            Global g;
            try
            {
                log = Logger.Instance;
                g = Global.Instance;
            }
            catch (Exception e)
            {
                ShowError(e);
                return;
            }
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
                log.Save();
            }
            catch (Exception e)
            {
                var msg = ShowError(e);
                log.Error(msg);
                log.Save();
            }
        }

        private static string ShowError(Exception e)
        {
            var innerMsg = e.InnerException?.Message ?? string.Empty;
            var msg = e.Message + Environment.NewLine + innerMsg;
            MessageBox.Show(msg, "Critical error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return msg;
        }
    }
}