using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversidadeAPI.Models;
using UniversidadeAPI.Services;

namespace UniversidadeAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/materias")] // Alias para compatibilidade com frontend que usa /api/materias
    public class DisciplinasController : ControllerBase
    {
        private readonly IDisciplinaService _disciplinaService;

        public DisciplinasController(IDisciplinaService disciplinaService)
        {
            _disciplinaService = disciplinaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Disciplina>>> GetAllDisciplinas()
        {
            var disciplinas = await _disciplinaService.GetAllDisciplinas();
            return Ok(disciplinas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Disciplina>> GetDisciplinaById(int id)
        {
            var disciplina = await _disciplinaService.GetDisciplinaById(id);

            if (disciplina == null)
            {
                return NotFound(new { message = $"Disciplina com ID {id} não encontrada." });
            }

            return Ok(disciplina);
        }

        [HttpGet("curso/{cursoId}")]
        public async Task<ActionResult<IEnumerable<Disciplina>>> GetDisciplinasByCurso(int cursoId)
        {
            var disciplinas = await _disciplinaService.GetDisciplinasByCurso(cursoId);
            return Ok(disciplinas);
        }

        [HttpGet("professor/{professorId}")]
        public async Task<ActionResult<IEnumerable<Disciplina>>> GetDisciplinasByProfessor(int professorId)
        {
            var disciplinas = await _disciplinaService.GetDisciplinasByProfessor(professorId);
            return Ok(disciplinas);
        }

        [HttpPost]
        public async Task<ActionResult<Disciplina>> AddDisciplina([FromBody] Disciplina disciplina)
        {
            var newDisciplina = await _disciplinaService.AddDisciplina(disciplina);
            return CreatedAtAction(nameof(GetDisciplinaById), new { id = newDisciplina.IdDisciplina }, newDisciplina);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDisciplina(int id, [FromBody] Disciplina disciplina)
        {
            if (id != disciplina.IdDisciplina)
            {
                return BadRequest(new { message = "O ID na URL não corresponde ao ID da disciplina no corpo da requisição." });
            }

            var updated = await _disciplinaService.UpdateDisciplina(disciplina);

            if (updated)
            {
                return NoContent();
            }

            return NotFound(new { message = $"Disciplina com ID {id} não encontrada." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDisciplina(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "ID inválido. O ID deve ser maior que zero." });
            }

            var deleted = await _disciplinaService.DeleteDisciplina(id);

            if (deleted)
            {
                return NoContent();
            }

            return NotFound(new { message = $"Disciplina com ID {id} não encontrada." });
        }
    }
}
