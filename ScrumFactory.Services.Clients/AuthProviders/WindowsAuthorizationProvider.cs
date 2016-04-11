using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Text;

namespace ScrumFactory.Services.Clients.AuthProviders {

    [Export(typeof(Services.AuthProviders.IOAuthProvider))]
    public class WindowsAuthorizationProvider : BaseOAuthProvider, Services.AuthProviders.IOAuthProvider {

        [Import]
        protected override IClientHelper ClientHelper { get; set; }

        [Import]
        private IServerUrl ServerUrl { get; set; }

   

        public override string ProviderName {
            get {
                return "Windows Authentication";
            }
        }

        protected override string ClientId {
            get {
                return "0";
            }
        }

        protected override string ClientSecret {
            get {
                return String.Empty;
            }
        }

        public override string LoginUrl {
            get {
                return ServerUrl.Url + "/WindowsAuth/default.aspx";
            }
        }

        protected override string RedirectUrl {
            get {
                return String.Empty; ;
            }
        }

        protected override string TokenUrl {
            get {
                return ServerUrl.Url + "/WindowsOAuth/token.aspx";
            }
        }

        
        public string ProviderImageUrl {
            get {
                return @"\Images\AuthorizationProviders\windows.png";
            }
        }

        public override bool GetAuthorizationToken(Uri url, string title) {

            string query = url.Query;
            if (!query.StartsWith("?code="))
                return false;

            string code = query.Replace("?code=", "");

            if (code.IndexOf("&") > 0)
                code = code.Substring(0, code.IndexOf("&"));

            AUTH_TOKEN = code;
            ACCESS_TOKEN = code;
            REFRESH_TOKEN = null;

            Properties.Settings.Default.Save();
            return true;
        }

        public override bool GetAccesToken() {
            return String.IsNullOrEmpty(ACCESS_TOKEN);
        }

        public override bool RefreshAccesToken() {
            return false;
        }


    }
}
