using System.Collections.Generic;
using System.Data.Entity;
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
            // Validación extra: el código no se puede repetir.
            if (await _context.Productos.AnyAsync(p => p.Codigo == viewModel.Producto.Codigo))
            {
                ModelState.AddModelError("Producto.Codigo", "Ya existe un producto con ese código.");
            }

            if (!ModelState.IsValid)
            {
                return View(await BuildViewModelAsync(viewModel.Producto));
            }

            _context.Productos.Add(viewModel.Producto);
            await _context.SaveChangesAsync();

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

            // El código no se puede repetir en OTRO producto.
            if (await _context.Productos.AnyAsync(p => p.Codigo == viewModel.Producto.Codigo && p.Id != id))
            {
                ModelState.AddModelError("Producto.Codigo", "Ya existe otro producto con ese código.");
            }

            if (!ModelState.IsValid)
            {
                return View(await BuildViewModelAsync(viewModel.Producto));
            }

            var productoDb = await _context.Productos.FindAsync(id);
            if (productoDb == null)
            {
                return HttpNotFound();
            }

            // Reasignación manual de los campos editables.
            productoDb.Codigo = viewModel.Producto.Codigo;
            productoDb.Nombre = viewModel.Producto.Nombre;
            productoDb.IdCategoria = viewModel.Producto.IdCategoria;
            productoDb.Precio = viewModel.Producto.Precio;
            productoDb.UnidadMedida = viewModel.Producto.UnidadMedida;
            productoDb.Existencia = viewModel.Producto.Existencia;
            productoDb.Activo = viewModel.Producto.Activo;

            await _context.SaveChangesAsync();

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
