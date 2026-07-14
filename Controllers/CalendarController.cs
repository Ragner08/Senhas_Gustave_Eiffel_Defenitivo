using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Senhas_Gustave_Eiffel.Data;
using Senhas_Gustave_Eiffel.Models;
using System.Globalization;

namespace Senhas_Gustave_Eiffel.Controllers
{
    // Controlador do calendário: constrói a vista do calendário, detalhe do dia
    // e ações relacionadas com refeições e marcações (BookMeal, CancelBooking,
    // criação/edição de Meal). Garante validações de datas e calcula preços.
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

        // Mostra o calendário do mês especificado (ou mês atual).
        // Prepara um `CalendarViewModel` com a lista de `DayViewModel` contendo
        // flags: `HasMeal`, `HasBooking`, `IsPast`, `IsToday`.
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

            // Ajusta para segunda-feira ser o primeiro dia da semana
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

            // Obtém todas as marcações do utilizador neste mês
            var userBookings = await _context.Bookings
                .Where(b => b.UserId == user.Id &&
                            b.DataMarcacao.Year == selectedYear &&
                            b.DataMarcacao.Month == selectedMonth)
                .ToListAsync();

            // Obtém todas as refeições deste mês
            var meals = await _context.Meals
                .Where(m => m.Data.Year == selectedYear && m.Data.Month == selectedMonth)
                .ToListAsync();

            // Dias do mês anterior
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
                    // IsPast: dias anteriores (exclui o dia atual)
                    IsPast = date.Date < DateTime.Today
                });
            }

            // Dias do mês atual
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
                    // IsPast: dias anteriores (exclui o dia atual)
                    IsPast = date.Date < DateTime.Today,
                    BookingId = booking?.Id
                });
            }

            // Dias do mês seguinte para preencher a grelha
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
                    // IsPast: dias anteriores (exclui o dia atual)
                    IsPast = date.Date < DateTime.Today
                });
            }

            return View(viewModel);
        }

        [HttpGet]
        // Mostra os detalhes de um dia: refeição definida, existência de
        // marcação do utilizador, preço calculado consoante o escalão.
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

            // Verifica se o utilizador já tem uma marcação para este dia
            ViewBag.HasBooking = hasBooking;
            ViewBag.BookingId = booking?.Id;

            ViewBag.Date = date;
            ViewBag.IsFuncionario = isFuncionario || isAdmin;
            // MODIFICADO: IsPast inclui o dia atual (não permite marcação no mesmo dia)
            ViewBag.IsPast = date.Date < DateTime.Today;
            ViewBag.UserEscalao = user.Escalao;
            ViewBag.WalletBalance = user.WalletBalance;
            ViewBag.HasMeal = meal != null;

            // Calcula o preço com base no escalão do utilizador
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
        // Processa a marcação de uma refeição para o utilizador.
        // Valida datas, existência de refeição, saldo suficiente e cria
        // `Booking` + `WalletTransaction` em BD.
        public async Task<IActionResult> BookMeal(DateTime date, string escalao)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // MODIFICADO: Verificar se a data é hoje ou anterior (proibido marcar)
            if (date.Date <= DateTime.Today)
            {
                TempData["Error"] = "Não é possível marcar senhas para hoje ou datas passadas!";
                return RedirectToAction(nameof(DayDetails), new { date });
            }

            // Verifica se o utilizador já tem uma marcação para este dia
            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.UserId == user.Id && b.DataMarcacao.Date == date.Date);

            if (existingBooking != null)
            {
                TempData["Error"] = "Já tem uma marcação para este dia!";
                return RedirectToAction(nameof(DayDetails), new { date });
            }

            // Verifica se existe refeição para este dia
            var meal = await _context.Meals
                .FirstOrDefaultAsync(m => m.Data.Date == date.Date);

            if (meal == null)
            {
                TempData["Error"] = "Não existe refeição definida para este dia!";
                return RedirectToAction(nameof(DayDetails), new { date });
            }

            // Calcula o preço
            decimal price = escalao switch
            {
                "Escalão A" => meal.PrecoEscalaoA,
                "Escalão B" => meal.PrecoEscalaoB,
                _ => meal.PrecoSemEscalao
            };

            // Verifica se o utilizador tem saldo suficiente
            if (user.WalletBalance < price)
            {
                TempData["Error"] = "Saldo insuficiente na carteira!";
                return RedirectToAction(nameof(DayDetails), new { date });
            }

            // Cria marcação
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

            // Cria transação de carteira
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
            return RedirectToAction(nameof(Index), new { year = date.Year, month = date.Month });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Cancela a marcação do utilizador (apenas para datas futuras).
        // Reembolsa o valor na carteira e cria transação de reembolso.
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

            // Permite cancelar apenas datas futuras (não permite hoje ou passado)
            if (booking.DataMarcacao.Date <= DateTime.Today)
            {
                TempData["Error"] = "Não é possível cancelar marcações para hoje ou datas passadas!";
                return RedirectToAction(nameof(Index));
            }

            // Reembolsa o utilizador
            user.WalletBalance += booking.ValorPago;

            // Cria transação de carteira para reembolso
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
            return RedirectToAction(nameof(Index), new { year = booking.DataMarcacao.Year, month = booking.DataMarcacao.Month });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Funcionário")]
        // Abre o formulário de criação de `Meal` para a data indicada.
        // (Restrito a Admin/Funcionário). Valida datas e fim de semana.
        public async Task<IActionResult> CreateMeal(DateTime date)
        {
            // MODIFICADO: Não permitir criar refeição para hoje, datas passadas ou fim de semana
            if (date.Date <= DateTime.Today || date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                TempData["Error"] = "Não é possível definir refeições para hoje, datas passadas ou fins de semana!";
                return RedirectToAction(nameof(Index));
            }

            var meal = new Meal
            {
                Data = date,
                PrecoEscalaoA = 0.00m,
                PrecoEscalaoB = 0.73m,
                PrecoSemEscalao = 1.46m
            };

            await PopulateFoodSelectListsAsync();
            return View(meal);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Funcionário")]
        [ValidateAntiForgeryToken]
        // Cria a `Meal` no BD após validação (Admin/Funcionário).
        public async Task<IActionResult> CreateMeal(Meal meal)
        {
            if (meal.Data.Date <= DateTime.Today || meal.Data.DayOfWeek == DayOfWeek.Saturday || meal.Data.DayOfWeek == DayOfWeek.Sunday)
            {
                TempData["Error"] = "Não é possível definir refeições para hoje, datas passadas ou fins de semana!";
                await PopulateFoodSelectListsAsync();
                return View(meal);
            }

            if (ModelState.IsValid)
            {
                // Verifica se já existe refeição para este dia
                var existingMeal = await _context.Meals
                    .FirstOrDefaultAsync(m => m.Data.Date == meal.Data.Date);

                if (existingMeal != null)
                {
                    ModelState.AddModelError("", "Já existe uma refeição definida para este dia!");
                    await PopulateFoodSelectListsAsync();
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

            await PopulateFoodSelectListsAsync();
            return View(meal);
        }

        // Preenche ViewBag com listas de alimentos para os selects do formulário.
        private async Task PopulateFoodSelectListsAsync()
        {
            var foods = await _context.Set<FoodItem>().ToListAsync();

            ViewBag.SopaItems = foods
                .Where(f => f.Categoria == "Sopa")
                .Select(f => new SelectListItem(f.Nome, f.Nome))
                .ToList();

            ViewBag.PratoPrincipalItems = foods
                .Where(f => f.Categoria == "Prato Principal")
                .Select(f => new SelectListItem(f.Nome, f.Nome))
                .ToList();

            ViewBag.VegetarianoItems = foods
                .Where(f => f.Categoria == "Vegetariano")
                .Select(f => new SelectListItem(f.Nome, f.Nome))
                .ToList();

            ViewBag.SobremesaItems = foods
                .Where(f => f.Categoria == "Sobremesa")
                .Select(f => new SelectListItem(f.Nome, f.Nome))
                .ToList();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Funcionário")]
        // Abre o formulário de edição de `Meal` (Admin/Funcionário).
        public async Task<IActionResult> EditMeal(int id)
        {
            var meal = await _context.Meals.FindAsync(id);
            if (meal == null)
            {
                return NotFound();
            }

            // MODIFICADO: Não permitir editar refeição para hoje ou datas passadas
            if (meal.Data.Date <= DateTime.Today)
            {
                TempData["Error"] = "Não é possível editar refeições para hoje ou datas passadas!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateFoodSelectListsAsync();
            return View(meal);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Funcionário")]
        [ValidateAntiForgeryToken]
        // Atualiza a `Meal` após validação (Admin/Funcionário).
        public async Task<IActionResult> EditMeal(int id, Meal meal)
        {
            if (id != meal.Id)
            {
                return NotFound();
            }

            // MODIFICADO: Não permitir editar refeição para hoje ou datas passadas
            if (meal.Data.Date <= DateTime.Today)
            {
                TempData["Error"] = "Não é possível editar refeições para hoje ou datas passadas!";
                await PopulateFoodSelectListsAsync();
                return View(meal);
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

            await PopulateFoodSelectListsAsync();
            return View(meal);
        }

        private bool MealExists(int id)
        {
            return _context.Meals.Any(e => e.Id == id);
        }
    }
}
