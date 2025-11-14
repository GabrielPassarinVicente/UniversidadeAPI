using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
            // Buscar usuário por username
            var usuario = await _usuarioRepository.GetByUsername(request.Username);

            if (usuario == null)
            {
                throw new UnauthorizedAccessException("Usuário ou senha inválidos");
            }

            // Verificar senha
            if (!VerifyPassword(request.Password, usuario.PasswordHash))
            {
                throw new UnauthorizedAccessException("Usuário ou senha inválidos");
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
            // Validações
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Username e password são obrigatórios");
            }

            if (await _usuarioRepository.UsernameExists(request.Username))
            {
                throw new ArgumentException("Username já está em uso");
            }

            if (await _usuarioRepository.EmailExists(request.Email))
            {
                throw new ArgumentException("Email já está em uso");
            }

            // Hash da senha
            var passwordHash = HashPassword(request.Password);

            // Criar novo usuário
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
            // Usar SHA256 para hash simples (para produção, use BCrypt ou PBKDF2)
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            var hash = HashPassword(password);
            return hash == passwordHash;
        }
    }
}
