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

        // GET: /ShoppingCart/Checkout (Muestra el formulario)
        [HttpGet]
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

            // Validamos stock antes de entrar al checkout
            foreach (var item in cartItems)
            {
                if (item.Quantity > item.Product.Stock)
                {
                    TempData["Error"] = $"El producto {item.Product.Name} ya no tiene suficiente stock.";
                    return RedirectToAction("Index");
                }
            }

            // Pasamos los datos del carrito a la vista para el resumen
            ViewBag.CartItems = cartItems;
            ViewBag.CartTotal = cartItems.Sum(i => i.Product.Price * i.Quantity);

            return View(new Order()); // Enviamos un modelo de Order vacío para el formulario
        }

        // POST: /ShoppingCart/ProcessCheckout (Procesa el pago y crea el pedido)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(Order order)
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

            // Si el modelo es válido (los campos de envío están llenos)
            if (ModelState.IsValid)
            {
                // Completamos los datos faltantes del pedido
                order.UserId = userId;
                order.OrderDate = DateTime.Now;
                order.OrderDetails = new List<OrderDetail>();
                order.Total = 0;

                foreach (var item in cartItems)
                {
                    // Validación final de stock
                    if (item.Quantity > item.Product.Stock)
                    {
                        TempData["Error"] = $"Stock insuficiente para {item.Product.Name}.";
                        return RedirectToAction("Index");
                    }

                    var orderDetail = new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price
                    };
                    order.OrderDetails.Add(orderDetail);
                    order.Total += (item.Product.Price * item.Quantity);

                    // Restamos el stock
                    item.Product.Stock -= item.Quantity;
                }

                _context.Orders.Add(order);
                _context.ShoppingCartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                return RedirectToAction("HistorialCompras", "Profile");
            }

            // Si algo falló en la validación, volvemos a mostrar el formulario
            ViewBag.CartItems = cartItems;
            ViewBag.CartTotal = cartItems.Sum(i => i.Product.Price * i.Quantity);
            return View("Checkout", order);
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
