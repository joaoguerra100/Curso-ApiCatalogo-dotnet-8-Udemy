using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories.Interface;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace APICatalogo.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")] // Faz o metodo de resposta ser o padrao o json
    [ApiConventionType(typeof(DefaultApiConventions))] // Adiciona os Status a todos os metodos actions como 200,404, 500 etc...
    public class ProdutosController : ControllerBase
    {
        private readonly IUnitOfWork _uof;
        private readonly IMapper _mapper;

        public ProdutosController(IUnitOfWork uof, IMapper mapper)
        {
            _uof = uof;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtem uma lista de produtos filtrados pelo preço
        /// </summary>
        /// <param name="produtosFiltroParams"></param>
        /// <returns> Uma paginação de produtos filtrados pelo preço</returns>
        [HttpGet("filter/preco/paginacao")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosFilterPreco([FromQuery] ProdutosFiltroPreco produtosFiltroParams)
        {
            var produtos = await _uof.ProdutoRepository.GetProdutosFiltroPrecoAsync(produtosFiltroParams);

            var metadata = new
            {
                produtos.TotalCount,
                produtos.PageSize,
                produtos.CurrentPage,
                produtos.TotalPages,
                produtos.HasNext,
                produtos.HasPrevious,
            };

            Response.Headers.Append("X-Paginacao", JsonConvert.SerializeObject(metadata));

            var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

            return Ok(produtosDto);
        }

        /// <summary>
        /// Uma lista de produtos Paginadas
        /// </summary>
        /// <param name="produtosParams"></param>
        /// <returns>Uma paginação de produtos</returns>
        [HttpGet("Paginacao")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> Get([FromQuery] ProdutosParameters produtosParams)
        {
            var produtos = await _uof.ProdutoRepository.GetProdutosPaginadoAsync(produtosParams);

            var metadata = new
            {
                produtos.TotalCount,
                produtos.PageSize,
                produtos.CurrentPage,
                produtos.TotalPages,
                produtos.HasNext,
                produtos.HasPrevious,
            };

            Response.Headers.Append("X-Paginacao", JsonConvert.SerializeObject(metadata));

            var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

            return Ok(produtosDto);
        }

        /// <summary>
        /// Exibe uma relação dos produtos
        /// </summary>
        /// <returns>Retorna uma lista de objetos Produto</returns>
        [HttpGet]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> Get()
        {
            try
            {
                var produtos = await _uof.ProdutoRepository.GetProdutosAsync();
                if (produtos is null)
                {
                    return NotFound("Produtos nao encontrados");
                }

                //var destino = _mapper.Map<Destino>(origem);
                var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);
                return Ok(produtosDto);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Obtem o produto pelo seu identificador produtoId
        /// </summary>
        /// <param name="id"> Codigo do produto</param>
        /// <returns>Um objeto Produto</returns>
        [HttpGet("{id:int}", Name = "ObterProduto")]
        public async Task<ActionResult<ProdutoDTO>> Get(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID de produto invalido");
            }

            var produto = await _uof.ProdutoRepository.GetProdutoAsync(id);
            if (produto == null)
            {
                return NotFound($"Nenhum produto com este Id={id}");
            }

            var produtoDto = _mapper.Map<ProdutoDTO>(produto);
            return Ok(produtoDto);
        }

        /// <summary>
        /// Inclui uma novo Produto
        /// </summary>
        /// <remarks>
        /// Exemplo de request:
        ///     POST api/Produto
        ///     {
        ///         "categoriaId":1,
        ///         "nome":"categoria1",
        ///         "descricao":"categoria1Desc",
        ///         "preco":"100",
        ///         "imagemUrl": "http://teste.net/1.jpg",
        ///         "categoriaId": "1"
        ///     }
        /// Retorna um objeto Produto incluído.
        /// </remarks>
        /// <param name="produtoDto">Objeto DTO da Produto</param>
        /// <returns>O objeto Produto incluído</returns>
        [HttpPost]
        public async Task<ActionResult<ProdutoDTO>> Post(ProdutoDTO produtoDto)
        {
            if (produtoDto == null)
            {
                return BadRequest("Dados invalidos");
            }

            var produto = _mapper.Map<Produto>(produtoDto);

            var novoProduto = _uof.ProdutoRepository.Create(produto);
            await _uof.CommitAsync();

            var novoProdutoDto = _mapper.Map<ProdutoDTO>(novoProduto);

            return new CreatedAtRouteResult("ObterProduto", new { id = novoProdutoDto.ProdutoId }, novoProdutoDto);
        }

        [HttpPatch("{id}/UpdatePartial")]
        public async Task<ActionResult<ProdutoDTOUpdateResponse>> Patch(int id, JsonPatchDocument<ProdutoDTOUpdateRequest> patchProdutoDTO)
        {
            if (patchProdutoDTO == null || id <= 0)
            {
                return BadRequest();
            }

            var produto = await _uof.ProdutoRepository.GetProdutoAsync(id);

            if (produto == null)
            {
                return NotFound();
            }

            var produtoUpdateRequest = _mapper.Map<ProdutoDTOUpdateRequest>(produto);

            patchProdutoDTO.ApplyTo(produtoUpdateRequest, ModelState);

            if (!ModelState.IsValid || !TryValidateModel(produtoUpdateRequest))
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(produtoUpdateRequest, produto);

            _uof.ProdutoRepository.Update(produto);
            await _uof.CommitAsync();

            return Ok(_mapper.Map<ProdutoDTOUpdateResponse>(produto));
        }

        /// <summary>
        /// Atualiza uma novo Produto
        /// </summary>
        /// <remarks>
        /// Exemplo de request:
        ///     Put api/Produto
        ///     {
        ///         "categoriaId":1,
        ///         "nome":"categoria1",
        ///         "descricao":"categoria1Desc",
        ///         "preco":"100",
        ///         "imagemUrl": "http://teste.net/1.jpg",
        ///         "categoriaId": "1"
        ///     }
        /// Retorna um objeto Produto Alterado.
        /// </remarks>
        /// <param name="produtoDto">Objeto DTO da Produto</param>
        /// <param name="id">id de Produto</param>
        /// <returns>O objeto Produto Alterado</returns>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProdutoDTO>> Put(int id, ProdutoDTO produtoDto)
        {
            if (id != produtoDto.ProdutoId)
            {
                return BadRequest("Dados invalidos");
            }

            var produto = _mapper.Map<Produto>(produtoDto);

            bool produtoAtualizado = _uof.ProdutoRepository.Update(produto);
            await _uof.CommitAsync();

            if (produtoAtualizado)
            {

                var produtoFinal = await _uof.ProdutoRepository.GetProdutoAsync(id);
                var produtoAtualizadoDto = _mapper.Map<ProdutoDTO>(produtoFinal);
                return Ok(produtoAtualizadoDto);
            }
            else
            {
                return StatusCode(500, $"Falha ao atualizar o produto com id: {id}");
            }
        }

        /// <summary>
        /// Deleta um Produto existente
        /// </summary>
        /// <remarks>
        /// Retorna Status code 200.
        /// </remarks>
        /// <param name="id">id de Produto</param>
        /// <returns>Retorna Status code 200.</returns>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ProdutoDTO>> Delete(int id)
        {
            var produto = await _uof.ProdutoRepository.GetProdutoAsync(id);
            if (produto == null)
            {
                return NotFound("Produto nao encontrado");
            }
            bool produtoDeletado = _uof.ProdutoRepository.Delete(id);
            await _uof.CommitAsync();

            if (produtoDeletado)
            {
                var produtoFinal = await _uof.ProdutoRepository.GetProdutoAsync(id);
                var produtoDeletadoDto = _mapper.Map<ProdutoDTO>(produtoFinal);
                return Ok(produtoDeletadoDto);
            }
            else
            {
                return StatusCode(500, $"Falha ao Excluir o produto com id: {id}");
            }
        }
    }
}