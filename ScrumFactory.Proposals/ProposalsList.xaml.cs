using System.ComponentModel.Composition;
using ScrumFactory.Proposals.ViewModel;
using System.Windows.Controls;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Proposals {

    /// <summary>
    /// Interaction logic for ProposalsList.xaml
    /// </summary>
    [Export]
    public partial class ProposalsList : UserControl, IView {

        private object model;

        public ProposalsList() {
            InitializeComponent();
        }

        [Import(typeof(ProposalsListViewModel))]
        public object Model {
            get {
                return model;
            }
            set {
                model = value;
                DataContext = model;
            }
        }
    }
}
