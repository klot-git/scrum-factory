using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;

namespace ScrumFactory.Services.Web.WindowsAuth {

    public partial class Default : System.Web.UI.Page {

        private ScrumFactory.Services.AuthProviders.IWindowsTokenStore tokenStore = null;

        const int LOGON32_LOGON_INTERACTIVE = 2;
        const int LOGON32_LOGON_NETWORK = 3;
        const int LOGON32_LOGON_BATCH = 4;
        const int LOGON32_LOGON_SERVICE = 5;
        const int LOGON32_LOGON_UNLOCK = 7;
        const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
        const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

        const int LOGON32_PROVIDER_DEFAULT = 0; 

           
        private string WindowsAutheticationServer {
            get {
                return System.Configuration.ConfigurationManager.AppSettings["WindowsAutheticationServer"];
            }
        }

        private string DomainName;

        private string GetFullUserName(string user) {
            if (String.IsNullOrEmpty(user))
                return user;
            if (user.IndexOf("@") > 0)
                return user.ToLower();

            if(user.IndexOf("\\") > 0)
                user = user.Substring(user.IndexOf("\\")+1);

            return (user + "@" + DomainName).ToLower();
        }

        private void GetTokenStore() {
            tokenStore = ScrumFactory.Services.Web.Application.CompositionContainer.GetExportedValue<AuthProviders.IWindowsTokenStore>();
        }

        private string WindowsUser {
            get {
                return GetFullUserName(Request.ServerVariables["LOGON_USER"]);
            }
        }

        protected void Page_Load(object sender, EventArgs e) {

            try {
                GetADContext();

                domainLiteral.Text = DomainName;
                widowsUserLiteral.Text = WindowsUser;


            }
            catch (Exception ex) {
                SetMessage(ex.Message);
                windowsAuthPanel.Visible = false;
                loginPanel.Visible = false;
                return;
            }
            
            if (!IsPostBack)
                SetDefaultPanelsVisibility();

        }

        private void SetDefaultPanelsVisibility() {
            if (!String.IsNullOrEmpty(WindowsUser)) {
                windowsAuthPanel.Visible = true;
                loginPanel.Visible = false;
            }
            else {
                windowsAuthPanel.Visible = false;
                loginPanel.Visible = true;
            }
        }

        protected void Show_LoginPanel_Click(object sender, EventArgs e) {

            loginPanel.Visible = true;
            windowsAuthPanel.Visible = false;
            LinkButton1.Visible = false;

        }

        protected void SigninWindowsUser_Click(object sender, EventArgs e) {
            GetTokenStore();
            string token = tokenStore.CreateTokenFor(WindowsUser);
            SuccessRedirect(token);
        }

        protected void Signin_Click(object sender, EventArgs e) {

            string user = GetFullUserName(userTextBox.Text);

            SetMessage("");

            if (!UserValidated(user, passwordTextBox.Text))
                return;

            GetTokenStore();

            string token = tokenStore.CreateTokenFor(user);

            SuccessRedirect(token);

        }

        private PrincipalContext GetADContext() {
            PrincipalContext adContext = null;

            if (String.IsNullOrEmpty(WindowsAutheticationServer)) {
                try {
                    adContext = new PrincipalContext(ContextType.Domain);
                    DomainName = System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain().Name;
                } catch (Exception) { }
                if (adContext == null) {
                    adContext = new PrincipalContext(ContextType.Machine);
                    if (adContext != null)
                        DomainName = adContext.Name ?? adContext.ConnectedServer;
                }
            } else {
                adContext = new PrincipalContext(ContextType.Domain, WindowsAutheticationServer);
                DomainName = WindowsAutheticationServer;
            }

            
            return adContext;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken
            );

        
        private bool UserValidated(string user, string password) {

            try {
                IntPtr hToken;
                if (LogonUser(user, DomainName, password,
                    LOGON32_LOGON_NETWORK, LOGON32_PROVIDER_DEFAULT, out hToken))
                    return true;
                else
                    SetMessage("Incorrect user or password.");
            }
            catch (Exception ex) {
                SetMessage("Error validating user at domain.<br/>" + ex.Message);
            }
            return false;


            //try {
            //    using (var adContext = GetADContext()) {
            //        if (adContext.ValidateCredentials(user, password))
            //            return true;
            //        else
            //            SetMessage("Incorrect user or password.");
            //    }
            //}
            //catch (Exception ex) {
            //    SetMessage("Error validating user at domain.<br/>" + ex.Message);
            //}
            
            //return false;            
        }

        private void SetMessage(string message) {
            messageLiteral.Text = message;
        }

        private void SuccessRedirect(string token) {
            Response.Redirect("AuthOk.aspx?code=" + token);
        }
    }
}