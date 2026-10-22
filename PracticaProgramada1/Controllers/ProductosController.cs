using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using PracticaProgramada1.Data;
using PracticaProgramada1.Models;
using PracticaProgramada1.ViewModels;

namespace PracticaProgramada1.Controllers
{
    public class ProductosController : Controller
    {
        private readonly TiendaContext _context = new TiendaContext();

        // READ: listado desde la base de datos, con filtro opcional por texto y categoría.
        public async Task<ActionResult> Index(string buscar, int? categoriaId)
        {
            var query = _context.Productos.Include(p => p.Categoria).AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var termino = buscar.Trim();
                query = query.Where(p => p.Nombre.Contains(termino)
                                         || p.Codigo.Contains(termino)
                                         || p.Categoria.NombreCategoria.Contains(termino));
            }

            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.IdCategoria == categoriaId.Value);
            }

            var viewModel = new ProductoIndexViewModel
            {
                Productos = await query.OrderBy(p => p.Nombre).ToListAsync(),
                Buscar = buscar,
                CategoriaId = categoriaId,
                Categorias = await ObtenerCategoriasAsync(categoriaId)
            };

            return View(viewModel);
        }

        // READ: detalle de un producto.
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
            {
                return HttpNotFound();
            }

            return View(producto);
        }

        public async Task<ActionResult> Create()
        {
            return View(await BuildViewModelAsync(new Producto()));
        }

        // CREATE: POST válido con Add y SaveChanges.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ProductoFormViewModel viewModel)
        {
            await ValidarProductoAsync(viewModel.Producto, null);

            if (!ModelState.IsValid)
            {
                return View(await BuildViewModelAsync(viewModel.Producto));
            }

            _context.Productos.Add(viewModel.Producto);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (EsCodigoDuplicado(ex))
            {
                // Otro envío guardó el mismo código entre la validación y el guardado.
                _context.Entry(viewModel.Producto).State = EntityState.Detached;
                ModelState.AddModelError("Producto.Codigo", "Ya existe un producto con ese código.");
                return View(await BuildViewModelAsync(viewModel.Producto));
            }

            TempData["SuccessMessage"] = "Producto creado correctamente.";
            return RedirectToAction("Details", new { id = viewModel.Producto.Id });
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return HttpNotFound();
            }

            return View(await BuildViewModelAsync(producto));
        }

        // UPDATE: carga, valida y guarda cambios.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, ProductoFormViewModel viewModel)
        {
            if (id != viewModel.Producto.Id)
            {
                return HttpNotFound();
            }

            var productoDb = await _context.Productos.FindAsync(id);
            if (productoDb == null)
            {
                return HttpNotFound();
            }

            await ValidarProductoAsync(viewModel.Producto, id);

            if (!ModelState.IsValid)
            {
                return View(await BuildViewModelAsync(viewModel.Producto));
            }

            // Reasignación manual de los campos editables.
            productoDb.Codigo = viewModel.Producto.Codigo;
            productoDb.Nombre = viewModel.Producto.Nombre;
            productoDb.IdCategoria = viewModel.Producto.IdCategoria;
            productoDb.Precio = viewModel.Producto.Precio;
            productoDb.UnidadMedida = viewModel.Producto.UnidadMedida;
            productoDb.Existencia = viewModel.Producto.Existencia;
            productoDb.Activo = viewModel.Producto.Activo;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (EsCodigoDuplicado(ex))
            {
                ModelState.AddModelError("Producto.Codigo", "Ya existe otro producto con ese código.");
                return View(await BuildViewModelAsync(viewModel.Producto));
            }

            TempData["SuccessMessage"] = "Producto actualizado correctamente.";
            return RedirectToAction("Details", new { id });
        }

        // DELETE (paso 1): pantalla de confirmación por GET.
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
            {
                return HttpNotFound();
            }

            return View(producto);
        }

        // DELETE (paso 2): borrado por POST con Remove y SaveChanges.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return RedirectToAction("Index");
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Eliminaste " + producto.Nombre + ".";
            return RedirectToAction("Index");
        }

        private async Task<ProductoFormViewModel> BuildViewModelAsync(Producto producto)
        {
            return new ProductoFormViewModel
            {
                Producto = producto,
                Categorias = await ObtenerCategoriasAsync(producto.IdCategoria)
            };
        }

        // Lista para los dropdowns (formulario y filtro).
        private async Task<List<SelectListItem>> ObtenerCategoriasAsync(int? seleccionada)
        {
            var categorias = await _context.Categorias.OrderBy(c => c.NombreCategoria).ToListAsync();

            return categorias.Select(c => new SelectListItem
            {
                Value = c.IdCategoria.ToString(),
                Text = c.NombreCategoria,
                Selected = c.IdCategoria == seleccionada
            }).ToList();
        }

        // Validaciones que necesitan la base de datos o el texto ya sin espacios.
        // idActual es null al crear; al editar, excluye al propio producto de la búsqueda de códigos.
        private async Task ValidarProductoAsync(Producto producto, int? idActual)
        {
            // Quita espacios al inicio y al final para que " P001 " cuente igual que "P001".
            producto.Codigo = producto.Codigo?.Trim();
            producto.Nombre = producto.Nombre?.Trim();

            if (ModelState.IsValidField("Producto.Nombre") && producto.Nombre != null && producto.Nombre.Length < 3)
            {
                ModelState.AddModelError("Producto.Nombre", "El nombre debe tener entre 3 y 80 caracteres.");
            }

            if (!string.IsNullOrEmpty(producto.Codigo)
                && await _context.Productos.AnyAsync(p => p.Codigo == producto.Codigo && p.Id != (idActual ?? 0)))
            {
                ModelState.AddModelError("Producto.Codigo", "Ya existe un producto con ese código.");
            }

            if (producto.IdCategoria > 0
                && !await _context.Categorias.AnyAsync(c => c.IdCategoria == producto.IdCategoria))
            {
                ModelState.AddModelError("Producto.IdCategoria", "La categoría seleccionada no existe.");
            }
        }

        // 2601 y 2627 son los errores de SQL Server por índice único repetido.
        private static bool EsCodigoDuplicado(DbUpdateException ex)
        {
            var sqlEx = ex.GetBaseException() as SqlException;
            return sqlEx != null && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
