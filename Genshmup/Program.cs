using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;

namespace Genshmup
{
    internal static class Program
    {
        public static MainForm? mainForm;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            mainForm = new();
            Application.Run(mainForm);
        }
    }
}
