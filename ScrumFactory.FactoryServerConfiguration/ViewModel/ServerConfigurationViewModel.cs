using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScrumFactory.Composition;
using System.ComponentModel.Composition;
using ScrumFactory.Composition.ViewModel;
using System.ComponentModel;
using System.Windows.Input;


namespace ScrumFactory.FactoryServerConfiguration.ViewModel {

    [Export]
    [Export(typeof(IFactoryServerConfigurationViewModel))]
    [Export(typeof(ITopMenuViewModel))]
    public class ServerConfigurationViewModel : BasePanelViewModel, IFactoryServerConfigurationViewModel, ITopMenuViewModel, INotifyPropertyChanged {

        private IBackgroundExecutor executor;
        private IEventAggregator aggregator;

        [Import]
        private IDialogService dialogs { get; set; }
        
        [Import]
        private IServerUrl ServerUrl { get; set; }

        [Import]
        private Services.IFactoryServerService factoryService { get; set; }

        private ScrumFactory.ServerConfiguration config;
        public ScrumFactory.ServerConfiguration Config {
            get {
                return config;
            }
            set {
                config = value;
                OnPropertyChanged("Config");
                OnPropertyChanged("v");
            }
        }

        public string ServerImageUrl {
            get {
                if (ServerUrl == null)
                    return String.Empty;
                return ServerUrl.Url + "/Images/Companylogo.png";
            }
        }

        [ImportingConstructor]
        public ServerConfigurationViewModel(
            [Import] IEventAggregator aggregator,
            [Import] IBackgroundExecutor executor) {

                this.aggregator = aggregator;
                this.executor = executor;

                aggregator.Subscribe<MemberProfile>(ScrumFactoryEvent.SignedMemberChanged, m => { NeedRefresh = true; });

                UploadLogoCommand = new DelegateCommand(UploadLogo);
                ApplyCommand = new DelegateCommand(Apply);
        }

        public void Show() {
            if (NeedRefresh)
                LoadData();
            dialogs.SetBackTopMenu();
            dialogs.SelectTopMenu(this);
        }

        private void LoadData() {
            IsLoadingData = true;
            executor.StartBackgroundTask<ScrumFactory.ServerConfiguration>(
                ()=> { return factoryService.GetConfiguration();},
                c => { 
                    Config = c;                    
                    IsLoadingData = false;
                    NeedRefresh = false;
                });
        }

        private void Apply() {
            IsLoadingData = true;
            executor.StartBackgroundTask(
                () => { factoryService.UpdateConfiguration(Config); },
                () => { 
                    IsLoadingData = false;
                    dialogs.GoBackSelectedTopMenu();
                });
        }

        private void UploadLogo() {

            Byte[] imageBytes = GetLogoImageAsBytes();
            if (imageBytes == null)
                return;

        }

        private Byte[] GetLogoImageAsBytes() {
            System.Drawing.Image image = null;
            Byte[] imageBytes = null;


            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Images|*.gif;*.jpg;*.png";

            bool? d = dialog.ShowDialog();
            if (d != true)
                return null;
            try {
                image = System.Drawing.Bitmap.FromFile(dialog.FileName);
            } catch (Exception) {
                //Windows.Error.ShowAlert(Properties.Resources.Member_image_is_invalid);
            }
            if (image == null)
                return null;

            System.Drawing.Image imageResized = AutoResize(image);


            try {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                imageResized.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                imageBytes = ms.ToArray();
            } catch (Exception) {
                
            }

            return imageBytes;
        }

        private System.Drawing.Image AutoResize(System.Drawing.Image originalImg) {
            double resizeFactor = 1;
            double MAX_DIMENSION = 200;
            int newWidth = originalImg.Width;
            int newHeight = originalImg.Height;


            // if image is bigger then MAX_DIMENSION x MAX_DIMENSION
            // calculates the new dimensiom
            if (originalImg.Width > MAX_DIMENSION || originalImg.Height > MAX_DIMENSION) {

                // use the lowest dimension to calculate the resize factor
                if (originalImg.Width < originalImg.Height)
                    resizeFactor = (double)(MAX_DIMENSION / originalImg.Width);
                else
                    resizeFactor = (double)(MAX_DIMENSION / originalImg.Height);

                newWidth = (int)(originalImg.Width * resizeFactor);
                newHeight = (int)(originalImg.Height * resizeFactor);

            }

            // Resize the image                                                
            System.Drawing.Bitmap newImg = new System.Drawing.Bitmap(originalImg, newWidth, newHeight);


            // Now crop it
            int m;
            System.Drawing.Rectangle crop;
            if (newImg.Width < newImg.Height) {
                m = (newImg.Height - newImg.Width) / 2;
                crop = new System.Drawing.Rectangle(0, m, newImg.Width, newImg.Width);
            } else {
                m = (newImg.Width - newImg.Height) / 2;
                crop = new System.Drawing.Rectangle(m, 0, newImg.Height, newImg.Height);
            }

            return newImg.Clone(crop, newImg.PixelFormat);
        }


        [Import(typeof(ServerConfiguration))]
        public Composition.View.IView View {
            get;
            set;
        }

        public string PanelName {
            get { return Properties.Resources.Server_settings; }
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
        
        public ICommand UploadLogoCommand { get; set; }
        public ICommand ApplyCommand { get; set; }
    }
}
