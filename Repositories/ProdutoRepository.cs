using APICatalogo.Context;
using APICatalogo.Models;
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

        public Produto GetProduto(int id)
        {
            var produto = _context.Produtos.AsNoTracking().FirstOrDefault(p => p.ProdutoId == id);
            if (produto == null)
            {
                throw new InvalidOperationException("Produto e null");
            }
            return produto;
        }

        public IQueryable<Produto> GetProdutos()
        {
            var produtos = _context.Produtos.AsNoTracking();
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