using Microsoft.AspNetCore.Mvc;
using UniversidadeAPI.Models;
using UniversidadeAPI.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UniversidadeAPI.Controllers
{
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
        public async Task<ActionResult<IEnumerable<Aluno>>> GetCursos()
        {
            var cursos = await _cursoService.GetAllCursos();
            return Ok(cursos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Curso>> GetCurso(int id)
        {
            var curso = await _cursoService.GetCursoById(id);

            if (curso == null)
            {
                return NotFound();
            }
            return Ok(curso);
        }

        [HttpPost]
        public async Task<ActionResult<Aluno>> PostCurso(Curso curso)
        {
            try
            {
                var newCurso = await _cursoService.AddCurso(curso);

                return CreatedAtAction(nameof(GetCurso), new { id = newCurso.IdCursos }, newCurso);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCurso(int id, Curso curso)
        {
            if (id != curso.IdCursos)
            {
                return BadRequest("O ID na URL não corresponde ao ID do curso no corpo da requisição.");
            }

            try
            {
                
                var updated = await _cursoService.UpdateCurso(curso);

                if (updated)
                {
                    return NoContent();
                }

                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCurso(int id)
        {
            if (await _cursoService.DeleteCurso(id))
            {
                return NoContent();
            }

            return NotFound();
        }
    }
}