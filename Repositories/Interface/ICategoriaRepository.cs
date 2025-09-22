using APICatalogo.Models;

namespace APICatalogo.Repositories.Interface
{
    public interface ICategoriaRepository
    {
        public IEnumerable<Categoria> GetCategorias();
        public Categoria GetCategoria(int id);
        public Categoria Create(Categoria categoria);
        public Categoria Update(Categoria categoria);
        public Categoria Delete(int id);
    }
}