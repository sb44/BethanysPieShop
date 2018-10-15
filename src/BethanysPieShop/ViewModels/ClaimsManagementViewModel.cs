using System.Collections.Generic;

namespace BethanysPieShop.ViewModels
{
    public class ClaimsManagementViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; } // SB: Ajout
        public string ClaimId { get; set; } // le type ...
        public List<string> AllClaimsList { get; set; }

        public List<string> UserClaims { get; set; } // AjoutSb (ceux possédés par le user)
    }
}
