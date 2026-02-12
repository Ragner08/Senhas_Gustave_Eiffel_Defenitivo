using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Senhas_Gustave_Eiffel.Data;
using Senhas_Gustave_Eiffel.Models;

namespace Senhas_Gustave_Eiffel.Controllers
{
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

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var transactions = await _context.WalletTransactions
                .Where(wt => wt.UserId == user.Id)
                .OrderByDescending(wt => wt.DataTransacao)
                .ToListAsync();

            var viewModel = new WalletDetailsViewModel
            {
                SaldoAtual = user.WalletBalance,
                Transacoes = transactions
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult AddFunds()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

            // Add funds to wallet
            user.WalletBalance += model.Valor;

            // Create transaction record
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllTransactions()
        {
            var transactions = await _context.WalletTransactions
                .Include(wt => wt.User)
                .OrderByDescending(wt => wt.DataTransacao)
                .ToListAsync();

            return View(transactions);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> AddFundsAdmin(string userId, decimal valor)
        {
            if (valor <= 0)
            {
                TempData["Error"] = "O valor deve ser maior que zero!";
                return RedirectToAction(nameof(UserWallet), new { userId });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.WalletBalance += valor;

            var transaction = new WalletTransaction
            {
                UserId = user.Id,
                Valor = valor,
                Tipo = "Carregamento",
                Descricao = $"Carregamento administrativo de {valor:C}"
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Foram adicionados {valor:C} à carteira de {user.Nome}!";
            return RedirectToAction(nameof(UserWallet), new { userId });
        }
    }
}
