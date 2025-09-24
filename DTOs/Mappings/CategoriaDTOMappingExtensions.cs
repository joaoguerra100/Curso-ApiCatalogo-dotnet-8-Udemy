using APICatalogo.Models;

namespace APICatalogo.DTOs.Mappings
{
    public static class CategoriaDTOMappingExtensions
    {
        public static CategoriaDTO? ToCategoriaDTO(this Categoria categoria)
        {
            if (categoria == null)
                return null;

            return new CategoriaDTO
            {
                CategoriaId = categoria.CategoriaId,
                Nome = categoria.Nome,
                ImagemURL = categoria.ImagemURL
            };
        }

        public static Categoria? ToCategoria(this CategoriaDTO categoriaDto)
        {
            if (categoriaDto == null)
                return null;

            return new Categoria
            {
                CategoriaId = categoriaDto.CategoriaId,
                Nome = categoriaDto.Nome,
                ImagemURL = categoriaDto.ImagemURL
            };
        }

        public static IEnumerable<CategoriaDTO> ToCategoriaDTOList(this IEnumerable<Categoria> categorias)
        {
            if (categorias == null || !categorias.Any())
            {
                return new List<CategoriaDTO>();
            }

            return categorias.Select(categoria => new CategoriaDTO
            {
                CategoriaId = categoria.CategoriaId,
                Nome = categoria.Nome,
                ImagemURL = categoria.ImagemURL
            }).ToList();
        }
    }
}