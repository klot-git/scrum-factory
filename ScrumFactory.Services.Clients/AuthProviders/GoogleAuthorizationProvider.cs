using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Text;

namespace ScrumFactory.Services.Clients.AuthProviders {

    [Export(typeof(Services.AuthProviders.IOAuthProvider))]
    public class GoogleAuthorizationProvider : BaseOAuthProvider, Services.AuthProviders.IOAuthProvider {

        [Import]
        protected override IClientHelper ClientHelper { get; set; }

        


        public override string ProviderName {
            get {
                return "Google";
            }
        }

        protected override string ClientId {
            get {
                return "587852127580.apps.googleusercontent.com";
            }
        }

        protected override string ClientSecret {
            get {
                return "oh6Y9mmzbdkzwoeN1i5s92_y";
            }
        }

        private string Scope {
            get {
                string scopes = "https://www.googleapis.com/auth/userinfo.email+https://www.googleapis.com/auth/userinfo.profile";
                foreach (ScrumFactory.Services.AuthProviders.IOAuthScope scope in Scopes)
                    scopes = scopes + "+" + scope.ScopeName;

                return scopes;                
            }
        }

        public override string LoginUrl {
            get {
                return "https://accounts.google.com/o/oauth2/auth?client_id=" + ClientId + "&scope=" + Scope + "&response_type=code&redirect_uri=" + RedirectUrl;
            }
        }

        protected override string RedirectUrl {
            get {
                return "urn:ietf:wg:oauth:2.0:oob";
            }
        }

        protected override string TokenUrl {
            get {
                return "https://accounts.google.com/o/oauth2/token";
            }
        }


        public string ProviderImageUrl {
            get {
                return @"\Images\AuthorizationProviders\Google.png";
            }
        }

       

        public override bool GetAuthorizationToken(Uri url, string title) {
            if (!title.StartsWith("Success code="))
                return false;

            AUTH_TOKEN = title.Replace("Success code=", "");
            ACCESS_TOKEN = null;
            REFRESH_TOKEN = null;
            Properties.Settings.Default.Save();

            return true;

        }


    }
}
