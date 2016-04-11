using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using ScrumFactory.Services.AuthProviders;
using System.Collections.Concurrent;


namespace ScrumFactory.Services.Logic.AuthProviders {

    [Export(typeof(IWindowsTokenStore))]
    public class WindowsTokenStore : ScrumFactory.Services.AuthProviders.IWindowsTokenStore {

        private static readonly ConcurrentDictionary<string, string> tokens = new ConcurrentDictionary<string, string>();

        public string CreateTokenFor(string user) {

            string token = null;

            KeyValuePair<string, string> pair = tokens.SingleOrDefault(t => t.Value == user);
            if(pair.Equals(new KeyValuePair<string, string>())) {
                token = Guid.NewGuid().ToString();
                tokens.TryAdd(token, user);
            }
            else {
                token = pair.Key;
            }

            return token;            
        }

        public MemberProfile ValidateToken(string token) {

            string memberUId = null;
            tokens.TryGetValue(token, out memberUId);

            if (memberUId == null)
                return null;

            MemberProfile member = new MemberProfile();
            member.MemberUId = memberUId;

            return member;

        }



    }

    class WindowsToken {
        public string Token { get; set; }
        public string MemberUId { get; set; }
    }
}
