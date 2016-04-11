using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Json;
using System.Net.Http;
using System.Net.Http.Formatting;
using ScrumFactory.Services;

namespace ScrumFactory.Projects.Repositories {

    [Export(typeof(ICodeRepositoryService))]
    public class GithubCodeRepositoryService : ICodeRepositoryService {

        [Import]
        private IAuthorizationService authorizator { get; set; }

        public string RepositoryTypeName {
            get {
                return "GitHub";
            }
        }

        public string RepositoryImage {
            get { return "GitHub"; }
        }

        

        public string CreateRepository(string repositoryName, out string errorMessage) {
            errorMessage = null;
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Add("user-agent", "SF-Client");
            var newRepo = (dynamic)new JsonObject();
            newRepo.name = repositoryName;
            
            var response = client.Post("https://api.github.com/user/repos?access_token=" + authorizator.SignedMemberToken,
                new StringContent(newRepo.ToString(), System.Text.Encoding.UTF8, "application/json"));
              
            if (!response.IsSuccessStatusCode) {
                string msg = response.Content.ReadAsString();
                errorMessage = "Error creating repo: " + response.StatusCode + "\n" + msg;
                return null;
            }
            dynamic repo = response.Content.ReadAs<JsonObject>();

            return (string)repo.svn_url;
        }


        public string RepositoryRoot {
            get;
            private set;
        }

        public bool LoadSettings(out string errorMessage) {
            errorMessage = null;
            var client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Add("user-agent", "SF-Client");

            var response = client.Get("https://api.github.com/user?access_token=" + authorizator.SignedMemberToken);
            if (response.StatusCode != System.Net.HttpStatusCode.OK) {
                errorMessage = "You must be signed in with a GitHub account.";
                return false;
            }
            dynamic user = response.Content.ReadAs<JsonObject>();
            RepositoryRoot = (string) user.html_url;
            return true;
        }


        public ICollection<string> LoadCodeReps(Project project) {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Add("user-agent", "SF-Client");

            var response = client.Get("https://api.github.com/user/repos?access_token=" + authorizator.SignedMemberToken);
            if(response.StatusCode!=System.Net.HttpStatusCode.OK)
                return new string[0];
            dynamic reposArray = response.Content.ReadAs<JsonArray>();
            List<string> repos = new List<string>();
            foreach (var repo in reposArray)
                repos.Add((string)repo.svn_url);
            return repos.ToArray();
        }
    }
}
