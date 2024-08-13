using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CVSante.Models;
using CVSante.ViewModels;
using Microsoft.AspNetCore.Identity;
using CVSante.Services;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using CVSante.Tools;

namespace CVSante.Controllers
{
    public class AdminController : Controller
    {
        private readonly CvsanteContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserValidation _UserValidation;

        public AdminController(CvsanteContext context, UserManager<IdentityUser> userManager, UserValidation userValidation)
        {
            _context = context;
            _userManager = userManager;
            _UserValidation = userValidation;
        }


        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // GET: Admin
        public async Task<IActionResult> Index()
        {
            //var currentUserId = _userManager.GetUserId(User);

            //// Fetch the current user's paramedic details
            //var userParam = await _context.UserParamedics
            //    .FirstOrDefaultAsync(up => up.FkIdentityUser == currentUserId);

            //// Check if the current user is in the "Paramedic" role
            //var currentUser = await _userManager.FindByIdAsync(currentUserId);
            //bool isParamedic = await _userManager.IsInRoleAsync(currentUser, "Paramedic");
            //bool isSuperAdmin = await _userManager.IsInRoleAsync(currentUser, "SuperAdmin");

            //if (isSuperAdmin || isParamedic && userParam != null)
            //{
                return View();
            //}
            //else
            //{
            //    // Redirect to Home if the user is not a "Paramedic" or userParam is null
            //    return RedirectToAction("Index", "Home");
            //}
        }



        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // GET: Admin/History
        public async Task<IActionResult> History(int? id, int? pageNumber, int? pageSize)
        {

            var currentUserId = _userManager.GetUserId(User); // currentUserId is already a string
            var userParam = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
                .FirstAsync(up => up.FkIdentityUser == currentUserId);

            if (userParam == null)
            {
                return NotFound();
            }

            // Check if the current user has the EditRole permission
            if (!userParam.FkRoleNavigation.GetHistorique)
            {
                return RedirectToAction("ManageCompany", "Admin");
            }

            var historique = _context.HistoriqueParams
                .Include(h => h.FkParam)
                .Include(h => h.FkUser)
                .Where(h => h.FkParamId == userParam.ParamId);

            return View(await PaginatedList<HistoriqueParam>.CreateAsync(historique, pageNumber ?? 1, pageSize ?? 10));
            //return View(historique);
            
        }



        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // GET: Admin/Roles/ManageRoles
        public async Task<IActionResult> ManageRoles(int? paramedicId)
        {
            // Get the current user's ID (assuming this is how you identify the current user)
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Fetch the current user's paramedic details
            var currentUser = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null)
            {
                return NotFound();
            }

            // Check if the current user has the EditRole permission
            if (!currentUser.FkRoleNavigation.EditRole)
            {
                return RedirectToAction("ManageCompany", "Admin");
            }

            // Fetch all paramedics associated with the company
            var paramedics = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
                .Where(u => u.FkCompany == currentUser.FkCompany)
                .ToListAsync();

            // Select the current paramedic if paramedicId is provided, otherwise default to the first paramedic
            var selectedParamedic = paramedicId.HasValue
                ? paramedics.FirstOrDefault(p => p.ParamId == paramedicId.Value)
                : paramedics.FirstOrDefault();

            if (selectedParamedic == null)
            {
                return NotFound();
            }

            var companyId = selectedParamedic.FkCompany;

            var viewModel = new ManageCompanyRoles
            {
                Paramedics = paramedics,
                SelectedParamedic = selectedParamedic,
                SelectedRole = selectedParamedic.FkRoleNavigation ?? new CompanyRole(),
                CompanyId = (int)companyId
            };

            return View(viewModel);
        }


        [Authorize(Roles = "SuperAdmin,Paramedic")]
        //POST: Admin/Roles/ManageRoles/EditRole
        [HttpPost]
        public async Task<IActionResult> EditRole(CompanyRole selectedRole, int selectedParamedicId)
        {

            if (!ModelState.IsValid)
            {
                // Log model state errors
                Console.WriteLine("ModelState is not valid.");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }

                // Retrieve required data to repopulate the view model
                var paramedics = await _context.UserParamedics
                    .Include(u => u.FkRoleNavigation)
                    .ToListAsync();

                var selectedParamedic = await _context.UserParamedics
                    .Include(p => p.FkRoleNavigation)
                    .FirstOrDefaultAsync(p => p.ParamId == selectedParamedicId);



                var viewModel = new ManageCompanyRoles
                {
                    Paramedics = paramedics,
                    SelectedParamedic = selectedParamedic,
                    SelectedRole = selectedRole,
                    CompanyId = selectedParamedic?.FkCompany ?? 0
                };

                // Return to the view with validation errors
                return View("ManageRoles", viewModel);
            }

            // Find the role to update
            var roleToUpdate = await _context.CompanyRoles
                .FirstOrDefaultAsync(r => r.IdRole == selectedRole.IdRole);

            if (roleToUpdate == null)
            {
                return NotFound();
            }

            // Update the role properties
            roleToUpdate.CreateParamedic = selectedRole.CreateParamedic;
            roleToUpdate.EditParamedic = selectedRole.EditParamedic;
            roleToUpdate.GetHistorique = selectedRole.GetHistorique;
            roleToUpdate.GetCitoyen = selectedRole.GetCitoyen;
            roleToUpdate.EditRole = selectedRole.EditRole;
            roleToUpdate.EditCompany = selectedRole.EditCompany;

            _context.CompanyRoles.Update(roleToUpdate);
            await _context.SaveChangesAsync();

            return RedirectToAction("ManageRoles", new { paramedicId = selectedParamedicId });
        }


        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // GET: Admin/ManageCompany
        public async Task<IActionResult> ManageCompany()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // currentUserId is already a string
            var currentUser = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null)
            {
                return NotFound();
            }

            if (!currentUser.FkRoleNavigation.EditCompany)
            {
                return RedirectToAction("Index", "Admin");
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.IdComp == currentUser.FkCompany);

            if (company == null)
            {
                return NotFound();
            }

            var paramedics = await _context.UserParamedics
                .Where(p => p.FkCompany == company.IdComp)
                .ToListAsync();

            var viewModel = new ManageCompany
            {
                Company = company,
                Paramedics = paramedics
            };

            return View(viewModel);
        }


        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // POST: Admin/ManageCompany/RemoveFromCompany
        [HttpPost]
        public async Task<IActionResult> ManageCompany(ManageCompany viewModel, int? removeParamedicId)
        {
            if (removeParamedicId.HasValue)
            {
                var paramedicToRemove = await _context.UserParamedics
                    .Include(p => p.FkRoleNavigation)
                    .FirstOrDefaultAsync(p => p.ParamId == removeParamedicId.Value);

                if (paramedicToRemove != null)
                {
                    // Set the paramedic's FkCompany to null (remove from company)
                    paramedicToRemove.FkCompany = null;

                    // Set the paramedic as inactive
                    paramedicToRemove.ParamIsActive = false;

                    // Set all roles to false
                    var role = paramedicToRemove.FkRoleNavigation;
                    if (role != null)
                    {
                        role.CreateParamedic = false;
                        role.EditParamedic = false;
                        role.GetHistorique = false;
                        role.GetCitoyen = false;
                        role.EditRole = false;
                        role.EditCompany = false;
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("ManageCompany");
            }

            if (viewModel.Paramedics != null)
            {
                // Save changes to the paramedics
                foreach (var paramedic in viewModel.Paramedics)
                {
                    var existingParamedic = await _context.UserParamedics.FindAsync(paramedic.ParamId);
                    if (existingParamedic != null)
                    {
                        existingParamedic.ParamIsActive = paramedic.ParamIsActive;
                    }
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                // Handle the case where Paramedics is null
                ModelState.AddModelError("", "Paramedics data is missing.");
            }

            return RedirectToAction("ManageCompany");
        }


        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // GET: Admin/ManageCompany/AddRespondent
        public async Task<IActionResult> AddRespondent()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null)
            {
                return NotFound();
            }

            if (!currentUser.FkRoleNavigation.CreateParamedic)
            {
                return RedirectToAction("Index", "Admin");
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.IdComp == currentUser.FkCompany);

            if (company == null)
            {
                return NotFound();
            }

            var viewModel = new AddRespondent
            {
                CompanyId = company.IdComp,
                CompanyName = company.CompName
            };

            return View(viewModel);
        }


        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // POST: Admin/ManageCompany/AddRespondent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRespondent(AddRespondent viewModel)
        {
            ModelState.Remove("UserParamedic.FkRoleNavigation");
            ModelState.Remove("UserParamedic.FkIdentityUser");
            ModelState.Remove("UserParamedic.FkIdentityUserNavigation");
            ModelState.Remove("CompanyName");

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(viewModel.Email);
                if (user == null)
                {
                    ModelState.AddModelError("Email", "Aucun utilisateur n'a été trouvé avec cet email.");
                    return View(viewModel);
                }

                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Paramedic"))
                {
                    ModelState.AddModelError("Email", "L'utilisateur est déjà un paramédic. Veuillez l'ajouter en utilisant son matricule.");
                    return View(viewModel);
                }

                var existingParamedic = await _context.UserParamedics
                    .FirstOrDefaultAsync(p => p.FkIdentityUser == user.Id);

                if (existingParamedic != null)
                {
                    ModelState.AddModelError("Email", "L'utilisateur est déjà enregistré en tant que paramédic. Veuillez l'ajouter en utilisant son matricule.");
                    return View(viewModel);
                }

                var newRole = new CompanyRole
                {
                    CreateParamedic = false,
                    EditParamedic = false,
                    GetHistorique = false,
                    GetCitoyen = false,
                    EditRole = false,
                    EditCompany = false,
                    FkCompany = viewModel.CompanyId
                };

                _context.CompanyRoles.Add(newRole);
                await _context.SaveChangesAsync();

                var newRoleId = newRole.IdRole;

                var newParamedic = viewModel.UserParamedic;
                newParamedic.FkCompany = viewModel.CompanyId;
                newParamedic.FkIdentityUser = user.Id;
                newParamedic.FkRole = newRoleId;

                _context.UserParamedics.Add(newParamedic);
                await _context.SaveChangesAsync();

                await _userManager.AddToRoleAsync(user, "Paramedic");

                return RedirectToAction("ManageCompany");
            }

            return View(viewModel);
        }


        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // POST: Admin/ManageCompany/AddByMatricule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddByMatricule(AddRespondent viewModel)
        {
            var existingParamedic = await _context.UserParamedics
                .Include(p => p.FkIdentityUserNavigation)
                .FirstOrDefaultAsync(p => p.Matricule == viewModel.UserParamedic.Matricule);

            if (existingParamedic == null)
            {
                //ModelState.AddModelError("Matricule", "Aucun paramédic trouvé avec ce matricule dans cette entreprise.");
                return View("AddRespondent", viewModel);
            }
            
            if (existingParamedic.FkCompany != null)
            {
                ModelState.AddModelError("Matricule", "Le paramédic appartient à une autre entreprise.");
                return View("AddRespondent", viewModel);
            }

            existingParamedic.FkCompany = viewModel.CompanyId;
            existingParamedic.ParamIsActive = true;

            await _context.SaveChangesAsync();

            return RedirectToAction("ManageCompany");
        }





        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // GET: Admin/ManageCompany/EditRespondent
        public async Task<IActionResult> EditRespondent(int? paramedicId)
        {
            // Get the current user's ID (assuming you have this from the authentication context)
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Fetch the current user's paramedic details
            var currentUser = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null)
            {
                return NotFound();
            }

            if (!currentUser.FkRoleNavigation.EditParamedic)
            {
                return RedirectToAction("ManageCompany", "Admin");
            }

            var currentCompanyId = currentUser.FkCompany;

            // Fetch paramedics belonging to the same company as the current user
            var paramedics = await _context.UserParamedics
                .Where(p => p.FkCompany == currentCompanyId)
                .ToListAsync();

            ViewBag.Paramedics = paramedics;

            // Retrieve the paramedic to edit if paramedicId is provided
            UserParamedic paramedic = null;
            if (paramedicId.HasValue)
            {
                paramedic = await _context.UserParamedics
                    .Include(p => p.FkRoleNavigation)
                    .FirstOrDefaultAsync(p => p.ParamId == paramedicId.Value && p.FkCompany == currentCompanyId);

                if (paramedic == null)
                {
                    return NotFound();
                }
            }

            return View(paramedic);
        }



        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // POST: Admin/ManageCompany/EditRespondent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRespondent(UserParamedic paramedic)
        {
            // Remove validation for fields that are not part of the form
            ModelState.Remove("FkRoleNavigation");
            ModelState.Remove("FkIdentityUserNavigation");

            if (ModelState.IsValid)
            {
                // Get the current user's ID
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Fetch the current user's paramedic details
                var currentUser = await _context.UserParamedics
                    .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

                if (currentUser == null)
                {
                    return NotFound();
                }

                var currentCompanyId = currentUser.FkCompany;

                // Fetch the existing paramedic entity within the same company
                var existingParamedic = await _context.UserParamedics
                    .Include(p => p.FkRoleNavigation)
                    .FirstOrDefaultAsync(p => p.ParamId == paramedic.ParamId && p.FkCompany == currentCompanyId);

                if (existingParamedic == null)
                {
                    return NotFound();
                }

                // Update only the allowed fields
                existingParamedic.Nom = paramedic.Nom;
                existingParamedic.Prenom = paramedic.Prenom;
                existingParamedic.Ville = paramedic.Ville;
                existingParamedic.Telephone = paramedic.Telephone;
                existingParamedic.Matricule = paramedic.Matricule;
                existingParamedic.ParamIsActive = paramedic.ParamIsActive;

                await _context.SaveChangesAsync();

                return RedirectToAction("ManageCompany");
            }

            // Re-populate the ViewBag with the filtered list of paramedics
            var currentUserAgain = await _context.UserParamedics
                .FirstOrDefaultAsync(u => u.FkIdentityUser == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (currentUserAgain != null)
            {
                var currentCompanyId = currentUserAgain.FkCompany;
                ViewBag.Paramedics = await _context.UserParamedics
                    .Where(p => p.FkCompany == currentCompanyId)
                    .ToListAsync();
            }

            return View(paramedic);
        }


        [Authorize(Roles = "SuperAdmin,Paramedic")]
        public async Task<IActionResult> SearchCitoyen(string searchString)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null)
            {
                return NotFound();
            }

            if (!currentUser.FkRoleNavigation.GetCitoyen)
            {
                return RedirectToAction("Index", "Admin");
            }

            IQueryable<UserInfo> citoyensQuery = _context.UserInfos;

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim(); // Remove leading/trailing whitespace

                int parsedUserId;
                bool isNumeric = int.TryParse(searchString, out parsedUserId);

                citoyensQuery = citoyensQuery.Where(c =>
                    c.Nom.Contains(searchString) ||
                    c.Prenom.Contains(searchString) ||
                    (isNumeric && c.FkUserId == parsedUserId)
                );
            }

            var citoyenList = await citoyensQuery.ToListAsync();

            // Pass searchString to the view using ViewBag
            ViewBag.SearchString = searchString;

            if (!string.IsNullOrWhiteSpace(searchString) && !citoyenList.Any())
            {
                ViewBag.Message = "No citizens found matching your search criteria.";
            }

            return View(citoyenList);
        }





        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // GET: Admin/ManageCompany/ViewCitoyen
        public async Task<IActionResult> ViewCitoyen(int? id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null)
            {
                return NotFound();
            }

            if (!currentUser.FkRoleNavigation.GetCitoyen)
            {
                return RedirectToAction("Index", "Admin");
            }

            var citoyen = new User
            {
                UserInfo = await _context.UserInfos.FirstAsync(u => u.FkUserId == id),
                Addresses = await _context.UserAdresses.Where(a => a.FkUserId == id).ToListAsync(),
                Allergies = await _context.UserAllergies.Where(a => a.FkUserId == id).ToListAsync(),
                Antecedent = await _context.UserAntecedents.FirstAsync(a => a.FkUserId == id),
                Medications = await _context.UserMedications.Where(m => m.FkUserId == id).ToListAsync(),
                Handicaps = await _context.UserHandicaps.Where(h => h.FkUserId == id).ToListAsync()
            };


            if (citoyen == null)
            {
                return NotFound();
            }

            return View(citoyen);
        }

    }
}

