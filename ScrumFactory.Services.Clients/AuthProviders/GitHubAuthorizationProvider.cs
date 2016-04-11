using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Json;
using System.Runtime.Serialization;

namespace ScrumFactory.Services.Clients.AuthProviders {

    [Export(typeof(Services.AuthProviders.IOAuthProvider))]
    public class GitHubAuthorizationProvider : BaseOAuthProvider, Services.AuthProviders.IOAuthProvider {

        [Import]
        protected override IClientHelper ClientHelper { get; set; }

        [Import]
        private ILogService log { get; set; }

        public override Services.AuthProviders.TokenGet LoginType {
            get {
                return Services.AuthProviders.TokenGet.CLIENT_SIDE;
            }
        }


        public override string ProviderName {
            get {
                return "GitHub";
            }
        }

        protected override string ClientId {
            get {
                return "b405b1173b5d6e2837bd";
            }
        }

        protected override string ClientSecret {
            get {
                return "39ba7b52e5dbf3c54cc3fbb9513108dc62c89a27";
            }
        }

        public override string LoginUrl {
            get {
                throw new NotSupportedException();
            }
        }

        protected override string RedirectUrl {
            get {
                throw new NotSupportedException();
            }
        }

        protected override string TokenUrl {
            get {
                throw new NotSupportedException();
            }
        }
     
        public string ProviderImageUrl {
            get {
                return @"\Images\AuthorizationProviders\github.png";
            }
        }

       

        public override bool GetAuthorizationToken(Uri url, string title) {
            throw new NotSupportedException();
        }


        public override bool GetAuthorizationToken(string user, string pass) {

            var data = new GitAuthenticationRequest() { client_id = ClientId, client_secret = ClientSecret, note = "scrum factory login", scopes = "public_repo" };

            string _auth = string.Format("{0}:{1}", user, pass);
            string _enc = Convert.ToBase64String(Encoding.UTF8.GetBytes(_auth));

            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Add("user-agent", "SF-Client");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _enc);

            var response = client.Post("https://api.github.com/authorizations", new ObjectContent<GitAuthenticationRequest>(data, JsonMediaTypeFormatter.DefaultMediaType));

            if (!response.IsSuccessStatusCode) {
                string r = response.Content.ReadAsString();
                log.LogText(r);
                return false;
            }

            dynamic obj = response.Content.ReadAs<JsonObject>();

            AUTH_TOKEN = obj.token;
            ACCESS_TOKEN = obj.token;
            REFRESH_TOKEN = null;
            Properties.Settings.Default.Save();

            return true;

        }

        


    }

    [DataContract]
    public class GitAuthenticationRequest {
        [DataMember]
        public string client_id { get; set; }
        [DataMember]
        public string client_secret { get; set; }
        [DataMember]
        public string note { get; set; }
        [DataMember]
        public string scopes { get; set; }
    }
}
