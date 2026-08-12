using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversidadeAPI.Models;
using UniversidadeAPI.Services;

namespace UniversidadeAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DepartamentosController : ControllerBase
    {
        private readonly IDepartamentoService _departamentoService;

        public DepartamentosController(IDepartamentoService departamentoService)
        {
            _departamentoService = departamentoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Departamento>>> GetAllDepartamentos()
        {
            var departamentos = await _departamentoService.GetAllDepartamentos();
            return Ok(departamentos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Departamento>> GetDepartamentoById(int id)
        {
            var departamento = await _departamentoService.GetDepartamentoById(id);

            if (departamento == null)
            {
                return NotFound(new { message = $"Departamento com ID {id} não encontrado." });
            }

            return Ok(departamento);
        }

        [HttpPost]
        public async Task<ActionResult<Departamento>> AddDepartamento([FromBody] Departamento departamento)
        {
            var newDepartamento = await _departamentoService.AddDepartamento(departamento);
            return CreatedAtAction(nameof(GetDepartamentoById), new { id = newDepartamento.IdDepartamentos }, newDepartamento);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartamento(int id, [FromBody] Departamento departamento)
        {
            if (id != departamento.IdDepartamentos)
            {
                return BadRequest(new { message = "O ID na URL não corresponde ao ID do departamento no corpo da requisição." });
            }

            var updated = await _departamentoService.UpdateDepartamento(departamento);

            if (updated)
            {
                return NoContent();
            }

            return NotFound(new { message = $"Departamento com ID {id} não encontrado." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartamento(int id)
        {
            var deleted = await _departamentoService.DeleteDepartamento(id);

            if (deleted)
            {
                return NoContent();
            }

            return NotFound(new { message = $"Departamento com ID {id} não encontrado." });
        }
    }
}
