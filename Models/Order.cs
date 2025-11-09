using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // NECESARIO PARA [ValidateNever]

namespace TiendaGamer.Models

{
    public class Order
    {
        public int Id { get; set; }
        [ValidateNever]
        public string UserId { get; set; }
        [ValidateNever]
        public ApplicationUser? User { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Total { get; set; }
        public List<OrderDetail>? OrderDetails { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string ShippingName { get; set; } // Nombre de quien recibe

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "La ciudad es obligatoria.")]
        public string ShippingCity { get; set; }

        [Required(ErrorMessage = "El código postal es obligatorio.")]
        public string ShippingPostalCode { get; set; }
    }
}
