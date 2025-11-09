using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TiendaGamer.Models;

namespace TiendaGamer.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Es importante llamar a la implementación base primero

            // Configurar la precisión para la propiedad Price en Product
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            // Configurar la precisión para la propiedad Total en Order
            modelBuilder.Entity<Order>()
                .Property(o => o.Total)
                .HasColumnType("decimal(18,2)");

            // Configurar la precisión para la propiedad Price en OrderDetail
            modelBuilder.Entity<OrderDetail>()
                .Property(d => d.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory) // Una categoría tiene un padre opcional.
                .WithMany() // Un padre puede tener muchos hijos (no necesitamos una propiedad para la lista de hijos).
                .HasForeignKey(c => c.ParentCategoryId) // La clave foránea es ParentCategoryId.
                .OnDelete(DeleteBehavior.Restrict); // Evita que se borren subcategorías en cascada.
        }
    }
}
