using Microsoft.AspNetCore.Mvc;
using UniversidadeAPI.Models;
using UniversidadeAPI.Services;

namespace UniversidadeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.Login(request);
            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<ActionResult<Usuario>> Register([FromBody] RegistroRequest request)
        {
            var usuario = await _authService.Register(request);
            return CreatedAtAction(nameof(Register), new { id = usuario.Id }, new
            {
                id = usuario.Id,
                username = usuario.Username,
                email = usuario.Email,
                dataCriacao = usuario.DataCriacao
            });
        }
    }
}
