namespace ScrumFactory.Data {
    using System.Collections.Generic;

    public interface IProposalsRepository {

        ICollection<Proposal> GetProjectProposals(string projectUId);
        Proposal GetProjectProposal(string projectUId, string proposalUId);
        ProposalDocument GetProposalDocument(string proposalUId);
        void SaveProposal(Proposal proposal);

        void CreateHourCosts(Project project, Project similarProject);
        RoleHourCost[] GetHourCosts(string projectUId);
        void SaveHourCosts(RoleHourCost[] costs);

        decimal GetProjectBudget(string projectUId);

        bool IsItemAtAnyProposal(string backlogItemUId);
        
    }
}
