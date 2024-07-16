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
    public class UserCitoyensController : Controller
    {
        private readonly CvsanteContext _context;

        public UserCitoyensController(CvsanteContext context)
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

        // GET: UserCitoyens/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userCitoyen = await _context.UserCitoyens.FindAsync(id);
            if (userCitoyen == null)
            {
                return NotFound();
            }
            ViewData["FkIdentityUser"] = new SelectList(_context.AspNetUsers, "Id", "Id", userCitoyen.FkIdentityUser);
            return View(userCitoyen);
        }

        // POST: UserCitoyens/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,FkIdentityUser")] UserCitoyen userCitoyen)
        {
            if (id != userCitoyen.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userCitoyen);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserCitoyenExists(userCitoyen.UserId))
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
            ViewData["FkIdentityUser"] = new SelectList(_context.AspNetUsers, "Id", "Id", userCitoyen.FkIdentityUser);
            return View(userCitoyen);
        }

        // GET: UserCitoyens/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: UserCitoyens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userCitoyen = await _context.UserCitoyens.FindAsync(id);
            if (userCitoyen != null)
            {
                _context.UserCitoyens.Remove(userCitoyen);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserCitoyenExists(int id)
        {
            return _context.UserCitoyens.Any(e => e.UserId == id);
        }
    }
}
