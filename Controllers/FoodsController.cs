using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Senhas_Gustave_Eiffel.Data;
using Senhas_Gustave_Eiffel.Models;

namespace Senhas_Gustave_Eiffel.Controllers
{
    [Authorize(Roles = "Admin,Funcionário")]
    public class FoodsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FoodsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var foods = await _context.FoodItems
                .OrderBy(f => f.Categoria)
                .ThenBy(f => f.Nome)
                .ToListAsync();

            return View(foods);
        }

        [HttpGet]
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
