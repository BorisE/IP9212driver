using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using System.Globalization;
using System.Threading;

namespace ASCOM.TestForm
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            var cul = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TestFrm());
        }
    }
}
