using System.Collections.Generic;
using System.Security.Claims;

namespace BethanysPieShop.ViewModels
{
    public class ClaimsManagementViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; } // SB: Ajout
        public string ClaimId { get; set; } // le type ...
        public string ClaimValue { get; set; } // SB: Ajout
        public List<string> AllClaimsList { get; set; }

        public IList<Claim> UserClaims { get; set; } // AjoutSb (ceux possédés par le user)
    }
}
