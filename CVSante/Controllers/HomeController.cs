using CVSante.Models;
using CVSante.Services;
using Google.Api;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CVSante.Controllers
{
    public class HomeController : Controller
    {
        private readonly CvsanteContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public HomeController(ILogger<HomeController> logger, CvsanteContext context, UserManager<IdentityUser> userManager, IHubContext<NotificationHub> hubContext)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var newFAQCount = await _context.FAQ.CountAsync(f => f.IsNew);
            ViewBag.NewFAQCount = newFAQCount;
            return View();
        }

        public async Task<IActionResult> Presentation()
        {
            var newFAQCount = await _context.FAQ.CountAsync(f => f.IsNew);
            ViewBag.NewFAQCount = newFAQCount;
            return View();
        }
        public async Task<IActionResult> MembreEquipe()
        {
            var newFAQCount = await _context.FAQ.CountAsync(f => f.IsNew);
            ViewBag.NewFAQCount = newFAQCount;
            return View();
        }

        // GET Home/FAQ
        public async Task<IActionResult> FAQ()
        {
            var newFAQCount = await _context.FAQ.CountAsync(f => f.IsNew);
            ViewBag.NewFAQCount = newFAQCount;
            return View();
        }


        // POST Home/FAQ
        [HttpPost]
        public async Task<IActionResult> FAQ(FAQ faq)
        {

            faq.IsNew = true;
           
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
