using System;
using System.Net.Http;

namespace ScrumFactory.Services {

    public interface IClientHelper {

        string ClientVersion { get; }
        void ConfigureHttpHandle();
        System.Net.Http.HttpClient GetClient();
        System.Net.Http.HttpClient GetClient(ScrumFactory.Services.IAuthorizationService authService);
        System.Net.IWebProxy GetProxy();
        void HandleHTTPErrorCode(System.Net.Http.HttpResponseMessage response, bool ignoreNotFound = false);
        string UserAgent { get; }

        HttpResponseMessage SafePut<T>(object lockObj, System.Net.Http.HttpClient client, System.Uri url, T data);
        HttpResponseMessage SafePost<T>(object lockObj, System.Net.Http.HttpClient client, System.Uri url, T data);
        HttpResponseMessage SafeGet(object lockObj, System.Net.Http.HttpClient client, System.Uri url);

        
    }
}
