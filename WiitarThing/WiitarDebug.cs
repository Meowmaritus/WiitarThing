using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace WiinUSoft
{
    class WiitarDebug
    {
        public static void Log(string message)
        {
#if DEBUG
            var utcDate = DateTime.Now.ToUniversalTime();
            string date = $"{utcDate.Hour:D2}:{utcDate.Minute:D2}:{utcDate.Second:D2}.{utcDate.Millisecond:D3}";
            string logFilePath = Frankenpath(new FileInfo(Application.ResourceAssembly.Location).DirectoryName, "WiitarLog.log");

            using (var logFile = File.Open(logFilePath, FileMode.Append, FileAccess.Write))
            {
                using (var logFileWriter = new StreamWriter(logFile))
                {
                    logFileWriter.WriteLine($"[{date}] {message}");
                }
            }
#endif
        }

        public static string Frankenpath(params string[] pathParts)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < pathParts.Length; i++)
            {
                sb.Append(pathParts[i].Trim('\\'));
                if (i < pathParts.Length - 1)
                    sb.Append('\\');
            }

            return sb.ToString();
        }

    }
}
