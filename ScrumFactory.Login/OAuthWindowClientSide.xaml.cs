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
using ScrumFactory.Services.AuthProviders;

namespace ScrumFactory.Login {
    /// <summary>
    /// Interaction logic for OAuthWindowClientSide.xaml
    /// </summary>
    public partial class OAuthWindowClientSide : Window {
     

         private IOAuthProvider provider;

         public OAuthWindowClientSide() {
            InitializeComponent();
        }

         public string MemberUId { get; private set; }

         public void ShowDialog(ScrumFactory.Services.AuthProviders.IOAuthProvider provider) {
             if (provider == null)
                 return;

             this.provider = provider;
             this.Title = provider.ProviderName;
             this.badLogin.Text = String.Format(Properties.Resources.bad_login, provider.ProviderName);
             this.badLogin.Visibility = Visibility.Collapsed;
             this.ShowDialog();
         }

         private void Button_Click(object sender, RoutedEventArgs e) {

             this.badLogin.Visibility = Visibility.Collapsed;
             bool ok = provider.GetAuthorizationToken(user.Text, pass.Password);
             if (ok) {
                 MemberUId = user.Text;
                 this.Close();
                 return;
             }
             this.badLogin.Visibility = Visibility.Visible;
         }
    }
}
