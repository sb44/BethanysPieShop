using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BethanysPieShop.Auth;
using BethanysPieShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace BethanysPieShop.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UserManagement()
        {
            var users = _userManager.Users;

            return View(users);
        }

        public IActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(AddUserViewModel addUserViewModel)
        {
            if (!ModelState.IsValid) return View(addUserViewModel);

            var user = new ApplicationUser()
            {
                UserName = addUserViewModel.UserName,
                Email = addUserViewModel.Email,
                Birthdate = addUserViewModel.Birthdate,
                City = addUserViewModel.City,
                Country = addUserViewModel.Country
            };

            IdentityResult result = await _userManager.CreateAsync(user, addUserViewModel.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("UserManagement", _userManager.Users);
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(addUserViewModel);
        }

        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return RedirectToAction("UserManagement", _userManager.Users);

            var claims = await _userManager.GetClaimsAsync(user);
            var vm = new EditUserViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Birthdate = user.Birthdate,
                City = user.City,
                Country = user.Country,
                UserClaims = claims //claims.Select(c => c.Value).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel editUserViewModel)
        {
            var user = await _userManager.FindByIdAsync(editUserViewModel.Id);

            if (user != null)
            {
                user.Email = editUserViewModel.Email;
                user.UserName = editUserViewModel.UserName;
                user.Birthdate = editUserViewModel.Birthdate;
                user.City = editUserViewModel.City;
                user.Country = editUserViewModel.Country;

                //SB ajout claim pour dateofBirth
                user.Claims.Add(new IdentityUserClaim<string>
                {
                    ClaimType = ClaimTypes.DateOfBirth,
                    ClaimValue = user.Birthdate.ToString()
                });

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                    return RedirectToAction("UserManagement", _userManager.Users);

                ModelState.AddModelError("", "User not updated, something went wrong.");

                return View(editUserViewModel);
            }

            return RedirectToAction("UserManagement", _userManager.Users);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                IdentityResult result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                    return RedirectToAction("UserManagement");
                else
                    ModelState.AddModelError("", "Something went wrong while deleting this user.");
            }
            else
            {
                ModelState.AddModelError("", "This user can't be found");
            }
            return View("UserManagement", _userManager.Users);
        }


        //Roles management
        public IActionResult RoleManagement()
        {
            var roles = _roleManager.Roles;
            return View(roles);
        }

        public IActionResult AddNewRole() => View();

        [HttpPost]
        public async Task<IActionResult> AddNewRole(AddRoleViewModel addRoleViewModel)
        {

            if (!ModelState.IsValid) return View(addRoleViewModel);

            var role = new IdentityRole
            {
                Name = addRoleViewModel.RoleName
            };

            IdentityResult result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                return RedirectToAction("RoleManagement", _roleManager.Roles);
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(addRoleViewModel);
        }

        public async Task<IActionResult> EditRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
                return RedirectToAction("RoleManagement", _roleManager.Roles);

            var editRoleViewModel = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name,
                Users = new List<string>()
            };


            foreach (var user in _userManager.Users)
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                    editRoleViewModel.Users.Add(user.UserName);
            }

            //SB: Ajout Gestion des claims inclus dans ce role:
            var claimsInRole = await _roleManager.GetClaimsAsync(role);
            editRoleViewModel.RoleClaims = claimsInRole; //claimsInRole.Select(c => c.Value).ToList();

            return View(editRoleViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel editRoleViewModel)
        {
            var role = await _roleManager.FindByIdAsync(editRoleViewModel.Id);

            if (role != null)
            {
                role.Name = editRoleViewModel.RoleName;

                var result = await _roleManager.UpdateAsync(role);

                if (result.Succeeded)
                    return RedirectToAction("RoleManagement", _roleManager.Roles);

                ModelState.AddModelError("", "Role not updated, something went wrong.");

                return View(editRoleViewModel);
            }

            return RedirectToAction("RoleManagement", _roleManager.Roles);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                    return RedirectToAction("RoleManagement", _roleManager.Roles);
                ModelState.AddModelError("", "Something went wrong while deleting this role.");
            }
            else
            {
                ModelState.AddModelError("", "This role can't be found.");
            }
            return View("RoleManagement", _roleManager.Roles);
        }

        // SB: Ajout Claims for role
        public async Task<IActionResult> ManageClaimsForRole(string roleId)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(roleId);
            
            if (role != null)
            {
                IList<Claim> roleClaimsInCurrentRole = await _roleManager.GetClaimsAsync(role);

                var vm = new ClaimsByRoleManagementViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    AllClaimsList = BethanysPieShopClaimTypes.ClaimsList,
                    RoleClaims = roleClaimsInCurrentRole //roleClaimsInCurrentRole.Select(c => c.Value).ToList()
                };

                return View(vm);
            }
            else
            {
                return RedirectToAction("RoleManagement");
            }
         
        }
        //SB: Claims add to role
        [HttpPost]
        public async Task<IActionResult> AddClaimForRole(ClaimsByRoleManagementViewModel vm)
        {
            var role = await _roleManager.FindByIdAsync(vm.RoleId);

            if (role == null)
                return RedirectToAction("RoleManagement", _roleManager.Roles);

            //SB: Vérifie si le role le possède déjà...
            var claimsForRole = await _roleManager.GetClaimsAsync(role);
            var lastAccessedClaim = claimsForRole.FirstOrDefault(t => t.Type == vm.ClaimId);
            if (lastAccessedClaim == null)
            {

                var roleClaim = new Claim(vm.ClaimId, vm.ClaimId);
                var result = await _roleManager.AddClaimAsync(role, roleClaim); //role.Claims.Add(claim);

                if (result.Succeeded)
                    return RedirectToAction("RoleManagement");
                else
                    ModelState.AddModelError("", "Something went wrong while adding the claim for this role");
            }
            else
            {
                ModelState.AddModelError("", "Role already has this claim, no update was executed.");
            }

            //Erreur:
            vm.AllClaimsList = BethanysPieShopClaimTypes.ClaimsList;
            vm.RoleClaims = claimsForRole; //claimsForRole.Select(c => c.Value).ToList();
            return View("ManageClaimsForRole", vm);
        }

        //SB: Claims remove from role
        [HttpPost]
        public async Task<IActionResult> RemoveRoleClaimFromRole(ClaimsByRoleManagementViewModel vm)
        {
            var role = await _roleManager.FindByIdAsync(vm.RoleId);
            IList<Claim> claimsForRole;
            if (role != null)
            {
                claimsForRole = await _roleManager.GetClaimsAsync(role);
                var lastAccessedClaim = claimsForRole.FirstOrDefault(t => t.Type == vm.ClaimId);
                if (lastAccessedClaim != null)
                {
                    IdentityResult resDelete = await _roleManager.RemoveClaimAsync(role, lastAccessedClaim);

                    if (resDelete.Succeeded)
                        return RedirectToAction("RoleManagement");
                    else
                        ModelState.AddModelError("", "Something went wrong while deleting the claim for this role");
                }
                else
                {
                    ModelState.AddModelError("", "The current claim for this role does not exist. No need to remove it!");
                }
            }
            else
            {
                return RedirectToAction("RoleManagement");
            }

            //Erreur
            vm.AllClaimsList = BethanysPieShopClaimTypes.ClaimsList;
            vm.RoleClaims = claimsForRole; //claimsForRole.Select(c => c.Value).ToList();
            return View("ManageClaimsForRole", vm);
        }


        //Users in roles

        public async Task<IActionResult> AddUserToRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
                return RedirectToAction("RoleManagement", _roleManager.Roles);

            var addUserToRoleViewModel = new UserRoleViewModel { RoleId = role.Id };

            foreach (var user in _userManager.Users)
            {
                if (!await _userManager.IsInRoleAsync(user, role.Name))
                {
                    addUserToRoleViewModel.Users.Add(user);
                }
            }

            return View(addUserToRoleViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddUserToRole(UserRoleViewModel userRoleViewModel)
        {
            var user = await _userManager.FindByIdAsync(userRoleViewModel.UserId);
            var role = await _roleManager.FindByIdAsync(userRoleViewModel.RoleId);

            var result = await _userManager.AddToRoleAsync(user, role.Name);

            if (result.Succeeded)
            {
                return RedirectToAction("RoleManagement");
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(userRoleViewModel);
        }

        public async Task<IActionResult> DeleteUserFromRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
                return RedirectToAction("RoleManagement");

            var addUserToRoleViewModel = new UserRoleViewModel { RoleId = role.Id };

            foreach (var user in _userManager.Users)
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    addUserToRoleViewModel.Users.Add(user);
                }
            }

            return View(addUserToRoleViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserFromRole(UserRoleViewModel userRoleViewModel)
        {
            var user = await _userManager.FindByIdAsync(userRoleViewModel.UserId);
            var role = await _roleManager.FindByIdAsync(userRoleViewModel.RoleId);

            var result = await _userManager.RemoveFromRoleAsync(user, role.Name);

            if (result.Succeeded)
            {
                return RedirectToAction("RoleManagement", _roleManager.Roles);
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(userRoleViewModel);
        }

        //Claims for user
        public async Task<IActionResult> ManageClaimsForUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return RedirectToAction("UserManagement", _userManager.Users);

            var claims = await _userManager.GetClaimsAsync(user);

            var claimsManagementViewModel = new ClaimsManagementViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                AllClaimsList = BethanysPieShopClaimTypes.ClaimsList,
                UserClaims  = claims //claims.Select(c => c.Value).ToList() //ajout sb
            };

            return View(claimsManagementViewModel);
        }

        //Claims add to user
        [HttpPost]
        public async Task<IActionResult> AddClaimForUser(ClaimsManagementViewModel claimsManagementViewModel)
        {
            var user = await _userManager.FindByIdAsync(claimsManagementViewModel.UserId);

            if (user == null)
                return RedirectToAction("UserManagement", _userManager.Users);

            //SB: Vérifie si le user le possède déjà...
            var claimsForUser = await _userManager.GetClaimsAsync(user);
            var lastAccessedClaim = claimsForUser.FirstOrDefault(t => t.Type == claimsManagementViewModel.ClaimId);
            if (lastAccessedClaim == null)
            { 

                IdentityUserClaim<string> claim = new IdentityUserClaim<string>
                {
                    ClaimType = claimsManagementViewModel.ClaimId,
                    ClaimValue = claimsManagementViewModel.ClaimId
                };

                user.Claims.Add(claim);
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                    return RedirectToAction("UserManagement", _userManager.Users);
                else
                    ModelState.AddModelError("", "Something went wrong while adding the claim for this user");

            } else
            {
                ModelState.AddModelError("", "User already has this claim, no update was executed.");
            }

            // Erreur
            claimsManagementViewModel.AllClaimsList = BethanysPieShopClaimTypes.ClaimsList;
            claimsManagementViewModel.UserClaims = claimsForUser; //claimsForUser.Select(c => c.Value).ToList();
            return View("ManageClaimsForUser", claimsManagementViewModel);
        }

        //Claims remove from user
        [HttpPost]
        public async Task<IActionResult> RemoveUserClaimFromUser(ClaimsManagementViewModel claimsManagementViewModel)
        {
            var user = await _userManager.FindByIdAsync(claimsManagementViewModel.UserId);
            IList<Claim> claimsForUser;
            if (user != null)
            {
                claimsForUser = await _userManager.GetClaimsAsync(user);
                Claim lastAccessedClaim = claimsForUser.FirstOrDefault(t => t.Type == claimsManagementViewModel.ClaimId);
                if (lastAccessedClaim != null)
                {
                    IdentityResult resDelete =  await _userManager.RemoveClaimAsync(user, lastAccessedClaim);

                    if (resDelete.Succeeded)
                        return RedirectToAction("UserManagement");
                    else
                        ModelState.AddModelError("", "Something went wrong while deleting the claim for this user");
                }
                else
                {
                    ModelState.AddModelError("", "The current claim for this user does not exist. No need to remove it from him!");
                }
            }
            else
            {
                return RedirectToAction("UserManagement");
            }

            // Erreur
            claimsManagementViewModel.AllClaimsList = BethanysPieShopClaimTypes.ClaimsList;
            claimsManagementViewModel.UserClaims = claimsForUser; //claimsForUser.Select(c => c.Value).ToList();
            return View("ManageClaimsForUser", claimsManagementViewModel);
        }

    }
}