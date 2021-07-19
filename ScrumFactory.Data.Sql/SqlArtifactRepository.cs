using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Objects;
using System.Data.EntityClient;
using System.Transactions;
using System.Linq;

namespace ScrumFactory.Data.Sql {

    [Export(typeof(IArtifactRepository))]
    public class SqlArtifactRepository : IArtifactRepository {

        private string connectionString;
        

        [ImportingConstructor]
        public SqlArtifactRepository([Import("ScrumFactoryEntitiesConnectionString")] string connectionString) {
            this.connectionString = connectionString;
        }

        public Artifact GetArtifact(string artifactUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.Artifacts.SingleOrDefault(a => a.ArtifactUId == artifactUId);
            };                
        }

        public ICollection<Artifact> GetArtifacts(string contextUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.Artifacts.Where(a => a.ContextUId == contextUId).OrderBy(a => a.ArtifactName).ToList();                
            };                    
        }

        public void SaveArtifact(Artifact artifact) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {

                Artifact oldArtifact = context.Artifacts.SingleOrDefault(a => a.ArtifactUId == artifact.ArtifactUId);

                if (oldArtifact == null)
                    context.Artifacts.AddObject(artifact);
                else {
                    context.AttachTo("Artifacts", oldArtifact);
                    context.ApplyCurrentValues<Artifact>("Artifacts", artifact);
                }

                context.SaveChanges();

            };            
        }


        public void RemoveArtifact(string artifactUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                Artifact artifact = context.Artifacts.SingleOrDefault(a => a.ArtifactUId == artifactUId);
                if (artifact == null)
                    return;
                context.Artifacts.DeleteObject(artifact);
                context.SaveChanges();
            };
        }

        public int GetArtifactContextCount(string contextUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.Artifacts.Where(a => a.ContextUId == contextUId).Count();
            }
        }


    }
}
