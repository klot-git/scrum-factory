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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScrumFactory.Composition.View;
using System.ComponentModel.Composition;

namespace ScrumFactory.Artifacts {
    /// <summary>
    /// Interaction logic for ArtifactDetail.xaml
    /// </summary>
    [Export]
    public partial class ArtifactDetail : UserControl, IView {
        public ArtifactDetail() {
            InitializeComponent();            
            //webb.LoadCompleted += new LoadCompletedEventHandler(webb_LoadCompleted);
        }

        private object model;

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>     
        [Import(typeof(Artifacts.ViewModel.ArtifactDetailViewModel))]
        public object Model {
            get {
                return this.model;
            }
            set {
                this.model = value;
                this.DataContext = value;
            }
        }

        
        public void CreateBrowserThumb(string url) {            
            //webb.Navigate(url);            
        }

        void webb_LoadCompleted(object sender, NavigationEventArgs e) {
            GenerateThumb();
        }

        private void GenerateThumb() {

            

            RenderTargetBitmap bmp = new RenderTargetBitmap(600, 500, 96, 96, PixelFormats.Pbgra32);

            bmp.Render(this);

            var encoder = new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(bmp));

            using (System.IO.Stream stm = System.IO.File.Create(@"C:\Users\marcos.martins\Desktop\test.png"))
                encoder.Save(stm);

        }

     


    }
}
