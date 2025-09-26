using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _context;

        public CategoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedList<Categoria>> GetCategoriasFiltrNomeAsync(CategoriasFiltroNome categoriasParams)
        {
            var categorias = await GetCategoriasAsync();

            if (!string.IsNullOrEmpty(categoriasParams.Nome))
            {
                categorias = categorias.Where(c => c.Nome.Contains(categoriasParams.Nome));
            }

            var categoriasFiltradas = await PagedList<Categoria>.ToPagedListAsync(categorias.AsQueryable(), categoriasParams.PageNumber, categoriasParams.PageSize);

            return categoriasFiltradas;
        }

        public async Task<PagedList<Categoria>> GetCategoriaPaginadoAsync(CategoriasParameters categoriasParams)
        {
            var categorias = await GetCategoriasAsync();

            var categoriasOrdenadas = categorias.OrderBy(p => p.CategoriaId).AsQueryable();

            var resultado = await PagedList<Categoria>.ToPagedListAsync(categoriasOrdenadas,
                categoriasParams.PageNumber, categoriasParams.PageSize);

            return resultado;
        }

        public async Task<IEnumerable<Categoria>> GetCategoriasAsync()
        {
            var categorias = await _context.Categorias.AsNoTracking().ToListAsync();
            return categorias;
        }

        public async Task<Categoria> GetCategoriaAsync(int id)
        {
            var categoria = await _context.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.CategoriaId == id);
            return categoria;
        }

        public Categoria Create(Categoria categoria)
        {
            if (categoria == null)
            {
                throw new ArgumentException(nameof(categoria));
            }

            _context.Categorias.Add(categoria);
            //_context.SaveChanges();

            return categoria;
        }

        public Categoria Update(Categoria categoria)
        {
            if (categoria == null)
            {
                throw new ArgumentException(nameof(categoria));
            }

            _context.Entry(categoria).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            // _context.SaveChanges();

            return categoria;
        }

        public Categoria Delete(int id)
        {
            var categoria = _context.Categorias.Find(id);
            if (categoria == null)
            {
                throw new ArgumentException(nameof(categoria));
            }

            _context.Categorias.Remove(categoria);
            //_context.SaveChanges();

            return categoria;
        }
    }
}