using System.ServiceModel;
using System.Collections.Generic;

namespace ScrumFactory.Services {

    [ServiceContract]
    public interface IProposalsService {

        [OperationContract]
        ICollection<Proposal> GetProjectProposals(string projectUId);

        [OperationContract]
        Proposal GetProjectProposal(string projectUId, string proposalUId);

        [OperationContract]
        ProposalDocument GetProposalDocument(string projectUId, string proposalUId);

        [OperationContract]
        Proposal AddProposal(string projectUId, Proposal newProposal);

        [OperationContract]
        void UpdateProposal(string projectUId, Proposal proposal);

        [OperationContract]
        RoleHourCost[] GetHourCosts(string projectUId);

        [OperationContract]
        void UpdateHourCosts(string projectUId, RoleHourCost[] hourCosts);

        [OperationContract]
        void ApproveProposal(string projectUId, string proposalUId, string documentXAML);

        [OperationContract]
        void RejectProposal(string projectUId, string proposalUId, string rejectReason, string documentXAML);

        [OperationContract]
        decimal GetBudgetIndicator(string projectUId);

        [OperationContract]
        string[] GetProposalTemplates();

        bool IsItemAtAnyProposal(string backlogItemUId);
    }
}
