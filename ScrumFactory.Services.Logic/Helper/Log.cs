using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ScrumFactory.Services.Logic.Helper {

    public class Log {

        private static string logFilePath {
            get {
                System.Web.HttpContext context = System.Web.HttpContext.Current;
                if (context == null)
                    return null;
                return context.Server.MapPath("~/App_Data/log.txt");
            }
        }

        

        public static void LogMessage(string message) {
            try {

                if (logFilePath == null)
                    return;

                // checks if the log is bigger than 1MB, and if so, overwrite it
                System.IO.FileMode mode = FileMode.Append;
                FileInfo info = new FileInfo(logFilePath);
                if (info.Length > 900000) {
                    mode = FileMode.Create;
                }

                System.IO.FileStream fstream = new FileStream(logFilePath, mode);

                StreamWriter writer = new StreamWriter(fstream);
                writer.WriteLine(DateTime.Now);
                writer.WriteLine(message);                
                writer.WriteLine("------------------------------------------------------------------------------");
                writer.Close();
            }
            catch (Exception) { }
        }

        public static void LogError(Exception ex) {

            try {

                if (logFilePath == null)
                    return;

                // checks if the log is bigger than 1MB, and if so, overwrite it
                System.IO.FileMode mode = FileMode.Append;
                FileInfo info = new FileInfo(logFilePath);
                if (info.Length > 900000) {
                    mode = FileMode.Create;
                }


                System.IO.FileStream fstream = new FileStream(logFilePath, mode);

                StreamWriter writer = new StreamWriter(fstream);
                writer.WriteLine(DateTime.Now);
                writer.WriteLine(ex.Message);
                writer.WriteLine(ex.StackTrace.ToString());
                writer.WriteLine("------------------------------------------------------------------------------");
                writer.Close();
            }
            catch (Exception) { }

        }

    }
}
