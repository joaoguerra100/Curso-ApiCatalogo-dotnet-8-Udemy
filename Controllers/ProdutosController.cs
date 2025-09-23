using APICatalogo.Models;
using APICatalogo.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly IUnitOfWork _uof;

        public ProdutosController(IUnitOfWork uof)
        {
            _uof = uof;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Produto>> Get()
        {
            var produtos = _uof.ProdutoRepository.GetProdutos().ToList();
            if (produtos is null)
            {
                return NotFound("Produtos nao encontrados");
            }
            return Ok(produtos);
        }

        [HttpGet("{id:int}", Name = "ObterProduto")]
        public ActionResult<Produto> Get(int id)
        {
            var produto = _uof.ProdutoRepository.GetProduto(id);
            if (produto is null)
            {
                return NotFound($"Nenhum produto com este Id={id}");
            }

            return Ok(produto);
        }

        [HttpPost]
        public ActionResult Post(Produto produto)
        {
            if (produto is null)
            {
                return BadRequest("Dados invalidos");
            }

            _uof.ProdutoRepository.Create(produto);
            _uof.Commit();

            return new CreatedAtRouteResult("ObterProduto", new { id = produto.ProdutoId }, produto);
        }

        [HttpPut("{id:int}")]
        public ActionResult Put(int id, Produto produto)
        {
            if (id != produto.ProdutoId)
            {
                return BadRequest("Dados invalidos");
            }

            bool atualizado = _uof.ProdutoRepository.Update(produto);
            _uof.Commit();
            if (atualizado)
            {
                return Ok(produto);
            }
            else
            {
                return StatusCode(500, $"Falha ao atualizar o produto com id: {id}");
            }
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            bool deletado = _uof.ProdutoRepository.Delete(id);
            _uof.Commit();

            if (deletado)
            {
                return Ok($"Produto de id: {id} foi excluido");
            }
            else
            {
                return StatusCode(500, $"Falha ao Excluir o produto com id: {id}");
            }
        }
    }
}