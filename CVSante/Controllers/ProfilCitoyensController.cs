using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CVSante.Models;
using CVSante.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace CVSante.Controllers
{
    public class ProfilCitoyenController : Controller
    {
        private readonly CvsanteContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfilCitoyenController(CvsanteContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ProfilCitoyen
        public async Task<IActionResult> Bienvenue()
        {
            var currentUserId = _userManager.GetUserId(User);
            var userCitoyen = await _context.UserCitoyens
                .FirstOrDefaultAsync(uc => uc.FkIdentityUser == currentUserId);

            if (userCitoyen != null)
            {
                TempData["UserID"] = userCitoyen.UserId;
            }
            else
            {
                TempData["UserID"] = null;
            }

            var profilCitoyen = await _context.UserCitoyens.Select(
                userInfos => new User
                {
                    UserInfo = _context.UserInfos.First(u => u.FkUserId == userInfos.UserId),
                    Addresses = _context.UserAdresses.Where(a => a.FkUserId == userInfos.UserId).ToList(),
                    Allergies = _context.UserAllergies.Where(al => al.FkUserId == userInfos.UserId).ToList(),
                    Antecedent = _context.UserAntecedents.First(an => an.FkUserId == userInfos.UserId),
                    Medications = _context.UserMedications.Where(m => m.FkUserId == userInfos.UserId).ToList(),
                    Handicaps = _context.UserHandicaps.Where(h => h.FkUserId == userInfos.UserId).ToList()
                }).ToListAsync();

            return View(profilCitoyen);
        }

        public async Task<IActionResult> CreateId()
        {
            if (TempData["UserID"] != null)
            {
                return RedirectToAction("Edit", new { id = TempData["UserID"] });
            }
            else
            {
                var cvsanteContext = _context.UserInfos.Include(u => u.FkUser);
                UserCitoyen userCitoyen = new UserCitoyen();
                userCitoyen.FkIdentityUser = _userManager.GetUserId(User);
                _context.Add(userCitoyen);
                await _context.SaveChangesAsync();
                var userId = userCitoyen.UserId;
                TempData["UserID"] = userId;
                return RedirectToAction("Create", new { id = userId });
            }
        }


            // GET: ProfilCitoyen/Create
            public async Task<IActionResult> Create(int id)
            {

            var user = new User
            {
                UserInfo = new UserInfo { FkUserId = id },
                Addresses = new List<UserAdresse> { new UserAdresse { FkUserId = id } },
                Allergies = new List<UserAllergy> { new UserAllergy { FkUserId = id } },
                Antecedent = new UserAntecedent { FkUserId = id },
                Medications = new List<UserMedication> { new UserMedication { FkUserId = id } },
                Handicaps = new List<UserHandicap> { new UserHandicap { FkUserId = id } }
            };

            return View(user);
            
        }

        // POST: ProfilCitoyen/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserInfo,Addresses,Allergies,Antecedent,Medications,Handicaps")] User userViewModel)
        {

            // Remove `FkUser` from ModelState if it exists to avoid validation errors
            ModelState.Remove("UserInfo.FkUser");

            if (userViewModel.Addresses != null)
            {
                for (int i = 0; i < userViewModel.Addresses.Count; i++)
                {
                    ModelState.Remove($"Addresses[{i}].FkUser");
                }
            }

            if (userViewModel.Allergies != null)
            {
                for (int i = 0; i < userViewModel.Allergies.Count; i++)
                {
                    ModelState.Remove($"Allergies[{i}].FkUser");
                }
            }

            if (userViewModel.Antecedent != null)
            {
                ModelState.Remove("Antecedent.FkUser");
            }

            if (userViewModel.Medications != null)
            {
                for (int i = 0; i < userViewModel.Medications.Count; i++)
                {
                    ModelState.Remove($"Medications[{i}].FkUser");
                }
            }

            if (userViewModel.Handicaps != null)
            {
                for (int i = 0; i < userViewModel.Handicaps.Count; i++)
                {
                    ModelState.Remove($"Handicaps[{i}].FkUser");
                }
            }





            // Check if the model state is valid
            if (!ModelState.IsValid)
            {
                return View(userViewModel);
            }

            try
            {
                // Fetch the UserId from TempData
                if (TempData["UserID"] == null)
                {
                    ModelState.AddModelError("", "User ID is missing. Unable to save the data.");
                    return View(userViewModel);
                }

                var userId = (int)TempData["UserID"];

                // Ensure UserInfo has the correct FkUserId and add to context
                if (userViewModel.UserInfo != null)
                {
                    userViewModel.UserInfo.FkUserId = userId;
                    _context.Add(userViewModel.UserInfo);
                }

                // Add Addresses
                foreach (var adresse in userViewModel.Addresses ?? Enumerable.Empty<UserAdresse>())
                {
                    adresse.FkUserId = userId;
                    _context.Add(adresse);
                }

                // Add Allergies
                foreach (var allergy in userViewModel.Allergies ?? Enumerable.Empty<UserAllergy>())
                {
                    allergy.FkUserId = userId;
                    _context.Add(allergy);
                }

                // Add Antecedent if it exists
                if (userViewModel.Antecedent != null)
                {
                    userViewModel.Antecedent.FkUserId = userId;
                    _context.Add(userViewModel.Antecedent);
                }

                // Add Medications
                foreach (var medication in userViewModel.Medications ?? Enumerable.Empty<UserMedication>())
                {
                    medication.FkUserId = userId;
                    _context.Add(medication);
                }

                // Add Handicaps
                foreach (var handicap in userViewModel.Handicaps ?? Enumerable.Empty<UserHandicap>())
                {
                    handicap.FkUserId = userId;
                    _context.Add(handicap);
                }

                // Save all changes
                await _context.SaveChangesAsync();

                // Redirect to the Edit page
                return RedirectToAction("Edit", new { id = userId });
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                ModelState.AddModelError("", "An error occurred while creating the profile. Please try again.");
                return View(userViewModel);
            }
        }



        // GET: ProfilCitoyen/Edit/5
        // GET: ProfilCitoyen/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Fetch the user profile and associated data
            var user = new User
            {
                UserInfo = await _context.UserInfos.FirstAsync(u => u.FkUserId == id),
                Addresses = await _context.UserAdresses.Where(a => a.FkUserId == id).ToListAsync(),
                Allergies = await _context.UserAllergies.Where(a => a.FkUserId == id).ToListAsync(),
                Antecedent = await _context.UserAntecedents.FirstAsync(a => a.FkUserId == id),
                Medications = await _context.UserMedications.Where(m => m.FkUserId == id).ToListAsync(),
                Handicaps = await _context.UserHandicaps.Where(h => h.FkUserId == id).ToListAsync()
            };

            if (user.UserInfo == null)
            {
                return NotFound();
            }

            // Pass the user object to the view
            return View(user);
        }

        // POST: ProfilCitoyen/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User userViewModel, int id)
        {
            if (id != userViewModel.UserInfo.FkUserId)
            {
                return NotFound();  
            }

            // Remove `FkUser` from ModelState if it exists to avoid validation errors
            ModelState.Remove("UserInfo.FkUser");

            if (userViewModel.Addresses != null)
            {
                for (int i = 0; i < userViewModel.Addresses.Count; i++)
                {
                    ModelState.Remove($"Addresses[{i}].FkUser");
                }
            }

            if (userViewModel.Allergies != null)
            {
                for (int i = 0; i < userViewModel.Allergies.Count; i++)
                {
                    ModelState.Remove($"Allergies[{i}].FkUser");
                }
            }

            if (userViewModel.Antecedent != null)
            {
                ModelState.Remove("Antecedent.FkUser");
            }

            if (userViewModel.Medications != null)
            {
                for (int i = 0; i < userViewModel.Medications.Count; i++)
                {
                    ModelState.Remove($"Medications[{i}].FkUser");
                }
            }

            if (userViewModel.Handicaps != null)
            {
                for (int i = 0; i < userViewModel.Handicaps.Count; i++)
                {
                    ModelState.Remove($"Handicaps[{i}].FkUser");
                }
            }


            if (ModelState.IsValid)
            {
                try
                {
                    // Update UserInfo
                    _context.Update(userViewModel.UserInfo);

                    // Remove existing records
                    _context.UserAdresses.RemoveRange(_context.UserAdresses.Where(a => a.FkUserId == id));
                    _context.UserAllergies.RemoveRange(_context.UserAllergies.Where(a => a.FkUserId == id));
                    _context.UserAntecedents.RemoveRange(_context.UserAntecedents.Where(a => a.FkUserId == id));
                    _context.UserMedications.RemoveRange(_context.UserMedications.Where(m => m.FkUserId == id));
                    _context.UserHandicaps.RemoveRange(_context.UserHandicaps.Where(h => h.FkUserId == id));

                    // Add updated records
                    if (userViewModel.Addresses != null)
                    {
                        foreach (var address in userViewModel.Addresses)
                        {
                            address.FkUserId = id;
                            _context.Add(address);
                        }
                    }

                    if (userViewModel.Allergies != null)
                    {
                        foreach (var allergy in userViewModel.Allergies)
                        {
                            allergy.FkUserId = id;
                            _context.Add(allergy);
                        }
                    }

                    if (userViewModel.Antecedent != null)
                    {
                        userViewModel.Antecedent.FkUserId = id;
                        _context.Add(userViewModel.Antecedent);
                    }

                    if (userViewModel.Medications != null)
                    {
                        foreach (var medication in userViewModel.Medications)
                        {
                            medication.FkUserId = id;
                            _context.Add(medication);
                        }
                    }

                    if (userViewModel.Handicaps != null)
                    {
                        foreach (var handicap in userViewModel.Handicaps)
                        {
                            handicap.FkUserId = id;
                            _context.Add(handicap);
                        }
                    }

                    // Save all changes
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserCitoyenExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Bienvenue));
            }

            // If we got this far, something failed, redisplay the form
            return View(userViewModel);
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
                    .Select(userInfo => new User
                    {
                        UserInfo = userInfo,
                        Addresses = _context.UserAdresses.Where(a => a.FkUserId == userInfo.FkUserId).ToList(),
                        Allergies = _context.UserAllergies.Where(a => a.FkUserId == userInfo.FkUserId).ToList(),
                        Antecedent = _context.UserAntecedents.First(a => a.FkUserId == userInfo.FkUserId),
                        Medications = _context.UserMedications.Where(m => m.FkUserId == userInfo.FkUserId).ToList(),
                        Handicaps = _context.UserHandicaps.Where(h => h.FkUserId == userInfo.FkUserId).ToList()
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

                return RedirectToAction(nameof(Bienvenue));
            }

            private bool UserCitoyenExists(int id)
            {
                return _context.UserInfos.Any(e => e.FkUserId == id);
            }
        }
    }
