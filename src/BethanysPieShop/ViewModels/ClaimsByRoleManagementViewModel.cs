using System.Collections.Generic;

namespace BethanysPieShop.ViewModels
{
    //SB ajout
    public class ClaimsByRoleManagementViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; } // SB: Ajout
        public string ClaimId { get; set; } // le type ...
        public List<string> AllClaimsList { get; set; }

        public List<string> RoleClaims { get; set; } // ceux possédés par le role
    }
}
