using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Text;

namespace ScrumFactory.Services.Clients.AuthProviders {

    //[Export(typeof(Services.AuthProviders.IOAuthProvider))]
    public class LiveAuthorizationProvider : BaseOAuthProvider, Services.AuthProviders.IOAuthProvider {

        [Import]
        protected override IClientHelper ClientHelper { get; set; }

       

        public override string ProviderName {
            get {
                return "Windows Live";
            }
        }

        protected override string ClientId {
            get {
                return "0000000044041929";
            }
        }

        protected override string ClientSecret {
            get {
                return String.Empty;
            }
        }

        public override string LoginUrl {
            get {
                return "https://login.live.com/oauth20_authorize.srf?client_id=" + ClientId + "&scope=wl.offline_access,wl.emails&response_type=code&redirect_uri=" + RedirectUrl;
            }
        }

        protected override string RedirectUrl {
            get {
                return "https://login.live.com/oauth20_desktop.srf";
            }
        }

        protected override string TokenUrl {
            get {
                return "https://login.live.com/oauth20_token.srf";
            }
        }

        
        public string ProviderImageUrl {
            get {
                return @"\Images\AuthorizationProviders\Live.png";
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
            ACCESS_TOKEN = null;
            REFRESH_TOKEN = null;

            Properties.Settings.Default.Save();
            return true;

        }


    }
}
