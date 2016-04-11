using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace ScrumFactory.Services {

    [ServiceContract]
    public interface IProjectConstraintsService {

        [OperationContract]
        ICollection<ProjectConstraint> GetProjectConstraints(string projectUId);

        [OperationContract]
        void AddProjectConstraint(string projectUId, ProjectConstraint constraint);

        [OperationContract]
        void UpdateProjectConstraint(string projectUId, ProjectConstraint constraint);

        [OperationContract]
        void RemoveProjectConstraint(string projectUId, string constraintUId);

        [OperationContract]
        ICollection<ProjectConstraint> GetDefaultContraints();

        [OperationContract]
        double GetPointsFactor(string projectUId);

    }
}
