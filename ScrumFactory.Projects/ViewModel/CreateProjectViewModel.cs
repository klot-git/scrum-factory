using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using ScrumFactory.Services;
using System.Collections.Generic;
using ScrumFactory.Composition.View;
using System.Windows.Input;


namespace ScrumFactory.Projects.ViewModel {


    /// <summary>
    /// Create Project View Model.
    /// </summary>
    [Export]
    public class CreateProjectViewModel : BasePanelViewModel, IViewModel, INotifyPropertyChanged {

        private string projectName;
        private string clientName;
        private string description;
        private string platform;
        private bool useLastProjectAsModel = true;

        private IEventAggregator aggregator;
        private IBackgroundExecutor executor;
        private IProjectsService projectService;

        private ICollection<string> clients;
        private ICollection<string> platforms;

        private bool isTicketProject;

        [Import]
        private Configuration SFConfig { get; set; }
                
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateProjectViewModel"/> class.
        /// </summary>
        /// <param name="backgroundExecutor">The background executor.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        /// <param name="projectService">The project service.</param>
        [ImportingConstructor]
        public CreateProjectViewModel(
            [Import] IBackgroundExecutor backgroundExecutor,
            [Import] IEventAggregator eventAggregator,
            [Import] IProjectsService projectService) {

                this.aggregator = eventAggregator;
                this.executor = backgroundExecutor;
         
        
                this.projectService = projectService;

                aggregator.Subscribe<string>(ScrumFactoryEvent.ConfigChanged, c => {
                    if (c == "TicketProjectsEnabled")
                        OnPropertyChanged("IsTicketProjectsEnabled");

                });

       
                CreateProjectCommand = new DelegateCommand(CanCreateProject, CreateProject);
                              
                CloseWindowCommand = new DelegateCommand(() => { Close(); });

        }


        [Import]
        private ScrumFactory.Composition.Configuration config { get; set; }


       

     
        public bool IsTicketProjectsEnabled {
            get {
                return config.GetBoolValue("TicketProjectsEnabled");                
            }
        }
        
        public bool IsTicketProject {
            get {
                return isTicketProject;
            }
            set {
                isTicketProject = value;
                OnPropertyChanged("IsTicketProject");
            }
        }

        public bool UseLastProjectAsModel {
            get {
                return useLastProjectAsModel;
            }
            set {
                useLastProjectAsModel = value;
                OnPropertyChanged("UseLastProjectAsModel");
            }
        }

        /// <summary>
        /// Determines whether this instance can create project.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance can create project; otherwise, <c>false</c>.
        /// </returns>
        private bool CanCreateProject() {
            if (String.IsNullOrWhiteSpace(ProjectName) || String.IsNullOrWhiteSpace(ClientName) || String.IsNullOrWhiteSpace(Description) || String.IsNullOrEmpty(Platform))
                return false;
            return true;
        }      

        /// <summary>
        /// Creates the project.
        /// </summary>
        private void CreateProject() {
         
            // creates the new project
            NewProject = new Project();
            NewProject.ProjectUId = Guid.NewGuid().ToString();

            if(IsTicketProject)
                NewProject.ProjectType = (short) ProjectTypes.TICKET_PROJECT;
            else
                NewProject.ProjectType = (short)ProjectTypes.NORMAL_PROJECT;

            NewProject.AnyoneCanJoin = true;

            NewProject.ProjectName = ProjectName;
            NewProject.ClientName = ClientName;
            NewProject.Description = Description;
            NewProject.Platform = Platform;
            NewProject.CreateDate = DateTime.Now;
         
            // create the deafult roles
            Role defaultRoleSM = new Role() {
                RoleUId = System.Guid.NewGuid().ToString(),
                RoleShortName = Properties.Resources.Scrum_Master_Role_SN,
                RoleName = Properties.Resources.Scrum_Master_Role,
                RoleDescription = Properties.Resources.Scrum_Master_Role_description,
                PermissionSet = (short) PermissionSets.SCRUM_MASTER,
                ProjectUId = NewProject.ProjectUId };            

            Role defaultRoleTEAM = new Role() {
                RoleUId = System.Guid.NewGuid().ToString(),
                RoleShortName = Properties.Resources.Team_Role_SN,
                RoleName = Properties.Resources.Team_Role,
                RoleDescription = Properties.Resources.Team_Role_description,
                PermissionSet = (short) PermissionSets.TEAM,
                IsDefaultRole = true,
                ProjectUId = NewProject.ProjectUId };

            Role defaultRolePRODUCT_OWNER = new Role() {
                RoleUId = System.Guid.NewGuid().ToString(),
                RoleShortName = Properties.Resources.Product_Owner_Role_SN,
                RoleName = Properties.Resources.Product_Owner_Role,
                RoleDescription = Properties.Resources.Product_Owner_description,
                PermissionSet = (short)PermissionSets.PRODUCT_OWNER,
                ProjectUId = NewProject.ProjectUId
            };

            NewProject.Roles = new List<Role>() { defaultRoleSM, defaultRoleTEAM, defaultRolePRODUCT_OWNER };


            // if is there a client folder configuration, set the project folder
            SetProjectFolder(NewProject);

            // saves it
            NewProject.ProjectNumber = projectService.CreateProject(NewProject, UseLastProjectAsModel);

            // if there is a client-side configuration creates the project folder
            CreateProjectFolder(NewProject);

            // close the window
            Close();

            aggregator.Publish<Project>(ScrumFactoryEvent.ProjectCreated, NewProject);

            NewProject = null;

        }

        private void SetProjectFolder(Project project) {

            string path = SFConfig.GetStringValue("ProjectFolderFilePath");
            if (String.IsNullOrEmpty(path))
                return;

            string formatedProjectName = FormatFolderString(project.ProjectName);
            string formatedClientName = FormatFolderString(project.ClientName);

            if (!path.EndsWith("\\"))
                path = path + "\\";

            try {
                if (!String.IsNullOrEmpty(path))
                    project.DocRepositoryPath = path + formatedClientName + "\\" + formatedProjectName;
            } catch (Exception) { }
        }

        private void CreateProjectFolder(Project project) {
            string path = SFConfig.GetStringValue("ProjectFolderFilePath");
            if (String.IsNullOrEmpty(path))
                return;

            System.Threading.Tasks.Task.Factory.StartNew(() => {
                try {
                    // creates the folder                    
                    System.IO.Directory.CreateDirectory(project.DocRepositoryPath);
                } catch (Exception ex) {
                    System.Windows.MessageBox.Show("Could not create project folder at:\n" + project.DocRepositoryPath + "\nPlease verify your Project Folder configuration at Options.");
                }
            });

        }

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

        public override void Show(IChildWindow parentViewModel) {
            NewProject = null;

            LoadClients();
            LoadPlatforms();

            ClientName = null;
            ProjectName = null;
            Description = null;
            Platform = null;

            base.Show(parentViewModel);
        }

        [Import(typeof(CreateProject))]
        public IView View { get; set; }


        private void LoadClients() {
            executor.StartBackgroundTask<ICollection<string>>(
                () => { return projectService.GetClientNames(); },
                clients => { Clients = clients; });
        }

        private void LoadPlatforms() {
            executor.StartBackgroundTask<ICollection<string>>(
                () => { return projectService.GetPlatforms(); },
                platforms => { Platforms = platforms; });
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

        public ICollection<string> Clients {
            get {
                return clients;
            }
            set {
                clients = value;
                OnPropertyChanged("Clients");
            }
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

        
        #region ICreateProjectViewModel Members

        /// <summary>
        /// Gets the new project.
        /// </summary>
        /// <value>The new project.</value>
        public Project NewProject { get; set; }

        /// <summary>
        /// Gets or sets the name of the project.
        /// </summary>
        /// <value>The name of the project.</value>
        public string ProjectName {
            get {
                return projectName;
            }
            set {
                projectName = value;
                OnPropertyChanged("ProjectName");
                ((DelegateCommand)CreateProjectCommand).NotifyCanExecuteChanged();               
            }
        }

        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        /// <value>The name of the client.</value>
        public string ClientName {
            get {
                return clientName;
            }
            set {
                clientName = value;
                OnPropertyChanged("ClientName");
                ((DelegateCommand)CreateProjectCommand).NotifyCanExecuteChanged();
               
            }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description {
            get {
                return description;
            }
            set {
                description = value;
                OnPropertyChanged("Description");
                ((DelegateCommand)CreateProjectCommand).NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Platform {
            get {
                return platform;
            }
            set {
                platform = value;
                OnPropertyChanged("Platform");
                ((DelegateCommand)CreateProjectCommand).NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets the create project command.
        /// </summary>
        /// <value>The create project command.</value>
        public System.Windows.Input.ICommand CreateProjectCommand { get; private set; }

        /// <summary>
        /// Gets the cancel command.
        /// </summary>
        /// <value>The cancel command.</value>
        public System.Windows.Input.ICommand CloseWindowCommand { get; private set; }

        #endregion

      
    }
}
