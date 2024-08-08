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
            var currentUserId = _userManager.GetUserId(User);
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

        // GET: Admin/History/5
        public async Task<IActionResult> History(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userParamedic = await _context.UserParamedics
                .FirstOrDefaultAsync(u => u.ParamId == id);

            if (userParamedic != null && userParamedic.Role <= data[])
            {
                TempData["ErrorMessage"] = "Vous n'avez pas les droits pour accéder à cette page";
                return RedirectToAction(nameof(Index));
            }

            var historiqueParams = await _context.HistoriqueParams
                .Include(p => p.FkParam)
                .Include(u => u.FkUser)
                .ToListAsync();

            return View(historiqueParams);
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
