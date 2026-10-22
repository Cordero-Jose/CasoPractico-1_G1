namespace PracticaProgramada1.Models
{
    public partial class Producto
    {
        public Producto()
        {
            Activo = true;
            UnidadMedida = UnidadMedida.Unidad;
        }

        public int Id { get; set; }

        public string Codigo { get; set; }

        public string Nombre { get; set; }

        public int IdCategoria { get; set; }

        public decimal Precio { get; set; }

        public UnidadMedida UnidadMedida { get; set; }

        // Admite fracciones (kilogramos y litros).
        public decimal Existencia { get; set; }

        public bool Activo { get; set; }

        public virtual Categoria Categoria { get; set; }
    }
}
