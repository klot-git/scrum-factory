using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using ScrumFactory.Services;
using System.Collections.Generic;
using ScrumFactory.Composition.View;
using System.Windows.Input;
using System.IO;

namespace ScrumFactory.Projects.ViewModel {


    /// <summary>
    /// Create Project View Model.
    /// </summary>
    [Export]
    public class CreateRepositoryViewModel : BasePanelViewModel, IViewModel, INotifyPropertyChanged {

        private IEventAggregator aggregator;
        private IBackgroundExecutor executor;
        private IProjectsService projectService;
        private IDialogService dialogs;

        private IDialogViewModel window;

        public Project Project { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateProjectViewModel"/> class.
        /// </summary>
        /// <param name="backgroundExecutor">The background executor.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        /// <param name="projectService">The project service.</param>
        [ImportingConstructor]
        public CreateRepositoryViewModel(
            [Import] IBackgroundExecutor backgroundExecutor,
            [Import] IEventAggregator eventAggregator,
            [Import] IDialogService dialogsService,
            [Import] IProjectsService projectService) {

                this.aggregator = eventAggregator;
                this.executor = backgroundExecutor;
                this.dialogs = dialogsService;
                this.projectService = projectService;
       
                SetCodeRepCommand = new DelegateCommand<string>(SetCodeRep);
                OpenCodeRepCommand = new DelegateCommand<string>(OpenCodeRep);
                SaveCommand = new DelegateCommand(Save);
                OnLoadCommand = new DelegateCommand(OnLoad);


        }

        private ICodeRepositoryService codeRepService;

        public void Show(Project project, ICodeRepositoryService codeRepService) {
            Project = project;
            this.codeRepService = codeRepService;

            NewRepName = FormatFolderString(Project.ClientName) + "-" + FormatFolderString(Project.ProjectName);
            IsCreateRepChecked = true;
            CodeReps = null;
            codeRep = null;
            ReturnMessage = null;

            window = dialogs.NewDialog(String.Format(Properties.Resources.Create_N_repository, codeRepService.RepositoryTypeName), View);
            window.Show();

        }

        private void OnLoad() {
            string message;

            
            if (!codeRepService.LoadSettings(out message)) {
                ReturnMessage = message;
                return;
            }
            

            Rep_root = codeRepService.RepositoryRoot;

            LoadCodeReps();
        }

        private string rep_root;
        public string Rep_root {
            get {
                return rep_root;
            }
            set {
                rep_root = value;
                OnPropertyChanged("Rep_root");
            }
        }

        private string newRepName;
        public string NewRepName {
            get {
                return newRepName;
            }
            set {
                newRepName = value;
                OnPropertyChanged("NewRepName");
            }
        }

        private void LoadCodeReps() {
            executor.StartBackgroundTask<ICollection<string>>(
                () => {
                    return codeRepService.LoadCodeReps(Project);
                },
                reps => { 
                    CodeReps = reps;            
                });
        }

        private void OpenCodeRep(string path) {
            try {
                System.Diagnostics.Process.Start(path);
            } catch (System.Exception) { }
        }

        public void Save() {

            string httpPath = codeRep;

            string message;
            
            if (IsCreateRepChecked) {                
                httpPath = codeRepService.CreateRepository(NewRepName, out message);
                ReturnMessage = message;
            }

            if (httpPath != null) {
                executor.StartBackgroundTask(
                    () => { projectService.SetCodeRepositoryPath(Project.ProjectUId, httpPath); },
                    () => { });

                Project.CodeRepositoryPath = httpPath;
                window.Close();
            }

            
        }

    

        private string returnMessage;
        public string ReturnMessage {
            get {
                return returnMessage;
            }
            set {
                returnMessage = value;
                OnPropertyChanged("ReturnMessage");
            }
        }

        private ICollection<string> codeReps;
        public ICollection<string> CodeReps {
            get {
                return codeReps;
            }
            set {
                codeReps = value;
                OnPropertyChanged("CodeReps");
            }
        }

        private string actionText = Properties.Resources.Ok;
        public string ActionText {
            get {
                return actionText;
            }
            set {
                actionText = value;
                OnPropertyChanged("ActionText");
            }
        }

        private bool isCreateRepChecked = true;
        public bool IsCreateRepChecked {
            get {
                return isCreateRepChecked;
            }
            set {
                isCreateRepChecked = value;
                OnPropertyChanged("IsCreateRepChecked");
                if (value)
                    ActionText = Properties.Resources.Create;
                else
                    ActionText = Properties.Resources.Ok;
            }        
        }

     

        private string codeRep = null;
        private void SetCodeRep(string rep) {
            codeRep = rep;
        }

       

        [Import(typeof(CreateRepository))]
        public IView View { get; set; }


        private string FormatFolderString(string text) {
            if (text == null)
                return string.Empty;

            // remove accents
            System.Text.RegularExpressions.Regex nonSpacingMarkRegex = new System.Text.RegularExpressions.Regex(@"\p{Mn}", System.Text.RegularExpressions.RegexOptions.Compiled);
            var normalizedText = text.Normalize(System.Text.NormalizationForm.FormD);
            normalizedText = nonSpacingMarkRegex.Replace(normalizedText, string.Empty);

            // replace spaces for _
            normalizedText = normalizedText.Replace(" ", "_");

            normalizedText = normalizedText.Replace("\"", "");
            normalizedText = normalizedText.Replace("'", "");
            normalizedText = normalizedText.Replace("\\", "");
            normalizedText = normalizedText.Replace("/", "");

            return normalizedText;
        }
       

        #region IPanelViewModel Members

        /// <summary>
        /// Gets the name of the panel.
        /// </summary>
        /// <value>The name of the panel.</value>
        public string PanelName {
            get { return Properties.Resources.ResourceManager.GetString("Create_new_project"); }
        }

  
        #endregion

        
  
        /// <summary>
        /// Gets the cancel command.
        /// </summary>
        /// <value>The cancel command.</value>
        public System.Windows.Input.ICommand SaveCommand { get; private set; }
        public ICommand OpenCodeRepCommand { get; set; }

        public ICommand SetCodeRepCommand { get; set; }

        public ICommand OnLoadCommand { get; set; }

       

       
      
    }
}
