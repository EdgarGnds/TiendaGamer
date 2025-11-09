using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaGamer.Data;
using TiendaGamer.Models;
using Microsoft.EntityFrameworkCore;

namespace TiendaGamer.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShoppingCartController(ApplicationDbContext context) { _context = context; }

        // Muestra los items del carrito del usuario
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var items = await _context.ShoppingCartItems
                .Where(i => i.UserId == userId)
                .Include(i => i.Product)
                .ToListAsync();
            return View(items);
        }

        // Acción para agregar un producto al carrito
        // POST: /ShoppingCart/Agregar
        [HttpPost]
        // [ValidateAntiForgeryToken] // Recomendado agregar esto también aquí si puedes
        public async Task<IActionResult> Agregar(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Buscamos el producto para verificar su stock
            var product = await _context.Products.FindAsync(productId);

            // 1. Validación inicial: ¿El producto existe y tiene stock > 0?
            if (product == null || product.Stock <= 0)
            {
                return Json(new { success = false, message = "Lo sentimos, este producto está agotado." });
            }

            var cartItem = await _context.ShoppingCartItems
                .FirstOrDefaultAsync(i => i.UserId == userId && i.ProductId == productId);

            // 2. Validación de cantidad: ¿Si sumo 1, supero el stock?
            int cantidadActual = cartItem?.Quantity ?? 0;
            if (cantidadActual + 1 > product.Stock)
            {
                return Json(new { success = false, message = $"Solo quedan {product.Stock} unidades disponibles." });
            }

            // Si pasa las validaciones, procedemos a agregar/actualizar
            if (cartItem != null)
            {
                cartItem.Quantity++;
            }
            else
            {
                var newItem = new ShoppingCartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = 1
                };
                _context.ShoppingCartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "¡Producto agregado al carrito!" });
        }

        // POST: /ShoppingCart/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken] // Buena práctica agregar esto al formulario de checkout también
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = await _context.ShoppingCartItems
                                        .Include(i => i.Product)
                                        .Where(i => i.UserId == userId)
                                        .ToListAsync();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index");
            }

            // --- VALIDACIÓN FINAL DE STOCK ---
            // Es vital volver a revisar justo antes de cobrar, por si alguien más compró el último artículo
            // mientras el usuario estaba en la página del carrito.
            foreach (var item in cartItems)
            {
                if (item.Quantity > item.Product.Stock)
                {
                    // En un caso real, aquí deberías mostrar un mensaje de error en la página del carrito.
                    // Por simplicidad, redirigimos al índice del carrito donde el usuario verá que no puede proceder.
                    TempData["Error"] = $"Lo sentimos, el producto {item.Product.Name} ya no tiene suficiente stock.";
                    return RedirectToAction("Index");
                }
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                OrderDetails = new List<OrderDetail>(),
                Total = 0
            };

            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                };
                order.OrderDetails.Add(orderDetail);
                order.Total += (item.Product.Price * item.Quantity);

                // --- RESTA DEL STOCK ---
                // Aquí es donde finalmente reducimos el inventario
                item.Product.Stock -= item.Quantity;
            }

            _context.Orders.Add(order);
            _context.ShoppingCartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return RedirectToAction("HistorialCompras", "Profile");
        }

        // POST: /ShoppingCart/UpdateCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCart(int productId, int newQuantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItem = await _context.ShoppingCartItems
                                         .Include(i => i.Product)
                                         .FirstOrDefaultAsync(i => i.UserId == userId && i.ProductId == productId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "Error: Artículo no encontrado." });
            }

            // Validación de Stock para la nueva cantidad solicitada
            // Solo validamos si está intentando AUMENTAR la cantidad
            if (newQuantity > cartItem.Quantity && newQuantity > cartItem.Product.Stock)
            {
                return Json(new { success = false, message = $"Solo hay {cartItem.Product.Stock} unidades disponibles." });
            }

            bool wasRemoved = false;
            string itemSubtotal = "0";

            if (newQuantity <= 0)
            {
                _context.ShoppingCartItems.Remove(cartItem);
                wasRemoved = true;
            }
            else
            {
                cartItem.Quantity = newQuantity;
                itemSubtotal = (cartItem.Product.Price * cartItem.Quantity).ToString("C");
            }

            await _context.SaveChangesAsync();

            // Recalcular total del carrito
            var cartItems = await _context.ShoppingCartItems
                                          .Include(i => i.Product)
                                          .Where(i => i.UserId == userId)
                                          .ToListAsync();
            decimal newCartTotal = cartItems.Sum(item => item.Product.Price * item.Quantity);

            return Json(new
            {
                success = true,
                message = wasRemoved ? "Producto eliminado" : "Cantidad actualizada",
                wasRemoved = wasRemoved,
                newSubtotal = itemSubtotal,
                newTotal = newCartTotal.ToString("C"),
                cartItemCount = cartItems.Sum(i => i.Quantity)
            });
        }
    }
}
