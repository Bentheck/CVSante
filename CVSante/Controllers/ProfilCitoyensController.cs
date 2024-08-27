using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CVSante.Models;
using CVSante.ViewModels;
using CVSante.Services;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.AspNet.SignalR;
using System.Net.NetworkInformation;
using FileSignatures;
using FileSignatures.Formats;

namespace CVSante.Controllers
{
    public class ProfilCitoyenController : Controller
    {
        private readonly CvsanteContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserValidation _UserValidation;

        public ProfilCitoyenController(CvsanteContext context, UserManager<IdentityUser> userManager, UserValidation userValidation)
        {
            _context = context;
            _userManager = userManager;
            _UserValidation = userValidation;
        }

        // GET: ProfilCitoyen
        public async Task<IActionResult> Bienvenue()
        {
            TempData["UserCheck"] = null;

            var currentUserId = _userManager.GetUserId(User);
            var userCitoyen = await _context.UserCitoyens
                .FirstOrDefaultAsync(uc => uc.FkIdentityUser == currentUserId);

            if (userCitoyen != null)
            {
                TempData["UserID"] = userCitoyen.UserId;
                TempData["UserCheck"] = userCitoyen.UserId;
                
                var userInfo = await _context.UserInfos.FirstOrDefaultAsync(u => u.FkUserId == userCitoyen.UserId);

                if (userInfo != null)
                {
                    TempData["Profil"] = 1;
                    TempData["ProfilCheck"] = 1;
                    TempData["ImageProfil"] = userInfo.ImageProfil ?? "photo.png";
                    ViewData["Nom"] = userInfo.Nom;
                    ViewData["Prenom"] = userInfo.Prenom;    
                }
                else
                {
                    TempData["Profil"] = null;
                    TempData["ProfilCheck"] = null;
                    ViewBag.ImageProfil = "photo.png";
                }
            }
            else
            {
                TempData["UserID"] = null;
                TempData["Profil"] = null;
                TempData["ProfilCheck"] = null;
                ViewBag.ImageProfil = "photo.png";
            }

            var profilCitoyen = _context.UserCitoyens.Select(
                userInfos => new
                {
                    UserInfo = _context.UserInfos.First(u => u.FkUserId == userInfos.UserId),
                    Addresses = _context.UserAdresses.Where(a => a.FkUserId == userInfos.UserId),
                    Allergies = _context.UserAllergies.Where(al => al.FkUserId == userInfos.UserId),
                    Antecedent = _context.UserAntecedents.First(an => an.FkUserId == userInfos.UserId),
                    Medications = _context.UserMedications.Where(m => m.FkUserId == userInfos.UserId),
                    Handicaps = _context.UserHandicaps.Where(h => h.FkUserId == userInfos.UserId)
                });


            return View();
        }

        public async Task<IActionResult> CreateId()
        {
            if (TempData["UserCheck"] != null)
            {
                if (TempData["ProfilCheck"] == null)
                {
                    return RedirectToAction("create", new { id = TempData["UserCheck"] });
                }
                else
                {
                    return RedirectToAction("Edit", new { id = TempData["UserCheck"] });
                }
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
                Allergies = new List<UserAllergy>(),
                Antecedent = new UserAntecedent { FkUserId = id },
                Medications = new List<UserMedication>(),
                Handicaps = new List<UserHandicap>(),
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


            var validationResult = _UserValidation.ValidateUserProfile(userViewModel);

            if (!validationResult.IsValid)
            {
                // Add errors to ModelState
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError("", error);
                }

                return View(userViewModel);
            }

            // Check if the model state is valid
            if (ModelState.IsValid)
            {
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
            // If we got this far, something failed, redisplay the form
            return View(userViewModel);
        }



        // GET: ProfilCitoyen/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            var profileId = await _context.UserCitoyens
                .FirstOrDefaultAsync(uc => uc.FkIdentityUser == currentUserId);
            

            if (id != profileId.UserId)
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

            var validationResult = _UserValidation.ValidateUserProfile(userViewModel);

            if (!validationResult.IsValid)
            {
                // Add errors to ModelState
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError("", error);
                }

                return View(userViewModel);
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
                    if (id == null)
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

            var currentUserId = _userManager.GetUserId(User);
            var profileId = await _context.UserCitoyens
                .FirstOrDefaultAsync(uc => uc.FkIdentityUser == currentUserId);


            if (id != profileId.UserId)
            {
                return NotFound();
            }

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

            return View();
        }

        // POST: ProfilCitoyen/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            // Remove related entities except UserCitoyen
            _context.UserAdresses.RemoveRange(_context.UserAdresses.Where(a => a.FkUserId == id));
            _context.UserAllergies.RemoveRange(_context.UserAllergies.Where(a => a.FkUserId == id));
            _context.UserAntecedents.RemoveRange(_context.UserAntecedents.Where(a => a.FkUserId == id));
            _context.UserMedications.RemoveRange(_context.UserMedications.Where(m => m.FkUserId == id));
            _context.UserHandicaps.RemoveRange(_context.UserHandicaps.Where(h => h.FkUserId == id));
            _context.UserInfos.RemoveRange(_context.UserInfos.Where(u => u.FkUserId == id));

            // Save changes
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Bienvenue));
        }




        // GET: Pictures/UploadImage
        [Authorize]
        public IActionResult Image(int? id)
        {
            return View();
        }


        //POST: Pictures/UploadImage
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(IFormFile imageFile, int? id)
        {
            if (_context.UserInfos.First(u => u.FkUserId == id) == null)
            {
                TempData["ErrorMessage"] = "Veuillez créer un profil avant de télécharger une image de profil.";
                return RedirectToAction("Bienvenue");
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var inspector = new FileFormatInspector();
                var format = inspector.DetermineFileFormat(imageFile.OpenReadStream());

                if (format is Png || format is Jpeg)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                    string uploadsFolder = Path.Combine("wwwroot", "assets", "photos");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    var userInfo = await _context.UserInfos.FirstOrDefaultAsync(u => u.FkUserId == id);
                    if (userInfo != null)
                    {
                        userInfo.ImageProfil = uniqueFileName;
                        _context.Update(userInfo);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "L'image de profil a été téléchargée avec succès.";
                    }

                    // Mettre à jour TempData pour s'assurer que la nouvelle image est affichée immédiatement
                    TempData["ImageProfil"] = uniqueFileName;
                    return RedirectToAction("Bienvenue");
                }
                else
                {
                    TempData["TypeError"] = "Erreur de type de fichier; Veuillez utiliser JPG ou PNG.";
                    return RedirectToAction("Bienvenue");
                }
            }

            TempData["ErrorMessage"] = "Veuillez sélectionner une image à télécharger.";
            return RedirectToAction("Bienvenue");
        }




    }

}

