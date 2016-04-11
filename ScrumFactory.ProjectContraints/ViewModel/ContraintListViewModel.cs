using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Services;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;
using System.Windows.Data;

namespace ScrumFactory.ProjectConstraints.ViewModel {

    [Export]
    [Export(typeof(IProjectTabViewModel))]
    public class ContraintListViewModel : BasePanelViewModel, IProjectTabViewModel, INotifyPropertyChanged {

        private IDialogService dialogs;
        private IEventAggregator aggregator;
        private IBackgroundExecutor executor;


        private IAuthorizationService authorizator;

        [Import]
        private IProjectConstraintsService constraintsService { get; set; }

        private Project project;


        private System.Windows.Data.CollectionViewSource suggestionsViewSource;

        private ICollection<ProjectConstraint> constraints;

        public ICollection<ProjectConstraint> Constraints {
            get {
                return constraints;
            }
            set {
                constraints = value;
                OnPropertyChanged("Constraints");
            }
        }

        public ICollectionView Suggestions {
            get {
                return suggestionsViewSource.View;
            }
        }

        private string newConstraint;
        public string NewConstraint {
            get {
                return newConstraint;
            }
            set {
                newConstraint = value;
                OnPropertyChanged("NewConstraint");
            }
        }

        private bool showSuggestions;
        public bool ShowSuggestions {
            get {
                return showSuggestions;
            }
            set {
                showSuggestions = value;
                if (suggestionsViewSource.Source == null)
                    LoadSuggestions();
                OnPropertyChanged("ShowSuggestions");
            }
        }

        [ImportingConstructor()]
        public ContraintListViewModel(
            [Import] IBackgroundExecutor backgroundExecutor,
            [Import] IEventAggregator eventAggregator,
            [Import] IDialogService dialogService,
            [Import] IAuthorizationService authorizationService) {

            this.executor = backgroundExecutor;
            this.dialogs = dialogService;
            this.aggregator = eventAggregator;

            this.authorizator = authorizationService;

            this.aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, OnViewProjectDetails);

            OnLoadCommand = new DelegateCommand(() => { if (NeedRefresh) LoadConstraints(); });
            AddConstraintCommand = new DelegateCommand(_CanEdit, AddConstraint);
            AddSuggestionCommand = new DelegateCommand<ProjectConstraint>(_CanEdit, AddSuggestion);
            UpdateConstraintCommand = new DelegateCommand<ProjectConstraint>(_CanEdit, UpdateConstraint);
            UpdateConstraintPointsCommand = new DelegateCommand<ProjectConstraint>(_CanEdit, UpdateConstraintPoints);
            RemoveConstraintCommand = new DelegateCommand<ProjectConstraint>(_CanEdit, RemoveContraint);

            suggestionsViewSource = new System.Windows.Data.CollectionViewSource();
            suggestionsViewSource.GroupDescriptions.Add(new PropertyGroupDescription("ConstraintGroupName"));

        }

        private void AddSuggestion(ProjectConstraint suggestion) {
            AddConstraint(suggestion.Constraint, (ContraintGroups) suggestion.ConstraintGroup, suggestion.AdjustPointFactor);
        }

        private void LoadSuggestions() {
            executor.StartBackgroundTask<ICollection<ProjectConstraint>>(
             () => { return constraintsService.GetDefaultContraints(); },
             c => {                 
                 suggestionsViewSource.Source = c;
                 OnPropertyChanged("Suggestions");
             });            
        }

        private void LoadConstraints() {

            executor.StartBackgroundTask<ICollection<ProjectConstraint>>(
                () => { return constraintsService.GetProjectConstraints(Project.ProjectUId); },
                c => { Constraints = new ObservableCollection<ProjectConstraint>(c); });            
        }

        private void AskForRefresh() {
            if (View != null && View.IsVisible) {
                LoadConstraints();
            }
            else
                NeedRefresh = true;
        }

        public Project Project {
            get {
                return project;
            }
            set {
                this.project = value;
                NeedRefresh = true;
                ((DelegateCommand)AddConstraintCommand).NotifyCanExecuteChanged();
                ((DelegateCommand<ProjectConstraint>)RemoveConstraintCommand).NotifyCanExecuteChanged();
                OnPropertyChanged("Project");
                OnPropertyChanged("CanEdit");
            }
        }
        private void OnViewProjectDetails(Project project) {
            Project = project;
            AskForRefresh();
        }

        private void AddConstraint() {
            if (string.IsNullOrEmpty(NewConstraint))
                return;
            
            AddConstraint(NewConstraint, ContraintGroups.BUSINESS_CONSTRAINT, 1);
        }

        private void AddConstraint(string constraintText, ContraintGroups group, double factor) {
            
            ProjectConstraint constraint = new ProjectConstraint() { 
                ConstraintUId = System.Guid.NewGuid().ToString(),
                ProjectUId = Project.ProjectUId,
                Constraint = constraintText, 
                AdjustPointFactor = factor,
                ConstraintGroup = (short) group,
                ConstraintId = GetNextConstraintId(group)
            };
            executor.StartBackgroundTask(
                () => { constraintsService.AddProjectConstraint(Project.ProjectUId, constraint); },
                () => { 
                    Constraints.Add(constraint);
                    NewConstraint = "";
                });            
        }

        private void UpdateConstraintPoints(ProjectConstraint constraint) {
            executor.StartBackgroundTask(
                () => { constraintsService.UpdateProjectConstraint(Project.ProjectUId, constraint); },
                () => {
                    aggregator.Publish(ScrumFactoryEvent.ProjectAdjustPointsChanged);
                });    
        }

        private void UpdateConstraint(ProjectConstraint constraint) {
            executor.StartBackgroundTask(
                () => { constraintsService.UpdateProjectConstraint(Project.ProjectUId, constraint); },
                () => {});    
        }

        private string GetNextConstraintId(ContraintGroups group) {
            
            string abbr = Properties.Resources.ResourceManager.GetString(group.ToString()).ToUpper()[0].ToString();
            string first = abbr + "1";
            if (Constraints == null || Constraints.Count == 0)
                return first;
            int? n = Constraints.Where(c => c.ConstraintGroup==(short)group).Count();
            if(!n.HasValue)
                return first;

            return abbr + (n + 1);
        }

        private void RemoveContraint(ProjectConstraint constraint) {
            executor.StartBackgroundTask(
                () => { constraintsService.RemoveProjectConstraint(Project.ProjectUId, constraint.ConstraintUId); },
                () => {
                    Constraints.Remove(constraint);
                });  
            
        }

        public bool CanEdit {
            get {
                if (Project == null)
                    return false;
                return Project.HasPermission(authorizator.SignedMemberProfile.MemberUId, PermissionSets.SCRUM_MASTER);
            }
        }

        private bool _CanEdit() {
            return CanEdit;
            
        }

        public ICommand RemoveConstraintCommand { get; set; }
        public ICommand AddConstraintCommand { get; set; }
        public ICommand AddSuggestionCommand { get; set; }
        public ICommand OnLoadCommand { get; set; }
        public ICommand UpdateConstraintCommand { get; set; }
        public ICommand UpdateConstraintPointsCommand { get; set; }


        public string PanelName {
            get {
                return Properties.Resources.Constraints;
            }
        }

        public int PanelDisplayOrder {
            get {
                return 700;
            }
        }

        public bool IsVisible {
            get {
                return true;
            }
        }

        public bool IsEspecialTab {
            get {
                return false;
            }
        }

        [Import(typeof(ConstraintList))]
        public Composition.View.IView View {
            get;
            set;
        }
    }
}
