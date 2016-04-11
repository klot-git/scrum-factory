using System.ComponentModel.Composition;
using System.Windows.Controls;
using ScrumFactory.Composition.View;


namespace ScrumFactory.Artifacts {

    /// <summary>
    /// Interaction logic for ArtifactsList.xaml
    /// </summary>
    
    public partial class ArtifactsList : UserControl, IView {


        private object model;
        
        public ArtifactsList() {
            InitializeComponent();
        }

        #region IView Members
        
    
        public object Model {
            get {
                return model;
            }
            set {
                model = value;
                DataContext = model;
            }
        }

        #endregion
    }

}
