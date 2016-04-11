using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Input;
using ScrumFactory.Composition;
using ScrumFactory.Windows.Helpers;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Services;
using System.Linq;
using System.Windows.Data;
using ScrumFactory.Composition.View;


namespace ScrumFactory.Tasks.ViewModel {

    public class AssigneeViewModel : BaseEditableObjectViewModel, INotifyPropertyChanged {



        public static ICollection<AssigneeViewModel> CreateAssigneeCollection(ICollection<ProjectMembership> memberships) {
            if (memberships == null)
                return new Collection<AssigneeViewModel>(); 
            ICollection<AssigneeViewModel> members = new ObservableCollection<AssigneeViewModel>();
            foreach (ProjectMembership m in memberships.OrderByDescending(m => m.IsActive).ThenBy(m => m.Role.PermissionSet))
                if (!members.Any(mm => mm.MemberUId == m.MemberUId))
                    members.Add(new AssigneeViewModel(m));
                
            return members;
        }

        private MemberProfile member;
        public MemberProfile Member {
            get {
                return member;
            }
            set {
                member = value;
                OnPropertyChanged("Member");
                OnPropertyChanged("FullName");
                OnPropertyChanged("MemberAvatarUrl");                
            }
        }
                
        public ProjectMembership Membership { get; private set; }

        public AssigneeViewModel(ProjectMembership membership) {
            Membership = membership;
            Member = membership.Member;            
        }

        public bool IsContactMember {
            get {
                if (Member == null)
                    return false;
                return Member.IsContactMember;
            }
        }

        public decimal? PlannedHoursForToday {
            get {
                if (Member == null)
                    return null;
                return Member.PlannedHoursForToday;
            }
            set {
                if (Member == null)
                    return;
                Member.PlannedHoursForToday = value;                
                OnPropertyChanged("IsTodayHalfPlanned");
                OnPropertyChanged("IsTodayOverPlanned");
            }
        }

        public bool IsTodayHalfPlanned {
            get {
                if (Member == null)
                    return false;
                return Member.IsTodayHalfPlanned;
            }            
        }

        public bool IsTodayOverPlanned {
            get {
                if (Member == null)
                    return false;
                return Member.IsTodayOverPlanned;
            }
        }

        public string MemberAvatarUrl {
            get {
                if (Member == null)
                    return null;
                return Member.MemberAvatarUrl;
            }
        }

        public string FullName {
            get {
                if (Member == null)
                    return "[" + Membership.MemberUId + "]";
                return Member.FullName;
            }
        }

        public string MemberUId {
            get {                
                return Membership.MemberUId;
            }
        }

    }
}
