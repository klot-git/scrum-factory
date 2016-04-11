using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScrumFactory.Services.AuthProviders;
using System.ComponentModel.Composition;

namespace ScrumFactory.Services.Logic.AuthProviders {

    [Export(typeof(IOAuthServerTokenValidator))]
    public class LiveOAuthTokenValidator : OAuthBaseTokenValidator,  IOAuthServerTokenValidator {

        public override string ProviderName {
            get { return "Windows Live"; }
        }


        public  override  string ValidateUrl {
            get {
                return "https://apis.live.net/v5.0/me";
            }
        }

        public override void SetMemberInfo(dynamic info) {
            MemberInfo = new MemberProfile();
            MemberInfo.MemberUId = info.emails["account"];
            MemberInfo.EmailAccount = info.emails["preferred"];
            MemberInfo.FullName = info.name;
        }


    }
}
