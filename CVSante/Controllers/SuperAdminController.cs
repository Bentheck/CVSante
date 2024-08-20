using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CVSante.Models;
using Microsoft.AspNetCore.Identity;
using CVSante.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace CVSante.Controllers
{
    public class SuperAdminController : Controller
    {
        private readonly CvsanteContext _context;

        public SuperAdminController(CvsanteContext context)
        {
            _context = context;
        }


        [Authorize(Roles = "SuperAdmin")]
        // GET: SuperAdmin
        public async Task<IActionResult> Index()
        {
            var cvsanteContext = _context.UserParamedics
                .Include(u => u.FkCompanyNavigation)
                .Include(u => u.FkIdentityUserNavigation)
                .Include(u => u.FkRoleNavigation);
            return View(await cvsanteContext.ToListAsync());
        }

        [Authorize(Roles = "SuperAdmin")]
        // GET: SuperAdmin/CreateCompany
        public IActionResult CreateCompany()
        {
            return View();
        }

        [Authorize(Roles = "SuperAdmin")]
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

        [Authorize(Roles = "SuperAdmin")]
        // GET: SuperAdmin/CreateRole
        public IActionResult CreateRole()
        {
            // Prepare ViewBag or ViewData if you need to populate dropdowns with company options
            ViewBag.FkCompany = new SelectList(_context.Companies.ToList(), "IdComp", "CompName");
            return View();
        }

        [Authorize(Roles = "SuperAdmin")]
        // POST: SuperAdmin/CreateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole([Bind("IdRole,CreateParamedic,EditParamedic,GetHistorique,GetCitoyen,EditRole,EditCompany,FkCompany")] CompanyRole role)
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

        [Authorize(Roles = "SuperAdmin")]
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

        [Authorize(Roles = "SuperAdmin")]
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

        [Authorize(Roles = "SuperAdmin")]
        // GET: SuperAdmin/CreateIdentityRole
        public IActionResult CreateIdentityRole()
        {
            return View();
        }

        [Authorize(Roles = "SuperAdmin")]
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

        [Authorize(Roles = "SuperAdmin")]
        // GET: SuperAdmin/ListRoles
        public async Task<IActionResult> ListRoles()
        {
            var roles = await _context.AspNetRoles.ToListAsync();
            return View(roles);
        }

        [Authorize(Roles = "SuperAdmin")]
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

        [Authorize(Roles = "SuperAdmin")]
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

        [Authorize(Roles = "SuperAdmin")]
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


        [Authorize(Roles = "SuperAdmin")]
        // GET: SuperAdmin/ASPUserRolesAndEdit
        public async Task<IActionResult> ASPUserRolesAndEdit(string userId)
        {
            var model = new ASPUserRolesAndEdit();

            if (string.IsNullOrEmpty(userId))
            {
                // Fetch all users and roles
                var users = await _context.AspNetUsers.ToListAsync();
                var roles = await _context.AspNetRoles.ToListAsync();

                // Fetch user roles for each user without overlapping DbContext usage
                var userRoles = new List<ASPUserRoles>();
                foreach (var user in users)
                {
                    var roleIds = await _context.AspNetUserRoles
                        .Where(ur => ur.UserId == user.Id)
                        .Select(ur => ur.RoleId)
                        .ToListAsync();

                    var roleNames = await _context.AspNetRoles
                        .Where(r => roleIds.Contains(r.Id))
                        .Select(r => r.Name)
                        .ToListAsync();

                    userRoles.Add(new ASPUserRoles
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        Roles = roleNames
                    });
                }

                model.Users = userRoles;
                model.AllRoles = roles;
            }
            else
            {
                // Fetch specific user's roles for editing
                var user = await _context.AspNetUsers.FindAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }

                var userRoles = await _context.AspNetUserRoles
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                var allRoles = await _context.AspNetRoles.ToListAsync();
                model.EditUserRoles = new EditASPUserRoles
                {
                    UserId = userId,
                    UserName = user.UserName,
                    AllRoles = allRoles,
                    SelectedRoles = userRoles
                };
            }

            return View(model);
        }


        [Authorize(Roles = "SuperAdmin")]
        // POST: SuperAdmin/EditUserRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserRoles(EditASPUserRoles model)
        {
            ModelState.Remove("UserName");
            ModelState.Remove("AllRoles");

            if (!ModelState.IsValid)
            {
                var allRoles = await _context.AspNetRoles.ToListAsync();
                model.AllRoles = allRoles;
                return View("ASPUserRolesAndEdit", new ASPUserRolesAndEdit { EditUserRoles = model, AllRoles = allRoles });
            }

            var existingUserRoles = await _context.AspNetUserRoles
                .Where(ur => ur.UserId == model.UserId)
                .ToListAsync();

            // Remove existing roles
            _context.AspNetUserRoles.RemoveRange(existingUserRoles);

            // Add selected roles
            if (model.SelectedRoles != null)
            {
                var newRoles = model.SelectedRoles
                    .Select(roleId => new AspNetUserRole { UserId = model.UserId, RoleId = roleId });

                await _context.AspNetUserRoles.AddRangeAsync(newRoles);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ASPUserRolesAndEdit)); // Redirect to refresh the view
        }


        [Authorize(Roles = "SuperAdmin")]
        // GET: SuperAdmin/FAQContact
        public async Task<IActionResult> FAQContact()
        {
            var faqContact = await _context.FAQ.ToListAsync();
            return View(faqContact);
        }


    }
}