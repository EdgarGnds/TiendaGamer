using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaGamer.Data;
using Microsoft.EntityFrameworkCore;

namespace TiendaGamer.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Página principal del perfil (puedes mostrar datos del usuario)
        public IActionResult Index()
        {
            return View();
        }

        // Página para el historial de compras
        public async Task<IActionResult> HistorialCompras()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Obtiene el ID del usuario actual

            var ordenes = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderDetails)    // Carga los detalles de la orden
                .ThenInclude(d => d.Product)     // Carga la información del producto en cada detalle
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(ordenes);
        }
    }
}
