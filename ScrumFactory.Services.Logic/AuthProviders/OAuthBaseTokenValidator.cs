using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Json;
using ScrumFactory.Services.AuthProviders;

namespace ScrumFactory.Services.Logic.AuthProviders {

    public abstract class OAuthBaseTokenValidator : IOAuthServerTokenValidator {

        public MemberProfile MemberInfo { get; protected set; }

        public abstract void SetMemberInfo(dynamic info);
        
        public abstract string ProviderName { get; }

        public abstract string ValidateUrl { get; }

        public virtual bool IsProviderEnabled() {
            bool setting = true;
            string settingStr = System.Configuration.ConfigurationManager.AppSettings["Enable-" + ProviderName.Replace(" ", "") + "Provider"];
            if (String.IsNullOrEmpty(settingStr))
                return setting;
            bool.TryParse(settingStr, out setting);

            return setting;
        }

        public virtual bool ValidateAccessToken(string token) {

            if (!IsProviderEnabled())
                throw new ScrumFactory.Exceptions.AuthorizationProviderNotSupportedException();


            System.Net.Http.HttpClientHandler handler = new System.Net.Http.HttpClientHandler();
            handler.Proxy = System.Net.WebRequest.DefaultWebProxy;
            System.Net.Http.HttpClient wc = new System.Net.Http.HttpClient(handler);
            

            System.Net.Http.HttpResponseMessage msg = null;
            try {
                msg = wc.Get(new Uri(ValidateUrl + "?access_token=" + token));
            }
            catch (Exception ex) {
                throw new System.ServiceModel.Web.WebFaultException<string>(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }

            if (msg.StatusCode == HttpStatusCode.Unauthorized)
                return false;

            if (msg.StatusCode != HttpStatusCode.OK) 
                throw new System.ServiceModel.Web.WebFaultException<string>("Server failed to validate token: " + msg.StatusCode.ToString(), System.Net.HttpStatusCode.BadRequest);
            
            dynamic info = msg.Content.ReadAs<JsonObject>();
            SetMemberInfo(info);
            return true;
            
        }

    }
}
