namespace ScrumFactory.Data.Sql {
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Data.Objects;
    using System.Data.EntityClient;
    using System.Transactions;
    using System.Linq;

    [Export(typeof(IProposalsRepository))]
    public class SqlProposalsRepository : IProposalsRepository {
        private string connectionString;

        [ImportingConstructor()]
        public SqlProposalsRepository(
            [Import("ScrumFactoryEntitiesConnectionString")]
            string connectionString) {
            this.connectionString = connectionString;
        }

        public ICollection<Proposal> GetProjectProposals(string projectUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.Proposals.Where(p => p.ProjectUId == projectUId).ToList();
            }
        }

        public Proposal GetProjectProposal(string projectUId, string proposalUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.Proposals.Include("Clauses").Include("Items").Include("FixedCosts").SingleOrDefault(p => p.ProjectUId == projectUId && p.ProposalUId == proposalUId);
            }
        }

        public ProposalDocument GetProposalDocument(string proposalUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.ProposalDocuments.SingleOrDefault(p => p.ProposalUId == proposalUId);
            }
        }

        public void SaveProposal(Proposal proposal) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {

                Proposal oldProposal = GetProjectProposal(proposal.ProjectUId, proposal.ProposalUId);

                using (TransactionScope scope = new TransactionScope()) {

                    // if its a new proposal
                    if (oldProposal == null) {
                        
                        // proposals
                        context.Proposals.AddObject(proposal);

                        // items
                        foreach (ProposalItem pitem in proposal.Items)
                            context.ProposalItems.AddObject(pitem);

                        // clauses
                        if(proposal.Clauses!=null) {
                        foreach (ProposalClause clause in proposal.Clauses)
                            context.ProposalClauses.AddObject(clause);
                            }

                    }
                    // if is an old one
                    else {

                        context.AttachTo("Proposals", oldProposal);
                        context.ApplyCurrentValues<Proposal>("Proposals", proposal);

                        // if is approving a proposal, adds its XAML document
                        if (proposal.ProposalDocument != null)
                            context.ProposalDocuments.AddObject(proposal.ProposalDocument);

                        if (oldProposal.Items == null)
                            oldProposal.Items = new List<ProposalItem>();

                        if (proposal.Items == null)
                            proposal.Items = new List<ProposalItem>();

                        UpdateProposalItems(context, proposal, oldProposal);

                        UpdateProposalClauses(context, proposal, oldProposal);

                        UpdateProposalFixedCosts(context, proposal, oldProposal);

                    }

                    context.SaveChanges();

                    scope.Complete();
                }

             
            }
        }

        private void UpdateProposalItems(ScrumFactoryEntities context, Proposal proposal, Proposal oldProposal) {

            // get new items added at the proposal and the removed ones
            ProposalItem[] newItems = new ProposalItem[0];            
            if (proposal.Items != null)
                newItems = proposal.Items.Where(i => !oldProposal.Items.Any(oi => oi.BacklogItemUId == i.BacklogItemUId)).ToArray();
            ProposalItem[] deletedItems = new ProposalItem[0];
            if (oldProposal.Items != null)
                deletedItems = oldProposal.Items.Where(oi => !proposal.Items.Any(i => i.BacklogItemUId == oi.BacklogItemUId)).ToArray();

            // add and delete proposal items
            foreach (ProposalItem item in newItems)
                context.ProposalItems.AddObject(item);
            foreach (ProposalItem item in deletedItems)
                context.ProposalItems.DeleteObject(item);
        }

        private void UpdateProposalFixedCosts(ScrumFactoryEntities context, Proposal proposal, Proposal oldProposal) {

            // make sure no proposal has null collections
            if (proposal.FixedCosts == null)
                proposal.FixedCosts = new List<ProposalFixedCost>();
            if (oldProposal.FixedCosts == null)
                oldProposal.FixedCosts = new List<ProposalFixedCost>();

            ProposalFixedCost[] newCosts = new ProposalFixedCost[0];
            newCosts = proposal.FixedCosts.Where(i => !oldProposal.FixedCosts.Any(oi => oi.ProposalFixedCostUId == i.ProposalFixedCostUId)).ToArray();

            ProposalFixedCost[] deletedCosts = new ProposalFixedCost[0];
            deletedCosts = oldProposal.FixedCosts.Where(oi => !proposal.FixedCosts.Any(i => i.ProposalFixedCostUId == oi.ProposalFixedCostUId)).ToArray();

            ProposalFixedCost[] updatedCosts = new ProposalFixedCost[0];
            updatedCosts = proposal.FixedCosts.Where(oi => oldProposal.FixedCosts.Any(i => i.ProposalFixedCostUId == oi.ProposalFixedCostUId)).ToArray();

            // add/update/delete proposal items
            foreach (ProposalFixedCost cost in newCosts)
                context.ProposalFixedCosts.AddObject(cost);
            foreach (ProposalFixedCost cost in deletedCosts)
                context.ProposalFixedCosts.DeleteObject(cost);
            foreach (ProposalFixedCost cost in updatedCosts)                 
                context.ApplyCurrentValues<ProposalFixedCost>("ProposalFixedCosts", cost);                
            
        }

        private void UpdateProposalClauses(ScrumFactoryEntities context, Proposal proposal, Proposal oldProposal) {

            while (oldProposal.Clauses.Count > 0) 
                context.ProposalClauses.DeleteObject(oldProposal.Clauses.First());
            
            foreach (ProposalClause clause in proposal.Clauses)
                context.ProposalClauses.AddObject(clause);
        }

        public void CreateHourCosts(Project project, Project similarProject) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {

                // get similar costs
                RoleHourCost[] similarCosts = null;
                if(similarProject!=null)
                    similarCosts = GetHourCosts(similarProject.ProjectUId);

                // fore each role at this project
                foreach (Role r in project.Roles) {

                    // checks if there is a role with the same shortname at the similar project, and if so, uses its costs
                    Role similarRole = null;
                    string roleName = r.RoleShortName.ToLower();
                    if(similarProject!=null)
                       similarRole = similarProject.Roles.FirstOrDefault(sr => sr.RoleShortName.ToLower() == roleName);

                    RoleHourCost similarHourCost = null;
                    if(similarRole!=null && similarCosts!=null)
                        similarHourCost = similarCosts.FirstOrDefault(c => c.RoleUId == similarRole.RoleUId);
                    if(similarHourCost==null)
                        similarHourCost = new RoleHourCost() { Price = 0, Cost = 0 };

                    // only if role is new
                    RoleHourCost oldCost = context.RoleHourCosts.SingleOrDefault(h => h.RoleUId == r.RoleUId);
                    if (oldCost == null) {
                        RoleHourCost cost = new RoleHourCost() { RoleUId = r.RoleUId, ProjectUId = project.ProjectUId, Cost = similarHourCost.Cost, Price = similarHourCost.Price };
                        context.RoleHourCosts.AddObject(cost);
                    }
                    
                }
                context.SaveChanges();
            }
        }

        public RoleHourCost[] GetHourCosts(string projectUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.RoleHourCosts.Where(c => c.ProjectUId == projectUId).ToArray();
            }
        }

        public void SaveHourCosts(RoleHourCost[] costs) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                foreach (RoleHourCost cost in costs)
                    SaveHourCost(cost, context);
                context.SaveChanges();
            }
        }

        private void SaveHourCost(RoleHourCost cost, ScrumFactoryEntities context) {
            RoleHourCost oldCost = context.RoleHourCosts.SingleOrDefault(c => c.RoleUId == cost.RoleUId);
            if (oldCost == null) {
                context.RoleHourCosts.AddObject(cost);
                return;
            }
            context.AttachTo("RoleHourCosts", oldCost);
            context.ApplyCurrentValues<RoleHourCost>("RoleHourCosts", cost);

        }

        public decimal GetProjectBudget(string projectUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {

                decimal? budget = context.Proposals
                    .Where(p => p.ProjectUId == projectUId && p.ProposalStatus == (short)ProposalStatus.PROPOSAL_APPROVED)                    
                    .Sum(p => (decimal?) p.TotalValue);
                if (!budget.HasValue)
                    budget = 0;
                
                decimal? fixedCosts = context.Proposals
                    .Where(p => p.ProjectUId == projectUId && p.ProposalStatus == (short)ProposalStatus.PROPOSAL_APPROVED)
                    .Sum(p => (decimal?)p.FixedCosts.Where(f => f.RepassToClient==false).Sum(f => f.Cost));                
                if (!fixedCosts.HasValue)
                    fixedCosts = 0;

                return (decimal) budget.Value - fixedCosts.Value;
            }
        }

        public bool IsItemAtAnyProposal(string backlogItemUId) {
            using (var context = new ScrumFactoryEntities(this.connectionString)) {
                return context.ProposalItems.Any(p => p.BacklogItemUId == backlogItemUId);
            }
        }
        

    }
}
