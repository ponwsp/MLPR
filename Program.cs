using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MLPR
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
       {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {



                Writeerrorlogfile("logApplicationunhanding: " + args.ExceptionObject);
                //Application.Restart();
                Environment.Exit(0);
                //System.Diagnostics.Process.Start(Application.StartupPath + @"\Restart.bat");






            };
            Bosch.VideoSDK.Core s_core;
            Bosch.VideoSDK.GCALib.ISecurityProperties m_SecProperty;
            s_core = new Bosch.VideoSDK.Core();
            m_SecProperty = (Bosch.VideoSDK.GCALib.ISecurityProperties)s_core;

            m_SecProperty.SecurityProperties = 63;
            s_core = null;




            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
        public static bool AnotherInstanceExists()
        {
            //Thread.Sleep(2000);
            Process currentRunningProcess = Process.GetCurrentProcess();
            Process[] listOfProcs = Process.GetProcessesByName(currentRunningProcess.ProcessName);

            foreach (Process proc in listOfProcs)
            {
                if ((proc.MainModule.FileName == currentRunningProcess.MainModule.FileName) && (proc.Id != currentRunningProcess.Id))

                {
                    return true;
                }
                Console.WriteLine(proc.ProcessName.ToString());

            }

            return false;
        }
        private static void Writeerrorlogfile(string message)
        {



            //find log is exist or not
            string curfile = Application.StartupPath + @"\LOG\logerrorstatus" + DateTime.Now.ToString("dd_MM_yyyy") + ".txt";
            if (!File.Exists(curfile))
            {
                using (TextWriter tw = new StreamWriter(curfile))
                {

                    tw.WriteLine(DateTime.Now.ToString("HH:mm:ss:ffff") + " : " + message);
                };
            }
            else
            {
                using (StreamWriter sw = File.AppendText(curfile))
                {
                    sw.WriteLine(DateTime.Now.ToString("HH:mm:ss:ffff") + " : " + message);
                }
            }







        }
    }
}
