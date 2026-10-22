using System.Collections.Generic;

namespace PracticaProgramada1.Models
{
    public partial class Categoria
    {
        public int IdCategoria { get; set; }

        public string NombreCategoria { get; set; }

        public virtual ICollection<Producto> Productos { get; set; } = new HashSet<Producto>();
    }
}
