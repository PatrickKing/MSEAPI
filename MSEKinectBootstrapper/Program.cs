using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MSEKinect;

namespace MSEKinectBootstrapper
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            MSEKinectManager km = new MSEKinectManager();
            km.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
