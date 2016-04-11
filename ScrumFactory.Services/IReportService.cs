using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace ScrumFactory.Services {

    [ServiceContract]
    public interface IReportService {

        [OperationContract]
        byte[] GetReport(string templateGroup, string template, string projectUId, string format, string proposalUId);

    }
}
