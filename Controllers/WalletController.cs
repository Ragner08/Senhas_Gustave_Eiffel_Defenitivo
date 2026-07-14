using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Senhas_Gustave_Eiffel.Data;
using Senhas_Gustave_Eiffel.Models;

namespace Senhas_Gustave_Eiffel.Controllers
{
    // Controlador da carteira do utilizador: ver saldo, histórico de transações
    // e operações para adicionar fundos (pelo próprio utilizador ou admin).
    [Authorize]
    public class WalletController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public WalletController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Mostra o detalhe da carteira do utilizador: saldo atual e transações.
        public async Task<IActionResult> Index(string transactionType, DateTime? date)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var query = _context.WalletTransactions
                .Where(wt => wt.UserId == user.Id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(transactionType) && transactionType != "Todos")
            {
                if (transactionType == "Carregamento")
                {
                    query = query.Where(wt => wt.Tipo.Contains("Carregamento", StringComparison.OrdinalIgnoreCase));
                }
                else if (transactionType == "Refeição")
                {
                    query = query.Where(wt =>
                        wt.Descricao.Contains("senha", StringComparison.OrdinalIgnoreCase) ||
                        wt.Descricao.Contains("refeição", StringComparison.OrdinalIgnoreCase) ||
                        wt.Descricao.Contains("pagamento", StringComparison.OrdinalIgnoreCase) ||
                        wt.Descricao.Contains("reembolso", StringComparison.OrdinalIgnoreCase));
                }
            }

            if (date.HasValue)
            {
                query = query.Where(wt => wt.DataTransacao.Date == date.Value.Date);
            }

            var transactions = await query
                .OrderByDescending(wt => wt.DataTransacao)
                .ToListAsync();

            ViewBag.TransactionType = transactionType;
            ViewBag.FilterDate = date;

            var viewModel = new WalletDetailsViewModel
            {
                SaldoAtual = user.WalletBalance,
                Transacoes = transactions
            };

            return View(viewModel);
        }

        [HttpGet]
        // Mostra a vista para adicionar fundos à carteira.
        public IActionResult AddFunds()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Processa o carregamento de fundos pelo utilizador e regista a transação.
        public async Task<IActionResult> AddFunds(WalletViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Adiciona fundos à carteira
            user.WalletBalance += model.Valor;

            // Cria registo de transação
            var transaction = new WalletTransaction
            {
                UserId = user.Id,
                Valor = model.Valor,
                Tipo = "Carregamento",
                Descricao = $"Carregamento manual de {model.Valor:C}"
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Foram adicionados {model.Valor:C} à sua carteira com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Funcionário")]
        // Admin/Funcionário: lista todas as transações da carteira (para monitorização).
        public async Task<IActionResult> AllTransactions(string transactionType, DateTime? date)
        {
            var query = _context.WalletTransactions
                .Include(wt => wt.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(transactionType) && transactionType != "Todos")
            {
                if (transactionType == "Carregamento")
                {
                    query = query.Where(wt => wt.Tipo.Contains("Carregamento", StringComparison.OrdinalIgnoreCase));
                }
                else if (transactionType == "Refeição")
                {
                    query = query.Where(wt =>
                        wt.Descricao.Contains("senha", StringComparison.OrdinalIgnoreCase) ||
                        wt.Descricao.Contains("refeição", StringComparison.OrdinalIgnoreCase) ||
                        wt.Descricao.Contains("pagamento", StringComparison.OrdinalIgnoreCase) ||
                        wt.Descricao.Contains("reembolso", StringComparison.OrdinalIgnoreCase));
                }
            }

            if (date.HasValue)
            {
                query = query.Where(wt => wt.DataTransacao.Date == date.Value.Date);
            }

            var transactions = await query
                .OrderByDescending(wt => wt.DataTransacao)
                .ToListAsync();

            ViewBag.TransactionType = transactionType;
            ViewBag.FilterDate = date;

            return View(transactions);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        // Admin: mostra os detalhes da carteira de um utilizador específico e o seu histórico.
        public async Task<IActionResult> UserWallet(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var transactions = await _context.WalletTransactions
                .Where(wt => wt.UserId == userId)
                .OrderByDescending(wt => wt.DataTransacao)
                .ToListAsync();

            var viewModel = new AdminUserWalletViewModel
            {
                UserId = user.Id,
                UserName = user.Nome,
                UserEmail = user.Email ?? string.Empty,
                SaldoAtual = user.WalletBalance,
                Transacoes = transactions
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        // Admin: adiciona fundos à carteira de outro utilizador e regista a transação.
        public async Task<IActionResult> AddFundsAdmin(string userId, string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                TempData["Error"] = "Introduza um valor válido!";
                return RedirectToAction(nameof(UserWallet), new { userId });
            }

            var parsedValue = ParseDecimalValue(valor);
            if (!parsedValue.HasValue || parsedValue.Value <= 0)
            {
                TempData["Error"] = "O valor deve ser maior que zero!";
                return RedirectToAction(nameof(UserWallet), new { userId });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.WalletBalance += parsedValue.Value;

            var transaction = new WalletTransaction
            {
                UserId = user.Id,
                Valor = parsedValue.Value,
                Tipo = "Carregamento",
                Descricao = $"Carregamento administrativo de {parsedValue.Value:C}"
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Foram adicionados {parsedValue.Value:C} à carteira de {user.Nome}!";
            return RedirectToAction(nameof(UserWallet), new { userId });
        }

        private static decimal? ParseDecimalValue(string value)
        {
            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariantValue))
            {
                return invariantValue;
            }

            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out var currentCultureValue))
            {
                return currentCultureValue;
            }

            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-PT"), out var ptValue))
            {
                return ptValue;
            }

            return null;
        }
    }
}
