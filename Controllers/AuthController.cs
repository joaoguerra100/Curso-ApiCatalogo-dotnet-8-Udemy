using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Services.Interface;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")] // Faz o metodo de resposta ser o padrao o json
    [ApiConventionType(typeof(DefaultApiConventions))] // Adiciona os Status a todos os metodos actions como 200,404, 500 etc...
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(ITokenService tokenService,
                              UserManager<ApplicationUser> userManager,
                              RoleManager<IdentityRole> roleManager,
                              IConfiguration configuration,
                              ILogger logger)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Verifica as credenciais de um usuario
        /// </summary>
        /// <param name="model">Um objeto do tipo UsuarioDTo</param>
        /// <returns>Status 200 e o token para o cliente</returns>
        /// <remarks> Retorna o Status 200 e o token </remarks>
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModelDTO model)
        {
            //1: verifica se o nome do usuario existe
            var user = await _userManager.FindByNameAsync(model.UserName!);
            //2:verifica se o usuario nao e nulo e se a senha fornecida e valida
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password!))
            {
                //3: Obtem os perfis do usuario
                var userRoles = await _userManager.GetRolesAsync(user);
                //4: Cria uma lista de claims que serve para construir o token de acesso de autenticaçao
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim("id", user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
                //5:Para cada perfil se adiciona uma nova claim do tipo role
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }
                //6: Pegamos o token usando as clains
                var token = _tokenService.GenerateAccessToken(authClaims, _configuration);

                //7:Gera o toekn de atualizaçao
                var refreshToken = _tokenService.GenerateRefreshToken();

                // o operador _ se chama discard e usado quando voce nao esta interresado no retorno, aqui ta sendo usado para pegar quantos minutos para expirar o token
                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"], out int refeshTokenValidityInMinutes);

                user.RefreshToken = refreshToken;

                //8: adiciona os minutos de acordo com a data de agora
                user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(refeshTokenValidityInMinutes);

                await _userManager.UpdateAsync(user);

                //9: retorna um objeto json
                return Ok(new
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        /// <summary>
        /// Registra um novo usuarui
        /// </summary>
        /// <param name="model">Um objeto UsuarioDTO</param>
        /// <returns>Status 200</returns>
        /// <remarks> Retorna o Status 200</remarks>
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModelDTO model)
        {
            // Verifica se o nome de usuario existe
            var userExists = await _userManager.FindByNameAsync(model.UserName!);
            // se for diferente de nulo ele ja existe
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDTO { Status = "Error", Message = "User already exists!" });
            }

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName
            };
            // cria o novo usuario
            var result = await _userManager.CreateAsync(user, model.Password!);
            // se ocorrer uma falha na criaçao ele da erro 500
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDTO { Status = "Error", Message = "User creation failed!" });
            }
            // caso seja criado com sucesso ele retorna ok
            return Ok(new ResponseDTO { Status = "Success", Message = "User Created success" });
        }

        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken(TokenModelDTO tokenModelDTO)
        {
            if (tokenModelDTO != null)
            {
                return BadRequest("Invalid client request");
            }
            //  Obtem o acess Token
            string? accessToken = tokenModelDTO!.AccessToken ?? throw new ArgumentNullException(nameof(tokenModelDTO));

            // Obtem o refreshToken
            string? refreshToken = tokenModelDTO.RefreshToken ?? throw new ArgumentException(nameof(tokenModelDTO));

            // Obtem as clains princiapais
            var princial = _tokenService.GetPrincialFromExpiredToken(accessToken!, _configuration);

            if (princial == null)
            {
                return BadRequest("Invalid access token/refresh token");
            }

            // Obtem o nome do usuario
            string userName = princial.Identity!.Name!;

            var user = await _userManager.FindByNameAsync(userName!);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid access token/refresh token");
            }

            // Gera um novo token de acesso
            var newAccessToken = _tokenService.GenerateAccessToken(princial.Claims.ToList(), _configuration);
            // Gera um novo refreshToken
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // atualiza o valor o refresh token do usuario
            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return new ObjectResult(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken
            });
        }

        [HttpPost]
        [Route("revoke/{username}")]
        [Authorize(Policy = "ExclusiveOnly")]
        public async Task<IActionResult> Revoke(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return BadRequest("Invalid user name");
            }

            user.RefreshToken = null;

            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        [HttpPost]
        [Route("CreateRole")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            var roleExist = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));

                if (roleResult.Succeeded)
                {
                    _logger.LogInformation(1, "Roles Added");
                    return StatusCode(StatusCodes.Status200OK,
                        new ResponseDTO { Status = "Success", Message = $"Role{roleName} added succefuly" });
                }
                else
                {
                    _logger.LogInformation(2, "Error");
                    return StatusCode(StatusCodes.Status400BadRequest,
                        new ResponseDTO { Status = "Error", Message = $"Issue adding the new{roleName} role" });
                }
            }
            return StatusCode(StatusCodes.Status400BadRequest,
                        new ResponseDTO { Status = "Error", Message = "Role already exist" });
        }

        [HttpPost]
        [Route("AddUserToRole")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> AddUserToRole(string email, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var result = await _userManager.AddToRoleAsync(user, roleName);

                if (result.Succeeded)
                {
                    _logger.LogInformation(1, $"User {user.Email} added to the {roleName} role");
                    return StatusCode(StatusCodes.Status200OK,
                        new ResponseDTO { Status = "Success", Message = $"User{user.Email}added to the{roleName} role" });
                }
                else
                {
                    _logger.LogInformation(1, $"Error: Unable to add user {user.Email} to the {roleName} role");
                    return StatusCode(StatusCodes.Status200OK,
                        new ResponseDTO { Status = "Error", Message = $"Error: Unable do add user{user.Email}to the{roleName} role" });
                }
            }
            return BadRequest(new { error = "Unable to find user" });
        }
    }
}