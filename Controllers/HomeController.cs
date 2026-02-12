using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Senhas_Gustave_Eiffel.Data;
using Senhas_Gustave_Eiffel.Models;
using System.Diagnostics;

namespace Senhas_Gustave_Eiffel.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserName = user.Nome;
            ViewBag.UserRole = roles.FirstOrDefault() ?? "Sem Role";
            ViewBag.WalletBalance = user.WalletBalance;

            // Get upcoming bookings
            var upcomingBookings = await _context.Bookings
                .Where(b => b.UserId == user.Id && b.DataMarcacao >= DateTime.Today)
                .OrderBy(b => b.DataMarcacao)
                .Take(5)
                .ToListAsync();

            ViewBag.UpcomingBookings = upcomingBookings;

            // Get today's meal
            var todayMeal = await _context.Meals
                .FirstOrDefaultAsync(m => m.Data.Date == DateTime.Today);

            ViewBag.TodayMeal = todayMeal;

            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
