using APICatalogo.DTOs;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Filters;
using APICatalogo.Pagination;
using APICatalogo.Repositories.Interface;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;

namespace APICatalogo.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [EnableRateLimiting("fixedwindow")]
    [Produces("application/json")] // Faz o metodo de resposta ser o padrao o json
    [ApiConventionType(typeof(DefaultApiConventions))] // Adiciona os Status a todos os metodos actions como 200,404, 500 etc...
    public class CategoriasController : ControllerBase
    {
        private readonly IUnitOfWork _uof;
        private readonly ILogger _logger;

        public CategoriasController(IUnitOfWork uof, ILogger<CategoriasController> logger)
        {
            _uof = uof;
            _logger = logger;
        }

        /// <summary>
        /// Obtem uma lista de categorias filtrados pelo nome
        /// </summary>
        /// <param name="categoriasFiltro"></param>
        /// <returns> Uma paginação de categorias filtrados pelo nome</returns>
        [HttpGet("filter/nome/paginacao")]
        public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetCategoriasFiltradas([FromQuery] CategoriasFiltroNome categoriasFiltro)
        {
            var categorias = await _uof.CategoriaRepository.GetCategoriasFiltrNomeAsync(categoriasFiltro);

            var metadata = new
            {
                categorias.TotalCount,
                categorias.PageSize,
                categorias.CurrentPage,
                categorias.TotalPages,
                categorias.HasNext,
                categorias.HasPrevious,
            };

            Response.Headers.Append("X-Paginacao", JsonConvert.SerializeObject(metadata));

            var categoriasDto = categorias.ToCategoriaDTOList();

            return Ok(categoriasDto);

        }

        /// <summary>
        /// Uma lista de categorias Paginadas
        /// </summary>
        /// <param name="categoriasParameters"></param>
        /// <returns>Uma paginação de categorias</returns>
        [HttpGet("Paginacao")]
        public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get([FromQuery] CategoriasParameters categoriasParameters)
        {
            var categorias = await _uof.CategoriaRepository.GetCategoriaPaginadoAsync(categoriasParameters);

            var metadata = new
            {
                categorias.TotalCount,
                categorias.PageSize,
                categorias.CurrentPage,
                categorias.TotalPages,
                categorias.HasNext,
                categorias.HasPrevious,
            };

            Response.Headers.Append("X-Paginacao", JsonConvert.SerializeObject(metadata));

            var categoriasDto = categorias.ToCategoriaDTOList();

            return Ok(categoriasDto);
        }

        /// <summary>
        /// Obtem uma lista de objetos Categoria
        /// </summary>
        /// <returns> Uma lista de objetos Categoria </returns>
        [HttpGet]
        [Authorize]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        [DisableRateLimiting]// Desabilita o limite de Requisiçoes que se pode fazer
        public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get()

        {
            var categorias = await _uof.CategoriaRepository.GetCategoriasAsync();
            if (categorias == null)
            {
                return NotFound("Não existem categorias");
            }

            var categoriasDto = categorias.ToCategoriaDTOList();

            return Ok(categoriasDto);
        }

        /// <summary>
        /// Obtem uma Categoria pelo seu ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Objetos Categoria</returns>
        [HttpGet("{id:int}", Name = "ObterCategoria")]
        public async Task<ActionResult<CategoriaDTO>> Get(int id)
        {
            var categoria = await _uof.CategoriaRepository.GetCategoriaAsync(id);

            _logger.LogInformation($"======================Get api/categorias/{id}==============");

            if (categoria == null)
            {
                _logger.LogInformation($"======================Get api/categorias/{id} NOT FOUND==============");
                return NotFound($"Categoria com id={id} nao encontrada");
            }

            var categoriaDto = categoria.ToCategoriaDTO();

            return Ok(categoriaDto);
        }

        /// <summary>
        /// Inclui uma nova Categoria
        /// </summary>
        /// <remarks>
        /// Exemplo de request:
        ///     POST api/categorias
        ///     {
        ///         "categoriaId":1,
        ///         "nome":"categoria1",
        ///         "imagemUrl": "http://teste.net/1.jpg"
        ///     }
        /// Retorna um objeto Categoria incluído.
        /// </remarks>
        /// <param name="categoriaDto">Objeto DTO da categoria</param>
        /// <returns>O objeto Categoria incluída</returns>
        [HttpPost]
        public async Task<ActionResult<CategoriaDTO>> Post(CategoriaDTO categoriaDto)
        {
            if (categoriaDto is null)
            {
                return BadRequest("Dados invalidos");
            }

            var categoria = categoriaDto.ToCategoria();

            var categoriaCriada = _uof.CategoriaRepository.Create(categoria!);
            await _uof.CommitAsync();

            var novaCategoriaDto = categoriaCriada.ToCategoriaDTO();

            return new CreatedAtRouteResult("ObterCategoria", new { id = novaCategoriaDto!.CategoriaId }, novaCategoriaDto);
        }

        /// <summary>
        /// Altera Categoria existente
        /// </summary>
        /// <remarks>
        /// Exemplo de request:
        ///     POST api/categorias
        ///     {
        ///         "nome":"categoria1",
        ///         "imagemUrl": "http://teste.net/1.jpg"
        ///     }
        /// Retorna um objeto Categoria alterado.
        /// </remarks>
        /// <param name="categoriaDto">Objeto DTO da categoria</param>
        /// <param name="id">Id da categoria</param>
        /// <returns>O objeto Categoria Alterado</returns>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<CategoriaDTO>> Put(int id, CategoriaDTO categoriaDto)
        {
            if (id != categoriaDto.CategoriaId)
            {
                return BadRequest("Dados invalidos");
            }

            var categoria = categoriaDto.ToCategoria();

            var categoriaAtualizada = _uof.CategoriaRepository.Update(categoria!);
            await _uof.CommitAsync();

            var categoriaAtualizadaDto = categoriaAtualizada.ToCategoriaDTO();

            return Ok(categoriaAtualizadaDto);
        }

        /// <summary>
        /// Deleta Categoria existente
        /// </summary>
        /// <remarks>
        /// Retorna Status code 200.
        /// </remarks>
        /// <param name="id">id da categoria</param>
        /// <returns>Retorna Status code 200.</returns>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<CategoriaDTO>> Delete(int id)
        {
            var categoria = await _uof.CategoriaRepository.GetCategoriaAsync(id);

            if (categoria == null)
            {
                return NotFound($"Caregoria com id={id} não encontrada.");
            }

            var categoriaExcluida = _uof.CategoriaRepository.Delete(id);
            await _uof.CommitAsync();

            var categoriaExcluidaDto = categoriaExcluida.ToCategoriaDTO();

            return Ok(categoriaExcluidaDto);
        }
    }
}