using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration.Conventions;
using PracticaProgramada1.Models;
using IndexAttribute = System.ComponentModel.DataAnnotations.Schema.IndexAttribute;

namespace PracticaProgramada1.Data
{
    public class TiendaContext : DbContext
    {
        public TiendaContext() : base("name=TiendaContext")
        {
        }

        // Cada DbSet representa una tabla de la base de datos (ver TABLASCASO1.sql).
        public DbSet<Categoria> Categorias { get; set; }

        public DbSet<Producto> Productos { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Sin borrado en cascada (equivalente a DeleteBehavior.Restrict de TiendaMVC).
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Entity<Categoria>().ToTable("Categorias");
            modelBuilder.Entity<Categoria>().HasKey(c => c.IdCategoria);
            modelBuilder.Entity<Categoria>().Property(c => c.NombreCategoria)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("unico_Categoria") { IsUnique = true }));

            modelBuilder.Entity<Producto>().ToTable("Productos");
            modelBuilder.Entity<Producto>().HasKey(p => p.Id);

            // El código del producto no se puede repetir.
            modelBuilder.Entity<Producto>().Property(p => p.Codigo)
                .IsRequired()
                .HasMaxLength(12)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("unico_codigo") { IsUnique = true }));

            modelBuilder.Entity<Producto>().Property(p => p.Nombre).IsRequired().HasMaxLength(80);
            modelBuilder.Entity<Producto>().Property(p => p.Precio).HasPrecision(10, 2);
            modelBuilder.Entity<Producto>().Property(p => p.Existencia).HasPrecision(10, 3);

            modelBuilder.Entity<Producto>()
                .HasRequired(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.IdCategoria);
        }
    }
}
