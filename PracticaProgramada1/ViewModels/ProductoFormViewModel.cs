using System.Collections.Generic;
using System.Web.Mvc;
using PracticaProgramada1.Models;

namespace PracticaProgramada1.ViewModels
{
    public class ProductoFormViewModel
    {
        public Producto Producto { get; set; } = new Producto();

        public List<SelectListItem> Categorias { get; set; } = new List<SelectListItem>();
    }
}
