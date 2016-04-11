using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using ScrumFactory.Services;
using ScrumFactory.Composition;


namespace ScrumFactory.Projects.Repositories {

    [Export(typeof(ICodeRepositoryService))]
    public class SVNCodeRepositoryServiceClient : ICodeRepositoryService {

        private SVNSettings svnServerSettings = null;
        private SVNSettings settings = null;

        private IEventAggregator aggregator;

        [ImportingConstructor]
        public SVNCodeRepositoryServiceClient([Import] IEventAggregator aggregator) {
            this.aggregator = aggregator;

            aggregator.Subscribe<MemberProfile>(ScrumFactoryEvent.SignedMemberChanged, m => { svnServerSettings = null; });
        }

        public string RepositoryTypeName {
            get {
                return "SVN";
            }
        }
        public string RepositoryImage { 
            get {
                return "SVN";
            }
        }

        public string RepositoryRoot {
            get {
                return settings.SVN_Url;
            }
        }

        [Import]
        private IProjectsService projectsService { get; set; }

        [Import]
        private ScrumFactory.Composition.Configuration SFConfig { get; set; }

        
        public bool LoadSettings(out string errorMessage) {
            errorMessage = null;

            settings = new SVNSettings();

            if (svnServerSettings == null) {

                try {
                    svnServerSettings = projectsService.GetSVNSettings();                   
                } catch (Exception ex) {
                    errorMessage = ex.Message;
                }
            }

            settings = svnServerSettings;


            // use local values
            if(!String.IsNullOrEmpty(SFConfig.GetStringValue("RepositoryUrl")))
                settings.SVN_Url = SFConfig.GetStringValue("RepositoryUrl");
            
            if (!String.IsNullOrEmpty(SFConfig.GetStringValue("RepositoryFilePath")))
                settings.SVN_FilePath = SFConfig.GetStringValue("RepositoryFilePath");

            if (!String.IsNullOrEmpty(SFConfig.GetStringValue("RepositoryVersion")))
                settings.SVN_Version = int.Parse(SFConfig.GetStringValue("RepositoryVersion"));

            if (String.IsNullOrEmpty(settings.SVN_Url) || String.IsNullOrEmpty(settings.SVN_FilePath)) {
                errorMessage = "Please go to 'Options...' > 'SVN' and configure the SVN Settings.";
                return false;
            }

            return true;
        }

        public ICollection<string> LoadCodeReps(Project project) {
            if (String.IsNullOrEmpty(project.ProjectName) || String.IsNullOrEmpty(project.ClientName))
                return new string[0];
            
             return projectsService.GetSimilarCodeRepositories(project.ProjectName, project.ClientName);
               
        }

        public string CreateRepository(string repositoryName, out string errorMessage) {

            errorMessage = null;

            if (!LoadSettings(out errorMessage)) {
                return null;
            }

            try {
                using (var client = new SharpSvn.SvnRepositoryClient()) {
                    client.CreateRepository(settings.SVN_FilePath + repositoryName, new SharpSvn.SvnCreateRepositoryArgs() {
                        RepositoryCompatibility = (SharpSvn.SvnRepositoryCompatibility)settings.SVN_Version
                    });
                }
            }
            catch (Exception ex) {
                errorMessage = ex.Message;
                return null;
            }

            string svnUrl = settings.SVN_Url + repositoryName;

            try {
                using (var client = new SharpSvn.SvnClient()) {
                    client.RemoteCreateDirectory(new Uri(svnUrl + "/trunk"), new SharpSvn.SvnCreateDirectoryArgs() { LogMessage = "Creating trunk folder" });
                    client.RemoteCreateDirectory(new Uri(svnUrl + "/doc"), new SharpSvn.SvnCreateDirectoryArgs() { LogMessage = "Creating doc folder" });
                }
            }
            catch (Exception ex) {
                errorMessage = ex.Message;
                return svnUrl;
            }
            
            return svnUrl + "/trunk";
        }
    }
}
