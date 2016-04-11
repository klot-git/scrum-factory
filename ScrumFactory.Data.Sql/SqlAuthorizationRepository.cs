using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Objects;
using System.Data.EntityClient;
using System.Transactions;
using System.Linq;

namespace ScrumFactory.Data.Sql {

    [Export(typeof(IAuthorizationRepository))]
    public class SqlAuthorizationRepository : IAuthorizationRepository {

        private string connectionString;
        

        [ImportingConstructor]
        public SqlAuthorizationRepository([Import("ScrumFactoryEntitiesConnectionString")] string connectionString) {
            this.connectionString = connectionString;
        }

        public AuthorizationInfo GetAuthorizationInfo(string token) {            
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                System.DateTime limit = System.DateTime.Now.AddHours(-24);
                return context.AuthorizationInfos.Where(a => a.Token == token && a.IssueDate > limit).FirstOrDefault();
            };    
        }

        public void SaveAuthorizationInfo(AuthorizationInfo info) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {

                AuthorizationInfo oldInfo = context.AuthorizationInfos.Where(a => a.MemberUId == info.MemberUId).FirstOrDefault();

                if (oldInfo == null)
                    context.AuthorizationInfos.AddObject(info);
                else {
                    context.AttachTo("AuthorizationInfos", oldInfo);
                    oldInfo.Token = info.Token;
                    oldInfo.IssueDate = info.IssueDate;
                }

                context.SaveChanges();

            };            
        }

        
    }
}
