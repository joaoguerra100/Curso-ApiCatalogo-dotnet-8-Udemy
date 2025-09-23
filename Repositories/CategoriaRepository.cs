using APICatalogo.Context;
using APICatalogo.Models;
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

        public IEnumerable<Categoria> GetCategorias()
        {
            var categorias = _context.Categorias.AsNoTracking().ToList();
            return categorias;
        }

        public Categoria GetCategoria(int id)
        {
            var categoria = _context.Categorias.AsNoTracking().FirstOrDefault(c => c.CategoriaId == id);
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