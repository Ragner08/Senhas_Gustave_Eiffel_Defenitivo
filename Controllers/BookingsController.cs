using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Senhas_Gustave_Eiffel.Data;
using Senhas_Gustave_Eiffel.Models;

namespace Senhas_Gustave_Eiffel.Controllers
{
    // Controlador responsável por gerir as marcações (senhas):
    // - Listagem de marcações (para o utilizador e para admin/funcionário)
    // - Visualização detalhada de uma marcação
    // - Confirmação e cancelamento de marcações (admin/funcionário)
    // - Relatórios diários e operações administrativas sobre refeições
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

        // Mostra a lista de marcações.
        // - Funcionários vêem todas as marcações.
        // - Administradores e utilizadores normais vêem apenas as suas próprias marcações.
        public async Task<IActionResult> Index(DateTime? date)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var isAdmin = roles.Contains("Admin");
            var isFuncionario = roles.Contains("Funcionário");

            var query = _context.Bookings.AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(b => b.UserId == user.Id)
                    .Include(b => b.User);
            }
            else
            {
                query = query.Include(b => b.User);
            }

            if (date.HasValue)
            {
                query = query.Where(b => b.DataMarcacao.Date == date.Value.Date);
            }

            var bookings = await query
                .OrderByDescending(b => b.DataMarcacao)
                .ToListAsync();

            ViewBag.IsAdmin = isAdmin;
            ViewBag.IsFuncionario = isFuncionario;
            ViewBag.FilterDate = date;

            return View(bookings);
        }

        [HttpGet]
        // Mostra os detalhes de uma marcação específica.
        // Apenas o utilizador dono, Admin ou Funcionário podem ver todos os detalhes.
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

            // Apenas permite ver as próprias marcações, exceto administradores
            if (!isAdmin && booking.UserId != user.Id)
            {
                return Forbid();
            }

            // Obtém detalhes da refeição para a data da marcação
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
        // Marca uma marcação como confirmada (acesso restrito a Admin/Funcionário).
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
        // Cancela uma marcação como administrador/funcionário e processa reembolso.
        public async Task<IActionResult> CancelBookingAdmin(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Reembolsa o utilizador
            var user = booking.User;
            if (user != null)
            {
                user.WalletBalance += booking.ValorPago;

                // Cria transação de carteira para reembolso
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
        // Gera o relatório diário de marcações para uma data (Admin/Funcionário).
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

        [HttpPost]
        [Authorize(Roles = "Admin,Funcionário")]
        // Remove a marcação para um utilizador (Admin/Funcionário) e processa reembolso.
        public async Task<IActionResult> UnmarkMeal(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            var mealDate = booking.DataMarcacao.Date;

            // Reembolsa o utilizador
            if (booking.User != null)
            {
                booking.User.WalletBalance += booking.ValorPago;
            }

            // Cria transação de reembolso
            var transaction = new WalletTransaction
            {
                UserId = booking.UserId,
                DataTransacao = DateTime.Now,
                Tipo = "Reembolso",
                Descricao = $"Reembolso da refeição de {booking.DataMarcacao:dd/MM/yyyy}",
                Valor = booking.ValorPago
            };

            _context.WalletTransactions.Add(transaction);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Refeição desmarcada e reembolso processado!";
            return RedirectToAction("DailyReport", new { date = mealDate });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Funcionário")]
        // Abre a vista de edição de uma marcação (Admin/Funcionário).
        public async Task<IActionResult> EditMeal(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Funcionário")]
        // Atualiza (persist) alterações na marcação (Admin/Funcionário).
        public async Task<IActionResult> UpdateMeal(int id)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            var mealDate = booking.DataMarcacao.Date;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Refeição atualizada com sucesso!";
            return RedirectToAction("DailyReport", new { date = mealDate });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Funcionário")]
        // Elimina uma marcação e processa o reembolso (Admin/Funcionário).
        public async Task<IActionResult> DeleteMeal(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            var mealDate = booking.DataMarcacao.Date;

            // Reembolsa o utilizador
            if (booking.User != null)
            {
                booking.User.WalletBalance += booking.ValorPago;
            }

            // Cria transação de reembolso
            var transaction = new WalletTransaction
            {
                UserId = booking.UserId,
                DataTransacao = DateTime.Now,
                Tipo = "Reembolso",
                Descricao = $"Reembolso da refeição eliminada de {booking.DataMarcacao:dd/MM/yyyy}",
                Valor = booking.ValorPago
            };

            _context.WalletTransactions.Add(transaction);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Refeição eliminada e reembolso processado!";
            return RedirectToAction("DailyReport", new { date = mealDate });
        }
    }
}
