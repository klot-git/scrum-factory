using System;
using System.Text;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Json;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace ScrumFactory.Services.Clients.AuthProviders {

    public abstract class BaseOAuthProvider : Services.AuthProviders.IOAuthProvider {

        protected abstract string ClientId { get; }
        protected abstract string ClientSecret { get; }
        protected abstract string TokenUrl { get; }
        protected abstract string RedirectUrl { get; }

        protected abstract IClientHelper ClientHelper { get; set; }

        public abstract string ProviderName { get; }
        public abstract string LoginUrl { get; }

        public virtual Services.AuthProviders.TokenGet LoginType {
            get {
                return Services.AuthProviders.TokenGet.SERVER_SIDE;
            }
        }

        public string ACCESS_TOKEN {
            get {
                return Properties.Settings.Default.oaut_access_token;
            }
            protected set {
                Properties.Settings.Default.oaut_access_token = value;
            }
        }

        protected string AUTH_TOKEN {
            get {
                return Properties.Settings.Default.oaut_auth_token;
            }
            set {
                Properties.Settings.Default.oaut_auth_token = value;
            }
        }

        protected string REFRESH_TOKEN {
            get {
                return Properties.Settings.Default.oaut_refresh_token;
            }
            set {
                Properties.Settings.Default.oaut_refresh_token = value;
            }
        }

        public virtual bool IsSignedIn {
            get {
                return !String.IsNullOrEmpty(ACCESS_TOKEN);
            }
        }

        public virtual bool GetAccesToken() {

            if (String.IsNullOrEmpty(AUTH_TOKEN))
                return false;

            ACCESS_TOKEN = null;

            System.Net.Http.HttpClient wc = ClientHelper.GetClient();
            string url = TokenUrl;

            string postData = "code={0}&client_id={1}&redirect_uri={2}&grant_type={3}&client_secret={4}";
            postData = string.Format(postData,
                System.Uri.EscapeDataString(AUTH_TOKEN),
                ClientId,
                System.Uri.EscapeDataString(RedirectUrl),
                "authorization_code",
                ClientSecret);


            System.Net.Http.HttpResponseMessage msg;
            try {
                msg = wc.Post(url, new System.Net.Http.StringContent(postData, Encoding.Default, "application/x-www-form-urlencoded"));
            } catch(Exception) { return false; }

            if (msg.StatusCode != HttpStatusCode.OK)
                return false;

            dynamic obj = msg.Content.ReadAs<JsonObject>();

            ACCESS_TOKEN = obj.access_token;
            REFRESH_TOKEN = obj.refresh_token;

            Properties.Settings.Default.Save();


            return true;

        }

        public virtual bool RefreshAccesToken() {

            if (String.IsNullOrEmpty(REFRESH_TOKEN))
                return false;

            System.Net.Http.HttpClient wc = ClientHelper.GetClient();
            string url = TokenUrl;

            string postData = "refresh_token={0}&client_id={1}&redirect_uri={2}&grant_type={3}&client_secret={4}";
            postData = string.Format(postData,
                System.Uri.EscapeDataString(REFRESH_TOKEN),
                ClientId,
                System.Uri.EscapeDataString(RedirectUrl),
                "refresh_token",
                ClientSecret);

            System.Net.Http.HttpResponseMessage msg;
            try {
                msg = wc.Post(url, new System.Net.Http.StringContent(postData, Encoding.Default, "application/x-www-form-urlencoded"));
            } 
            catch (Exception) { return false; }

            if (msg.StatusCode != HttpStatusCode.OK)
                return false;

            dynamic obj = msg.Content.ReadAs<JsonObject>();

            ACCESS_TOKEN = obj.access_token;

            Properties.Settings.Default.Save();

            return true;

        }

        public virtual bool GetAuthorizationToken(string user, string pass) {
            throw new NotSupportedException();
        }

        public virtual void SignOut() {
            ACCESS_TOKEN = null;
            REFRESH_TOKEN = null;
            AUTH_TOKEN = null;
            Properties.Settings.Default.Save();
        }

        public abstract bool GetAuthorizationToken(Uri url, string title);

        [ImportMany(typeof(ScrumFactory.Services.AuthProviders.IOAuthScope))]
        private IEnumerable<ScrumFactory.Services.AuthProviders.IOAuthScope> allScopes { get; set; }

        protected ICollection<ScrumFactory.Services.AuthProviders.IOAuthScope> Scopes {
            get {
                return allScopes.Where(s => s.ProviderName == ProviderName).ToList();
            }
        }

    }
}
