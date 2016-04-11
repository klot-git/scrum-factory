using System;
using System.ComponentModel.Composition;
using System.Deployment.Application;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace ScrumFactory.Services.Clients {

    [Export(typeof(IClientHelper))]
    public class ClientHelper : ScrumFactory.Services.IClientHelper {

        private System.Net.Http.HttpClientHandler httpHandle;

        public string ClientVersion {
            get {
                string version = "DEV";                
                if (ApplicationDeployment.IsNetworkDeployed)
                    version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                return version;
            }
        }

        public string UserAgent {
            get {
                return "SFClient-" + ClientVersion;
            }
        }

        public System.Net.Http.HttpClient GetClient() {
            return GetClient(null);
        }

        public System.Net.Http.HttpClient GetClient(IAuthorizationService authService) {
            ConfigureHttpHandle();

            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient(httpHandle);

            if (authService != null) {
                if (authService.SignedMemberProfile == null)
                    throw new ScrumFactory.Exceptions.NotAuthorizedException();

                client.DefaultRequestHeaders.AddWithoutValidation("Authorization", authService.SignedMemberProfile.AuthorizationProvider + " auth=" + authService.SignedMemberToken);
            }

            // user agent
            client.DefaultRequestHeaders.Add("UserAgent", UserAgent);

            // max response buffer
            client.MaxResponseContentBufferSize = 1024 * 1024 * 10;
            
            return client;

        }


        public void HandleHTTPErrorCode(System.Net.Http.HttpResponseMessage response, bool ignoreNotFound = false) {

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return;

            string content = response.Content.ReadAsString().Replace("\"", "");

            LogServerError(content, response.StatusCode);

            if (response.StatusCode == System.Net.HttpStatusCode.HttpVersionNotSupported) {                
                throw new ScrumFactory.Exceptions.VersionMissmatchException(content, ClientVersion);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) {                
                if (content.StartsWith("BRE_"))
                    throw new ScrumFactory.Exceptions.BusinessRuleViolationException(content);

               
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) {
                if (ignoreNotFound) {
                    return;
                }
                throw new ScrumFactory.Exceptions.NotFoundException();
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) {
                throw new ScrumFactory.Exceptions.ForbittenException();
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                throw new ScrumFactory.Exceptions.NotAuthorizedException();
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadGateway
                || response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout) {
                throw new ScrumFactory.Exceptions.NetworkException();
            }

            throw new Exceptions.ServerErrorException();
        }


        public void ConfigureHttpHandle() {

            if (httpHandle != null)
                return;

            // create handle
            httpHandle = new System.Net.Http.HttpClientHandler();
            
            // configure proxy
            httpHandle.Proxy = GetProxy();

            // max request buffer
            httpHandle.MaxRequestContentBufferSize = 1024 * 1024 * 10;

            
            

        }

        private bool proxyCached = false;
        private System.Net.IWebProxy proxy = null;

        public System.Net.IWebProxy GetProxy() {

            if (!proxyCached) {
                proxy = System.Net.WebRequest.DefaultWebProxy;
                proxyCached = true;
            }

            return proxy;

            //System.Net.IWebProxy proxy = null;

            //// configure proxy
            //Microsoft.Win32.RegistryKey internetSettingsKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings");
            //int? proxyEnable = internetSettingsKey.GetValue("ProxyEnable") as int?;
            //string proxyServer = internetSettingsKey.GetValue("ProxyServer") as string;
            //string proxyOverride = internetSettingsKey.GetValue("ProxyOverride") as string;
            //bool byPassLocal = "<local>".Equals(proxyOverride);

            //if (proxyEnable.HasValue && proxyEnable.Value == 1)
            //    proxy = new System.Net.WebProxy(proxyServer, byPassLocal, null, System.Net.CredentialCache.DefaultCredentials);

            //return proxy;
        }

        private void LogServerError(string response, System.Net.HttpStatusCode code) {

            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain, null, null);

            IsolatedStorageFileStream oStream = new IsolatedStorageFileStream("factoryErrorLog.txt", FileMode.Append, FileAccess.Write, isoStore);

            StreamWriter writer = new StreamWriter(oStream);
            writer.WriteLine(DateTime.Now);
            writer.WriteLine("SERVER RESPONSE: " + code);
            writer.WriteLine(response);

            writer.WriteLine("------------------------------------------------------------------------------");
            writer.Close();

        }

        public HttpResponseMessage SafePut<T>(object lockObj, System.Net.Http.HttpClient client, System.Uri url, T data) {            
            HttpResponseMessage response;
            lock (lockObj) {
                response = client.Put(url, new ObjectContent<T>(data, JsonValueMediaTypeFormatter.DefaultMediaType));
            }
            return response;
        }

        public HttpResponseMessage SafePost<T>(object lockObj, System.Net.Http.HttpClient client, System.Uri url, T data) {
            HttpResponseMessage response;
            lock (lockObj) {
                response = client.Post(url, new ObjectContent<T>(data, JsonValueMediaTypeFormatter.DefaultMediaType));
            }
            return response;
        }

        public HttpResponseMessage SafeGet(object lockObj, System.Net.Http.HttpClient client, System.Uri url) {
            HttpResponseMessage response;
            lock (lockObj) {
                response = client.Get(url);
            }
            return response;
        }
    }
}
