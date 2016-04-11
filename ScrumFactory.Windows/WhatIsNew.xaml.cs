using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Collections.Generic;
using ScrumFactory.Composition.View;
using System.Windows.Input;
using ScrumFactory.Windows.ViewModel;
using System;
using System.Windows.Documents;
using System.Windows;

namespace ScrumFactory.Windows{
    /// <summary>
    /// Interaction logic for WhatIsNew.xaml
    /// </summary>

    [Export]
    public partial class WhatIsNew : UserControl, IView {

        private object model;

        public WhatIsNew() {
            InitializeComponent();
            SetReportDocument();
        }

        [Import(typeof(WhatIsNewViewModel))]
        public object Model {
            get {
                return model;
            }
            set {
                model = value;
                DataContext = model;
            }
        }

        private void SetReportDocument()
        {
            documentReader.Document = Application.LoadComponent(new Uri("WhatIsNewDocument.xaml", UriKind.RelativeOrAbsolute)) as FlowDocument; ;
        }
    }
}