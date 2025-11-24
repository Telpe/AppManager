using AppManager.Core.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace AppManager.Core
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Create and run the tray application
            using (var trayApp = new TrayApplication())
            {
                Application.Run();
            }
        }
    }

    
}
