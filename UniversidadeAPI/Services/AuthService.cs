using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UniversidadeAPI.Models;
using UniversidadeAPI.Repositories;

namespace UniversidadeAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            // Buscar usu�rio por username
            var usuario = await _usuarioRepository.GetByUsername(request.Username);

            if (usuario == null)
            {
                throw new UnauthorizedAccessException("Usu�rio ou senha inv�lidos");
            }

            // Verificar senha
            if (!VerifyPassword(request.Password, usuario.PasswordHash))
            {
                throw new UnauthorizedAccessException("Usu�rio ou senha inv�lidos");
            }

            // Gerar token
            var token = GenerateJwtToken(usuario);
            var expiration = DateTime.UtcNow.AddHours(8);

            return new LoginResponse
            {
                Token = token,
                Expiration = expiration,
                Username = usuario.Username,
                Email = usuario.Email
            };
        }

        public async Task<Usuario> Register(RegistroRequest request)
        {
            // Valida��es
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Username e password s�o obrigat�rios");
            }

            if (await _usuarioRepository.UsernameExists(request.Username))
            {
                throw new ArgumentException("Username j� est� em uso");
            }

            if (await _usuarioRepository.EmailExists(request.Email))
            {
                throw new ArgumentException("Email j� est� em uso");
            }

            // Hash da senha
            var passwordHash = HashPassword(request.Password);

            // Criar novo usu�rio
            var usuario = new Usuario
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                Email = request.Email,
                DataCriacao = DateTime.UtcNow
            };

            return await _usuarioRepository.Add(usuario);
        }

        public string GenerateJwtToken(Usuario usuario)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationHours = int.Parse(jwtSettings["ExpirationHours"]);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Username),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Username)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expirationHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
        }
    }
}
