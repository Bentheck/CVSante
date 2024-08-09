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

        // GET: Admin
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User); // currentUserId is already a string
            var userParam = await _context.UserParamedics
                .FirstOrDefaultAsync(up => up.FkIdentityUser == currentUserId);

            if (userParam == null)
            {
                return NotFound();
            }

            var profilAdmin = new Paramedic
            {
                paramInfo = userParam,
                historique = _context.HistoriqueParams.Where(h => h.FkParamId == userParam.ParamId).ToList(),
                compRole = _context.CompanyRoles.FirstOrDefault(c => c.IdRole == userParam.FkCompany),
                company = _context.Companies.FirstOrDefault(c => c.IdComp == userParam.FkCompany),
                commentaires = _context.Commentaires.Where(c => c.FkUserparamedic == userParam.ParamId).ToList()
            };

            return View(profilAdmin);
        }

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


        //POST: Admin/Roles/ManageRoles
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

            var viewModel = new ManageCompany
            {
                Company = company,
                Paramedics = _context.UserParamedics.Where(p => p.FkCompany == company.IdComp).ToList()
            };

            return View(viewModel);
        }


        // POST: Admin/ManageCompany
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

            if (ModelState.IsValid)
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
                return RedirectToAction("Index");
            }

            return View(viewModel);
        }


        



    }
}
