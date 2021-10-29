using System;
using System.Linq;
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
        internal static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();
            mainForm = new();
            if (args.Contains("invc"))
                mainForm.invincible = true;
            Application.Run(mainForm);
        }
    }
}
