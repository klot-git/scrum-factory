using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Data.Objects.SqlClient;

namespace ScrumFactory.Data.Sql {


    [Export(typeof(IProjectConstraintsRepository))]
    public class SqlProjectConstraintsRepository : IProjectConstraintsRepository {

        private string connectionString;

        [ImportingConstructor()]
        public SqlProjectConstraintsRepository([Import("ScrumFactoryEntitiesConnectionString")] string connectionString) {
            this.connectionString = connectionString;
        }

        public ICollection<ProjectConstraint> GetProjectConstraints(string projectUId) {
            using (var context = new ScrumFactoryEntities(connectionString)) {
                return context.ProjectConstraints.Where(c => c.ProjectUId == projectUId).ToList();
            }         
        }

        public void SaveProjectConstraint(ProjectConstraint constraint) {
            using (var context = new ScrumFactoryEntities(connectionString)) {
                var old = context.ProjectConstraints.SingleOrDefault(c => c.ConstraintUId == constraint.ConstraintUId);
                if (old != null) {
                    context.AttachTo("ProjectConstraints", old);
                    context.ApplyCurrentValues<ProjectConstraint>("ProjectConstraints", constraint);
                }
                else
                    context.ProjectConstraints.AddObject(constraint);
                context.SaveChanges();
            }
        }

        public void DeleteProjectConstraint(string constraintUId) {
            using (var context = new ScrumFactoryEntities(connectionString)) {
                var constraint = context.ProjectConstraints.SingleOrDefault(c => c.ConstraintUId == constraintUId);
                if (constraint == null)
                    return;
                context.ProjectConstraints.DeleteObject(constraint);
                context.SaveChanges();
            }
        }

        public double GetPointsFactor(string projectUId) {
            double factor = 1;
            using (var context = new ScrumFactoryEntities(connectionString)) {
                var constraints = context.ProjectConstraints.Where(c => c.ProjectUId == projectUId).ToList();                
                foreach (var constraint in constraints) {
                    if (constraint.AdjustPointFactor != 0) {
                        factor = factor * constraint.AdjustPointFactor;
                    }
                }
            }    
            return factor;
        }

    }
}