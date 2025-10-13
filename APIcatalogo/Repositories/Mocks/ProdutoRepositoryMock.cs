using System.Text.Json;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories.Interface;

namespace APICatalogo.Repositories.Mocks
{
    public class ProdutoRepositoryMock : IProdutoRepository
    {
        private readonly List<Produto> _produtos;

        public ProdutoRepositoryMock()
        {
            var json = File.ReadAllText("FakeData/dados_mockados.json");
            var dados = JsonSerializer.Deserialize<Dictionary<string, List<Produto>>>(json);
            _produtos = dados?["Produtos"] ?? new List<Produto>();
        }

        public async Task<IEnumerable<Produto>> GetProdutosAsync()
        {
            return await Task.FromResult(_produtos);
        }

        public async Task<Produto> GetProdutoAsync(int id)
        {
            var produto = _produtos.FirstOrDefault(p => p.ProdutoId == id);
            return await Task.FromResult(produto!);
        }

        public async Task<PagedList<Produto>> GetProdutosPaginadoAsync(ProdutosParameters produtosParameters)
        {
            var produtosOrdenados = _produtos.OrderBy(p => p.ProdutoId).AsQueryable();
            var paged = PagedList<Produto>.ToPagedList(produtosOrdenados, produtosParameters.PageNumber, produtosParameters.PageSize);
            return await Task.FromResult(paged);
        }

        public async Task<PagedList<Produto>> GetProdutosFiltroPrecoAsync(ProdutosFiltroPreco produtosFiltroParams)
        {
            var produtos = _produtos.AsQueryable();

            if (produtosFiltroParams.Preco.HasValue && !string.IsNullOrEmpty(produtosFiltroParams.PrecoCriterio))
            {
                switch (produtosFiltroParams.PrecoCriterio.ToLower())
                {
                    case "maior":
                        produtos = produtos.Where(p => p.Preco > produtosFiltroParams.Preco.Value);
                        break;
                    case "menor":
                        produtos = produtos.Where(p => p.Preco < produtosFiltroParams.Preco.Value);
                        break;
                    case "igual":
                        produtos = produtos.Where(p => p.Preco == produtosFiltroParams.Preco.Value);
                        break;
                }
            }

            var produtosOrdenados = produtos.OrderBy(p => p.Preco);
            var paged = PagedList<Produto>.ToPagedList(produtosOrdenados, produtosFiltroParams.PageNumber, produtosFiltroParams.PageSize);
            return await Task.FromResult(paged);
        }

        public Produto Create(Produto produto)
        {
            if (produto == null)
                return null!;

            produto.ProdutoId = _produtos.Max(p => p.ProdutoId) + 1;
            _produtos.Add(produto);
            SalvarDados();
            return produto;
        }

        public bool Update(Produto produto)
        {
            if (produto == null)
                return false;

            var index = _produtos.FindIndex(p => p.ProdutoId == produto.ProdutoId);
            if (index == -1) return false;

            _produtos[index] = produto;
            SalvarDados();
            return true;
        }

        public bool Delete(int id)
        {
            var produto = _produtos.FirstOrDefault(p => p.ProdutoId == id);
            if (produto == null) return false;

            _produtos.Remove(produto);
            SalvarDados();
            return true;
        }

        private void SalvarDados()
        {
            // Carrega o conteúdo atual do JSON
            var jsonAtual = File.ReadAllText("FakeData/dados_mockados.json");
            var dados = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonAtual);

            // Atualiza apenas a parte de produtos
            dados!["Produtos"] = _produtos;

            // Reescreve o JSON mantendo categorias
            var jsonNovo = JsonSerializer.Serialize(dados, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("FakeData/dados_mockados.json", jsonNovo);
        }
    }
}