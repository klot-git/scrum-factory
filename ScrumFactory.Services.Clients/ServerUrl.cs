using System.ComponentModel.Composition;

namespace ScrumFactory.Services.Clients {

    [Export(typeof(IServerUrl))]
    public class ServerUrl : IServerUrl {
        

        private string url = "http://localhost";
        public string Url {
            get {
                return url;
            }
            set {
                url = value;
            }
        }

        
    }
}
