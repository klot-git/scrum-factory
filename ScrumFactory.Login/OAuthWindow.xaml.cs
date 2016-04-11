using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScrumFactory.Login {
    /// <summary>
    /// Interaction logic for OAuthWindow.xaml
    /// </summary>
    public partial class OAuthWindow : Window {

        private ScrumFactory.Services.AuthProviders.IOAuthProvider provider = null;


        public OAuthWindow() {
            InitializeComponent();
            webView.ContextMenu = null;
            
            webView.LoadCompleted += webView_LoadCompleted;
        }

        public void ShowDialog(ScrumFactory.Services.AuthProviders.IOAuthProvider provider) {
            if (provider == null)
                return;

            this.provider = provider;
            webView.Navigate(provider.LoginUrl);

            this.Owner = Application.Current.MainWindow;

            this.ShowDialog();
        }

        void webView_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e) {

            if (provider == null) {
                return;
            }

            
            mshtml.HTMLDocument html = webView.Document as mshtml.HTMLDocument;
            if (html == null) {
                return;
            }

            string title = "";
            try {
                title = html.title;
            } catch (Exception) { }

            if (!provider.GetAuthorizationToken(e.Uri, title))
                return;

            provider.GetAccesToken();

            this.Close();

        }
    }
}
