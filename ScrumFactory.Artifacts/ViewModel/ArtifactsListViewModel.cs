using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel;
using ScrumFactory;
using ScrumFactory.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Services;
using ScrumFactory.Windows.Helpers.Extensions;

namespace ScrumFactory.Artifacts.ViewModel {

    [Export]    
    [Export(typeof(IArtifactsListViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.NonShared)]
    public class ArtifactsListViewModel : BasePanelViewModel, IArtifactsListViewModel, INotifyPropertyChanged {

        private ICollection<Artifact> artifacts;
        private Project project;
        
        private IBackgroundExecutor executor;
        private IEventAggregator aggregator;
        private IAuthorizationService authorizator;

        private IArtifactsService artifactsService;

        [Import]
        private ArtifactDetailViewModel ArtifactDetail { get; set; }

        [ImportingConstructor]
        public ArtifactsListViewModel(
            [Import] IAuthorizationService authorizator,
            [Import] IArtifactsService artifactsService,
            [Import] IEventAggregator aggregator,            
            [Import] IBackgroundExecutor executor) {

                this.authorizator = authorizator;
                this.aggregator = aggregator;                
                this.executor = executor;

                this.artifactsService = artifactsService;


                aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, OnViewProjectDetails);

                AddArtifactCommand = new DelegateCommand(CanAddArtifact, AddArtifact);
                RemoveArtifactCommand = new DelegateCommand<string>(CanAddArtifact, RemoveArtifact);
                GoToLinkCommand = new DelegateCommand<string>(GoToLink);
                StartEditCommand = new DelegateCommand<string>(CanAddArtifact, StartEdit);

                View = new ArtifactsList();
                View.Model = this;
            
        }

        public ArtifactContexts ListContext { get;  private set; }
        public string ContextUId { get;  private set; }

        public ICollection<Artifact> Artifacts {
            get {
                return artifacts;
            }
            set {
                artifacts = value;
                OnPropertyChanged("Artifacts");
            }
        }

        private void OnViewProjectDetails(Project project) {
            this.project = project;
            ((DelegateCommand)AddArtifactCommand).NotifyCanExecuteChanged();
            ((DelegateCommand<string>)RemoveArtifactCommand).NotifyCanExecuteChanged();
            ((DelegateCommand<string>)StartEditCommand).NotifyCanExecuteChanged();
        }

        private Action<int> afterAdded = null;

        public void ChangeContext(ArtifactContexts context, String contextUId, Action<int> after = null) {
            ListContext = context;
            ContextUId = contextUId;
            LoadArtifacts();
            afterAdded = after;
        }

        private void LoadArtifacts() {

            if (String.IsNullOrEmpty(ContextUId))
                return;

            IsLoadingData = true;
            executor.StartBackgroundTask<ICollection<Artifact>>(
                () => {
                    return artifactsService.GetArtifacts(ContextUId);                                        
                },
                a => {
                    Artifacts = new ObservableCollection<Artifact>(a);
                    IsLoadingData = false;
                });
        }

        private void StartEdit(string artifactUId) {
            ArtifactDetail.EditingArtifact = Artifacts.SingleOrDefault(a => a.ArtifactUId == artifactUId);
            ArtifactDetail.Show();

            Artifact updatedArtifact = ArtifactDetail.EditingArtifact;

            updatedArtifact.ArtifactContext = (short)ListContext;
            updatedArtifact.ContextUId = ContextUId;
            updatedArtifact.ProjectUId = project.ProjectUId;

            ArtifactDetail.EditingArtifact = null;

            executor.StartBackgroundTask(
                () => {
                    artifactsService.UpdateArtifact(updatedArtifact.ContextUId, updatedArtifact.ArtifactUId, updatedArtifact);
                },
                () => {});
        }

        private void AddArtifact() {

            ArtifactDetail.EditingArtifact = null;
            ArtifactDetail.Show();

            if (ArtifactDetail.Canceled)
                return;

            Artifact newArtifact = ArtifactDetail.EditingArtifact;

            newArtifact.ArtifactContext = (short)ListContext;
            newArtifact.ContextUId = ContextUId;
            newArtifact.ProjectUId = project.ProjectUId;

            ArtifactDetail.EditingArtifact = null;

            executor.StartBackgroundTask(
                () => {
                    return artifactsService.AddArtifact(newArtifact);
                },
                count => {
                    Artifacts.Add(newArtifact);
                    if (afterAdded != null)
                        afterAdded(count);
                });
        }

        private void RemoveArtifact(string artifactUId) {
            Artifact removed = Artifacts.SingleOrDefault(a => a.ArtifactUId == artifactUId);
            if (removed == null)
                return;
            executor.StartBackgroundTask(
                () => {                    
                    return artifactsService.RemoveArtifact(removed.ContextUId, removed.ArtifactUId);
                },
                count => {                                        
                    Artifacts.Remove(removed);
                    if (afterAdded != null)
                        afterAdded(count);
                });
        }

        private bool CanAddArtifact() {            
            if (project == null)
                return false;
            return project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);

        }

        private void GoToLink(string path) {
            try {
                System.Diagnostics.Process.Start(path);
            }
            catch (System.Exception ex) {
                System.Windows.Forms.MessageBox.Show(ex.Message, Properties.Resources.Could_not_open_artifact, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        public ScrumFactory.Composition.View.IView View {
            get;
            set;
        }


        public ICommand AddArtifactCommand { get; set; }
        public ICommand RemoveArtifactCommand { get; set; }
        public ICommand GoToLinkCommand { get; set; }
        public ICommand StartEditCommand { get; set; }

    }
}
