using APICatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories.Interface
{
    public interface IProdutoRepository 
    {
        Task<PagedList<Produto>> GetProdutosPaginadoAsync(ProdutosParameters produtosParameters);
        Task<PagedList<Produto>> GetProdutosFiltroPrecoAsync(ProdutosFiltroPreco produtosFiltroParams);
        Task<IEnumerable<Produto>> GetProdutosAsync();
        Task<Produto> GetProdutoAsync(int id);
        public Produto Create(Produto produto);
        public bool Update(Produto produto);
        public bool Delete(int id);
    }
}