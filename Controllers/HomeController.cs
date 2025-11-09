using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TiendaGamer.Data;
using TiendaGamer.Models;
using Microsoft.EntityFrameworkCore;

namespace TiendaGamer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Consultamos los productos
            var productosDestacados = await _context.Products
                .Include(p => p.Category) // Incluimos la categoría
                .OrderByDescending(p => p.Id) // Ordenamos por los más nuevos
                .Take(4) // Tomamos solo 4
                .ToListAsync();

            // Enviamos los productos a la vista
            return View(productosDestacados);
        }

        public async Task<IActionResult> PcsArmadas()
        {
            // Buscamos la categoría "PCs Armadas"
            var categoriaPadre = await _context.Categories
                                               .FirstOrDefaultAsync(c => c.Name == "PCs Armadas");

            if (categoriaPadre == null) return View(new List<Category>());

            // Buscamos sus subcategorías (Gama Entrada, Media, Alta) y sus productos
            var subcategorias = await _context.Categories
                .Where(c => c.ParentCategoryId == categoriaPadre.Id)
                .Include(c => c.Products)
                .ToListAsync();

            return View(subcategorias); // Enviamos las subcategorías a la vista
        }

        public async Task<IActionResult> Componentes()
        {
            // 1. Buscamos la categoría "Componentes"
            var categoriaPadre = await _context.Categories
                                               .FirstOrDefaultAsync(c => c.Name == "Componentes");

            if (categoriaPadre == null)
            {
                // Si no existe, enviamos una lista vacía
                return View(new List<Category>());
            }

            // 2. Buscamos todas las SUBCATEGORÍAS que sean hijas de "Componentes"
            //    Y, lo más importante, ¡Incluimos los productos de cada una!
            var subcategorias = await _context.Categories
                .Where(c => c.ParentCategoryId == categoriaPadre.Id)
                .Include(c => c.Products) // <-- ¡Esta es la clave!
                .ToListAsync();

            // 3. Enviamos esta lista de subcategorías (con sus productos) a la vista
            return View(subcategorias);
        }

        public async Task<IActionResult> Perifericos()
        {
            // Buscamos la categoría "Periféricos"
            var categoriaPadre = await _context.Categories
                                               .FirstOrDefaultAsync(c => c.Name == "Periféricos");

            if (categoriaPadre == null) return View(new List<Category>());

            // Buscamos sus subcategorías (Mouse, Teclados, etc.) y sus productos
            var subcategorias = await _context.Categories
                .Where(c => c.ParentCategoryId == categoriaPadre.Id)
                .Include(c => c.Products)
                .ToListAsync();

            return View(subcategorias); // Enviamos las subcategorías a la vista
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // GET: /Home/Search?query=texto_buscado
        public async Task<IActionResult> Search(string query)
        {
            // Guardamos el término de búsqueda para mostrarlo en la vista
            ViewData["CurrentQuery"] = query;

            if (string.IsNullOrWhiteSpace(query))
            {
                // Si no buscan nada, mostramos una lista vacía
                return View("SearchResults", new List<Product>());
            }

            // 1. Convertimos el término de búsqueda a minúsculas una sola vez
            var lowerQuery = query.ToLower();

            // 2. Traemos TODOS los productos a la memoria del servidor.
            //    Esto resuelve el error de traducción.
            var allProducts = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();

            // 3. Filtramos la lista EN MEMORIA usando C# (que sí entiende ToLower)
            //    Agregamos 'p.Name != null' para evitar errores si un nombre está vacío
            var products = allProducts
                .Where(p => (p.Name != null && p.Name.ToLower().Contains(lowerQuery)) ||
                            (p.Description != null && p.Description.ToLower().Contains(lowerQuery)));

            // 4. Enviamos los productos ya filtrados a la vista
            return View("SearchResults", products);
        }
    }
}
