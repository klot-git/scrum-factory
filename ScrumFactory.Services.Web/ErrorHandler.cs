using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace ScrumFactory.Services.Web {

    public class ErrorHandler : System.ServiceModel.Dispatcher.IErrorHandler {

        public bool HandleError(Exception error) {
            LogError(error);
            return false;
        }

        public void ProvideFault(Exception error, System.ServiceModel.Channels.MessageVersion version, ref System.ServiceModel.Channels.Message fault) {
            LogError(error);
            return;
        }


        private void LogError(Exception ex) {

            try {
                string logPath = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/log.txt");


                // checks if the log is bigger than 1MB, and if so, overwrite it
                System.IO.FileMode mode = FileMode.Append;
                FileInfo info = new FileInfo(logPath);
                if (info.Length > 900000) {
                    mode = FileMode.Create;
                }

                System.IO.FileStream fstream = new FileStream(logPath, mode);



                StreamWriter writer = new StreamWriter(fstream);
                writer.WriteLine(DateTime.Now);
                writer.WriteLine(ex.Message);

                if (ex.InnerException != null) {
                    writer.WriteLine("INNER: " + ex.InnerException.Message);
                    writer.WriteLine(ex.InnerException.StackTrace.ToString());
                }
                else
                    writer.WriteLine(ex.StackTrace.ToString());
                
                writer.WriteLine("------------------------------------------------------------------------------");
                writer.Close();
            }
            catch (Exception e2) {
                string m = e2.Message;
            }

        }
    }
}