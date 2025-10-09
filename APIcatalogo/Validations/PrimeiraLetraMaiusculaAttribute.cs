using System.ComponentModel.DataAnnotations;

namespace APICatalogo.Validations
{
    public class PrimeiraLetraMaiusculaAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }

            // Pega a primeira letra da string no valor de array 0
            var primeiraLetra = value.ToString()[0].ToString();
            //Verifica se a primeira letra do nome e maiuscula
            if (primeiraLetra != primeiraLetra.ToUpper())
            {
                return new ValidationResult("A primeira letra do nome do produto deve ser maiuscula");
            }

            return ValidationResult.Success;
        }
    }
}