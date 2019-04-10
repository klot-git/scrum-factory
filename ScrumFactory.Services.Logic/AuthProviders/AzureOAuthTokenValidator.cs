using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScrumFactory.Services.AuthProviders;
using System.ComponentModel.Composition;
using System.Net;
using System.Net.Http;
using System.Json;

namespace ScrumFactory.Services.Logic.AuthProviders {

    [Export(typeof(IOAuthServerTokenValidator))]
    public class AzureOAuthTokenValidator : OAuthBaseTokenValidator,  IOAuthServerTokenValidator {

        public override string ProviderName {
            get { return "Azure"; }
        }


        public  override  string ValidateUrl {
            get {                
                return "https://graph.microsoft.com/v1.0/me";
                // return "https://login.microsoftonline.com/9188040d-6c67-4c5b-b112-36a304b66dad/v2.0";
            }
        }

        public override void SetMemberInfo(dynamic info) {
            MemberInfo = new MemberProfile();
            MemberInfo.MemberUId = info["userPrincipalName"];
            MemberInfo.EmailAccount = info["userPrincipalName"];
            MemberInfo.FullName = info["displayName"];
        }

        public override bool ValidateAccessToken(string token)
        {

            if (!IsProviderEnabled())
                throw new ScrumFactory.Exceptions.AuthorizationProviderNotSupportedException();


            System.Net.Http.HttpClientHandler handler = new System.Net.Http.HttpClientHandler();
            handler.Proxy = System.Net.WebRequest.DefaultWebProxy;
            System.Net.Http.HttpClient wc = new System.Net.Http.HttpClient(handler);

            wc.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            System.Net.Http.HttpResponseMessage msg = null;
            try
            {
                msg = wc.Get(new Uri(ValidateUrl));
            }
            catch (Exception ex)
            {
                throw new System.ServiceModel.Web.WebFaultException<string>(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }

            if (msg.StatusCode == HttpStatusCode.Unauthorized)
            {
                var err = msg.Content.ReadAsString();
                return false;
            }

            if (msg.StatusCode != HttpStatusCode.OK)
                throw new System.ServiceModel.Web.WebFaultException<string>("Server failed to validate token: " + msg.StatusCode.ToString(), System.Net.HttpStatusCode.BadRequest);

            dynamic info = msg.Content.ReadAs<JsonObject>();
            SetMemberInfo(info);
            return true;

        }


    }
}
