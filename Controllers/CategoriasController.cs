using APICatalogo.DTOs;
using APICatalogo.Filters;
using APICatalogo.Models;
using APICatalogo.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly IUnitOfWork _uof;
        private readonly ILogger _logger;

        public CategoriasController(IUnitOfWork uof, ILogger<CategoriasController> logger)
        {
            _uof = uof;
            _logger = logger;
        }

        /* [HttpGet("produtos")]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategoriasProdutos()
        {
            _logger.LogInformation("======================Get api/categorias/produtos==============");

            var categoria = await _reporitory.Categorias.Include(p => p.Produtos).AsNoTracking().ToListAsync();
            return categoria;
        } */

        [HttpGet]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        public ActionResult<IEnumerable<CategoriaDTO>> Get()
        {
            var categorias = _uof.CategoriaRepository.GetCategorias();

            var categoriasDto = new List<CategoriaDTO>();
            foreach (var categoria in categorias)
            {
                var categoriaDto = new CategoriaDTO
                {
                    CategoriaId = categoria.CategoriaId,
                    Nome = categoria.Nome,
                    ImagemURL = categoria.ImagemURL
                };
                categoriasDto.Add(categoriaDto);
            }
            return Ok(categoriasDto);
        }

        [HttpGet("{id:int}", Name = "ObterCategoria")]
        public ActionResult<CategoriaDTO> Get(int id)
        {
            var categoria = _uof.CategoriaRepository.GetCategoria(id);

            _logger.LogInformation($"======================Get api/categorias/{id}==============");

            if (categoria == null)
            {
                _logger.LogInformation($"======================Get api/categorias/{id} NOT FOUND==============");
                return NotFound($"Categoria com id={id} nao encontrada");
            }

            var categoriaDto = new CategoriaDTO()
            {
                CategoriaId = categoria.CategoriaId,
                Nome = categoria.Nome,
                ImagemURL = categoria.ImagemURL
            };

            return Ok(categoriaDto);
        }

        [HttpPost]
        public ActionResult<CategoriaDTO> Post(CategoriaDTO categoriaDto)
        {
            if (categoriaDto is null)
            {
                return BadRequest("Dados invalidos");
            }

            var categoria = new Categoria()
            {
                CategoriaId = categoriaDto.CategoriaId,
                Nome = categoriaDto.Nome,
                ImagemURL = categoriaDto.ImagemURL
            };

            var categoriaCriada = _uof.CategoriaRepository.Create(categoria);
            _uof.Commit();

            var novaCategoriaDto = new CategoriaDTO()
            {
                CategoriaId = categoriaCriada.CategoriaId,
                Nome = categoriaCriada.Nome,
                ImagemURL = categoriaCriada.ImagemURL
            };

            return new CreatedAtRouteResult("ObterCategoria", new { id = novaCategoriaDto.CategoriaId }, novaCategoriaDto);
        }

        [HttpPut("{id:int}")]
        public ActionResult<CategoriaDTO> Put(int id, CategoriaDTO categoriaDto)
        {
            if (id != categoriaDto.CategoriaId)
            {
                return BadRequest("Dados invalidos");
            }

            var categoria = new Categoria()
            {
                CategoriaId = categoriaDto.CategoriaId,
                Nome = categoriaDto.Nome,
                ImagemURL = categoriaDto.ImagemURL
            };

            var categoriaAtualizada = _uof.CategoriaRepository.Update(categoria);
            _uof.Commit();

            var categoriaAtualizadaDto = new CategoriaDTO()
            {
                CategoriaId = categoriaAtualizada.CategoriaId,
                Nome = categoriaAtualizada.Nome,
                ImagemURL = categoriaAtualizada.ImagemURL
            };

            return Ok(categoriaAtualizadaDto);
        }

        [HttpDelete("{id:int}")]
        public ActionResult<CategoriaDTO> Delete(int id)
        {
            var categoria = _uof.CategoriaRepository.GetCategoria(id);

            if (categoria == null)
            {
                return NotFound($"Caregoria com id={id} não encontrada.");
            }

            var categoriaExcluida = _uof.CategoriaRepository.Delete(id);
            _uof.Commit();

            var categoriaExcluidaDto = new CategoriaDTO()
            {
                CategoriaId = categoriaExcluida.CategoriaId,
                Nome = categoriaExcluida.Nome,
                ImagemURL = categoriaExcluida.ImagemURL
            };

            return Ok(categoriaExcluidaDto);
        }
    }
}