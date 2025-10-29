using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UniversidadeAPI.Services
{
    public class TokenService
    {
        public string GerarToken()
        {
            string chave = "minha-chave-secreta-agora-tem-mais-de-16-bytes";

            byte[] chavesbyte = Encoding.UTF8.GetBytes(chave);

            var handler = new JwtSecurityTokenHandler();
            ClaimsIdentity identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Name, "usuario"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));

            SecurityTokenDescriptor security = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(chavesbyte), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenCreate = handler.CreateJwtSecurityToken(security);
            var token = handler.WriteToken(tokenCreate);
            return token;
        }
    }
}
