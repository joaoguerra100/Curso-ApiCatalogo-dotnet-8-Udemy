using System.ComponentModel.DataAnnotations;

namespace APICatalogo.DTOs
{
    public class RegisterModelDTO
    {
        [Required(ErrorMessage = "Nome de usuario e requerido")]
        public string? UserName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
        
        [Required(ErrorMessage = "Senha de usuario e requerido")]
        public string? Password { get; set; }
    }
}