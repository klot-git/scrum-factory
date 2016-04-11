using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScrumFactory.Services.AuthProviders;
using System.ComponentModel.Composition;

namespace ScrumFactory.Services.Logic.AuthProviders {

    [Export(typeof(IOAuthServerTokenValidator))]
    public class WindowsOAuthTokenValidator : OAuthBaseTokenValidator,  IOAuthServerTokenValidator {

        [Import]
        private IWindowsTokenStore WindowsTokenStore { get; set; }

        public override string ProviderName {
            get { return "Windows Authentication"; }
        }

        public override string ValidateUrl {
            get { throw new NotImplementedException(); }
        }
        
        public override bool ValidateAccessToken(string token) {

         
            if (!IsProviderEnabled())
                throw new ScrumFactory.Exceptions.AuthorizationProviderNotSupportedException();

            MemberProfile memberInfo = WindowsTokenStore.ValidateToken(token);

            if (memberInfo == null)
                return false;

            SetMemberInfo(memberInfo);
            return true;
        }

        public override void SetMemberInfo(dynamic info) {
            MemberInfo = new MemberProfile();
            MemberInfo.MemberUId = info.MemberUId;            
        }


    }
}
