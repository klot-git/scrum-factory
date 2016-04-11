using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScrumFactory.Services;
using System.IO;
using System.IO.IsolatedStorage;
using System.ComponentModel.Composition;

namespace ScrumFactory.Services.Clients {

    [Export(typeof(ILogService))]
    public class LogServiceClient : Services.ILogService {

        public void LogError(Exception ex) {

            try {
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain, null, null);

                IsolatedStorageFileStream oStream = new IsolatedStorageFileStream("factoryErrorLog.txt", FileMode.Append, FileAccess.Write, isoStore);

                StreamWriter writer = new StreamWriter(oStream);
                writer.WriteLine(DateTime.Now);
                writer.WriteLine(ex.Message);

                if (ex is System.Reflection.TargetInvocationException && ex.InnerException != null) {
                    writer.WriteLine("INNER: " + ex.InnerException.Message);
                    writer.WriteLine(ex.InnerException.StackTrace.ToString());
                } else
                    writer.WriteLine(ex.StackTrace.ToString());

                writer.WriteLine("------------------------------------------------------------------------------");
                writer.Close();
            } catch (Exception) { }

        }

        public void LogText(string text) {
            try {
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain, null, null);

                IsolatedStorageFileStream oStream = new IsolatedStorageFileStream("factoryErrorLog.txt", FileMode.Append, FileAccess.Write, isoStore);

                StreamWriter writer = new StreamWriter(oStream);
                writer.WriteLine(DateTime.Now);
                writer.WriteLine(text);

                writer.WriteLine("------------------------------------------------------------------------------");
                writer.Close();
            } catch (Exception) { }
        }
    }
}
