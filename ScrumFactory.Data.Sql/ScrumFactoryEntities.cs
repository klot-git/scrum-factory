using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Objects;
using System.Data.EntityClient;
using ScrumFactory;



namespace ScrumFactory.Data {

    public class ScrumFactoryEntities : ObjectContext {


        public IObjectSet<ProjectMembership> ProjectMemberships { get; private set; }
        public IObjectSet<MemberProfile> MembersProfile { get; private set; }
        public IObjectSet<MemberAvatar> MembersAvatar { get; private set; }

        public IObjectSet<Task> Tasks { get; private set; }
        public IObjectSet<TaskDetail> TaskDetails { get; private set; }
        public IObjectSet<TaskInfo> TaskInfos { get; private set; }
        public IObjectSet<TaskTag> TaskTags { get; private set; }

        public IObjectSet<BacklogItemEffectiveHours> BacklogItemEffectiveHours { get; private set; }
     
        public IObjectSet<Project> Projects { get; private set; }
        public IObjectSet<Role> Roles { get; private set; }
        public IObjectSet<Sprint> Sprints { get; private set; }

        public IObjectSet<BacklogItem> BacklogItems { get; private set; }        
        public IObjectSet<PlannedHour> PlannedHours { get; private set; }
        public IObjectSet<ItemSize> ItemSizes { get; private set; }
        public IObjectSet<SizeIdealHour> SizeIdealHours { get; private set; }
        public IObjectSet<BacklogItemGroup> BacklogItemGroups { get; private set; }
        
        public IObjectSet<Proposal> Proposals { get; private set; }
        public IObjectSet<ProposalDocument> ProposalDocuments { get; private set; }
        public IObjectSet<ProposalItem> ProposalItems { get; private set; }
        public IObjectSet<ProposalClause> ProposalClauses { get; private set; }
        public IObjectSet<ProposalFixedCost> ProposalFixedCosts { get; private set; }
        public IObjectSet<RoleHourCost> RoleHourCosts { get; private set; }

        public IObjectSet<ProjectConstraint> ProjectConstraints { get; private set; }

        public IObjectSet<Risk> Risks { get; private set; }

        public IObjectSet<Artifact> Artifacts { get; private set; }

        public IObjectSet<AuthorizationInfo> AuthorizationInfos { get; private set; }

        public IObjectSet<CalendarDay> CalendarDays { get; private set; }

        public IObjectSet<PokerCard> PokerCards { get; private set; }

        /// <summary>
        /// Initializes a new ScrumFactoryEntities object using the connection string found in the 'ScrumFactoryEntities' section of the application configuration file.
        /// </summary>
        public ScrumFactoryEntities() : base("name=ScrumFactoryEntities", "ScrumFactoryEntities") {                        
            CreateObjectSets();
        }

        /// <summary>
        /// Initialize a new ScrumFactoryEntities object.
        /// </summary>
        public ScrumFactoryEntities(string connectionString) : base(connectionString, "ScrumFactoryEntities")
        {            
            CreateObjectSets();            
        }
    
        /// <summary>
        /// Initialize a new ScrumFactoryEntities object.
        /// </summary>
        public ScrumFactoryEntities(EntityConnection connection) : base(connection, "ScrumFactoryEntities")
        {            
            CreateObjectSets();

        }

        private void CreateObjectSets() {
            
            Projects = CreateObjectSet<Project>();                                    
            Sprints = CreateObjectSet<Sprint>();
            Roles = CreateObjectSet<Role>();

            Risks = CreateObjectSet<Risk>();

            BacklogItems = CreateObjectSet<BacklogItem>();
            PlannedHours = CreateObjectSet<PlannedHour>();
            ItemSizes = CreateObjectSet<ItemSize>();
            SizeIdealHours = CreateObjectSet<SizeIdealHour>();
            BacklogItemGroups = CreateObjectSet<BacklogItemGroup>();
                        
            MembersProfile = CreateObjectSet<MemberProfile>();
            ProjectMemberships = CreateObjectSet<ProjectMembership>();
            MembersAvatar = CreateObjectSet<MemberAvatar>();

            Tasks = CreateObjectSet<Task>();
            TaskInfos = CreateObjectSet<TaskInfo>();
            TaskDetails = CreateObjectSet<TaskDetail>();
            TaskTags = CreateObjectSet<TaskTag>();

            

            BacklogItemEffectiveHours = CreateObjectSet<BacklogItemEffectiveHours>();
            

            Proposals = CreateObjectSet<Proposal>();
            ProposalDocuments = CreateObjectSet<ProposalDocument>();
            ProposalClauses = CreateObjectSet<ProposalClause>();
            ProposalFixedCosts = CreateObjectSet<ProposalFixedCost>();
            ProposalItems = CreateObjectSet<ProposalItem>();
            RoleHourCosts = CreateObjectSet<RoleHourCost>();

            Artifacts = CreateObjectSet<Artifact>();

            AuthorizationInfos = CreateObjectSet<AuthorizationInfo>();

            CalendarDays = CreateObjectSet<CalendarDay>();

            ProjectConstraints = CreateObjectSet<ProjectConstraint>();

            PokerCards = CreateObjectSet<PokerCard>();
        }



    }
}



    public static class Extensions {

        public static IQueryable<TSource> Include<TSource> (this IQueryable<TSource> source, string path) {
            var objectQuery = source as ObjectQuery<TSource>;
            if (objectQuery != null) {
                return objectQuery.Include(path);
            }
            return source;
    }

}
