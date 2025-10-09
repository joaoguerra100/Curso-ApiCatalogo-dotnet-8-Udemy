using System.Text.Json;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories.Interface;

namespace APICatalogo.Repositories.Mocks
{
    public class CategoriaRepositoryMock : ICategoriaRepository
    {
        private readonly List<Categoria> _categorias;

        public CategoriaRepositoryMock()
        {
            var json = File.ReadAllText("FakeData/dados_mockados.json");
            var dados = JsonSerializer.Deserialize<Dictionary<string, List<Categoria>>>(json);
            _categorias = dados?["Categorias"] ?? new List<Categoria>();
        }

        public async Task<IEnumerable<Categoria>> GetCategoriasAsync()
        {
            return await Task.FromResult(_categorias);
        }

        public async Task<Categoria> GetCategoriaAsync(int id)
        {
            var categoria = _categorias.FirstOrDefault(c => c.CategoriaId == id);
            return await Task.FromResult(categoria);
        }

        public async Task<PagedList<Categoria>> GetCategoriaPaginadoAsync(CategoriasParameters categoriasParams)
        {
            var categoriasOrdenadas = _categorias.OrderBy(c => c.CategoriaId).AsQueryable();
            var paged = PagedList<Categoria>.ToPagedList(categoriasOrdenadas, categoriasParams.PageNumber, categoriasParams.PageSize);
            return await Task.FromResult(paged);
        }

        public async Task<PagedList<Categoria>> GetCategoriasFiltrNomeAsync(CategoriasFiltroNome categoriasParams)
        {
            var categorias = _categorias.AsQueryable();

            if (!string.IsNullOrEmpty(categoriasParams.Nome))
            {
                categorias = categorias.Where(c => c.Nome.Contains(categoriasParams.Nome));
            }

            return await PagedList<Categoria>.ToPagedListAsync(categorias, categoriasParams.PageNumber, categoriasParams.PageSize);
        }

        public Categoria Create(Categoria categoria)
        {
            categoria.CategoriaId = _categorias.Max(c => c.CategoriaId) + 1;
            _categorias.Add(categoria);
            SalvarDados();
            return categoria;
        }

        public Categoria Update(Categoria categoria)
        {
            var index = _categorias.FindIndex(c => c.CategoriaId == categoria.CategoriaId);
            if (index == -1) throw new ArgumentException("Categoria não encontrada");
            _categorias[index] = categoria;
            SalvarDados();
            return categoria;
        }

        public Categoria Delete(int id)
        {
            var categoria = _categorias.FirstOrDefault(c => c.CategoriaId == id);
            if (categoria == null) throw new ArgumentException("Categoria não encontrada");
            _categorias.Remove(categoria);
            SalvarDados();
            return categoria;
        }

        private void SalvarDados()
        {
            // Carrega o conteúdo atual do JSON
            var jsonAtual = File.ReadAllText("FakeData/dados_mockados.json");
            var dados = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonAtual);

            // Atualiza apenas a parte de Categorias
            dados["Categorias"] = _categorias;

            // Reescreve o JSON mantendo produtos
            var jsonNovo = JsonSerializer.Serialize(dados, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("FakeData/dados_mockados.json", jsonNovo);
        }
    }
}