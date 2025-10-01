using Microsoft.AspNetCore.Identity;

namespace APICatalogo.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }


        // Campos temporários para testes com dados mockados
        public string? PlainPassword { get; set; }
        public List<string> Roles { get; set; } = new();

    }
}