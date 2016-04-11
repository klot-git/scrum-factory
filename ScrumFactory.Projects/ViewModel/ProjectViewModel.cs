using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.ComponentModel.Composition;
using System.ComponentModel;
using ScrumFactory.Services;
using ScrumFactory.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Extensions;

namespace ScrumFactory.Projects.ViewModel {

    [Export]
    public class ProjectViewModel : BasePanelViewModel, IViewModel, INotifyPropertyChanged {

        private IProjectsService projectsService;
        private IBackgroundExecutor executor;
        private IEventAggregator aggregator;
      
        private Project oldProject;
        private ProjectSummaryViewModel projectSummary;


        private ICollection<string> clients;
        private ICollection<string> platforms;

        [Import]
        private System.Lazy<IProjectContainer> projectContainer { get; set; }
                
        [ImportingConstructor]
        public ProjectViewModel(
            [Import] IProjectsService projectsService,
            [Import] IBackgroundExecutor backgroundExecutor,
            [Import] IEventAggregator eventAggregator) {

            this.projectsService = projectsService;
            this.executor = backgroundExecutor;
            this.aggregator = eventAggregator;
         

            CloseWindowCommand = new DelegateCommand(CloseWindow);
 
           

          
           
        }

        public ICollection<string> Clients {
            get {
                return clients;
            }
            set {
                clients = value;
                OnPropertyChanged("Clients");
            }
        }

        public ICollection<string> Platforms {
            get {
                return platforms;
            }
            set {
                platforms = value;
                OnPropertyChanged("Platforms");
            }
        }

        private void LoadClients() {
            executor.StartBackgroundTask<ICollection<string>>(
                () => { return projectsService.GetClientNames(); },
                clients => { Clients = clients; });
        }

        private void LoadPlatforms() {
            executor.StartBackgroundTask<ICollection<string>>(
                () => { return projectsService.GetPlatforms(); },
                platforms => { Platforms = platforms; });
        }

  
        public ProjectSummaryViewModel ProjectSummary{
            get {
                return projectSummary;
            }
            set {
                projectSummary = value;
                OnPropertyChanged("ProjectSummary");
            }
        }

        private void Save() {
            executor.StartBackgroundTask(
                () => {                    
                    projectsService.UpdateProject(ProjectSummary.Project.ProjectUId, ProjectSummary.Project);
                },
                () => { });
        }

 
        public void Show(ProjectSummaryViewModel projectSummaryVM) {
            ProjectSummary = projectSummaryVM;
            oldProject = ProjectSummary.Project.Clone();
            LoadClients();
            LoadPlatforms();
            Show(projectContainer.Value);            
        }

        private void CloseWindow() {
            if (!ProjectSummary.Project.IsTheSame(oldProject)) {
                Save();
                aggregator.Publish<Project>(ScrumFactoryEvent.ProjectDetailsChanged, ProjectSummary.Project);
            }
            Close();
        }

        /// <summary>
        /// Gets the name of the panel.
        /// </summary>
        /// <value>The name of the panel.</value>
        public string PanelName {
            get {
                return Properties.Resources.Project_details;
            }
        }

       

       

      

        #region Commands

        public ICommand CloseWindowCommand { get; set; }


        #endregion

        #region IViewModel Members

        [Import(typeof(ProjectDetail))]
        public Composition.View.IView View { get; set; }

        #endregion
    }
}
