using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScrumFactory.Services.AuthProviders;
using System.ComponentModel.Composition;

namespace ScrumFactory.Services.Logic.AuthProviders {

    [Export(typeof(IOAuthServerTokenValidator))]
    public class GoogleOAuthTokenValidator : OAuthBaseTokenValidator,  IOAuthServerTokenValidator {

        public override string ProviderName {
            get { return "Google"; }
        }


        public override string ValidateUrl {
            get {
                return "https://www.googleapis.com/oauth2/v1/userinfo";
            }
        }
         
        public override void SetMemberInfo(dynamic info) {
            MemberInfo = new MemberProfile();
            MemberInfo.MemberUId = info.email;
            MemberInfo.EmailAccount = info.email;
            MemberInfo.FullName = String.Format("{0} {1}",info.given_name, info.family_name);
            MemberInfo.FullName = MemberInfo.FullName.Replace("\"", "");
        }
    }
}
