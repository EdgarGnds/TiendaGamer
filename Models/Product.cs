using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace TiendaGamer.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar una categoría.")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }
    }
}
