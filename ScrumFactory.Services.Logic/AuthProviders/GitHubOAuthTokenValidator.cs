using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScrumFactory.Services.AuthProviders;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Json;


namespace ScrumFactory.Services.Logic.AuthProviders {

    [Export(typeof(IOAuthServerTokenValidator))]
    public class GitHubOAuthTokenValidator : OAuthBaseTokenValidator, IOAuthServerTokenValidator {


        private string CLIENT_ID = "b405b1173b5d6e2837bd";
        private string CLIENT_SECRET = "39ba7b52e5dbf3c54cc3fbb9513108dc62c89a27";


        public override string ProviderName {
            get { return "GitHub"; }
        }

        public override bool ValidateAccessToken(string token) {
            string _auth = string.Format("{0}:{1}", CLIENT_ID, CLIENT_SECRET);
            string _enc = Convert.ToBase64String(Encoding.UTF8.GetBytes(_auth));


            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Add("user-agent", "SF-Client");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _enc);

            var response = client.Get(ValidateUrl + token);

            if (response.StatusCode != System.Net.HttpStatusCode.OK) {
                return false;
            }

            client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Add("user-agent", "SF-Client");

            response = client.Get("https://api.github.com/user?access_token=" + token);
            dynamic user = response.Content.ReadAs<JsonObject>();

            SetMemberInfo(user);
            return true;
        }

        public override string ValidateUrl {
            get {
                return "https://api.github.com/applications/" + CLIENT_ID + "/tokens/";
            }
        }
         
        public override void SetMemberInfo(dynamic info) {
            MemberInfo = new MemberProfile();
            MemberInfo.MemberUId = info.email;
            MemberInfo.EmailAccount = info.email;
            MemberInfo.FullName = info.name;
            MemberInfo.CompanyName = info.company;
        }
    }
}
