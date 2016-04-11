using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ScrumFactory.Composition.ViewModel;
using ScrumFactory.Composition;

namespace ScrumFactory.Team.ViewModel {

    public class RoleViewModel : BaseEditableObjectViewModel {

        public RoleViewModel(Role role) {
            Role = role;
        }

        public bool IsDefaultRole {
            get {
                return Role.IsDefaultRole;
            }
            set {
                Role.IsDefaultRole = value;
                OnPropertyChanged("IsDefaultRole");
            }
        }


        #region IRoleViewModel Members

        public Role Role { get; private set; }

        #endregion


    }
}
