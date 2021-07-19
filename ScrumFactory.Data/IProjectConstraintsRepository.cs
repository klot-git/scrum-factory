using System.Collections.Generic;

namespace ScrumFactory.Data {

    public interface IProjectConstraintsRepository {
        ICollection<ProjectConstraint> GetProjectConstraints(string projectUId);
        void SaveProjectConstraint(ProjectConstraint constraint);
        void DeleteProjectConstraint(string constraintUId);
        double GetPointsFactor(string projectUId);
    }
}
