using System.ComponentModel.DataAnnotations;

namespace APICatalogo.DTOs
{
    public class LoginModelDTO
    {
        [Required(ErrorMessage = "Nome de usuario e requerido")]
        public string? UserName { get; set; }
        
        [Required(ErrorMessage = "Senha de usuario e requerido")]
        public string? Password { get; set; }
    }
}