using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CVSante.Models;
using CVSante.ViewModels;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Common;

namespace CVSante.Controllers
{
    public class ProfilCitoyenController : Controller
    {
        private readonly CvsanteContext _context;

        public ProfilCitoyenController(CvsanteContext context)
        {
            _context = context;
        }

        // GET: ProfilCitoyen
        public async Task<IActionResult> Index()
        {
            var profilCitoyen = await _context.UserCitoyens.Select(
                userInfos => new User
                {
                    userInfo = _context.UserInfos.FirstOrDefault(u => u.FkUserId == userInfos.UserId),
                    adresse = _context.UserAdresses.FirstOrDefault(a => a.FkUserId == userInfos.UserId),
                    allergies = _context.UserAllergies.Where(al => al.FkUserId == userInfos.UserId).ToList(),
                    antecedents = _context.UserAntecedents.Where(an => an.FkUserId == userInfos.UserId).ToList(),
                    medications = _context.UserMedications.Where(m => m.FkUserId == userInfos.UserId).ToList(),
                    handicaps = _context.UserHandicaps.Where(h => h.FkUserId == userInfos.UserId).ToList()
                }).ToListAsync();

            return View(profilCitoyen);
        }

        // GET: ProfilCitoyen/Create
        public IActionResult Create()
        {
            ViewData["FkIdentityUser"] = new SelectList(_context.AspNetUsers, "Id", "Id");
            return View();
        }

        // POST: ProfilCitoyen/Create
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

        // GET: ProfilCitoyen/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userCitoyen = await _context.UserInfos
                .Where(u => u.FkUserId == id)
                .Select(userInfo => new
                {
                    UserInfo = userInfo,
                    UserAdresse = _context.UserAdresses.FirstOrDefault(a => a.FkUserId == userInfo.FkUserId),
                    UserAllergies = _context.UserAllergies.Where(a => a.FkUserId == userInfo.FkUserId).ToList(),
                    UserAntecedents = _context.UserAntecedents.Where(a => a.FkUserId == userInfo.FkUserId).ToList(),
                    UserMedications = _context.UserMedications.Where(m => m.FkUserId == userInfo.FkUserId).ToList(),
                    UserHandicaps = _context.UserHandicaps.Where(h => h.FkUserId == userInfo.FkUserId).ToList()
                })
                .FirstOrDefaultAsync();

            if (userCitoyen == null)
            {
                return NotFound();
            }

            ViewData["FkIdentityUser"] = new SelectList(_context.AspNetUsers, "Id", "Id", userCitoyen.UserInfo.FkUserId);
            return View(userCitoyen);
        }

        // POST: ProfilCitoyen/Edit/5
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

        // GET: ProfilCitoyen/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userCitoyen = await _context.UserInfos
                .Where(u => u.FkUserId == id)
                .Select(userInfo => new
                {
                    UserInfo = userInfo,
                    UserAdresse = _context.UserAdresses.FirstOrDefault(a => a.FkUserId == userInfo.FkUserId),
                    UserAllergies = _context.UserAllergies.Where(a => a.FkUserId == userInfo.FkUserId).ToList(),
                    UserAntecedents = _context.UserAntecedents.Where(a => a.FkUserId == userInfo.FkUserId).ToList(),
                    UserMedications = _context.UserMedications.Where(m => m.FkUserId == userInfo.FkUserId).ToList(),
                    UserHandicaps = _context.UserHandicaps.Where(h => h.FkUserId == userInfo.FkUserId).ToList()
                })
                .FirstOrDefaultAsync();

            if (userCitoyen == null)
            {
                return NotFound();
            }

            return View(userCitoyen);
        }

        // POST: ProfilCitoyen/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userCitoyen = await _context.UserInfos.FindAsync(id);
            if (userCitoyen != null)
            {
                _context.UserInfos.Remove(userCitoyen);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserCitoyenExists(int id)
        {
            return _context.UserInfos.Any(e => e.FkUserId == id);
        }
    }
}
