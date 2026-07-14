using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Senhas_Gustave_Eiffel.Data;
using Senhas_Gustave_Eiffel.Models;

namespace Senhas_Gustave_Eiffel.Controllers
{
    // Controlador para gestão de itens alimentares (`FoodItem`).
    // Permite a administração das comidas usadas na definição de menus (Sopa,
    // Prato Principal, Vegetariano, Sobremesa). Restrito a Admin/Funcionário.
    [Authorize(Roles = "Admin,Funcionário")]
    public class FoodsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FoodsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lista todos os `FoodItem` disponíveis ordenados por categoria/nome.
        public async Task<IActionResult> Index(string searchTerm)
        {
            var query = _context.FoodItems.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(f => f.Nome.Contains(term));
            }

            var foods = await query
                .OrderBy(f => f.Categoria)
                .ThenBy(f => f.Nome)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            return View(foods);
        }

        [HttpGet]
        // Mostra o formulário para criar um novo `FoodItem`.
        public IActionResult Create()
        {
            ViewBag.Categories = new List<string>
            {
                "Sopa",
                "Prato Principal",
                "Vegetariano",
                "Sobremesa"
            };

            return View(new FoodItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Cria um novo `FoodItem` após validação e guarda no BD.
        public async Task<IActionResult> Create(FoodItem foodItem)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new List<string>
                {
                    "Sopa",
                    "Prato Principal",
                    "Vegetariano",
                    "Sobremesa"
                };
                return View(foodItem);
            }

            _context.FoodItems.Add(foodItem);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Comida adicionada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
