using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {
        public readonly AppDbContext _context;

        public ProdutoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedList<Produto>> GetProdutosFiltroPrecoAsync(ProdutosFiltroPreco produtosFiltroParams)
        {
            var produtos = await GetProdutosAsync();

            if (produtosFiltroParams.Preco.HasValue && !string.IsNullOrEmpty(produtosFiltroParams.PrecoCriterio))
            {
                if (produtosFiltroParams.PrecoCriterio.Equals("maior", StringComparison.OrdinalIgnoreCase))
                {
                    produtos = produtos.Where(p => p.Preco > produtosFiltroParams.Preco.Value).OrderBy(p => p.Preco);
                }

                else if (produtosFiltroParams.PrecoCriterio.Equals("menor", StringComparison.OrdinalIgnoreCase))
                {
                    produtos = produtos.Where(p => p.Preco > produtosFiltroParams.Preco.Value).OrderBy(p => p.Preco);
                }

                else if (produtosFiltroParams.PrecoCriterio.Equals("igual", StringComparison.OrdinalIgnoreCase))
                {
                    produtos = produtos.Where(p => p.Preco > produtosFiltroParams.Preco.Value).OrderBy(p => p.Preco);
                }
            }

            var produtosFiltrados = await PagedList<Produto>.ToPagedListAsync(produtos.AsQueryable(), produtosFiltroParams.PageNumber, produtosFiltroParams.PageSize);

            return produtosFiltrados;
        }

        public async Task<PagedList<Produto>> GetProdutosPaginadoAsync(ProdutosParameters produtosParameters)
        {
            var produtos = await GetProdutosAsync();

            var produtosOrdenados = produtos.OrderBy(p => p.ProdutoId).AsQueryable();

            var resultado = await PagedList<Produto>.ToPagedListAsync(produtosOrdenados, produtosParameters.PageNumber, produtosParameters.PageSize);

            return resultado;
        }

        public async Task<Produto> GetProdutoAsync(int id)
        {
            var produto = await _context.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.ProdutoId == id);
            if (produto == null)
            {
                throw new InvalidOperationException("Produto e null");
            }
            return produto;
        }

        public async Task<IEnumerable<Produto>> GetProdutosAsync()
        {
            var produtos = await _context.Produtos.AsNoTracking().ToListAsync();
            return produtos;
        }

        public Produto Create(Produto produto)
        {
            if (produto == null)
            {
                throw new InvalidOperationException("Produto e null");
            }

            _context.Produtos.Add(produto);
            //_context.SaveChanges();
            return produto;
        }

        public bool Update(Produto produto)
        {
            if (produto == null)
            {
                throw new InvalidOperationException("Produto e null");
            }

            if (_context.Produtos.Any(p => p.ProdutoId == produto.ProdutoId))
            {
                _context.Produtos.Update(produto);
                //_context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool Delete(int id)
        {
            var produto = _context.Produtos.Find(id);

            if (produto != null)
            {
                _context.Produtos.Remove(produto);
                //_context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}