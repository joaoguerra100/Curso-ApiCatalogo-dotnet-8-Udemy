using APICatalogo.Context;
using APICatalogo.Repositories.Interface;
using APICatalogo.Repositories.Mocks;

namespace APICatalogo.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private IProdutoRepository? _produtoRepo;

        private ICategoriaRepository? _categoriaRepo;

        public AppDbContext _context;
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IProdutoRepository ProdutoRepository
        {
            get
            {
                //Nesta linha esta usando o banco de dados
                //return _produtoRepo = _produtoRepo ?? new ProdutoRepository(_context);
                return _produtoRepo = _produtoRepo ?? new ProdutoRepositoryMock();
            }
        }

        public ICategoriaRepository CategoriaRepository
        {
            get
            {
                //Nesta linha esta usando o banco de dados
                //return _categoriaRepo = _categoriaRepo ?? new CategoriaRepository(_context);
                return _categoriaRepo = _categoriaRepo ?? new CategoriaRepositoryMock();
            }
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}