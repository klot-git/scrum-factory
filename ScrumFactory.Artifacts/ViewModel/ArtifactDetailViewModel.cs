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

namespace ScrumFactory.Artifacts.ViewModel {

    [Export]
    [Export(typeof(IStartsWithApp))]
    public class ArtifactDetailViewModel : BasePanelViewModel, IStartsWithApp, IViewModel, INotifyPropertyChanged  {

        private IAuthorizationService authorizator;
        private IEventAggregator aggregator;        
        private IDialogService dialogs;

        private IDialogViewModel window;

        public ICommand SearchPathCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand OnPathChangedCommand { get; set; }


        private Artifact editingArtifact = null;

        public Artifact EditingArtifact {
            get {
                return editingArtifact;
            }
            set {
                editingArtifact = value;
                OnPropertyChanged("EditingArtifact");
            }
        }

        public bool Canceled { get; set; }

        [ImportingConstructor]
        public ArtifactDetailViewModel(
            [Import(typeof(IEventAggregator))] IEventAggregator aggregator,            
            [Import(typeof(IAuthorizationService))] IAuthorizationService authorizator,
            [Import(typeof(IDialogService))] IDialogService dialogService) {

            this.aggregator = aggregator;            
            this.authorizator = authorizator;
            this.dialogs = dialogService;

            OnPathChangedCommand = new DelegateCommand(PathChanged);
            SearchPathCommand = new DelegateCommand(SearchPath);
            AddCommand = new DelegateCommand(Add);

        }


        [Import(typeof(ArtifactDetail))]
        public IView View { get; set; }

        private void Add() {

            //((ArtifactDetail)View).CreateBrowserThumb(EditingArtifact.ArtifactPath);

            Canceled = false;
            window.Close();
        }

        public void Show() {

            if (View.IsVisible)
                return;

            if (EditingArtifact == null)
                EditingArtifact = new Artifact() { ArtifactUId = Guid.NewGuid().ToString() };

            Canceled = true;

            window = dialogs.NewDialog(Properties.Resources.Links_AND_Resources, View);
            window.Show();

            

        }

        private string GetArtifactShortName(string fullName) {
            if (fullName == null)
                return "";
            string shortName = fullName;
            int slashIdx = shortName.LastIndexOf('/');
            if (slashIdx < 0)
                slashIdx = shortName.LastIndexOf('\\');

            shortName = shortName.Substring(slashIdx + 1);

            return shortName;
        }

        private void PathChanged() {
            if (!String.IsNullOrEmpty(EditingArtifact.ArtifactName))
                return;
            EditingArtifact.ArtifactName = GetArtifactShortName(EditingArtifact.ArtifactPath);
            OnPropertyChanged("EditingArtifact");
        }

        private void SearchPath() {
            string artifactPath = dialogs.ShowOpenFileDialog(Properties.Resources.Add_artifact, null, null, true);
            if (artifactPath == null)
                return;
                    
            EditingArtifact.ArtifactPath = artifactPath;
            PathChanged();
        }
    }

}
