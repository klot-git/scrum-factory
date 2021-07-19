using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScrumFactory.Exceptions {

    public class VersionMissmatchException : ScrumFactoryException {

        

        public VersionMissmatchException(string serverVersion, string clientVersion) : base("VERSION_MISSMATCH") {
            
            ServerVersion = serverVersion;
            ClientVersion = clientVersion;
            CompareServerVersion();
        }

        public string ServerVersion { get; private set; }
        public string ClientVersion { get; private set; }

        public bool IsServerVersionNewer { get; private set; }

        
        private void CompareServerVersion() {

            IsServerVersionNewer = false;
            
            // no server version (SHOULD NEVER OCCUR)
            if (ServerVersion == null) {
                IsServerVersionNewer = false;
                return;
            }

            // no server version (SHOULD NEVER OCCUR)
            if (ClientVersion == null) {
                IsServerVersionNewer = true;
                return;
            }

            // split versions parts
            string[] verServer = ServerVersion.Split('.');
            string[] verClient = ClientVersion.Split('.');

            // compare first part
            int f1 = Compare(verServer[0], verClient[0]);
            if (f1 > 0) {
                IsServerVersionNewer = true;
                return;
            }
            else if(f1 < 0) {
                IsServerVersionNewer = false;
                return;
            }
            
            // compare second part
            int f2 = Compare(verServer[1], verClient[1]);
            if (f2 > 0) {
                IsServerVersionNewer = true;
                return;
            }

        }

        private int Compare(string s1, string s2) {
            int i1 = 0;
            int i2 = 0;
            int.TryParse(s1, out i1);
            int.TryParse(s2, out i2);            
            
            if (i1 > i2)
                return 1;
            if (i1 < i2)
                return -1;

            return 0;
        }
    }
}
