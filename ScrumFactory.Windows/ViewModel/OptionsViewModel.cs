using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using ScrumFactory.Composition;
using ScrumFactory.Composition.View;
using ScrumFactory.Composition.ViewModel;

namespace ScrumFactory.Windows.ViewModel {

    [Export]
    [Export(typeof(ITopMenuViewModel))]
    public class OptionsViewModel : BasePanelViewModel,  ITopMenuViewModel, INotifyPropertyChanged {


        private IDialogService dialogs;

        [Import]
        private ScrumFactory.Composition.Configuration SFConfig { get; set; }

        [ImportingConstructor]
        public OptionsViewModel([Import] IDialogService dialogs) {

            this.dialogs = dialogs;

            CloseWindowCommand = new DelegateCommand(Close);

            DefineAvaiableLanguages();
            ReadSelectedLanguage();
        }

        public int SprintLength {
            get {
                int len = SFConfig.GetIntValue("SprintLength");
                if (len == 0)
                    len = 10;
                return len;
            }
            set {
                SFConfig.SetValue("SprintLength", value);
            }
        }

        public string SVNCommand {
            get {
                return SFConfig.GetStringValue("RepositoryLogCommand");
            }
            set {
                SFConfig.SetValue("RepositoryLogCommand", value);
            }
        }

        public string RepositoryUrl {
            get {
                return SFConfig.GetStringValue("RepositoryUrl");
            }
            set {
                SFConfig.SetValue("RepositoryUrl", value);
            }
        }

        public string RepositoryFilePath {
            get {
                return SFConfig.GetStringValue("RepositoryFilePath");
            }
            set {
                SFConfig.SetValue("RepositoryFilePath", value);
            }
        }

        public string RepositoryVersion {
            get {
                return SFConfig.GetStringValue("RepositoryVersion");
            }
            set {
                SFConfig.SetValue("RepositoryVersion", value);
            }
        }

        public string ProjectFolderFilePath {
            get {
                return SFConfig.GetStringValue("ProjectFolderFilePath");
            }
            set {
                SFConfig.SetValue("ProjectFolderFilePath", value);
            }
        }

        private bool oldTickedEnabled;
        public bool TicketProjectsEnabled {
            get {
                return SFConfig.GetBoolValue("TicketProjectsEnabled");
            }
            set {
                SFConfig.SetValue("TicketProjectsEnabled", value);
            }
        }

        private bool oldUsePoints;
        public bool UsePoints {
            get {
                return SFConfig.GetBoolValue("UsePoints");
            }
            set {
                SFConfig.SetValue("UsePoints", value);
            }
        }

        public LanguageInfo[] SpellCheckLanguages { get; private set; }

        public LanguageInfo[] Languages { get; private set; }

        private string oldSelectedLanguageCode;
        private LanguageInfo selectedLanguage;
        public LanguageInfo SelectedLanguage {
            get {
                return selectedLanguage;
            }
            set {
                selectedLanguage = value;
                if(selectedLanguage!=null)
                    Properties.Settings.Default.Language = value.Code;
                OnPropertyChanged("SelectedLanguage");
            }
        }

        private LanguageInfo spellCheckLanguage;
        public LanguageInfo SpellCheckLanguage {
            get {
                return spellCheckLanguage;
            }
            set {
                spellCheckLanguage = value;
                if (spellCheckLanguage != null) {
                    Properties.Settings.Default.SpellCheckLanguage = value.Code;
                    System.Windows.KExtensions.SpellCheck.DefaultLanguage = value.Code;
                    if (System.Windows.KExtensions.SpellCheck.DefaultLanguage == "-")
                        System.Windows.KExtensions.SpellCheck.DisableAll = true;
                    else
                        System.Windows.KExtensions.SpellCheck.DisableAll = false;
                }
                OnPropertyChanged("SpellCheckLanguage");
            }
        }

        private void ReadSelectedLanguage() {
            var lang = Languages.SingleOrDefault(l => l.Code == Properties.Settings.Default.Language);
            if (lang == null)
                SelectedLanguage = null;
            else 
                SelectedLanguage = lang;
        }

        private void ReadSpellCheckLanguage() {
            var lang = SpellCheckLanguages.SingleOrDefault(l => l.Code == Properties.Settings.Default.SpellCheckLanguage);
            if (lang == null)
                SpellCheckLanguage = null;
            else
                SpellCheckLanguage = lang;
        }

        private void DefineAvaiableLanguages() {
            Languages = new LanguageInfo[2];
            Languages[0] = new LanguageInfo() { Code = "en-US", Description = "en_US_description" };
            Languages[1] = new LanguageInfo() { Code = "pt-BR", Description = "pt_BR_description" };
        }

        private void DefineSpellCheckLanguages() {
            if (SpellCheckLanguages != null)
                return;
            try {
                string myDocumentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                var files = System.IO.Directory.GetFileSystemEntries(myDocumentsPath + "\\Scrum Factory\\SpellDics\\", "*.dic");
                SpellCheckLanguages = new LanguageInfo[files.Length + 1];
                SpellCheckLanguages[0] = new LanguageInfo() { Code = "-", Description = Properties.Resources.disabled };
                for (int i = 0; i < files.Length; i++) {
                    string code = System.IO.Path.GetFileNameWithoutExtension(files[i]);
                    SpellCheckLanguages[i+1] = new LanguageInfo() { Code = code, Description = code };
                }
            } catch (Exception) {
                SpellCheckLanguages = new LanguageInfo[0];
            }
            OnPropertyChanged("SpellCheckLanguages");
        }

        public void Show() {
            if(SelectedLanguage!=null)
                oldSelectedLanguageCode = SelectedLanguage.Code;
            oldTickedEnabled = TicketProjectsEnabled;
            oldUsePoints = UsePoints;
            DefineSpellCheckLanguages();
            ReadSpellCheckLanguage();
            
            dialogs.SelectTopMenu(this);
        }

    
 
        [Import(typeof(Options))]
        public IView View {
            get;
            set;
        }

        public ICommand CloseWindowCommand { get; private set; }


        public string PanelName {
            get { return Properties.Resources.Options_; }
        }

        public int PanelDisplayOrder {
            get { return int.MaxValue; }
        }

        public PanelPlacements PanelPlacement {
            get { return PanelPlacements.Hidden; }
        }

        public string ImageUrl {
            get { return null; }
        }
    }


    public class LanguageInfo {
        public string Code { get; set; }
        public string Description { get; set; }
        public string ImagePath {
            get {
                return "/images/languages/" + Code + ".png";
            }
        }
    }
}
