using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Globalization;
using System.IO;

namespace SCW_APP
{
    class LogClass
    {
        private static void Log(string logMessage)
        {
            string logPath = AppDomain.CurrentDomain.BaseDirectory 
                + ConfigurationManager.AppSettings.Get("LogPath");  // path

            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            string filename = logPath
                + DateTime.Today.ToString("yyyy-MM-dd", new CultureInfo("en-US")) + ".log";     // filename

            using (StreamWriter w = File.AppendText(filename))
            {
                w.WriteLine("[{0}]  {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("en-US")), logMessage);
            }
        }

        public static void writeApplicationName()
        {
            Log("===================================================================");
            Log(AppDomain.CurrentDomain.FriendlyName);
        }

        public static void writeLogMethod(string message)
        {
            Log("  " + message);
        }

        public static void writeLogDetail(string message)
        {
            Log("    " + message);
        }

        public static void writeResponse(string message)
        {
            Log("------------------------");
            Log("Response : " + message + "\n");
        }

    }
}
