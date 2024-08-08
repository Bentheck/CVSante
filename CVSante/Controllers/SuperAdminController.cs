using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CVSante.Models;
using CVSante.ViewModels;

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
            var cvsanteContext = _context.UserParamedics.Include(u => u.FkCompanyNavigation).Include(u => u.FkIdentityUserNavigation).Include(u => u.FkRoleNavigation);
            return View(await cvsanteContext.ToListAsync());
        }


        // GET: SuperAdmin/Create
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
                return RedirectToAction(nameof(CreateRole)); // Redirect to the list of companies or another relevant page
            }

            // If there is an error, return the same view with the current model
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
                return RedirectToAction(nameof(CreateParamedic)); // Redirect to the list of roles or another relevant page
            }

            // Populate ViewBag again in case of error to repopulate dropdowns
            ViewData["FkCompany"] = new SelectList(_context.Companies, "IdComp", "CompName", role.FkCompany);
            return View(role);
        }

        // GET: SuperAdmin/CreateParamedic
        public IActionResult CreateParamedic()
        {
            // Retrieve data for dropdowns
            var companies = _context.Companies.ToList();
            var users = _context.AspNetUsers.ToList();
            var roles = _context.CompanyRoles.ToList();

            // Ensure the lists are not empty
            if (companies == null || users == null || roles == null)
            {
                // Handle error (e.g., log the error, redirect, etc.)
                throw new InvalidOperationException("Data not found for dropdowns.");
            }

            // Populate dropdown lists
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

            // Repopulate dropdowns in case of validation errors
            var companies = _context.Companies.ToList();
            var users = _context.AspNetUsers.ToList();
            var roles = _context.CompanyRoles.ToList();

            // Ensure the lists are not empty
            if (companies == null || users == null || roles == null)
            {
                // Handle error (e.g., log the error, redirect, etc.)
                throw new InvalidOperationException("Data not found for dropdowns.");
            }

            ViewBag.FkCompany = new SelectList(companies, "IdComp", "CompName", userParamedic.FkCompany);
            ViewBag.FkIdentityUser = new SelectList(users, "Id", "UserName", userParamedic.FkIdentityUser);
            ViewBag.FkRole = new SelectList(roles, "IdRole", "IdRole", userParamedic.FkRole);

            return View(userParamedic);
        }

    }
}
