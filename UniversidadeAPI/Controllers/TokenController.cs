using Microsoft.AspNetCore.Mvc;
using UniversidadeAPI.Services;

namespace UniversidadeAPI.Controllers
{
    public class TokenController : Controller
    {
        
        [HttpGet("Gerar Token")]
        public IActionResult GerarToken()
        {
            TokenService tokenService = new TokenService();
            return Ok(tokenService.GerarToken());
        }
    }
}
