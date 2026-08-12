using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversidadeAPI.Models;
using UniversidadeAPI.Services;

namespace UniversidadeAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CursosController : ControllerBase
    {
        private readonly ICursoService _cursoService;

        public CursosController(ICursoService cursoService)
        {
            _cursoService = cursoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CursoResponseComProfessores>>> GetCursos()
        {
            var cursos = await _cursoService.GetAllCursosWithProfessores();
            return Ok(cursos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CursoResponseComProfessores>> GetCurso(int id)
        {
            var curso = await _cursoService.GetCursoByIdWithProfessores(id);

            if (curso == null)
            {
                return NotFound();
            }
            return Ok(curso);
        }

        [HttpPost]
        public async Task<ActionResult<CursoResponseComProfessores>> PostCurso(CreateCursoRequest request)
        {
            var newCurso = await _cursoService.AddCursoWithProfessores(request);
            return CreatedAtAction(nameof(GetCurso), new { id = newCurso.IdCursos }, newCurso);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCurso(int id, UpdateCursoRequest request)
        {
            if (id != request.IdCursos)
            {
                return BadRequest(new { message = "O ID na URL não corresponde ao ID do curso no corpo da requisição." });
            }

            var updated = await _cursoService.UpdateCursoWithProfessores(request);

            if (updated)
            {
                var cursoAtualizado = await _cursoService.GetCursoByIdWithProfessores(id);
                return Ok(cursoAtualizado);
            }

            return NotFound(new { message = "Curso não encontrado." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCurso(int id)
        {
            if (await _cursoService.DeleteCurso(id))
            {
                return NoContent();
            }

            return NotFound(new { message = "Curso não encontrado." });
        }
    }
}