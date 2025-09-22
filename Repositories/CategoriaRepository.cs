using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Repositories.Interface;

namespace APICatalogo.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _context;

        public CategoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Categoria> GetCategorias()
        {
            var categorias = _context.Categorias.ToList();
            return categorias;
        }

        public Categoria GetCategoria(int id)
        {
            var categoria = _context.Categorias.FirstOrDefault(c => c.CategoriaId == id);
            return categoria;
        }

        public Categoria Create(Categoria categoria)
        {
            if (categoria == null)
            {
                throw new ArgumentException(nameof(categoria));
            }

            _context.Categorias.Add(categoria);
            _context.SaveChanges();

            return categoria;
        }

        public Categoria Update(Categoria categoria)
        {
            throw new NotImplementedException();
        }

        public Categoria Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}