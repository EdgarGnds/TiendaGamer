using Microsoft.AspNetCore.Mvc;
using TiendaGamer.Data;
using TiendaGamer.Models;
using Microsoft.EntityFrameworkCore;

namespace TiendaGamer.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        // GET: /Products/Create
        public IActionResult Create()
        {
            // Enviamos a la vista solo las categorías que NO son padres de otras.
            // Es decir, "PCs Armadas", "Periféricos", "Procesadores", "Tarjetas Gráficas", etc.
            var categoriesForForm = _context.Categories
                .Where(c => !_context.Categories.Any(sub => sub.ParentCategoryId == c.Id))
                .ToList();

            ViewBag.Categories = categoriesForForm;
            return View();
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Redirige a la lista de productos
            }
            // Si hay un error, vuelve a mostrar el formulario con los datos
            var categoriesForForm = _context.Categories
        .Where(c => !_context.Categories.Any(sub => sub.ParentCategoryId == c.Id))
        .ToList();
            ViewBag.Categories = categoriesForForm;
            return View(product);
        }

        // GET: Products/Edit/5  (El "5" es un ejemplo de ID)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Busca el producto en la base de datos por su ID
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Preparamos la lista de categorías (solo las que no son padres)
            // EXACTAMENTE igual que en el método Create()
            var categoriesForForm = _context.Categories
                .Where(c => !_context.Categories.Any(sub => sub.ParentCategoryId == c.Id))
                .ToList();

            ViewBag.Categories = categoriesForForm;
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,ImageUrl,CategoryId")] Product product)
        {
            // Verifica que el ID de la URL coincida con el del formulario
            if (id != product.Id)
            {
                return NotFound();
            }

            // Usamos el mismo 'ModelState.IsValid' para validar
            if (ModelState.IsValid)
            {
                try
                {
                    // Le dice a Entity Framework que este producto ha sido modificado
                    _context.Update(product);
                    // Guarda los cambios en la base de datos
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Esto es por si ocurre un error avanzado, pero es bueno tenerlo
                    if (!_context.Products.Any(e => e.Id == product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Redirige a la lista de productos si todo salió bien
                return RedirectToAction(nameof(Index));
            }

            // Si la validación falla, recargamos las categorías y volvemos a mostrar el formulario
            var categoriesForForm = _context.Categories
                .Where(c => !_context.Categories.Any(sub => sub.ParentCategoryId == c.Id))
                .ToList();
            ViewBag.Categories = categoriesForForm;

            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Busca el producto y su categoría para mostrar los detalles
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Envía el producto a la vista de confirmación
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Busca el producto
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Lo elimina del contexto
                _context.Products.Remove(product);
            }

            // Guarda los cambios en la base de datos
            await _context.SaveChangesAsync();
            // Redirige a la lista
            return RedirectToAction(nameof(Index));
        }
    }
}
