using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MSEKinect;
using System.Diagnostics;

namespace MSEKinectBootstrapper
{
    static class Program
    {
        private static TraceSource _source = new TraceSource("MSEKinectBootstrapper");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            _source.TraceEvent(TraceEventType.Start, 0, "MSEKinectBootstrapper");

            MSEKinectManager km = new MSEKinectManager();
            km.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
