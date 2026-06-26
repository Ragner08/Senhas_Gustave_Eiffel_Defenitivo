using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Senhas_Gustave_Eiffel.Data;
using Senhas_Gustave_Eiffel.Models;
using System.Diagnostics;

namespace Senhas_Gustave_Eiffel.Controllers
{
    // Controlador inicial da aplicação.
    // Mantém a navegação simples e encaminha o utilizador para o calendário principal.
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

        // Redireciona o utilizador para a área do calendário.
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Calendar");
        }

        // Mostra a página de política de privacidade.
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        // Mostra a página de erro da aplicação.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
