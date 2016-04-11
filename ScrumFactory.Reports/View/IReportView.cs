using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScrumFactory.Composition.View;
using System.Windows.Documents;


namespace ScrumFactory.Reports.View {

    public interface IReportView : IView {

        
        void SetReportDocument(string xaml, string title, ReportHelper.Report reports, ReportHelper.ReportConfig config);

        void SetElementViewModel(string name, object model);

        bool Print();

        FlowDocument Report { get; }
    }
}
