using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CVSante.Models;

namespace CVSante.Controllers
{
    public class SuperAdminController : Controller
    {
        private readonly CvsanteContext _context;

        public SuperAdminController(CvsanteContext context)
        {
            _context = context;
        }

        // GET: SuperAdmin
        public async Task<IActionResult> Index()
        {
            var cvsanteContext = _context.UserParamedics
                .Include(u => u.FkCompanyNavigation)
                .Include(u => u.FkIdentityUserNavigation)
                .Include(u => u.FkRoleNavigation);
            return View(await cvsanteContext.ToListAsync());
        }

        // GET: SuperAdmin/CreateCompany
        public IActionResult CreateCompany()
        {
            return View();
        }

        // POST: SuperAdmin/CreateCompany
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCompany([Bind("IdComp,CompName,AdLink,AdId")] Company company)
        {
            if (ModelState.IsValid)
            {
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Redirect to the list of companies or another relevant page
            }
            return View(company);
        }

        // GET: SuperAdmin/CreateRole
        public IActionResult CreateRole()
        {
            // Prepare ViewBag or ViewData if you need to populate dropdowns with company options
            ViewBag.FkCompany = new SelectList(_context.Companies.ToList(), "IdComp", "CompName");
            return View();
        }

        // POST: SuperAdmin/CreateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole([Bind("IdRole,CreateParamedic,EditParamedic,GetHistorique,GetCitoyen,EditRole,FkCompany")] CompanyRole role)
        {
            if (ModelState.IsValid)
            {
                _context.CompanyRoles.Add(role);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Redirect to the list of roles or another relevant page
            }
            ViewData["FkCompany"] = new SelectList(_context.Companies, "IdComp", "CompName", role.FkCompany);
            return View(role);
        }

        // GET: SuperAdmin/CreateParamedic
        public IActionResult CreateParamedic()
        {
            var companies = _context.Companies.ToList();
            var users = _context.AspNetUsers.ToList();
            var roles = _context.CompanyRoles.ToList();

            if (companies == null || users == null || roles == null)
            {
                throw new InvalidOperationException("Data not found for dropdowns.");
            }

            ViewBag.FkCompany = new SelectList(companies, "IdComp", "CompName");
            ViewBag.FkIdentityUser = new SelectList(users, "Id", "UserName");
            ViewBag.FkRole = new SelectList(roles, "IdRole", "IdRole");

            return View();
        }

        // POST: SuperAdmin/CreateParamedic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateParamedic([Bind("ParamId,Nom,Prenom,Ville,Telephone,ParamIsActive,Matricule,FkCompany,FkIdentityUser,FkRole")] UserParamedic userParamedic)
        {
            ModelState.Remove("FkCompanyNavigation");
            ModelState.Remove("FkIdentityUserNavigation");
            ModelState.Remove("FkRoleNavigation");

            if (ModelState.IsValid)
            {
                _context.UserParamedics.Add(userParamedic);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Redirect to the list of paramedics or another relevant page
            }

            var companies = _context.Companies.ToList();
            var users = _context.AspNetUsers.ToList();
            var roles = _context.CompanyRoles.ToList();

            if (companies == null || users == null || roles == null)
            {
                throw new InvalidOperationException("Data not found for dropdowns.");
            }

            ViewBag.FkCompany = new SelectList(companies, "IdComp", "CompName", userParamedic.FkCompany);
            ViewBag.FkIdentityUser = new SelectList(users, "Id", "UserName", userParamedic.FkIdentityUser);
            ViewBag.FkRole = new SelectList(roles, "IdRole", "IdRole", userParamedic.FkRole);

            return View(userParamedic);
        }

        // GET: SuperAdmin/CreateIdentityRole
        public IActionResult CreateIdentityRole()
        {
            return View();
        }

        // POST: SuperAdmin/CreateIdentityRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIdentityRole([Bind("Name")] AspNetRole role)
        {
            ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                role.Id = Guid.NewGuid().ToString();
                role.NormalizedName = role.Name?.ToUpper();
                role.ConcurrencyStamp = Guid.NewGuid().ToString();

                _context.AspNetRoles.Add(role);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Redirect to the list of roles or another relevant page
            }
            return View(role);
        }

        // GET: SuperAdmin/ListRoles
        public async Task<IActionResult> ListRoles()
        {
            var roles = await _context.AspNetRoles.ToListAsync();
            return View(roles);
        }

        // GET: SuperAdmin/EditIdentityRole/5
        public async Task<IActionResult> EditIdentityRole(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.AspNetRoles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // POST: SuperAdmin/EditIdentityRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditIdentityRole(string id, [Bind("Id,Name")] AspNetRole role)
        {
            if (id != role.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRole = await _context.AspNetRoles.FindAsync(id);
                    if (existingRole != null)
                    {
                        existingRole.Name = role.Name;
                        existingRole.NormalizedName = role.Name?.ToUpper();
                        existingRole.ConcurrencyStamp = Guid.NewGuid().ToString(); // Update concurrency stamp

                        _context.Update(existingRole);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AspNetRoleExists(role.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ListRoles)); // Redirect to the list of roles
            }
            return View(role);
        }

        private bool AspNetRoleExists(string id)
        {
            return _context.AspNetRoles.Any(e => e.Id == id);
        }

        // POST: SuperAdmin/DeleteIdentityRole/5
        [HttpPost, ActionName("DeleteIdentityRole")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteIdentityRoleConfirmed(string id)
        {
            var role = await _context.AspNetRoles.FindAsync(id);
            if (role != null)
            {
                _context.AspNetRoles.Remove(role);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ListRoles)); // Redirect to the list of roles
        }
    }
}