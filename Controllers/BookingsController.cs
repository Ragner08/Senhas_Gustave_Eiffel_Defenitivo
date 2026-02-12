using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Senhas_Gustave_Eiffel.Data;
using Senhas_Gustave_Eiffel.Models;

namespace Senhas_Gustave_Eiffel.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public BookingsController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
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
            var isAdmin = roles.Contains("Admin");
            var isFuncionario = roles.Contains("Funcionário");

            List<Booking> bookings;

            if (isAdmin || isFuncionario)
            {
                // Admin and Funcionario can see all bookings
                bookings = await _context.Bookings
                    .Include(b => b.User)
                    .OrderByDescending(b => b.DataMarcacao)
                    .ToListAsync();
            }
            else
            {
                // Regular users only see their own bookings
                bookings = await _context.Bookings
                    .Where(b => b.UserId == user.Id)
                    .Include(b => b.User)
                    .OrderByDescending(b => b.DataMarcacao)
                    .ToListAsync();
            }

            ViewBag.IsAdmin = isAdmin;
            ViewBag.IsFuncionario = isFuncionario;

            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var isAdmin = roles.Contains("Admin");
            var isFuncionario = roles.Contains("Funcionário");

            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Only allow viewing own bookings unless admin or funcionario
            if (!isAdmin && !isFuncionario && booking.UserId != user.Id)
            {
                return Forbid();
            }

            // Get meal details for the booking date
            var meal = await _context.Meals
                .FirstOrDefaultAsync(m => m.Data.Date == booking.DataMarcacao.Date);

            ViewBag.Meal = meal;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.IsFuncionario = isFuncionario;

            return View(booking);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Funcionário")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.Confirmado = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Marcação confirmada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Funcionário")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBookingAdmin(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Refund the user
            var user = booking.User;
            if (user != null)
            {
                user.WalletBalance += booking.ValorPago;

                // Create wallet transaction for refund
                var transaction = new WalletTransaction
                {
                    UserId = user.Id,
                    Valor = booking.ValorPago,
                    Tipo = "Carregamento",
                    Descricao = $"Reembolso administrativo de cancelamento para {booking.DataMarcacao:dd/MM/yyyy}"
                };

                _context.WalletTransactions.Add(transaction);
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Marcação cancelada com sucesso! O valor foi reembolsado ao utilizador.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Funcionário")]
        public async Task<IActionResult> DailyReport(DateTime? date)
        {
            var reportDate = date ?? DateTime.Today;

            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Where(b => b.DataMarcacao.Date == reportDate.Date)
                .ToListAsync();

            var meal = await _context.Meals
                .FirstOrDefaultAsync(m => m.Data.Date == reportDate.Date);

            var viewModel = new DailyReportViewModel
            {
                Data = reportDate,
                Meal = meal,
                Bookings = bookings,
                TotalEscalaoA = bookings.Count(b => b.EscalaoUtilizado == "Escalão A"),
                TotalEscalaoB = bookings.Count(b => b.EscalaoUtilizado == "Escalão B"),
                TotalSemEscalao = bookings.Count(b => b.EscalaoUtilizado == "Sem escalão"),
                TotalValor = bookings.Sum(b => b.ValorPago)
            };

            return View(viewModel);
        }
    }
}
