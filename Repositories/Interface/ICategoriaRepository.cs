using APICatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories.Interface
{
    public interface ICategoriaRepository
    {
        Task<PagedList<Categoria>> GetCategoriaPaginadoAsync(CategoriasParameters categoriasParams);
        Task<PagedList<Categoria>> GetCategoriasFiltrNomeAsync(CategoriasFiltroNome categoriasParams);
        Task<IEnumerable<Categoria>> GetCategoriasAsync();
        Task <Categoria> GetCategoriaAsync(int id);
        Categoria Create(Categoria categoria);
        Categoria Update(Categoria categoria);
        Categoria Delete(int id);
    }
}