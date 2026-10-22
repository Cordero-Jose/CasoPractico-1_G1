using System.ComponentModel.DataAnnotations;

namespace PracticaProgramada1.Models
{
    // Las validaciones viven en clases "Metadata" aparte y se enlazan con [MetadataType], así el modelo principal queda limpio.

    [MetadataType(typeof(CategoriaMetadata))]
    public partial class Categoria
    {
    }

    [MetadataType(typeof(ProductoMetadata))]
    public partial class Producto
    {
    }

    public class CategoriaMetadata
    {
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede pasar de 50 caracteres.")]
        [Display(Name = "Categoría")]
        public string NombreCategoria { get; set; }
    }

    public class ProductoMetadata
    {
        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(12, ErrorMessage = "El código no puede pasar de 12 caracteres.")]
        [Display(Name = "Código")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 80 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Seleccione una categoría.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoría.")]
        [Display(Name = "Categoría")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(typeof(decimal), "0.01", "99999999", ErrorMessage = "El precio debe ser mayor que cero.")]
        public decimal Precio { get; set; }

        [Display(Name = "Unidad de medida")]
        public UnidadMedida UnidadMedida { get; set; }

        [Required(ErrorMessage = "La existencia es obligatoria.")]
        [Range(typeof(decimal), "0", "9999", ErrorMessage = "La existencia debe estar entre 0 y 9999.")]
        public decimal Existencia { get; set; }

        public bool Activo { get; set; }
    }
}
