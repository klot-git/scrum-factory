using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Text;

namespace ScrumFactory.Services.Clients.AuthProviders {

    [Export(typeof(Services.AuthProviders.IOAuthProvider))]
    public class AzureAuthorizationProvider : BaseOAuthProvider, Services.AuthProviders.IOAuthProvider {

        [Import]
        protected override IClientHelper ClientHelper { get; set; }


        public string ProviderFriendlyName {
            get {
                return "Microsoft account";
            }
        }

        public override string ProviderName {
            get {
                return "Azure";
            }
        }

        protected override string ClientId {
            get {
                return "aea877c0-d609-4e43-aade-8832a346fa10";
            }
        }

        protected override string ClientSecret {
            get {
                return String.Empty;
            }
        }

        public override string LoginUrl {
            get {                
                return "https://login.microsoftonline.com/common/oauth2/v2.0/authorize?prompt=login&client_id=" + ClientId + "&redirect_uri=" + RedirectUrl + "&response_type=code&scope=user.read openid email profile";
            }
        }

        protected override string RedirectUrl {
            get {
                return "https://login.live.com/oauth20_desktop.srf";
            }
        }

        protected override string TokenUrl {
            get {
                return "https://login.microsoftonline.com/common/oauth2/v2.0/token";
            }
        }


        public new string ProviderImageUrl {
            get {
                return @"\Images\AuthorizationProviders\Live.png";
            }
        }

        public string ProviderSymbol {
            get {
                return ((char)0xf17a).ToString();
            }
        }

        public string ProviderColor {
            get {
                return "Blue";
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

            //Properties.Settings.Default.Save();
            return true;

        }

        public override void SignOut()
        {
            base.SignOut();
            var url = "https://login.microsoftonline.com/common/oauth2/logout?post_logout_redirect_uri=" + RedirectUrl;
            System.Net.Http.HttpClient wc = ClientHelper.GetClient();
            System.Net.Http.HttpResponseMessage msg;
            try
            {
                msg = wc.GetAsync(url).Result;
            }
            catch (Exception) {
            }

        }


    }
}
