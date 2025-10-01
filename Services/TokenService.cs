using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using APICatalogo.Services.Interface;
using Microsoft.IdentityModel.Tokens;

namespace APICatalogo.Services
{
    public class TokenService : ITokenService
    {
        //Gera o token de acesso
        public JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims, IConfiguration _config)
        {
            //1: pegar a chave secreta que esta no app.setigns .json e se nao achar ele manda uma exeçao
            var key = _config.GetSection("JWT").GetValue<string>("SecretKey") ??
                        throw new InvalidOperationException("Invalid secret Key");

            //2: converte isso para uma rede bytes, pois esta em string e converte em bytes
            var privateKey = Encoding.UTF8.GetBytes(key);

            //3: cria as chaves de assinatura usando a chave secreta para assinar o token usando a criptografia HmacSha256
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(privateKey),
                                    SecurityAlgorithms.HmacSha256Signature);

            //4: criar o destritor do token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                //claisn relacionadas com o usuario
                Subject = new ClaimsIdentity(claims),
                //Data de expiraçao do token que esta configurado no app.setings
                Expires = DateTime.UtcNow.AddMinutes(_config.GetSection("JWT")
                                         .GetValue<double>("TokenValidityInMinutes")),
                //Valor da audiencia
                Audience = _config.GetSection("JWT").GetValue<string>("ValidAudience"),
                //valor do emissor
                Issuer = _config.GetSection("JWT").GetValue<string>("ValidIssuer"),
                //pega as credenciais geradas usando a chave secreta e atribuindo as SigningCredentials
                SigningCredentials = signingCredentials
            };
            //5: criar uma instancia do token
            var tokenHandler = new JwtSecurityTokenHandler();

            //6:criar o token usando o create
            var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);

            return token;

        }

        //faz com que o usuario nao precise recolocar as suas credenciais novamente usando o token de atualizaçao
        public string GenerateRefreshToken()
        {
            // cria uma variavel com um array de bytes que armazena 128bytes
            var secureRandomBytes = new byte[128];
            // cria um gerador de numeros aleatorios
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            //Preenche a variavel com os bytes aleatorios
            randomNumberGenerator.GetBytes(secureRandomBytes);
            //Converte esses bytes criados em uma base de formato string64
            var refreshToken = Convert.ToBase64String(secureRandomBytes);
            return refreshToken;
        }

        //Valida o token de acesso e obtem as clains principais do token
        public ClaimsPrincipal GetPrincialFromExpiredToken(string token, IConfiguration _config)
        {
            //1: pegar a chave secreta que esta no app.setigns.json e se nao achar ele manda uma exeçao
            var secretKey = _config["JWT:SecretKey"] ?? throw new InvalidOperationException("Invalid Key");

            //2: obtem os parametros de validaçao do token
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime = false
            };
            //3: cria uma instancia
            var tokenHandler = new JwtSecurityTokenHandler();
            //4:valida o token
            var princial = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                                                                                                              StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return princial;
        }
    }
}