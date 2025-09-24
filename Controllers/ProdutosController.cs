using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Repositories.Interface;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly IUnitOfWork _uof;
        private readonly IMapper _mapper;

        public ProdutosController(IUnitOfWork uof, IMapper mapper)
        {
            _uof = uof;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ProdutoDTO>> Get()
        {
            var produtos = _uof.ProdutoRepository.GetProdutos().ToList();
            if (produtos is null)
            {
                return NotFound("Produtos nao encontrados");
            }

            //var destino = _mapper.Map<Destino>(origem);
            var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);
            return Ok(produtosDto);
        }

        [HttpGet("{id:int}", Name = "ObterProduto")]
        public ActionResult<ProdutoDTO> Get(int id)
        {
            var produto = _uof.ProdutoRepository.GetProduto(id);
            if (produto is null)
            {
                return NotFound($"Nenhum produto com este Id={id}");
            }

            var produtoDto = _mapper.Map<ProdutoDTO>(produto);
            return Ok(produtoDto);
        }

        [HttpPost]
        public ActionResult<ProdutoDTO> Post(ProdutoDTO produtoDto)
        {
            if (produtoDto is null)
            {
                return BadRequest("Dados invalidos");
            }

            var produto = _mapper.Map<Produto>(produtoDto);

            var novoProduto = _uof.ProdutoRepository.Create(produto);
            _uof.Commit();

            var novoProdutoDto = _mapper.Map<ProdutoDTO>(novoProduto);

            return new CreatedAtRouteResult("ObterProduto", new { id = novoProdutoDto.ProdutoId }, novoProdutoDto);
        }

        [HttpPatch("{id}/UpdatePartial")]
        public ActionResult<ProdutoDTOUpdateResponse> Patch(int id, JsonPatchDocument<ProdutoDTOUpdateRequest> patchProdutoDTO)
        {
            if (patchProdutoDTO == null || id <= 0)
            {
                return BadRequest();
            }

            var produto = _uof.ProdutoRepository.GetProduto(id);

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
            _uof.Commit();

            return Ok(_mapper.Map<ProdutoDTOUpdateResponse>(produto));
        }

        [HttpPut("{id:int}")]
        public ActionResult<ProdutoDTO> Put(int id, ProdutoDTO produtoDto)
        {
            if (id != produtoDto.ProdutoId)
            {
                return BadRequest("Dados invalidos");
            }

            var produto = _mapper.Map<Produto>(produtoDto);

            bool produtoAtualizado = _uof.ProdutoRepository.Update(produto);
            _uof.Commit();
            if (produtoAtualizado)
            {
                var produtoAtualizadoDto = _mapper.Map<ProdutoDTO>(produtoAtualizado);
                return Ok(produtoAtualizadoDto);
            }
            else
            {
                return StatusCode(500, $"Falha ao atualizar o produto com id: {id}");
            }
        }

        [HttpDelete("{id:int}")]
        public ActionResult<ProdutoDTO> Delete(int id)
        {
            var produto = _uof.ProdutoRepository.GetProduto(id);
            if (produto == null)
            {
                return NotFound("Produto nao encontrado");
            }
            bool produtoDeletado = _uof.ProdutoRepository.Delete(id);
            _uof.Commit();

            if (produtoDeletado)
            {
                var produtoDeletadoDto = _mapper.Map<ProdutoDTO>(produtoDeletado);
                return Ok(produtoDeletadoDto);
            }
            else
            {
                return StatusCode(500, $"Falha ao Excluir o produto com id: {id}");
            }
        }
    }
}