using System.Data.Entity.Migrations;
using PracticaProgramada1.Data;
using PracticaProgramada1.Models;

namespace PracticaProgramada1.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<TiendaContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        // Catálogo inicial. AddOrUpdate evita duplicados al ejecutar Update-Database varias veces.
        protected override void Seed(TiendaContext context)
        {
            context.Categorias.AddOrUpdate(
                c => c.NombreCategoria,
                new Categoria { NombreCategoria = "Abarrotes" },
                new Categoria { NombreCategoria = "Bebidas" },
                new Categoria { NombreCategoria = "Frutas y verduras" },
                new Categoria { NombreCategoria = "Lácteos" },
                new Categoria { NombreCategoria = "Limpieza" });
        }
    }
}
