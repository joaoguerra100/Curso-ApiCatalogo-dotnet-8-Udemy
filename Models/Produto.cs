using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using APICatalogo.Validations;

namespace APICatalogo.Models
{
    public class Produto : IValidatableObject
    {
        [Key]
        public int ProdutoId { get; set; }

        [Required(ErrorMessage = "O nome e obrigatorio")]
        [StringLength(20, ErrorMessage = "O nome deve ter entre 5 e 20 caracteres", MinimumLength = 5)]
        [PrimeiraLetraMaiuscula]
        public string? Nome { get; set; }

        [Required(ErrorMessage ="A Descricao e obrigatorio")]
        [StringLength(10, ErrorMessage ="A descriçao deve ter no maximo {10} caractere")]
        public string? Descricao { get; set; }

        [Required(ErrorMessage ="O Preco e obrigatorio")]
        [Range(1,10000, ErrorMessage = "O preço deve estar entre 1 a 10000")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage ="A ImagemUrl e obrigatorio")]
        [StringLength(300, MinimumLength = 10)]
        public string? ImagemUrl { get; set; }

        
        public float Estoque { get; set; }
        public DateTime DataCadastro { get; set; }

        public int CategoriaId { get; set; }

        [JsonIgnore]
        public Categoria? Categoria { get; set; }

        //Regras de negocio para produtos
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //Verifica se a string nao esta vazia
            if (!string.IsNullOrEmpty(this.Nome))
            {
                //pega a primeira letra de nome
                var primeiraLetra = this.Nome[0].ToString();
                // verifica se ela esta maiuscula
                if (primeiraLetra != primeiraLetra.ToUpper())
                {
                    yield return new ValidationResult("A primeira letra do produto deve ser maiuscula", new[] { nameof(this.Nome) });
                }
            }
            if (this.Estoque <= 0)
            {
                yield return new ValidationResult("O estoque deve ser maior que zero", new[] { nameof(this.Estoque) });
            }
        }
    }
}