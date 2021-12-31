using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Transactions;
using System;

namespace ScrumFactory.Services.Logic {

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    [Export(typeof(IProposalsService))]
    [Export(typeof(IProposalsService_ServerSide))]
    public class ProposalService : IProposalsService, IProposalsService_ServerSide {

        [Import]
        private Data.IProposalsRepository proposalsRepository { get; set; }

        [Import]
        private IAuthorizationService authorizationService { get; set; }

        [Import]
        private IProjectsService projectsService { get; set; }


        [Import]
        private ITasksService tasksService { get; set; }

        [Import]
        private IBacklogService backlogservice { get; set; }

        [Import]
        private IReportService reportService { get; set; }


        [WebGet(UriTemplate = "ProjectProposals/{projectUId}/", ResponseFormat = WebMessageFormat.Json)]
        public ICollection<Proposal> GetProjectProposals(string projectUId) {
            authorizationService.VerifyPermissionAtProjectOrFactoryOwner(projectUId, new PermissionSets[] { PermissionSets.SCRUM_MASTER });
            authorizationService.VerifyCanSeeProposalValues();
            return proposalsRepository.GetProjectProposals(projectUId);

        }

        [WebGet(UriTemplate = "ProjectProposals/{projectUId}/{proposalUId}/", ResponseFormat = WebMessageFormat.Json)]
        public Proposal GetProjectProposal(string projectUId, string proposalUId) {
            authorizationService.VerifyPermissionAtProjectOrFactoryOwner(projectUId, new PermissionSets[] { PermissionSets.SCRUM_MASTER });
            authorizationService.VerifyCanSeeProposalValues();
            return proposalsRepository.GetProjectProposal(projectUId, proposalUId);
        }

        [WebGet(UriTemplate = "ProjectProposals/{projectUId}/{proposalUId}/ProposalDocument", ResponseFormat = WebMessageFormat.Json)]
        public ProposalDocument GetProposalDocument(string projectUId, string proposalUId) {
            authorizationService.VerifyPermissionAtProjectOrFactoryOwner(projectUId, new PermissionSets[] { PermissionSets.SCRUM_MASTER });
            authorizationService.VerifyCanSeeProposalValues();
            return proposalsRepository.GetProposalDocument(proposalUId);
        }

        [WebInvoke(Method="POST", UriTemplate = "ProjectProposals/{projectUId}/", ResponseFormat = WebMessageFormat.Json)]
        public Proposal AddProposal(string projectUId, Proposal newProposal) {

            // verifies permission
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);
            authorizationService.VerifyCanSeeProposalValues();

            // get proposals project
            Project project = projectsService.GetProject(projectUId);

            // crete new proposal                        
            newProposal.CreateDate = DateTime.Now;
            newProposal.TemplateName = "default";

            // sets proposal template
            String[] templates = GetProposalTemplates();
            if (templates.Length > 0) {
                if(!templates.Contains("default"))
                    newProposal.TemplateName = templates.OrderBy(t => t).ToArray()[0];
            }
            

            // create hours costs if not exist at project
            RoleHourCost[] costs = proposalsRepository.GetHourCosts(projectUId);
            if (costs == null || costs.Length == 0) {
                // try to find a similar project                
                Project similar = projectsService.GetLastSimilarProject(projectUId, true);                
                proposalsRepository.CreateHourCosts(project, similar);
            }

            // include all backlog items at the proposal
            newProposal.Items = new List<ProposalItem>();
            ICollection<BacklogItem> projectItems = backlogservice.GetCurrentBacklog(projectUId, (short) BacklogFiltersMode.ALL);
            foreach (BacklogItem item in projectItems)
                newProposal.Items.Add(new ProposalItem() { ProposalUId = newProposal.ProposalUId, BacklogItemUId = item.BacklogItemUId, Item = item });

            // calcs total value
            newProposal.TotalValue = newProposal.CalcTotalPrice(costs);

            proposalsRepository.SaveProposal(newProposal);

            return newProposal;
        }

        
        public void RefreshHourCosts(string projectUId) {
            // try to find a similar project       
            Project project = projectsService.GetProject(projectUId);
            Project similar = projectsService.GetLastSimilarProject(projectUId, true);
            proposalsRepository.CreateHourCosts(project, similar);
        }

        [WebInvoke(Method = "PUT", UriTemplate = "ProjectProposals/{projectUId}/?revert={revert}", ResponseFormat = WebMessageFormat.Json)]
        public void UpdateProposal(string projectUId, Proposal proposal, bool revert = false) {

            // verifies permission
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);
            authorizationService.VerifyCanSeeProposalValues();

            if(revert)
            {
                RevertProposal(projectUId, proposal.ProposalUId);
                return;
            }

            proposalsRepository.SaveProposal(proposal);

        }

        private void RevertProposal(string projectUId, string proposalUId)
        {
            // update proposal status
            Proposal proposal = GetProjectProposal(projectUId, proposalUId);
            proposal.ApprovalDate = null;
            proposal.RejectReason = null;
            proposal.ProposalStatus = (short)ProposalStatus.PROPOSAL_WAITING;
            proposal.ApprovedBy = null;

            proposal.ProposalDocument = null;

            proposalsRepository.SaveProposal(proposal, true);

            // update project status
            Project project = projectsService.GetProject(projectUId);
            ICollection<Proposal> projectProposals = GetProjectProposals(projectUId);
            if (projectProposals.Any(pp => pp.ProposalStatus == (short)ProposalStatus.PROPOSAL_APPROVED))
                projectsService.ChangeProjectStatus(projectUId, "", (short)ProjectStatus.PROPOSAL_APPROVED);
            else
                projectsService.ChangeProjectStatus(projectUId, "", (short)ProjectStatus.PROPOSAL_CREATION);
        }

        [WebGet(UriTemplate = "ProjectRoleHoursCosts/{projectUId}/", ResponseFormat = WebMessageFormat.Json)]
        public RoleHourCost[] GetHourCosts(string projectUId) {

            // verifies permission
            authorizationService.VerifyPermissionAtProjectOrFactoryOwner(projectUId, new PermissionSets[] { PermissionSets.SCRUM_MASTER });
            authorizationService.VerifyCanSeeProposalValues();

            // always call CREATE HOUR COSTS
            // it will create ony hour costs for new roles
            Project project = projectsService.GetProject(projectUId);
            Project similar = projectsService.GetLastSimilarProject(projectUId, true);
            proposalsRepository.CreateHourCosts(project, similar);

            return proposalsRepository.GetHourCosts(projectUId);

        }

        [WebInvoke(Method = "PUT", UriTemplate = "ProjectRoleHoursCosts/{projectUId}/", ResponseFormat = WebMessageFormat.Json)]
        public void UpdateHourCosts(string projectUId, RoleHourCost[] hourCosts) {

            // verifies permission
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);
            authorizationService.VerifyCanSeeProposalValues();

            // save new costs
            proposalsRepository.SaveHourCosts(hourCosts);

            // updates the project proposals with new total price
            ICollection<BacklogItem> backlog = backlogservice.GetCurrentBacklog(projectUId, (short)BacklogFiltersMode.ALL);
            ICollection<Proposal> proposals = GetProjectProposals(projectUId);
            foreach (Proposal p in proposals.Where(p => p.ProposalStatus == (short)ProposalStatus.PROPOSAL_WAITING)) {
                Proposal pWithItems = GetProjectProposal(projectUId, p.ProposalUId);
                pWithItems.SetBacklogItems(backlog);
                pWithItems.TotalValue = pWithItems.CalcTotalPrice(hourCosts);
                proposalsRepository.SaveProposal(pWithItems);
            }
            
        }

        [WebInvoke(Method = "PUT", UriTemplate = "RejectedProposals/{projectUId}/{proposalUId}/{rejectReason}", ResponseFormat = WebMessageFormat.Json)]
        public void RejectProposal(string projectUId, string proposalUId, string rejectReason, string documentXAML) {

            // verifies permission
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);
            authorizationService.VerifyCanSeeProposalValues();

            // update proposal status
            Proposal proposal = GetProjectProposal(projectUId, proposalUId);
            proposal.ApprovalDate = DateTime.Now;
            proposal.RejectReason = short.Parse(rejectReason);
            proposal.ProposalStatus = (short)ProposalStatus.PROPOSAL_REJECTED;
            proposal.ApprovedBy = authorizationService.SignedMemberProfile.MemberUId;

            if (documentXAML == null)
                documentXAML = reportService.GetReportXAMLOnly("ProposalReport", proposal.TemplateName, proposal.ProjectUId, proposal.ProposalUId);

            proposal.ProposalDocument = new ProposalDocument() { ProposalUId = proposalUId, ProposalXAML = documentXAML };

            UpdateProposal(projectUId, proposal);

            // update project status
            Project project = projectsService.GetProject(projectUId);
            ICollection<Proposal> projectProposals = GetProjectProposals(projectUId);
            if (projectProposals.All(pp => pp.ProposalStatus==(short)ProposalStatus.PROPOSAL_REJECTED))
                projectsService.ChangeProjectStatus(projectUId, "", (short)ProjectStatus.PROPOSAL_REJECTED);
            
        }

        [WebInvoke(Method = "PUT", UriTemplate = "ApprovedProposals/{projectUId}/{proposalUId}", ResponseFormat = WebMessageFormat.Json)]
        public void ApproveProposal(string projectUId, string proposalUId, string documentXAML) {

            // verifies permission
            authorizationService.VerifyPermissionAtProject(projectUId, PermissionSets.SCRUM_MASTER);
            authorizationService.VerifyCanSeeProposalValues();

            // update proposal status
            Proposal proposal = GetProjectProposal(projectUId, proposalUId);
            proposal.ApprovalDate = DateTime.Now;
            proposal.ApprovedBy = authorizationService.SignedMemberProfile.MemberUId;
            proposal.ProposalStatus = (short)ProposalStatus.PROPOSAL_APPROVED;

            if (documentXAML == null)
                documentXAML = reportService.GetReportXAMLOnly("ProposalReport", proposal.TemplateName, proposal.ProjectUId, proposal.ProposalUId);

            proposal.ProposalDocument = new ProposalDocument() { ProposalUId = proposalUId, ProposalXAML = documentXAML };

            UpdateProposal(projectUId, proposal);

            // update project status
            Project project = projectsService.GetProject(projectUId);
            if (project.Status == (short)ProjectStatus.PROPOSAL_CREATION || project.Status == (short)ProjectStatus.PROPOSAL_REJECTED)
                projectsService.ChangeProjectStatus(projectUId, "", (short)ProjectStatus.PROPOSAL_APPROVED);
            
        }

        public decimal GetBudgetIndicator_skipAuth(string projectUId) {

            // get the budget
            decimal projectBudget = proposalsRepository.GetProjectBudget(projectUId);

            // get the tasks proce
            RoleHourCost[] costs = proposalsRepository.GetHourCosts(projectUId);
            decimal tasksPrice = tasksService.GetProjectTasksPrice(costs);

            // if there is no budget, and tasks has been made, assumes the 100% has been used
            if (projectBudget == 0 && tasksPrice > 0)
                return 100;

            // if there is no budget, and no tasks has been made, assumes 0%
            if (projectBudget == 0 && tasksPrice == 0)
                return 0;

            return tasksPrice / projectBudget * 100;
            
        }

        [WebGet(UriTemplate = "BudgetIndicator/{projectUId}", ResponseFormat = WebMessageFormat.Json)]
        public decimal GetBudgetIndicator(string projectUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return GetBudgetIndicator_skipAuth(projectUId);
            
        }

        [WebGet(UriTemplate = "ProposalTemplates", ResponseFormat = WebMessageFormat.Json)]
        public string[] GetProposalTemplates() {
            Helper.ReportTemplate report = new Helper.ReportTemplate();
            return report.GetTemplateList("ProposalReport");
        }

        public bool IsItemAtAnyProposal(string backlogItemUId) {
            authorizationService.VerifyRequestAuthorizationToken();
            return proposalsRepository.IsItemAtAnyProposal(backlogItemUId);
        }
        
    }
}
