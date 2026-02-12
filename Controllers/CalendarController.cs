using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Senhas_Gustave_Eiffel.Data;
using Senhas_Gustave_Eiffel.Models;
using System.Globalization;

namespace Senhas_Gustave_Eiffel.Controllers
{
    [Authorize]
    public class CalendarController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public CalendarController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index(int? year, int? month)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var isFuncionario = roles.Contains("Funcionário");
            var isAdmin = roles.Contains("Admin");

            var currentDate = DateTime.Now;
            var selectedYear = year ?? currentDate.Year;
            var selectedMonth = month ?? currentDate.Month;

            var firstDayOfMonth = new DateTime(selectedYear, selectedMonth, 1);
            var daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);
            var startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;

            // Adjust for Monday as first day of week
            startDayOfWeek = startDayOfWeek == 0 ? 6 : startDayOfWeek - 1;

            var viewModel = new CalendarViewModel
            {
                Year = selectedYear,
                Month = selectedMonth,
                MonthName = firstDayOfMonth.ToString("MMMM", new CultureInfo("pt-PT")),
                IsFuncionario = isFuncionario || isAdmin,
                UserEscalao = user.Escalao,
                WalletBalance = user.WalletBalance
            };

            // Get all bookings for the user in this month
            var userBookings = await _context.Bookings
                .Where(b => b.UserId == user.Id &&
                            b.DataMarcacao.Year == selectedYear &&
                            b.DataMarcacao.Month == selectedMonth)
                .ToListAsync();

            // Get all meals for this month
            var meals = await _context.Meals
                .Where(m => m.Data.Year == selectedYear && m.Data.Month == selectedMonth)
                .ToListAsync();

            // Previous month days
            var prevMonth = firstDayOfMonth.AddMonths(-1);
            var daysInPrevMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
            for (int i = startDayOfWeek - 1; i >= 0; i--)
            {
                var date = new DateTime(prevMonth.Year, prevMonth.Month, daysInPrevMonth - i);
                viewModel.Days.Add(new DayViewModel
                {
                    Date = date,
                    IsCurrentMonth = false,
                    IsToday = date.Date == DateTime.Today,
                    HasBooking = false,
                    HasMeal = meals.Any(m => m.Data.Date == date.Date),
                    IsPast = date < DateTime.Today
                });
            }

            // Current month days
            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(selectedYear, selectedMonth, day);
                var booking = userBookings.FirstOrDefault(b => b.DataMarcacao.Date == date.Date);

                viewModel.Days.Add(new DayViewModel
                {
                    Date = date,
                    IsCurrentMonth = true,
                    IsToday = date.Date == DateTime.Today,
                    HasBooking = booking != null,
                    HasMeal = meals.Any(m => m.Data.Date == date.Date),
                    IsPast = date < DateTime.Today,
                    BookingId = booking?.Id
                });
            }

            // Next month days to fill the grid
            var remainingDays = 42 - viewModel.Days.Count; // 6 rows * 7 days = 42
            for (int i = 1; i <= remainingDays; i++)
            {
                var date = new DateTime(selectedYear, selectedMonth, 1).AddMonths(1).AddDays(i - 1);
                viewModel.Days.Add(new DayViewModel
                {
                    Date = date,
                    IsCurrentMonth = false,
                    IsToday = date.Date == DateTime.Today,
                    HasBooking = false,
                    HasMeal = meals.Any(m => m.Data.Date == date.Date),
                    IsPast = date < DateTime.Today
                });
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> DayDetails(DateTime date)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var isFuncionario = roles.Contains("Funcionário");
            var isAdmin = roles.Contains("Admin");

            var meal = await _context.Meals
                .FirstOrDefaultAsync(m => m.Data.Date == date.Date);

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.UserId == user.Id && b.DataMarcacao.Date == date.Date);

            var hasBooking = booking != null;

            // Check if user already has a booking for this day
            if (hasBooking)
            {
                ViewBag.HasBooking = true;
                ViewBag.BookingId = booking.Id;
            }
            else
            {
                ViewBag.HasBooking = false;
            }

            ViewBag.Date = date;
            ViewBag.IsFuncionario = isFuncionario || isAdmin;
            ViewBag.IsPast = date < DateTime.Today;
            ViewBag.UserEscalao = user.Escalao;
            ViewBag.WalletBalance = user.WalletBalance;
            ViewBag.HasMeal = meal != null;

            // Calculate price based on user's escalão
            decimal price = user.Escalao switch
            {
                "Escalão A" => meal?.PrecoEscalaoA ?? 2.00m,
                "Escalão B" => meal?.PrecoEscalaoB ?? 3.00m,
                _ => meal?.PrecoSemEscalao ?? 4.00m
            };
            ViewBag.Price = price;

            return View(meal ?? new Meal { Data = date });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookMeal(DateTime date, string escalao)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if user already has a booking for this day
            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.UserId == user.Id && b.DataMarcacao.Date == date.Date);

            if (existingBooking != null)
            {
                TempData["Error"] = "Já tem uma marcação para este dia!";
                return RedirectToAction(nameof(DayDetails), new { date });
            }

            // Check if meal exists for this day
            var meal = await _context.Meals
                .FirstOrDefaultAsync(m => m.Data.Date == date.Date);

            if (meal == null)
            {
                TempData["Error"] = "Não existe refeição definida para este dia!";
                return RedirectToAction(nameof(DayDetails), new { date });
            }

            // Calculate price
            decimal price = escalao switch
            {
                "Escalão A" => meal.PrecoEscalaoA,
                "Escalão B" => meal.PrecoEscalaoB,
                _ => meal.PrecoSemEscalao
            };

            // Check if user has enough balance
            if (user.WalletBalance < price)
            {
                TempData["Error"] = "Saldo insuficiente na carteira!";
                return RedirectToAction(nameof(DayDetails), new { date });
            }

            // Create booking
            var booking = new Booking
            {
                UserId = user.Id,
                DataMarcacao = date,
                EscalaoUtilizado = escalao,
                ValorPago = price,
                Confirmado = true
            };

            // Deduct from wallet
            user.WalletBalance -= price;

            // Create wallet transaction
            var transaction = new WalletTransaction
            {
                UserId = user.Id,
                Valor = -price,
                Tipo = "Pagamento",
                Descricao = $"Pagamento de senha para {date:dd/MM/yyyy}"
            };

            _context.Bookings.Add(booking);
            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Senha marcada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == user.Id);

            if (booking == null)
            {
                return NotFound();
            }

            // Only allow cancellation for future dates
            if (booking.DataMarcacao <= DateTime.Today)
            {
                TempData["Error"] = "Não é possível cancelar marcações para hoje ou datas passadas!";
                return RedirectToAction(nameof(Index));
            }

            // Refund the user
            user.WalletBalance += booking.ValorPago;

            // Create wallet transaction for refund
            var transaction = new WalletTransaction
            {
                UserId = user.Id,
                Valor = booking.ValorPago,
                Tipo = "Carregamento",
                Descricao = $"Reembolso de cancelamento para {booking.DataMarcacao:dd/MM/yyyy}"
            };

            _context.WalletTransactions.Add(transaction);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Marcação cancelada com sucesso! O valor foi reembolsado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Funcionário")]
        public IActionResult CreateMeal(DateTime date)
        {
            var meal = new Meal
            {
                Data = date,
                PrecoEscalaoA = 2.00m,
                PrecoEscalaoB = 3.00m,
                PrecoSemEscalao = 4.00m
            };
            return View(meal);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Funcionário")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMeal(Meal meal)
        {
            if (ModelState.IsValid)
            {
                // Check if meal already exists for this day
                var existingMeal = await _context.Meals
                    .FirstOrDefaultAsync(m => m.Data.Date == meal.Data.Date);

                if (existingMeal != null)
                {
                    ModelState.AddModelError("", "Já existe uma refeição definida para este dia!");
                    return View(meal);
                }

                var user = await _userManager.GetUserAsync(User);
                meal.CriadoPor = user?.Nome ?? "Sistema";
                meal.DataCriacao = DateTime.Now;

                _context.Meals.Add(meal);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Refeição criada com sucesso!";
                return RedirectToAction(nameof(DayDetails), new { date = meal.Data });
            }

            return View(meal);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Funcionário")]
        public async Task<IActionResult> EditMeal(int id)
        {
            var meal = await _context.Meals.FindAsync(id);
            if (meal == null)
            {
                return NotFound();
            }
            return View(meal);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Funcionário")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMeal(int id, Meal meal)
        {
            if (id != meal.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(meal);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Refeição atualizada com sucesso!";
                    return RedirectToAction(nameof(DayDetails), new { date = meal.Data });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MealExists(meal.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return View(meal);
        }

        private bool MealExists(int id)
        {
            return _context.Meals.Any(e => e.Id == id);
        }
    }
}
