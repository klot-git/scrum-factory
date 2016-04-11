using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Threading;

namespace ScrumFactory.Windows.Helpers.Extensions {

    /// <summary>
    /// Exeutes de URL bind async, to avoid slow DNS lookup at network with problems
    /// </summary>
    public class ImageAsyncHelper : Freezable {

        public static Uri GetSourceUri(DependencyObject obj) { return (Uri)obj.GetValue(SourceUriProperty); }
        public static void SetSourceUri(DependencyObject obj, Uri value) { obj.SetValue(SourceUriProperty, value); }

        public static readonly DependencyProperty SourceUriProperty = DependencyProperty.RegisterAttached("SourceUri", typeof(Uri), typeof(ImageAsyncHelper), new PropertyMetadata {
            PropertyChangedCallback = (obj, e) => {
                ImageAsyncHelper helper = new ImageAsyncHelper();
                helper.GivenUri = (Uri)e.NewValue;                

                //freeze to be used by other threads
                helper.Freeze();

                Binding bind = new Binding("VerifiedUri") { Source = helper, IsAsync = true };                
                ((Image)obj).SetBinding(Image.SourceProperty, bind);
            }
        });


        // Typical implementation of CreateInstanceCore 
        protected override Freezable CreateInstanceCore() {
            return new ImageAsyncHelper();
        }

        
        Uri GivenUri;
        public Uri VerifiedUri {
            get {                
                try {
                    Dns.GetHostEntry(GivenUri.DnsSafeHost);                    
                    return GivenUri;
                }
                catch (Exception) {
                    return null;
                }
                
            }
        }
    }
}
