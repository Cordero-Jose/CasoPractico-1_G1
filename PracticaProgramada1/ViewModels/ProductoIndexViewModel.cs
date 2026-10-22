using System.Collections.Generic;
using System.Web.Mvc;
using PracticaProgramada1.Models;

namespace PracticaProgramada1.ViewModels
{
    public class ProductoIndexViewModel
    {
        public List<Producto> Productos { get; set; } = new List<Producto>();

        public string Buscar { get; set; }

        public int? CategoriaId { get; set; }

        public List<SelectListItem> Categorias { get; set; } = new List<SelectListItem>();
    }
}
