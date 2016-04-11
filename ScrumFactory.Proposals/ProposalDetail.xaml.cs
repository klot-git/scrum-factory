using System.ComponentModel.Composition;
using ScrumFactory.Proposals.ViewModel;
using System.Windows.Controls;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Proposals {

    
    [Export]
    public partial class ProposalDetail : UserControl, IView {

        private object model;

        public ProposalDetail() {
            InitializeComponent();
        }

        [Import(typeof(ProposalViewModel))]
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
