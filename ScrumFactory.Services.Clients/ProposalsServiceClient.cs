using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Net.Http.Formatting;
using ScrumFactory.Extensions;


namespace ScrumFactory.Services.Clients {

    [Export(typeof(IProposalsService))]
    public class ProposalsServiceClient : IProposalsService {

        [Import]
        private IClientHelper ClientHelper { get; set; }

        [Import(typeof(IServerUrl))]
        private IServerUrl serverUrl { get; set; }

        [Import("ProposalsServiceUrl")]
        private string serviceUrl { get; set; }

        [Import(typeof(IAuthorizationService))]
        private IAuthorizationService authorizationService { get; set; }


        private Uri Url(string relative) {
            return new Uri(serviceUrl.Replace("$ScrumFactoryServer$", serverUrl.Url) + relative);
        }

        public ICollection<Proposal> GetProjectProposals(string projectUId) {
            var client = ClientHelper.GetClient(authorizationService);            
            HttpResponseMessage response = client.Get(Url("ProjectProposals/" + projectUId + "/"));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ICollection<Proposal>>();
        }

        public Proposal GetProjectProposal(string projectUId, string proposalUId) {
            var client = ClientHelper.GetClient(authorizationService);
            HttpResponseMessage response = client.Get(Url("ProjectProposals/" + projectUId + "/" + proposalUId + "/"));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<Proposal>();
        }

        public ProposalDocument GetProposalDocument(string projectUId, string proposalUId) {
            var client = ClientHelper.GetClient(authorizationService);
            HttpResponseMessage response = client.Get(Url("ProjectProposals/" + projectUId + "/" + proposalUId + "/ProposalDocument"));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<ProposalDocument>();
        }

        public Proposal AddProposal(string projectUId, Proposal newProposal) {
            var client = ClientHelper.GetClient(authorizationService);
            HttpResponseMessage response = client.Post(Url("ProjectProposals/" + projectUId + "/"), new ObjectContent<Proposal>(newProposal, JsonMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<Proposal>();            
        }

        public void UpdateProposal(string projectUId, Proposal proposal) {
            var client = ClientHelper.GetClient(authorizationService);
            HttpResponseMessage response = client.Put(Url("ProjectProposals/" + projectUId + "/"), new ObjectContent<Proposal>(proposal, JsonMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);            
        }

        public RoleHourCost[] GetHourCosts(string projectUId) {
            var client = ClientHelper.GetClient(authorizationService);
            HttpResponseMessage response = client.Get(Url("ProjectRoleHoursCosts/" + projectUId + "/"));            
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<RoleHourCost[]>();
        }

        public void UpdateHourCosts(string projectUId, RoleHourCost[] hourCosts) {
            var client = ClientHelper.GetClient(authorizationService);            
            HttpResponseMessage response = client.Put(Url("ProjectRoleHoursCosts/" + projectUId + "/"), new ObjectContent<RoleHourCost[]>(hourCosts, JsonMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);            
        }

        public void ApproveProposal(string projectUId, string proposalUId, string documentXAML) {
            var client = ClientHelper.GetClient(authorizationService);            
            HttpResponseMessage response = client.Put(Url("ApprovedProposals/" + projectUId + "/" + proposalUId), new ObjectContent<string>(documentXAML, JsonMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);                    
        }

        public void RejectProposal(string projectUId, string proposalUId, string rejectReason, string documentXAML) {
            var client = ClientHelper.GetClient(authorizationService);            
            HttpResponseMessage response = client.Put(Url("RejectedProposals/" + projectUId + "/" + proposalUId + "/" + rejectReason), new ObjectContent<string>(documentXAML, JsonMediaTypeFormatter.DefaultMediaType));
            ClientHelper.HandleHTTPErrorCode(response);            
        }

        public decimal GetBudgetIndicator(string projectUId) {
            var client = ClientHelper.GetClient(authorizationService);
            HttpResponseMessage response = client.Get(Url("BudgetIndicator/" + projectUId));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<decimal>();
        }

        public string[] GetProposalTemplates() {
            var client = ClientHelper.GetClient(authorizationService);
            HttpResponseMessage response = client.Get(Url("ProposalTemplates"));
            ClientHelper.HandleHTTPErrorCode(response);
            return response.Content.ReadAs<string[]>();

        }

        public bool IsItemAtAnyProposal(string backlogItemUId) {
            throw new NotImplementedException();
        }
    }
}
