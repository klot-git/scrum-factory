using System.Runtime.Serialization;
using System.Collections.Generic;
using System;



namespace ScrumFactory {


    public enum PermissionSets : short {        
        SCRUM_MASTER,
        TEAM,
        PRODUCT_OWNER,        
        COMMERCIAL_GUY,        

    }

    
    [DataContract]
    [Serializable]  // for clipboard reasons
    public class Role {

        public static readonly PermissionSets[] ANY_PERMISSION = new PermissionSets[] {
            PermissionSets.SCRUM_MASTER,
            PermissionSets.TEAM,
            PermissionSets.PRODUCT_OWNER,            
            PermissionSets.COMMERCIAL_GUY
        };

        [DataMember]
        public string RoleUId { get; set; }

        [DataMember]
        public string ProjectUId { get; set; }

        [DataMember]
        public string RoleName { get; set; }

        [DataMember]
        public string RoleDescription { get; set; }

        [DataMember]
        public short PermissionSet { get; set; }

        [DataMember]
        public bool IsPlanned { get; set; }

        [DataMember]
        public List<PlannedHour> PlannedHours { get; set; }

        [DataMember]
        public string RoleShortName { get; set; }

        [DataMember]
        public bool IsDefaultRole { get; set; }

        /// <summary>
        /// Determines whether is the same the specified role.
        /// It means, all fields values are equal.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <returns>
        /// 	<c>true</c> if is the same the specified role; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTheSame(Role role) {
            return
                IsDefaultRole == role.IsDefaultRole &&
                IsPlanned == role.IsPlanned &&
                PermissionSet == role.PermissionSet &&
                RoleName == role.RoleName &&
                RoleShortName == role.RoleShortName &&
                RoleDescription == role.RoleDescription &&
                RoleUId == role.RoleUId &&
                ProjectUId == role.ProjectUId;
        }
    }
}
