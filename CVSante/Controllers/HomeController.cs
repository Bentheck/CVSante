using CVSante.Models;
using CVSante.Services;
using Google.Api;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CVSante.Controllers
{
    public class HomeController : Controller
    {
        private readonly CvsanteContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, CvsanteContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Presentation()
        {
            return View();
        }
        public IActionResult MembreEquipe()
        {
            return View();
        }

        // GET Home/FAQ
        public async Task<IActionResult> FAQ()
        {
            return View();
        }


        // POST Home/FAQ
        [HttpPost]
        public async Task<IActionResult> FAQ(FAQ faq)
        {
           
           if (ModelState.IsValid)
            {
                _context.Add(faq);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("FAQ");
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
