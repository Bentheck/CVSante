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
using static Google.Cloud.RecaptchaEnterprise.V1.TransactionData.Types;

namespace CVSante.Controllers
{
    public class ParamedicController : Controller
    {
        private readonly CvsanteContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserValidation _UserValidation;
        private readonly IHistoryService _historyService;
        private int _currentUser;

        public ParamedicController(CvsanteContext context, UserManager<IdentityUser> userManager, UserValidation userValidation, IHistoryService historyService)
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

            var currentUser = _context.UserParamedics
                    .FirstOrDefaultAsync(up => up.FkIdentityUser == _currentUser.ToString()).Result;
            if (currentUser == null)
                {
                return NotFound();
            }

            await _historyService.LogActionAsync(null, _currentUser, "Accès à la page d'index de l'administrateur");
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
            await _historyService.LogActionAsync(null, _currentUser, "Accès à la page d'historique");
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
                return RedirectToAction("Index", "Paramedic");
            }

            var historique = _context.HistoriqueParams
                .Include(h => h.FkParam)
                .Include(h => h.FkUser)
                .Where(h => h.FkParamId == userParam.ParamId);

            // Appliquer les filtres
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

            // Appliquer le tri
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
            ViewData["CurrentSortOrder"] = sortOrder ?? "date_desc"; // Assurer une valeur par défaut

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
            // Obtenir l'ID de l'utilisateur actuel (en supposant que c'est ainsi que vous identifiez l'utilisateur actuel)
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Récupérer les détails du paramédical de l'utilisateur actuel
            var currentUser = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null)
            {
                return NotFound();
            }

            // Vérifier si l'utilisateur actuel a la permission EditRole
            if (!currentUser.FkRoleNavigation.EditRole)
            {
                return RedirectToAction("ManageCompany", "Paramedic");
            }

            // Récupérer tous les paramédicaux associés à l'entreprise
            var paramedics = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
                .Where(u => u.FkCompany == currentUser.FkCompany)
                .ToListAsync();

            // Sélectionner le paramédical actuel si paramedicId est fourni, sinon utiliser le premier paramédical par défaut
            var selectedParamedic = paramedicId.HasValue
                ? paramedics.FirstOrDefault(p => p.ParamId == paramedicId.Value)
                : paramedics.FirstOrDefault();

            if (selectedParamedic == null)
            {
                return NotFound();
            }

            var companyId = selectedParamedic.FkCompany;

            await _historyService.LogActionAsync(null, _currentUser, $"Accès à la page ManageRoles (SelectedParamedicId: {selectedParamedic.ParamId})");

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
                // Journaliser les erreurs de l'état du modèle
                Console.WriteLine("L'état du modèle n'est pas valide.");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Clé: {error.Key}, Erreurs: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }

                // Récupérer les données requises pour reconstituer le modèle de vue
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

                // Retourner à la vue avec les erreurs de validation
                return View("ManageRoles", viewModel);
            }

            // Trouver le rôle à mettre à jour
            var roleToUpdate = await _context.CompanyRoles
                .FirstOrDefaultAsync(r => r.IdRole == selectedRole.IdRole);

            if (roleToUpdate == null)
            {
                return NotFound();
            }

            // Mettre à jour les propriétés du rôle
            roleToUpdate.CreateParamedic = selectedRole.CreateParamedic;
            roleToUpdate.EditParamedic = selectedRole.EditParamedic;
            roleToUpdate.GetHistorique = selectedRole.GetHistorique;
            roleToUpdate.GetCitoyen = selectedRole.GetCitoyen;
            roleToUpdate.EditRole = selectedRole.EditRole;
            roleToUpdate.EditCompany = selectedRole.EditCompany;

            _context.CompanyRoles.Update(roleToUpdate);
            await _context.SaveChangesAsync();

            await _historyService.LogActionAsync(null, selectedParamedicId,
                $"Rôle {selectedRole.IdRole} mis à jour avec succès");

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

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // currentUserId est déjà une chaîne de caractères
            var currentUser = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null)
            {
                return NotFound();
            }

            if (!currentUser.FkRoleNavigation.EditCompany)
            {
                return RedirectToAction("Index", "Paramedic");
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

            await _historyService.LogActionAsync(null, _currentUser, $"Accès à la page ManageCompany pour l'ID de l'entreprise {company.IdComp}");

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
                // Journaliser si currentUser n'est pas trouvé
                await _historyService.LogActionAsync(null, _currentUser, $"Échec d'accès à la page ManageCompany : UserParamedic non trouvé pour l'ID utilisateur {currentUserId}");
                return NotFound();
            }

            if (removeParamedicId.HasValue)
            {
                var paramedicToRemove = await _context.UserParamedics
                    .Include(p => p.FkRoleNavigation)
                    .FirstOrDefaultAsync(p => p.ParamId == removeParamedicId.Value);

                if (paramedicToRemove != null)
                {
                    // Journaliser l'action de suppression
                    await _historyService.LogActionAsync(null, _currentUser,
                        $"Suppression du paramédical avec l'ID {paramedicToRemove.Matricule} de l'entreprise. Mise à jour du statut du paramédical à inactif.");

                    // Définir FkCompany du paramédical sur null (supprimer de l'entreprise)
                    paramedicToRemove.FkCompany = null;

                    // Définir le paramédical comme inactif
                    paramedicToRemove.ParamIsActive = false;

                    // Définir tous les rôles sur false
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
                // Enregistrer les modifications apportées aux paramédicaux
                foreach (var paramedic in viewModel.Paramedics)
                {
                    var existingParamedic = await _context.UserParamedics.FindAsync(paramedic.ParamId);
                    if (existingParamedic != null)
                    {
                        existingParamedic.ParamIsActive = paramedic.ParamIsActive;
                    }
                }

                await _context.SaveChangesAsync();

                // Journaliser les modifications apportées au statut actif des paramédicaux
                await _historyService.LogActionAsync(null, _currentUser,
                    "Mise à jour du statut actif des paramédicaux dans ManageCompany.");
            }
            else
            {
                // Gérer le cas où Paramedics est null
                ModelState.AddModelError("", "Données des paramédicaux manquantes.");

                // Journaliser le problème des données manquantes
                await _historyService.LogActionAsync(null, _currentUser,
                    "Échec de la mise à jour des paramédicaux : Données des paramédicaux manquantes.");
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
                return RedirectToAction("Index", "Paramedic");
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
                    await _historyService.LogActionAsync(null, _currentUser, $"Échec de l'ajout du répondant : aucun utilisateur trouvé avec l'email {viewModel.Email}");
                    return View(viewModel);
                }

                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Paramedic"))
                {
                    ModelState.AddModelError("Email", "L'utilisateur est déjà un paramédic. Veuillez l'ajouter en utilisant son matricule.");
                    await _historyService.LogActionAsync(null, _currentUser, $"Échec de l'ajout du répondant : l'utilisateur avec l'email {viewModel.Email} est déjà un paramédic.");
                    return View(viewModel);
                }

                var existingParamedic = await _context.UserParamedics
                    .FirstOrDefaultAsync(p => p.FkIdentityUser == user.Id);

                if (existingParamedic != null)
                {
                    ModelState.AddModelError("Email", "L'utilisateur est déjà enregistré en tant que paramédic. Veuillez l'ajouter en utilisant son matricule.");
                    await _historyService.LogActionAsync(null, _currentUser, $"Échec de l'ajout du répondant : l'utilisateur avec l'email {viewModel.Email} est déjà enregistré en tant que paramédic.");
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

                // Enregistrer l'ajout réussi d'un nouveau paramédic
                await _historyService.LogActionAsync(null, _currentUser,
                    $"Nouveau paramédic ajouté avec succès avec l'ID {newParamedic.ParamId} et l'ID de rôle {newRoleId}.");

                return RedirectToAction("ManageCompany");
            }

            // Enregistrer les erreurs de l'état du modèle
            await _historyService.LogActionAsync(null, _currentUser,
                $"Échec de l'ajout du répondant en raison d'un état de modèle non valide : {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");

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
                // Enregistrer lorsque aucun paramédic n'est trouvé avec le matricule fourni
                await _historyService.LogActionAsync(null, _currentUser,
                    $"Échec de l'ajout du paramédic par matricule : aucun paramédic trouvé avec le matricule {viewModel.UserParamedic.Matricule}.");

                ModelState.AddModelError("Matricule", "Aucun paramédic trouvé avec ce matricule dans cette entreprise.");
                return View("AddRespondent", viewModel);
            }

            if (existingParamedic.FkCompany != null)
            {
                // Enregistrer si le paramédic appartient à une autre entreprise
                await _historyService.LogActionAsync(null, _currentUser,
                    $"Échec de l'ajout du paramédic par matricule : le paramédic avec le matricule {viewModel.UserParamedic.Matricule} appartient déjà à une autre entreprise.");

                ModelState.AddModelError("Matricule", "Le paramédic appartient à une autre entreprise.");
                return View("AddRespondent", viewModel);
            }

            existingParamedic.FkCompany = viewModel.CompanyId;
            existingParamedic.ParamIsActive = true;

            await _context.SaveChangesAsync();

            // Enregistrer l'ajout réussi du paramédic
            await _historyService.LogActionAsync(null, _currentUser,
                $"Paramédic ajouté avec succès avec le matricule {viewModel.UserParamedic.Matricule} à l'ID de l'entreprise {viewModel.CompanyId}.");

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
            // Obtenir l'ID de l'utilisateur actuel
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Récupérer les détails du paramédic de l'utilisateur actuel
            var currentUser = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null)
            {
                // Enregistrer lorsque l'utilisateur actuel n'est pas trouvé
                await _historyService.LogActionAsync(null, _currentUser,
                    $"Échec de la récupération des détails du paramédic pour l'ID utilisateur {currentUserId}. Utilisateur actuel non trouvé.");

                return NotFound();
            }

            if (!currentUser.FkRoleNavigation.EditParamedic)
            {
                // Enregistrer lorsque l'utilisateur n'a pas les autorisations nécessaires
                await _historyService.LogActionAsync(null, _currentUser,
                    $"L'utilisateur avec l'ID {currentUserId} a tenté d'accéder à EditRespondent sans les autorisations nécessaires.");

                return RedirectToAction("ManageCompany", "Paramedic");
            }

            var currentCompanyId = currentUser.FkCompany;

            // Récupérer les paramédics appartenant à la même entreprise que l'utilisateur actuel
            var paramedics = await _context.UserParamedics
                .Where(p => p.FkCompany == currentCompanyId)
                .ToListAsync();

            ViewBag.Paramedics = paramedics;

            // Récupérer le paramédic à modifier si paramedicId est fourni
            UserParamedic paramedic = null;
            if (paramedicId.HasValue)
            {
                paramedic = await _context.UserParamedics
                    .Include(p => p.FkRoleNavigation)
                    .FirstOrDefaultAsync(p => p.ParamId == paramedicId.Value && p.FkCompany == currentCompanyId);

                if (paramedic == null)
                {
                    // Enregistrer lorsque le paramédic à modifier ne peut pas être trouvé
                    await _historyService.LogActionAsync(null, _currentUser,
                        $"Échec de la récupération du paramédic avec l'ID {paramedicId.Value} pour la modification. Le paramédic n'a pas été trouvé.");

                    return NotFound();
                }

                // Enregistrer éventuellement la récupération réussie du paramédic
                await _historyService.LogActionAsync(null, _currentUser,
                    $"Paramédic récupéré avec succès avec l'ID {paramedic.ParamId} pour la modification.");
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
            // Supprimer la validation des champs qui ne font pas partie du formulaire
            ModelState.Remove("FkRoleNavigation");
            ModelState.Remove("FkIdentityUserNavigation");

            if (ModelState.IsValid)
            {
                // Obtenir l'ID de l'utilisateur actuel
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Récupérer les détails du paramédic de l'utilisateur actuel
                var currentUser = await _context.UserParamedics
                    .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

                if (currentUser == null)
                {
                    return NotFound();
                }

                var currentCompanyId = currentUser.FkCompany;

                // Récupérer l'entité paramédic existante dans la même entreprise
                var existingParamedic = await _context.UserParamedics
                    .Include(p => p.FkRoleNavigation)
                    .FirstOrDefaultAsync(p => p.ParamId == paramedic.ParamId && p.FkCompany == currentCompanyId);

                if (existingParamedic == null)
                {
                    return NotFound();
                }

                // Mettre à jour uniquement les champs autorisés
                existingParamedic.Nom = paramedic.Nom;
                existingParamedic.Prenom = paramedic.Prenom;
                existingParamedic.Ville = paramedic.Ville;
                existingParamedic.Telephone = paramedic.Telephone;
                existingParamedic.Matricule = paramedic.Matricule;
                existingParamedic.ParamIsActive = paramedic.ParamIsActive;

                await _context.SaveChangesAsync();

                await _historyService.LogActionAsync(null, _currentUser,
                $"Paramédic mis à jour avec succès avec l'ID {paramedic.Matricule}.");

                return RedirectToAction("ManageCompany");
            }

            // Réinitialiser le ViewBag avec la liste filtrée des paramédics
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

            // Enregistrer l'action d'accéder à la page SearchCitoyen
            await _historyService.LogActionAsync(null, paramId, $"Accès à la page SearchCitoyen");

            var currentUser = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null)
            {
                return NotFound();
            }

            if (!currentUser.FkRoleNavigation.GetCitoyen)
            {
                return RedirectToAction("Index", "Paramedic");
            }

            IQueryable<UserInfo> citoyensQuery = _context.UserInfos;

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim(); // Supprimer les espaces en début et fin de chaîne

                int parsedUserId;
                bool isNumeric = int.TryParse(searchString, out parsedUserId);

                citoyensQuery = citoyensQuery.Where(c =>
                    c.Nom.Contains(searchString) ||
                    c.Prenom.Contains(searchString) ||
                    (isNumeric && c.FkUserId == parsedUserId)
                );
            }

            var citoyenList = await citoyensQuery.ToListAsync();

            // Enregistrer le résultat de la requête de recherche
            await _historyService.LogActionAsync(null, paramId, $"Recherche effectuée pour les citoyens avec la chaîne de recherche : {searchString}. Résultats trouvés : {citoyenList.Count}");

            // Passer la chaîne de recherche à la vue en utilisant ViewBag
            ViewBag.SearchString = searchString;

            if (!string.IsNullOrWhiteSpace(searchString) && !citoyenList.Any())
            {
                // Enregistrer lorsque aucun citoyen n'est trouvé
                await _historyService.LogActionAsync(null, paramId, $"Aucun citoyen trouvé correspondant aux critères de recherche : {searchString}");
                ViewBag.Message = "Aucun citoyen trouvé correspondant à vos critères de recherche.";
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
            // Enregistrer la tentative d'accéder à ViewCitoyen
            await _historyService.LogActionAsync(id, _currentUser, $"Tentative d'afficher les détails du citoyen. ID : {id}");

            if (id == null)
            {
                // Enregistrer le cas où aucun ID n'est fourni
                await _historyService.LogActionAsync(id, _currentUser, "Tentative d'afficher les détails du citoyen sans fournir d'ID.");
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null)
            {
                // Enregistrer si l'utilisateur actuel n'est pas trouvé
                await _historyService.LogActionAsync(id, _currentUser, "Tentative d'afficher les détails du citoyen, mais l'utilisateur actuel n'a pas été trouvé.");
                return NotFound();
            }

            if (!currentUser.FkRoleNavigation.GetCitoyen)
            {
                // Enregistrer si l'utilisateur actuel n'a pas les autorisations suffisantes
                await _historyService.LogActionAsync(id, _currentUser, "Tentative d'afficher les détails du citoyen sans les autorisations suffisantes.");
                return RedirectToAction("Index", "Paramedic");
            }

            var userInfo = await _context.UserInfos.FirstOrDefaultAsync(u => u.FkUserId == id);
            if (userInfo == null)
            {
                // Enregistrer si les informations sur l'utilisateur ne sont pas trouvées
                await _historyService.LogActionAsync(id, _currentUser, $"UserInfo avec ID : {id} n'a pas été trouvé.");
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

            // Enregistrer la récupération réussie des détails de l'utilisateur
            await _historyService.LogActionAsync(id, _currentUser, $"Détails récupérés avec succès pour UserInfo ID : {id}");

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
            // Enregistrer la tentative d'ajouter un commentaire
            await _historyService.LogActionAsync(null, _currentUser, $"Tentative d'ajouter un commentaire. Texte du commentaire : {commentText}");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.UserParamedics
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null)
            {
                // Enregistrer si l'utilisateur actuel n'est pas trouvé
                await _historyService.LogActionAsync(null, _currentUser, "Échec de l'ajout du commentaire : utilisateur actuel non trouvé.");
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(commentText))
            {
                // Enregistrer si le texte du commentaire est vide
                await _historyService.LogActionAsync(null, _currentUser, "Échec de l'ajout du commentaire : le texte du commentaire était vide.");
                ModelState.AddModelError("", "Le texte du commentaire ne peut pas être vide.");
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

            // Enregistrer l'ajout réussi du commentaire
            await _historyService.LogActionAsync(null, _currentUser, $"Commentaire ajouté avec succès {commentaire.Id}. Texte du commentaire : {commentText}");

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
            // Enregistrer la tentative de modifier un commentaire
            await _historyService.LogActionAsync(null, _currentUser, $"Tentative de modifier le commentaire {commentId}. Nouveau texte du commentaire : {commentText}");

            if (string.IsNullOrWhiteSpace(commentText))
            {
                // Enregistrer si le texte du commentaire est vide
                await _historyService.LogActionAsync(null, _currentUser, "Échec de la modification du commentaire : le texte du commentaire était vide.");
                ModelState.AddModelError("", "Le texte du commentaire ne peut pas être vide.");
                return RedirectToAction("ViewCitoyen", new { id = commentId, showModal = true });
            }

            var comment = await _context.Commentaires.FindAsync(commentId);
            if (comment == null)
            {
                // Enregistrer si le commentaire n'est pas trouvé
                await _historyService.LogActionAsync(null, _currentUser, "Échec de la modification du commentaire : commentaire non trouvé.");
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.UserParamedics
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null || comment.FkUserparamedic != currentUser.ParamId)
            {
                // Enregistrer si l'utilisateur actuel n'est pas autorisé
                await _historyService.LogActionAsync(null, _currentUser, "Tentative non autorisée de modifier le commentaire.");
                return Unauthorized();
            }

            comment.Comment = commentText;
            _context.Commentaires.Update(comment);
            await _context.SaveChangesAsync();

            // Enregistrer la modification réussie du commentaire
            await _historyService.LogActionAsync(null, _currentUser, $"Commentaire modifié avec succès. Nouveau texte du commentaire : {commentText}");

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
            // Enregistrer la tentative de supprimer un commentaire
            await _historyService.LogActionAsync(null, _currentUser, $"Tentative de supprimer le commentaire avec l'ID : {commentId}");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.UserParamedics
                .FirstOrDefaultAsync(u => u.FkIdentityUser == currentUserId);

            if (currentUser == null)
            {
                // Enregistrer la tentative d'accès non autorisée
                await _historyService.LogActionAsync(null, _currentUser, "Échec de la suppression du commentaire : utilisateur non autorisé.");
                return Unauthorized();
            }

            var comment = await _context.Commentaires.FindAsync(commentId);
            if (comment == null)
            {
                // Enregistrer si le commentaire n'est pas trouvé
                await _historyService.LogActionAsync(null, _currentUser, "Échec de la suppression du commentaire : commentaire non trouvé.");
                return NotFound();
            }

            _context.Commentaires.Remove(comment);
            await _context.SaveChangesAsync();

            // Enregistrer la suppression réussie du commentaire
            await _historyService.LogActionAsync(null, _currentUser, $"Commentaire supprimé avec succès avec l'ID : {commentId}");

            return RedirectToAction("ViewCitoyen", new { id = userId, showModal = true });
        }

    }
}