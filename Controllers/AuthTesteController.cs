using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthTesteController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        private static List<ApplicationUser> _mockUsers = new();
        private static List<string> _mockRoles = new();

        public AuthTesteController(IConfiguration configuration, ITokenService tokenService)
        {
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpPost]
        [Route("login")]
        public IActionResult LoginMockado([FromBody] LoginModelDTO model)
        {
            var user = _mockUsers.FirstOrDefault(u => u.UserName == model.UserName);

            if (user != null && user.PlainPassword == model.Password)
            {
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                var token = _tokenService.GenerateAccessToken(authClaims, _configuration);
                var refreshToken = _tokenService.GenerateRefreshToken();

                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"], out int refeshTokenValidityInMinutes);

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(refeshTokenValidityInMinutes);

                return Ok(new
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo
                });
            }

            return Unauthorized();
        }

        [HttpPost]
        [Route("register")]
        public IActionResult RegisterMockado([FromBody] RegisterModelDTO model)
        {
            // Verifica se o nome de usuario existe
            var userExists = _mockUsers.FirstOrDefault(u => u.UserName == model.UserName);
            // se for diferente de nulo ele ja existe
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDTO { Status = "Error", Message = "User already exists!" });
            }

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName,
                PlainPassword = model.Password
            };
            // cria o novo usuario
            _mockUsers.Add(user);
            // caso seja criado com sucesso ele retorna ok
            return Ok(new ResponseDTO { Status = "Success", Message = "User Created success" });
        }

        [HttpPost]
        [Route("refresh-token")]
        public IActionResult RefreshTokenMockado([FromBody] TokenModelDTO tokenModelDTO)
        {
            if (tokenModelDTO == null)
            {
                return BadRequest("Invalid client request");
            }

            string? accessToken = tokenModelDTO.AccessToken;
            string? refreshToken = tokenModelDTO.RefreshToken;

            var principal = _tokenService.GetPrincialFromExpiredToken(accessToken!, _configuration);

            if (principal == null)
            {
                return BadRequest("Invalid access token/refresh token");
            }

            string userName = principal.Identity?.Name ?? "";

            var user = _mockUsers.FirstOrDefault(u => u.UserName == userName);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid access token/refresh token");
            }

            var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims.ToList(), _configuration);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(
                int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"], out int minutes) ? minutes : 30
            );

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken
            });
        }

        [HttpPost]
        [Route("revoke/{username}")]
        [Authorize(Policy = "ExclusiveOnly")]
        public IActionResult RevokeMockado(string username)
        {
            var user = _mockUsers.FirstOrDefault(u => u.UserName == username);

            if (user == null)
            {
                return BadRequest("Invalid user name");
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.MinValue;

            return NoContent();
        }

        [HttpPost]
        [Route("CreateRole")]
        [Authorize(Policy = "SuperAdminOnly")]
        public IActionResult CreateRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return BadRequest(new ResponseDTO { Status = "Error", Message = "Role name is required" });
            }

            var roleExists = _mockRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);

            if (!roleExists)
            {
                _mockRoles.Add(roleName);

                return Ok(new ResponseDTO
                {
                    Status = "Success",
                    Message = $"Role '{roleName}' added successfully (mock)"
                });
            }

            return BadRequest(new ResponseDTO
            {
                Status = "Error",
                Message = $"Role '{roleName}' already exists (mock)"
            });
        }

        [HttpPost]
        [Route("AddUserToRole")]
        [Authorize(Policy = "SuperAdminOnly")]
        public IActionResult AddUserToRole(string email, string roleName)
        {
            var user = _mockUsers.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return BadRequest(new ResponseDTO { Status = "Error", Message = "User not found" });
            }

            if (!_mockRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new ResponseDTO { Status = "Error", Message = "Role does not exist" });
            }

            if (user.Roles.Contains(roleName, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new ResponseDTO { Status = "Error", Message = "User already has this role" });
            }

            user.Roles.Add(roleName);

            return Ok(new ResponseDTO
            {
                Status = "Success",
                Message = $"User {user.Email} added to role {roleName} (mock)"
            });
        }
    }
}