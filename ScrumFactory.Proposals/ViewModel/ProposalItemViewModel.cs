using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;
using ScrumFactory.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ScrumFactory.Composition.View;
using System.Windows.Input;
using System.Linq;


namespace ScrumFactory.Proposals.ViewModel {

    public class ProposalItemViewModel : BaseEditableObjectViewModel  {


        private ProposalViewModel proposal;

        public BacklogItem Item { get; set; }

        public BacklogItemGroup ItemGroup { get; set; }

        public ProposalItemViewModel(ProposalViewModel proposal, BacklogItem item, BacklogItemGroup group) {
            this.proposal = proposal;
            this.Item = item;
            this.ItemGroup = group;
            OnPropertyChanged("IsAtProposal");
        }

        ~ProposalItemViewModel() {
            System.Console.WriteLine("***< proposal died here");
        }

        public void RefreshUI() {
            OnPropertyChanged("ItemPrice");
        }

        public decimal ItemPrice {
            get {
                return proposal.Proposal.CalcItemPrice(Item, proposal.HourCosts);
            }
        }

        public bool IsAtProposal {
            get {
                if (proposal==null || proposal.Proposal == null || proposal.Proposal.Items == null)
                    return false;
                return proposal.Proposal.Items.Any(i => i.BacklogItemUId == Item.BacklogItemUId);
            }
            set {
                if (proposal.Proposal.Items == null)
                    proposal.Proposal.Items = new List<ProposalItem>();
                ProposalItem pItem = proposal.Proposal.Items.SingleOrDefault(i => i.BacklogItemUId == Item.BacklogItemUId);
                if (value && pItem==null) {
                    proposal.Proposal.Items.Add(
                        new ProposalItem() {
                            ProposalUId = proposal.Proposal.ProposalUId,                            
                            BacklogItemUId = Item.BacklogItemUId,
                            Item = this.Item
                        });
                }
                else if(pItem!=null) {
                    proposal.Proposal.Items.Remove(pItem);
                }
                proposal.ItemsPrice = proposal.Proposal.CalcItemsPrice(proposal.HourCosts);
                OnPropertyChanged("IsAtProposal");
            }
        }
    }
}
