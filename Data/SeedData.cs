using Microsoft.EntityFrameworkCore;
using TiendaGamer.Models;

namespace TiendaGamer.Data
{
    public static class SeedData
    {
        // Este método es el que llamaremos desde Program.cs
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                if (context.Categories.Any())
                {
                    return; // La base de datos ya tiene datos
                }

                // --- SECCIÓN MODIFICADA ---

                // 1. Crear categorías principales
                var pcsArmadas = new Category { Name = "PCs Armadas" };
                var componentes = new Category { Name = "Componentes" };
                var perifericos = new Category { Name = "Periféricos" };

                context.Categories.AddRange(pcsArmadas, componentes, perifericos);
                context.SaveChanges(); // Guardar para obtener sus IDs

                // 2. Crear subcategorías y asignarlas a su padre "Componentes"
                // Hijas de "PCs Armadas"
                var gamaEntrada = new Category { Name = "Gama de Entrada", ParentCategoryId = pcsArmadas.Id };
                var gamaMedia = new Category { Name = "Gama Media", ParentCategoryId = pcsArmadas.Id };
                var gamaAlta = new Category { Name = "Gama Alta", ParentCategoryId = pcsArmadas.Id };

                // Hijas de "Componentes"
                var procesadores = new Category { Name = "Procesadores", ParentCategoryId = componentes.Id };
                var tarjetasGraficas = new Category { Name = "Tarjetas Gráficas", ParentCategoryId = componentes.Id };
                var placasBase = new Category { Name = "Tarjetas Madre", ParentCategoryId = componentes.Id };
                var memoriasRam = new Category { Name = "Memoria RAM", ParentCategoryId = componentes.Id };
                var almacenamiento = new Category { Name = "Almacenamiento", ParentCategoryId = componentes.Id };
                var gabinetes = new Category { Name = "Gabinetes", ParentCategoryId = componentes.Id };
                var fuentesPoder = new Category { Name = "Fuentes de Poder", ParentCategoryId = componentes.Id };
                // ...etc.

                // Hijas de "Periféricos"
                var mouses = new Category { Name = "Mouse", ParentCategoryId = perifericos.Id };
                var teclados = new Category { Name = "Teclados", ParentCategoryId = perifericos.Id };
                var monitores = new Category { Name = "Monitores", ParentCategoryId = perifericos.Id };
                var audifonos = new Category { Name = "Audífonos", ParentCategoryId = perifericos.Id };

                context.Categories.AddRange(procesadores, tarjetasGraficas, memoriasRam, placasBase, almacenamiento, gabinetes, mouses, teclados, monitores, audifonos, gamaAlta, gamaEntrada, gamaMedia, fuentesPoder);
                context.SaveChanges();

                // 3. Asignar productos a las categorías y subcategorías correctas
                context.Products.AddRange(
                    new Product
                    {
                        Name = "PC Gamer Pro",
                        Description = "Una bestia para jugar en 4K.",
                        Price = 35000.00m,
                        Stock = 10,
                        ImageUrl = "https://via.placeholder.com/150",
                        Category = gamaAlta // Asignado a "PCs Armadas"
                    },
                    new Product
                    {
                        Name = "Tarjeta Gráfica RTX 9090",
                        Description = "Potencia gráfica sin límites.",
                        Price = 25000.00m,
                        Stock = 10,
                        ImageUrl = "https://via.placeholder.com/150",
                        Category = tarjetasGraficas // Asignado a la subcategoría "Tarjetas Gráficas"
                    },
                    new Product
                    {
                        Name = "Kit 16GB RAM DDR5",
                        Description = "Velocidad y rendimiento para tu PC.",
                        Price = 2200.00m,
                        Stock = 10,
                        ImageUrl = "https://m.media-amazon.com/images/I/61uXihcspuL._AC_SL1500_.jpg",
                        Category = memoriasRam // Asignado a la subcategoría "Memorias RAM"
                    },
                    new Product
                    {
                        Name = "Teclado Mecánico RGB",
                        Description = "Siente cada pulsación con luces personalizables.",
                        Price = 1800.00m,
                        Stock = 10,
                        ImageUrl = "https://via.placeholder.com/150",
                        Category = teclados // Asignado a "Periféricos"
                    }
                );

                context.SaveChanges();
                // -----------------------------
            }
        }
    }
}
