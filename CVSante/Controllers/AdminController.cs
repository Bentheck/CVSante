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
using System.Globalization;

namespace CVSante.Controllers
{
    public class AdminController : Controller
    {
        private readonly CvsanteContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserValidation _UserValidation;
        private readonly IHistoryService _historyService;
        private int _currentUser;

        public AdminController(CvsanteContext context, UserManager<IdentityUser> userManager, UserValidation userValidation, IHistoryService historyService)
        {
            _context = context;
            _userManager = userManager;
            _UserValidation = userValidation;
            _historyService = historyService;
        }


        public int GetCurrentUser()
        {
            // Define a constant or a static readonly field for the default value
            const int UserNotFound = -1;

            // Get the current user ID from the UserManager
            var user = _userManager.GetUserAsync(User).Result; // Synchronously wait for the task to complete

            if (user == null)
            {
                // Handle case where the user is not found
                Console.WriteLine("User not found.");
                _currentUser = UserNotFound; // Set _currentUser to a default value indicating not found
                return _currentUser;
            }

            // Fetch the UserParamedic entity using the user ID
            var currentUser = _context.UserParamedics
                .FirstOrDefaultAsync(up => up.FkIdentityUser == user.Id).Result; // Synchronously wait for the task to complete

            if (currentUser == null)
            {
                Console.WriteLine($"No UserParamedic found for userId: {user.Id}");
                _currentUser = UserNotFound; // Set _currentUser to a default value indicating not found
                return _currentUser;
            }

            // Update the _currentUser field and return its value
            _currentUser = currentUser.ParamId; // Set _currentUser to the paramId of the current user
            return _currentUser;
        }


        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // GET: Admin
        public async Task<IActionResult> Index()
        {
            GetCurrentUser();
            if (_currentUser == null)
            {
                return NotFound();
            }

            await _historyService.LogActionAsync(null, _currentUser, "Accessed Admin Index page");
                return View();

            
        }


        // REDIRECT: Admin/History
        public async Task<IActionResult> RedirectToHistory(int? id)
        {
            GetCurrentUser();
            if (_currentUser == null)
            {
                return NotFound();
            }
            if (id == null)
            {
                return NotFound();
            }
            await _historyService.LogActionAsync(null, _currentUser, "Accessed History page");
            return RedirectToAction("History", new { id = id });
        }


        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // GET: Admin/History
        public async Task<IActionResult> History(
            int? pageNumber,
            int? pageSize,
            string dateFrom,
            string dateTo,
            string idParam,
            string nomParam,
            string actionType,
            string sortOrder)
        {
            var currentUserId = _userManager.GetUserId(User);
            var userParam = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
                .FirstOrDefaultAsync(up => up.FkIdentityUser == currentUserId);

            if (userParam == null)
            {
                return NotFound();
            }

            if (!userParam.FkRoleNavigation.GetHistorique)
            {
                return RedirectToAction("Index", "Admin");
            }

            var historique = _context.HistoriqueParams
                .Include(h => h.FkParam)
                .Include(h => h.FkUser)
                .Where(h => h.FkParamId == userParam.ParamId);

            // Apply filters
            if (!string.IsNullOrEmpty(dateFrom))
            {
                if (DateTime.TryParseExact(dateFrom, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fromDate))
                {
                    historique = historique.Where(h => h.Date >= fromDate.Date);
                }
            }

            if (!string.IsNullOrEmpty(dateTo))
            {
                if (DateTime.TryParseExact(dateTo, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime toDate))
                {
                    historique = historique.Where(h => h.Date <= toDate.Date.AddDays(1).AddTicks(-1));
                }
            }

            if (!string.IsNullOrEmpty(idParam))
            {
                historique = historique.Where(h => h.FkParam.Matricule.Contains(idParam));
            }

            if (!string.IsNullOrEmpty(nomParam))
            {
                historique = historique.Where(h => h.FkParam.Nom.Contains(nomParam));
            }

            if (!string.IsNullOrEmpty(actionType))
            {
                historique = historique.Where(h => h.Action.Contains(actionType));
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "id_desc":
                    historique = historique.OrderByDescending(h => h.HistId);
                    break;
                case "id_asc":
                    historique = historique.OrderBy(h => h.HistId);
                    break;
                case "date_desc":
                    historique = historique.OrderByDescending(h => h.Date);
                    break;
                case "date_asc":
                    historique = historique.OrderBy(h => h.Date);
                    break;
                case "name_desc":
                    historique = historique.OrderByDescending(h => h.FkParam.Nom);
                    break;
                case "name_asc":
                    historique = historique.OrderBy(h => h.FkParam.Nom);
                    break;
                case "action_desc":
                    historique = historique.OrderByDescending(h => h.Action);
                    break;
                case "action_asc":
                    historique = historique.OrderBy(h => h.Action);
                    break;
                default:
                    historique = historique.OrderByDescending(h => h.Date);
                    break;
            }

            int pageSizeValue = pageSize ?? 10;
            var paginatedList = await PaginatedList<HistoriqueParam>.CreateAsync(historique.AsNoTracking(), pageNumber ?? 1, pageSizeValue);

            ViewData["CurrentFilterDateFrom"] = dateFrom;
            ViewData["CurrentFilterDateTo"] = dateTo;
            ViewData["CurrentFilterIdParam"] = idParam;
            ViewData["CurrentFilterNomParam"] = nomParam;
            ViewData["CurrentFilterActionType"] = actionType;
            ViewData["CurrentSortOrder"] = sortOrder ?? "date_desc"; // Ensure a default value

            return View(paginatedList);
        }




        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // GET: Admin/Roles/ManageRoles
        public async Task<IActionResult> ManageRoles(int? paramedicId)
        {
            GetCurrentUser();
            if (_currentUser == null)
            {
                return NotFound();
            }
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

            await _historyService.LogActionAsync(null, _currentUser, $"Accessed ManageRoles page (SelectedParamedicId: {selectedParamedic.ParamId})");

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
            GetCurrentUser();
            if (_currentUser == null)
            {
                return NotFound();
            }

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

            await _historyService.LogActionAsync(null, selectedParamedicId,
                $"Successfully updated role {selectedRole.IdRole}");

            return RedirectToAction("ManageRoles", new { paramedicId = selectedParamedicId });
        }


        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // GET: Admin/ManageCompany
        public async Task<IActionResult> ManageCompany()
        {
            GetCurrentUser();
            if (_currentUser == null)
            {
                return NotFound();
            }

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

            await _historyService.LogActionAsync(null, _currentUser, $"Accessed ManageCompany page for Company ID {company.IdComp}");

            return View(viewModel);
        }


        // POST: Admin/ManageCompany/RemoveFromCompany
        [HttpPost]
        public async Task<IActionResult> ManageCompany(ManageCompany viewModel, int? removeParamedicId)
        {
            GetCurrentUser();
            if (_currentUser == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null)
            {
                // Log if currentUser is not found
                await _historyService.LogActionAsync(null, _currentUser, $"Failed to access ManageCompany page: UserParamedic not found for user ID {currentUserId}");
                return NotFound();
            }

            if (removeParamedicId.HasValue)
            {
                var paramedicToRemove = await _context.UserParamedics
                    .Include(p => p.FkRoleNavigation)
                    .FirstOrDefaultAsync(p => p.ParamId == removeParamedicId.Value);

                if (paramedicToRemove != null)
                {
                    // Log removal action
                    await _historyService.LogActionAsync(null, _currentUser,
                        $"Removed paramedic with ID {paramedicToRemove.Matricule} from company. Updated paramedic's status to inactive.");

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

                // Log changes to paramedics' active status
                await _historyService.LogActionAsync(null, _currentUser,
                    "Updated paramedics' active status in ManageCompany.");
            }
            else
            {
                // Handle the case where Paramedics is null
                ModelState.AddModelError("", "Paramedics data is missing.");

                // Log missing data issue
                await _historyService.LogActionAsync(null, _currentUser,
                    "Failed to update paramedics: Missing paramedics data.");
            }

            return RedirectToAction("ManageCompany");
        }


        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // GET: Admin/ManageCompany/AddRespondent
        public async Task<IActionResult> AddRespondent()
        {
            GetCurrentUser();
            if (_currentUser == null)
            {
                return NotFound();
            }

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
            GetCurrentUser();
            if (_currentUser == null)
            {
                return NotFound();
            }

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
                    await _historyService.LogActionAsync(0, 0, $"Failed to add respondent: User not found with email {viewModel.Email}");
                    return View(viewModel);
                }

                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Paramedic"))
                {
                    ModelState.AddModelError("Email", "L'utilisateur est déjà un paramédic. Veuillez l'ajouter en utilisant son matricule.");
                    await _historyService.LogActionAsync(null, _currentUser, $"Failed to add respondent: User with email {viewModel.Email} is already a paramedic.");
                    return View(viewModel);
                }

                var existingParamedic = await _context.UserParamedics
                    .FirstOrDefaultAsync(p => p.FkIdentityUser == user.Id);

                if (existingParamedic != null)
                {
                    ModelState.AddModelError("Email", "L'utilisateur est déjà enregistré en tant que paramédic. Veuillez l'ajouter en utilisant son matricule.");
                    await _historyService.LogActionAsync(null, _currentUser, $"Failed to add respondent: User with email {viewModel.Email} is already registered as a paramedic.");
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

                // Log successful addition of a new paramedic
                await _historyService.LogActionAsync(null, _currentUser,
                    $"Successfully added new paramedic with ID {newParamedic.ParamId} and role ID {newRoleId}.");

                return RedirectToAction("ManageCompany");
            }

            // Log model state errors
            await _historyService.LogActionAsync(null, _currentUser,
                $"Failed to add respondent due to invalid model state: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");

            return View(viewModel);
        }


        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // POST: Admin/ManageCompany/AddByMatricule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddByMatricule(AddRespondent viewModel)
        {
            GetCurrentUser();
            if (_currentUser == null)
            {
                return NotFound();
            }

            var existingParamedic = await _context.UserParamedics
                .Include(p => p.FkIdentityUserNavigation)
                .FirstOrDefaultAsync(p => p.Matricule == viewModel.UserParamedic.Matricule);

            if (existingParamedic == null)
            {
                // Log when no paramedic is found with the provided matricule
                await _historyService.LogActionAsync(null, _currentUser,
                    $"Failed to add paramedic by matricule: No paramedic found with matricule {viewModel.UserParamedic.Matricule}.");

                ModelState.AddModelError("Matricule", "Aucun paramédic trouvé avec ce matricule dans cette entreprise.");
                return View("AddRespondent", viewModel);
            }

            if (existingParamedic.FkCompany != null)
            {
                // Log if the paramedic belongs to another company
                await _historyService.LogActionAsync(null, _currentUser,
                    $"Failed to add paramedic by matricule: The paramedic with matricule {viewModel.UserParamedic.Matricule} already belongs to another company.");

                ModelState.AddModelError("Matricule", "Le paramédic appartient à une autre entreprise.");
                return View("AddRespondent", viewModel);
            }

            existingParamedic.FkCompany = viewModel.CompanyId;
            existingParamedic.ParamIsActive = true;

            await _context.SaveChangesAsync();

            // Log successful addition of the paramedic
            await _historyService.LogActionAsync(existingParamedic.ParamId, viewModel.CompanyId,
                $"Successfully added paramedic with matricule {viewModel.UserParamedic.Matricule} to company ID {viewModel.CompanyId}.");

            return RedirectToAction("ManageCompany");
        }





        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // GET: Admin/ManageCompany/EditRespondent
        public async Task<IActionResult> EditRespondent(int? paramedicId)
        {
            GetCurrentUser();
            if (_currentUser == null)
            {
                return NotFound();
            }
            // Get the current user's ID
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Fetch the current user's paramedic details
            var currentUser = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null)
            {
                // Log when the current user is not found
                await _historyService.LogActionAsync(null, _currentUser,
                    $"Failed to retrieve paramedic details for user ID {currentUserId}. Current user not found.");

                return NotFound();
            }

            if (!currentUser.FkRoleNavigation.EditParamedic)
            {
                // Log when the user does not have the necessary permissions
                await _historyService.LogActionAsync(currentUser.ParamId, 0,
                    $"User ID {currentUserId} attempted to access EditRespondent without the necessary permissions.");

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
                    // Log when the paramedic to be edited cannot be found
                    await _historyService.LogActionAsync(currentUser.ParamId, paramedicId.Value,
                        $"Failed to retrieve paramedic with ID {paramedicId.Value} for editing. The paramedic was not found.");

                    return NotFound();
                }

                // Optionally log successful retrieval of the paramedic
                await _historyService.LogActionAsync(currentUser.ParamId, paramedic.ParamId,
                    $"Successfully retrieved paramedic with ID {paramedic.ParamId} for editing.");
            }

            return View(paramedic);
        }



        [Authorize(Roles = "SuperAdmin,Paramedic")]
        // POST: Admin/ManageCompany/EditRespondent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRespondent(UserParamedic paramedic)
        {
            GetCurrentUser();
            if (_currentUser == null)
            {
                return NotFound();
            }
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

                await _historyService.LogActionAsync(null, _currentUser,
                $"Successfully updated paramedic with ID {paramedic.Matricule}.");

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
        // GET: Admin/ManageCompany/SearchCitoyen
        public async Task<IActionResult> SearchCitoyen(string searchString)
        {
            GetCurrentUser();
            if (_currentUser == null)
            {
                return NotFound();
            }
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var paramId = _context.UserParamedics.Where(u => u.FkIdentityUser == currentUserId)
                .Select(u => u.ParamId)
                .FirstOrDefault();

            // Log the action of accessing the SearchCitoyen page
            await _historyService.LogActionAsync(null, paramId, $"Accessed SearchCitoyen page");

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

            // Log the result of the search query
            await _historyService.LogActionAsync(null, paramId, $"Performed search for citizens with searchString: {searchString}. Results found: {citoyenList.Count}");

            // Pass searchString to the view using ViewBag
            ViewBag.SearchString = searchString;

            if (!string.IsNullOrWhiteSpace(searchString) && !citoyenList.Any())
            {
                // Log when no citizens are found
                await _historyService.LogActionAsync(null, paramId, $"No citizens found matching the search criteria: {searchString}");
                ViewBag.Message = "No citizens found matching your search criteria.";
            }

            return View(citoyenList);
        }





        // GET: Admin/ManageCompany/ViewCitoyen
        [Authorize(Roles = "SuperAdmin,Paramedic")]
        public async Task<IActionResult> ViewCitoyen(int? id)
        {
            GetCurrentUser();
            if (_currentUser == null)
            {
                return NotFound();
            }
            // Log the attempt to access ViewCitoyen
            await _historyService.LogActionAsync(id, _currentUser, $"Attempting to view citizen details. ID: {id}");

            if (id == null)
            {
                // Log the case when no ID is provided
                await _historyService.LogActionAsync(id, _currentUser, "Attempted to view citizen details without providing an ID.");
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null)
            {
                // Log if the current user is not found
                await _historyService.LogActionAsync(id, _currentUser, "Attempted to view citizen details, but the current user was not found.");
                return NotFound();
            }

            if (!currentUser.FkRoleNavigation.GetCitoyen)
            {
                // Log if the current user lacks permission
                await _historyService.LogActionAsync(id, _currentUser, "Attempted to view citizen details without sufficient permissions.");
                return RedirectToAction("Index", "Admin");
            }

            var userInfo = await _context.UserInfos.FirstOrDefaultAsync(u => u.FkUserId == id);
            if (userInfo == null)
            {
                // Log if the user information is not found
                await _historyService.LogActionAsync(id, _currentUser, $"UserInfo with ID: {id} was not found.");
                return NotFound();
            }

            var citoyen = new ParamedicUserView
            {
                UserInfo = userInfo,
                Addresses = await _context.UserAdresses.Where(a => a.FkUserId == id).OrderByDescending(a => a.AdressePrimaire).ToListAsync(),
                Allergies = await _context.UserAllergies.Where(a => a.FkUserId == id).ToListAsync(),
                Antecedent = await _context.UserAntecedents.FirstOrDefaultAsync(a => a.FkUserId == id),
                Medications = await _context.UserMedications.Where(m => m.FkUserId == id).ToListAsync(),
                Handicaps = await _context.UserHandicaps.Where(h => h.FkUserId == id).ToListAsync(),
                Commentaires = await _context.Commentaires.Where(c => c.FkUserId == id).OrderByDescending(c => c.Date).Take(100).ToListAsync(),
                CurrentUserParamId = currentUser.ParamId
            };

            // Log successful retrieval of user details
            await _historyService.LogActionAsync(id, _currentUser, $"Successfully retrieved details for UserInfo ID: {id}");

            return View(citoyen);
        }


        // POST: Admin/ManageCompany/AddComment
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Paramedic")]
        public async Task<IActionResult> AddComment(int userId, string commentText)
        {
            GetCurrentUser();
            if (_currentUser == null)
            {
                return NotFound();
            }
            // Log the attempt to add a comment
            await _historyService.LogActionAsync(null, _currentUser, $"Attempting to add a comment. Comment Text: {commentText}");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.UserParamedics
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null)
            {
                // Log if the current user is not found
                await _historyService.LogActionAsync(null, _currentUser, "Failed to add comment: current user not found.");
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(commentText))
            {
                // Log if the comment text is empty
                await _historyService.LogActionAsync(null, _currentUser, "Failed to add comment: comment text was empty.");
                ModelState.AddModelError("", "Comment text cannot be empty.");
                return RedirectToAction("ViewCitoyen", new { id = userId, showModal = true });
            }

            var commentaire = new Commentaire
            {
                Date = DateTime.Now,
                Comment = commentText,
                FkUserId = userId,
                FkUserparamedic = currentUser.ParamId
            };

            _context.Commentaires.Add(commentaire);
            await _context.SaveChangesAsync();

            // Log the successful addition of the comment
            await _historyService.LogActionAsync(null, _currentUser, $"Successfully added a comment {commentaire.Id}. Comment Text: {commentText}");

            return RedirectToAction("ViewCitoyen", new { id = userId, showModal = true });
        }


        // POST: Admin/ManageCompany/EditComment
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Paramedic")]
        public async Task<IActionResult> EditComment(int commentId, string commentText)
        {
            GetCurrentUser();
            if (_currentUser == null)
            {
                return NotFound();
            }
            // Log the attempt to edit a comment
            await _historyService.LogActionAsync(null, _currentUser, $"Attempting to edit comment {commentId}. New Comment Text: {commentText}");

            if (string.IsNullOrWhiteSpace(commentText))
            {
                // Log if the comment text is empty
                await _historyService.LogActionAsync(null, _currentUser, "Failed to edit comment: comment text was empty.");
                ModelState.AddModelError("", "Comment text cannot be empty.");
                return RedirectToAction("ViewCitoyen", new { id = commentId, showModal = true });
            }

            var comment = await _context.Commentaires.FindAsync(commentId);
            if (comment == null)
            {
                // Log if the comment is not found
                await _historyService.LogActionAsync(null, _currentUser, "Failed to edit comment: comment not found.");
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.UserParamedics
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null || comment.FkUserparamedic != currentUser.ParamId)
            {
                // Log if the current user is unauthorized
                await _historyService.LogActionAsync(null, _currentUser, "Unauthorized attempt to edit comment.");
                return Unauthorized();
            }

            comment.Comment = commentText;
            _context.Commentaires.Update(comment);
            await _context.SaveChangesAsync();

            // Log the successful editing of the comment
            await _historyService.LogActionAsync(null, _currentUser, $"Successfully edited comment. New Comment Text: {commentText}");

            return RedirectToAction("ViewCitoyen", new { id = comment.FkUserId, showModal = true });
        }

        // POST: Admin/ManageCompany/DeleteComment
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Paramedic")]
        public async Task<IActionResult> DeleteComment(int commentId, int userId)
        {
            GetCurrentUser();
            if (_currentUser == null)
            {
                return NotFound();
            }
            // Log the attempt to delete a comment
            await _historyService.LogActionAsync(null, _currentUser, $"Attempting to delete comment with ID: {commentId}");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.UserParamedics
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null)
            {
                // Log unauthorized access attempt
                await _historyService.LogActionAsync(null, _currentUser, "Failed to delete comment: Unauthorized user.");
                return Unauthorized();
            }

            var comment = await _context.Commentaires.FindAsync(commentId);
            if (comment == null)
            {
                // Log if the comment is not found
                await _historyService.LogActionAsync(null, _currentUser, "Failed to delete comment: Comment not found.");
                return NotFound();
            }

            _context.Commentaires.Remove(comment);
            await _context.SaveChangesAsync();

            // Log successful deletion of the comment
            await _historyService.LogActionAsync(null, _currentUser, $"Successfully deleted comment with ID: {commentId}");

            return RedirectToAction("ViewCitoyen", new { id = userId, showModal = true });
        }

    }
}

