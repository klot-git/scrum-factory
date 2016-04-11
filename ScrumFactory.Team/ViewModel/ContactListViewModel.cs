using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ScrumFactory.Composition.ViewModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition;
using ScrumFactory.Services;
using System.Collections.ObjectModel;
using ScrumFactory.Composition.View;
using ScrumFactory.Extensions;
using System.Diagnostics;


namespace ScrumFactory.Team.ViewModel {

    [Export]
    public class ContactListViewModel: BasePanelViewModel, IViewModel, INotifyPropertyChanged {

        private IEventAggregator aggregator;
        private ITeamService teamServices;        
        private IBackgroundExecutor executor;
        private IAuthorizationService authorizator;

        private Project project;
        private ICollection<MemberProfile> contacts;

        private MemberProfile oldContact;
        private MemberProfile selectedContact;

        private IDialogService dialogs;

        [Import]
        private System.Lazy<IProjectContainer> projectContainer { get; set; }

        [ImportingConstructor]
        public ContactListViewModel(
            [Import]IEventAggregator eventAggregator,            
            [Import]ITeamService teamServices,
            [Import]IBackgroundExecutor backgroundExecutor,
            [Import] IDialogService dialogs,
            [Import]IAuthorizationService authorizationService) {

            this.aggregator = eventAggregator;            
            this.teamServices = teamServices;
            this.executor = backgroundExecutor;
            this.authorizator = authorizationService;
            this.dialogs = dialogs;

            aggregator.Subscribe<Project>(ScrumFactoryEvent.ViewProjectDetails, p => { project = p; });

            CloseWindowCommand = new DelegateCommand(CloseWindow);
            AddNewContactCommand = new DelegateCommand(AddNewContact);
            OnLoadCommand = new DelegateCommand(LoadData);
            SendEmailCommand = new DelegateCommand<string>(SendEmail);
        }

        private void AddNewContact() {

            MemberProfile newContact = new MemberProfile() {
                MemberUId = Guid.NewGuid().ToString(),
                AuthorizationProvider = "Factory Contact",
                CompanyName = project.ClientName,
                CreateBy = authorizator.SignedMemberProfile.MemberUId,
                IsActive = true,
                FullName = Properties.Resources.New_contact
            };

            executor.StartBackgroundTask(
                () => { teamServices.CreateContactMember(newContact); },
                () => {
                    Contacts.Add(newContact);                    
                    SelectedContact = newContact;                    
                });
        }

        public MemberProfile SelectedContact {
            get {
                return selectedContact;
            }
            set {
                
                if (ContactChanged)
                    SaveContact(selectedContact);

                selectedContact = value;
                oldContact = selectedContact.Clone();

                OnPropertyChanged("SelectedContact");
                OnPropertyChanged("CanEditSelectedContact");
                
            }
        }

        public bool CanEditSelectedContact {
            get {
                if (SelectedContact == null || authorizator.SignedMemberProfile==null)
                    return false;
                return SelectedContact.CreateBy.Equals(authorizator.SignedMemberProfile.MemberUId);
            }
        }

        private void SaveContact(MemberProfile contact) {
            SaveContact(contact, null);
        }

        private void SaveContact(MemberProfile contact, Action afterSave) {
            executor.StartBackgroundTask(
                () => { teamServices.UpdateMember(contact.MemberUId, contact); },
                () => {
                    if(afterSave!=null)
                        afterSave.Invoke();
                });
        }

        public ICollection<MemberProfile> Contacts {
            get {
                return contacts;
            }
            set {
                contacts = value;
                OnPropertyChanged("Contacts");
            }
        }

        private bool ContactChanged {
            get {
                if (SelectedContact == null || oldContact==null)
                    return false;                
                return !SelectedContact.IsTheSame(oldContact);
            }
        }

        private void CloseWindow() {

            if (ContactChanged)
                SaveContact(SelectedContact, ClearContacts);
            else
                ClearContacts();

            Close();
        }

        private void ClearContacts() {
            if(Contacts!=null)
                Contacts.Clear();
        }

        private void LoadData() {
            executor.StartBackgroundTask<ICollection<MemberProfile>>(
                () => { return teamServices.GetContacts(project.ClientName); },
                c => {
                    Contacts = new ObservableCollection<MemberProfile>(c.OrderBy(o => o.FullName));
                });
        }

        public void Show() {
            OnPropertyChanged("PanelName");
            Show(projectContainer.Value);
        }

        private void SendEmail(string email) {
            Process.Start(new ProcessStartInfo("mailto:" + email));
        }

        [Import(typeof(Contacts))]
        public IView View { get; set; }

        public string PanelName {
            get {
                if(project==null)
                    return Properties.Resources.Contacts;
                else
                    return String.Format(Properties.Resources.Contacts_from_N, project.ClientName);
            }
        }

        public ICommand CloseWindowCommand { get; private set; }
        public ICommand OnLoadCommand { get; set; }
        public ICommand AddNewContactCommand { get; set; }
        public ICommand SendEmailCommand { get; set; }

    }
}
