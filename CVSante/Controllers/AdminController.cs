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
            // Fetch all paramedics associated with the company
            var paramedics = await _context.UserParamedics
                .Include(u => u.FkRoleNavigation)
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

            _context.CompanyRoles.Update(roleToUpdate);
            await _context.SaveChangesAsync();

            return RedirectToAction("ManageRoles", new { paramedicId = selectedParamedicId });
        }




















        // GET: Admin/Create
        public IActionResult Create()
        {
            ViewData["FkCompany"] = new SelectList(_context.Companies, "IdComp", "IdComp");
            ViewData["FkIdentityUser"] = new SelectList(_context.AspNetUsers, "Id", "Id");
            return View();
        }

        // POST: Admin/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,Prenom,Ville,Telephone,ParamId,ParamIsActive,Matricule,FkCompany,FkIdentityUser")] UserParamedic userParamedic)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userParamedic);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FkCompany"] = new SelectList(_context.Companies, "IdComp", "IdComp", userParamedic.FkCompany);
            ViewData["FkIdentityUser"] = new SelectList(_context.AspNetUsers, "Id", "Id", userParamedic.FkIdentityUser);
            return View(userParamedic);
        }

        // GET: Admin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userParamedic = await _context.UserParamedics.FindAsync(id);
            if (userParamedic == null)
            {
                return NotFound();
            }
            ViewData["FkCompany"] = new SelectList(_context.Companies, "IdComp", "IdComp", userParamedic.FkCompany);
            ViewData["FkIdentityUser"] = new SelectList(_context.AspNetUsers, "Id", "Id", userParamedic.FkIdentityUser);
            return View(userParamedic);
        }

        // POST: Admin/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Nom,Prenom,Ville,Telephone,ParamId,ParamIsActive,Matricule,FkCompany,FkIdentityUser")] UserParamedic userParamedic)
        {
            if (id != userParamedic.ParamId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userParamedic);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserParamedicExists(userParamedic.ParamId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FkCompany"] = new SelectList(_context.Companies, "IdComp", "IdComp", userParamedic.FkCompany);
            ViewData["FkIdentityUser"] = new SelectList(_context.AspNetUsers, "Id", "Id", userParamedic.FkIdentityUser);
            return View(userParamedic);
        }

        // GET: Admin/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userParamedic = await _context.UserParamedics
                .Include(u => u.FkCompanyNavigation)
                .Include(u => u.FkIdentityUserNavigation)
                .FirstOrDefaultAsync(m => m.ParamId == id);
            if (userParamedic == null)
            {
                return NotFound();
            }

            return View(userParamedic);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userParamedic = await _context.UserParamedics.FindAsync(id);
            if (userParamedic != null)
            {
                _context.UserParamedics.Remove(userParamedic);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserParamedicExists(int id)
        {
            return _context.UserParamedics.Any(e => e.ParamId == id);
        }
    }
}
