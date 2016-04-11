using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using ScrumFactory.Services;
using System.Configuration;
using ScrumFactory.Composition.View;
using System.Diagnostics;
using System.Deployment.Application;
using CassiniDev;

namespace ScrumFactory.Login.ViewModel {


    /// <summary>
    /// Login view model.
    /// </summary>
    [Export(typeof(LoginViewModel))]
    [Export(typeof(ITopMenuViewModel))]
    public class LoginViewModel : BasePanelViewModel, ITopMenuViewModel, INotifyPropertyChanged {

        public CassiniDevServer LocalServer { private get; set; }


        private IAuthorizationService authorizator;
        private IEventAggregator aggregator;
        private IBackgroundExecutor executor;
        private IDialogService dialogs;

        private IServerUrl serverUrl;

        private bool signing = false;

        private bool validatingTokenAtServer = false;

        [ImportingConstructor]
        public LoginViewModel(
            [Import(typeof(IEventAggregator))] IEventAggregator aggregator,
            [Import(typeof(IBackgroundExecutor))] IBackgroundExecutor executor,
            [Import(typeof(IAuthorizationService))] IAuthorizationService authorizator,
            [Import(typeof(IDialogService))] IDialogService dialogService,
            [Import] IServerUrl serverUrl,
            [ImportMany(typeof(Services.AuthProviders.IOAuthProvider))] IEnumerable<Services.AuthProviders.IOAuthProvider> providers) {

                this.aggregator = aggregator;
                this.executor = executor;
                this.authorizator = authorizator;
                this.dialogs = dialogService;
                this.serverUrl = serverUrl;

                AllProviders = providers.OrderBy(p =>p.ProviderName);

                aggregator.Subscribe(ScrumFactoryEvent.ShellStarted, TryToSignIn);
                aggregator.Subscribe(ScrumFactoryEvent.NotAuthorizedServerExceptionThrowed, TryToSignIn);
                aggregator.Subscribe(ScrumFactoryEvent.ShowLogin, Show);
                aggregator.Subscribe<string>(ScrumFactoryEvent.ServerArgOnInit, OnSetupServer);

                SignInCommand = new DelegateCommand<Services.AuthProviders.IOAuthProvider>(CanSignIn, p => { p.SignOut(); SignIn(p); });

                
                SetDemoServerCommand = new DelegateCommand(SetDemoServer);
                SetStandAloneServerCommand = new DelegateCommand(SetStandAloneServer);
                SetCompanyServerCommand = new DelegateCommand(SetCompanyServer);

                ShowServerPanelCommand = new DelegateCommand(() => { ShowServerSelection = true; IsServerUrlVisible = false; });
                ShowProviderPanelCommand = new DelegateCommand(() => { ShowServerSelection = false; });

                ServerUrl = Properties.Settings.Default.serverUrl;

                System.Windows.Application.Current.Exit += Current_Exit;

                if (Properties.Settings.Default.lastLoginWasLocal)
                    StartLocalServer();

        }

        void Current_Exit(object sender, System.Windows.ExitEventArgs e) {            
            StopLocalServer();
        }

        private bool serverRunning = false;
        void StartLocalServer() {
            if(LocalServer==null)
                LocalServer = new CassiniDevServer();
            if (serverRunning)
                return;
            LocalServer.StartServer(System.IO.Path.Combine(Environment.CurrentDirectory, "_localServerRoot"));
            serverRunning = true;
        }

        void StopLocalServer() {
            if (LocalServer == null)
                return;
            if (!serverRunning)
                return;
            LocalServer.StopServer();
            LocalServer = null;
            serverRunning = false;
        }


        private bool showServerSelection = true;
        public bool ShowServerSelection {
            get {
                if (IsSigning)
                    return false;
                return showServerSelection;
            }
            set {
                showServerSelection = value;
                OnPropertyChanged("ShowServerSelection");
                OnPropertyChanged("ShowProviderSelection");
            }
        }
        
        public bool ShowProviderSelection {
            get {
                if (IsSigning)
                    return false;
                return !showServerSelection;
            }            
        }
        

        private void TryToSignIn() {            
            SignIn(GetLastSelectedProvider());
        }

        private bool isServerUrlVisible;
        public bool IsServerUrlVisible {
            get {
                return isServerUrlVisible;
            }
            set {
                isServerUrlVisible = value;
                OnPropertyChanged("IsServerUrlVisible");
            }
        }


        public bool IsPublicServer {
            get {
                return (ServerUrl == "http://www.scrum-factory.com/demo" || ServerUrl == "http://public.scrum-factory.com" || ServerUrl == "http://www.scrum-factory.com/hub");
            }
        }

        public bool IsStandAloneServer {
            get {
                if (LocalServer == null)
                    return false;
                return ServerUrl + "/" == LocalServer.RootUrl;
            }
        }

        public bool IsCompanyServer {
            get {
                return !IsPublicServer && !IsStandAloneServer;
            }
        }

        private void SetStandAloneServer() {
            StartLocalServer();
            ServerUrl = LocalServer.RootUrl.Substring(0, LocalServer.RootUrl.Length - 1);
            ShowServerSelection = false;
        }

        private void SetDemoServer() {
            StopLocalServer();
            ServerUrl = "http://www.scrum-factory.com/hub";
            ShowServerSelection = false;
        }

        private void SetCompanyServer() {
            StopLocalServer();
            ServerUrl = "http://your-company-server";            
            IsServerUrlVisible = true;            
        }



        private void OnSetupServer(string server) {
            this.ServerUrl = server;
            Properties.Settings.Default.Save();
        }

        public IEnumerable<Services.AuthProviders.IOAuthProvider> AllProviders { get; private set; }

        
        

        public string ServerUrl { 
            get {
                return serverUrl.Url;
            }
            set {
                serverUrl.Url = value;
                Properties.Settings.Default.serverUrl = value;
                ScrumFactory.Windows.Helpers.Converters.MemberAvatarUrlConverter.ServerUrl = serverUrl.Url;
                IsServerUrlVisible = !IsPublicServer;
                OnPropertyChanged("ServerUrl");
                OnPropertyChanged("IsPublicServer");
                OnPropertyChanged("IsStandAloneServer");
                OnPropertyChanged("IsCompanyServer");                
            }
        }

       
        private string titleMessage;
        public string TitleMessage {
            get {
                return titleMessage;
            }
            set {
                titleMessage = value;
                OnPropertyChanged("TitleMessage");
            }
        }

        public bool IsSigning {
            get {
                return signing;
            }
            set {
                signing = value;
                OnPropertyChanged("Signing");
                OnPropertyChanged("IsSigning");
                OnPropertyChanged("ShowServerSelection");
                OnPropertyChanged("ShowProviderSelection");
                ((DelegateCommand<ScrumFactory.Services.AuthProviders.IOAuthProvider>)SignInCommand).NotifyCanExecuteChanged();
                aggregator.Publish<bool>(ScrumFactoryEvent.Signing, signing);
            }
        }

        

        private Services.AuthProviders.IOAuthProvider selectedProvider;
        public Services.AuthProviders.IOAuthProvider SelectedProvider {
            get {
                return selectedProvider;
            }
            set {
                selectedProvider = value;                
                OnPropertyChanged("SelectedProvider");
                if(selectedProvider!=null)
                    Properties.Settings.Default.lastProvider = selectedProvider.ProviderName;
                else
                    Properties.Settings.Default.lastProvider = null;
            }
        }

        private void Close() {
            if (!View.IsVisible)
                return;
            dialogs.GoToFirstTopMenu();
        }

        private bool ValidateTokenAtServer(string memberUId=null) {

            

            LoginStatusMessage = null;

            if (SelectedProvider == null || String.IsNullOrEmpty(SelectedProvider.ACCESS_TOKEN))                
                return false;
            
            string token = SelectedProvider.ACCESS_TOKEN;

            // validates token with Scrum Factory Server
            MemberProfile myProfile = null;
            validatingTokenAtServer = true;
            try {
                myProfile = authorizator.SignInMember(SelectedProvider.ProviderName, token, memberUId);
                if (myProfile==null || myProfile.IsActive == false) {
                    LoginStatusMessage = Properties.Resources.user_blocked_message;
                }
            }
            catch (ScrumFactory.Exceptions.VersionMissmatchException ex) {
                if (ex.IsServerVersionNewer)
                    LoginStatusMessage = Properties.Resources.old_client_version_message;
                else
                    LoginStatusMessage = Properties.Resources.old_server_version_message;
            }
            catch (ScrumFactory.Exceptions.AuthorizationProviderNotSupportedException) {
                LoginStatusMessage = String.Format(Properties.Resources.Authorization_provider_not_supported, SelectedProvider.ProviderName, ServerUrl);
            }
            catch (ScrumFactory.Exceptions.NotFoundException) {
                LoginStatusMessage = Properties.Resources.Login_method_not_found;
            }
            catch (ScrumFactory.Exceptions.NotAuthorizedException) {
            }
            catch (Exception ex) {
                LoginStatusMessage = Properties.Resources.Factory_signin_error + "\n" + ex.Message;
            }

            validatingTokenAtServer = false;

            if (myProfile == null) {                
                return false;                
            }

            Properties.Settings.Default.lastLoginWasLocal = IsStandAloneServer;
            Properties.Settings.Default.Save();
            
            OnPropertyChanged("IsMemberSigned");
            OnPropertyChanged("SignedMemberProfile");
            
            return true;

        }

        private bool CanSignIn() {            
            return !IsSigning; 
        }

        private void SignIn(Services.AuthProviders.IOAuthProvider provider) {
            IsServerUrlVisible = false;
            SignIn(provider, null);
        }

        private void SignIn(Services.AuthProviders.IOAuthProvider provider, string memberUId) {

            if (validatingTokenAtServer)
                return;

            // if no provider is selected yet, shows the login view model for selection
            if (provider == null) {
                Show();
                return;
            }

            SelectedProvider = provider;            
            TitleMessage = Properties.Resources.Signing;

            // tells everyone that we are signin
            IsSigning = true;
            dialogs.CloseAlertMessage();
            
            executor.StartBackgroundTask(
                () => {


                    // validate access token at server
                    if (ValidateTokenAtServer(memberUId))
                        return false;

                    // try to refresh token
                    SelectedProvider.RefreshAccesToken();
                    if (ValidateTokenAtServer(memberUId))
                        return false;
                    
                    // if no error, needs to open auth window
                    if(String.IsNullOrEmpty(LoginStatusMessage))
                        return true;

                    return false;
                    
                },
                AfterSignIn);
        }

        private void AfterSignIn(bool needOpenAuthWindow) {

            string memberUId = null; // used only for client-side

            // could not use the token, opens the auth Window
            if (needOpenAuthWindow && SelectedProvider!=null) {                
                SelectedProvider.SignOut();
                Show();
                if (SelectedProvider.LoginType == Services.AuthProviders.TokenGet.SERVER_SIDE) {
                    OAuthWindow oauthWindow = new OAuthWindow();
                    oauthWindow.ShowDialog(SelectedProvider);
                }
                else {
                    OAuthWindowClientSide oauthWindow = new OAuthWindowClientSide();
                    oauthWindow.ShowDialog(SelectedProvider);
                    memberUId = oauthWindow.MemberUId;
                }
                
                if (SelectedProvider != null && !String.IsNullOrEmpty(SelectedProvider.ACCESS_TOKEN))
                    SignIn(SelectedProvider, memberUId);
                else
                    IsSigning = false;

                return;
            }

            
            // sucess!!
            if (authorizator.SignedMemberProfile != null) {
            
                // closes the loginview model, and tells everyone the new signed member
                Close();

                ScrumFactory.Windows.Helpers.Converters.MemberAvatarUrlConverter.SignedMember = authorizator.SignedMemberProfile;
                ScrumFactory.Windows.Helpers.Converters.MemberAvatarUrlConverter.ResetCache();
                
                aggregator.Publish<MemberProfile>(ScrumFactoryEvent.SignedMemberChanged, authorizator.SignedMemberProfile);

                // not signing any more
                IsSigning = false;

                return;
            }

            // if could not sign in
            dialogs.ShowAlertMessage(Properties.Resources.Factory_could_not_validate_credentials, LoginStatusMessage, null);

            // not signing any more
            IsSigning = false;


        }

        private void SignOut() {
            authorizator.SignOutMember(SelectedProvider.ProviderName);

            if (SelectedProvider != null) {
                SelectedProvider.SignOut();
                SelectedProvider = null;
                Properties.Settings.Default.Save();
            }

            aggregator.Publish<MemberProfile>(ScrumFactoryEvent.SignedMemberChanged, null);
        }
        
        private string loginStatusMessage;
        public string LoginStatusMessage {
            get {
                return loginStatusMessage;
            }
            set {
                loginStatusMessage = value;
                OnPropertyChanged("LoginStatusMessage");
            }
        }

        public System.Windows.Input.ICommand SignInCommand {
            get;
            set;
        }

        public System.Windows.Input.ICommand SetStandAloneServerCommand { get; set; }
        public System.Windows.Input.ICommand SetDemoServerCommand { get; set; }
        public System.Windows.Input.ICommand SetCompanyServerCommand { get; set; }

        public System.Windows.Input.ICommand ShowServerPanelCommand { get; set; }
        public System.Windows.Input.ICommand ShowProviderPanelCommand { get; set; }
        

        /// <summary>
        /// Gets or sets the view.
        /// </summary>
        /// <value>The view.</value>        
        [Import(typeof(Login))]
        public IView View { get; set; }



        public void Show() {

            
            OnPropertyChanged("IsPublicServer");
            OnPropertyChanged("IsServerUrlVisible");

            //if (View.IsVisible)
            //    return;

            try {
                if (authorizator.SignedMemberProfile != null)
                    SignOut();
            } catch (Exception) { }

            dialogs.SelectTopMenu(this);            
        }

        private Services.AuthProviders.IOAuthProvider GetLastSelectedProvider() {
            return AllProviders.SingleOrDefault(p => p.ProviderName == Properties.Settings.Default.lastProvider);
            
        }
               
        public string PanelName {
            get { return Properties.Resources.Sign_in; }
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

}
