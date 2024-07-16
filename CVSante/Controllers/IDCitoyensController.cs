using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CVSante.Models;

namespace CVSante.Controllers
{
    public class IDCitoyensController : Controller
    {
        private readonly CvsanteContext _context;

        public IDCitoyensController(CvsanteContext context)
        {
            _context = context;
        }

        // GET: UserCitoyens
        public async Task<IActionResult> Index()
        {
            var cvsanteContext = _context.UserCitoyens.Include(u => u.FkIdentityUserNavigation);
            return View(await cvsanteContext.ToListAsync());
        }

        // GET: UserCitoyens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userCitoyen = await _context.UserCitoyens
                .Include(u => u.FkIdentityUserNavigation)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (userCitoyen == null)
            {
                return NotFound();
            }

            return View(userCitoyen);
        }

        // GET: UserCitoyens/Create
        public IActionResult Create()
        {
            ViewData["FkIdentityUser"] = new SelectList(_context.AspNetUsers, "Id", "Id");
            return View();
        }

        // POST: UserCitoyens/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,FkIdentityUser")] UserCitoyen userCitoyen)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userCitoyen);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FkIdentityUser"] = new SelectList(_context.AspNetUsers, "Id", "Id", userCitoyen.FkIdentityUser);
            return View(userCitoyen);
        }

    }
}
