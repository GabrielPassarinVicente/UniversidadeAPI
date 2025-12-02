using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            try
            {
                var newCurso = await _cursoService.AddCursoWithProfessores(request);
                return CreatedAtAction(nameof(GetCurso), new { id = newCurso.IdCursos }, newCurso);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao criar curso", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCurso(int id, UpdateCursoRequest request)
        {
            if (id != request.IdCursos)
            {
                return BadRequest(new { message = "O ID na URL não corresponde ao ID do curso no corpo da requisição." });
            }

            try
            {
                var updated = await _cursoService.UpdateCursoWithProfessores(request);

                if (updated)
                {
                    var cursoAtualizado = await _cursoService.GetCursoByIdWithProfessores(id);
                    return Ok(cursoAtualizado);
                }

                return NotFound(new { message = "Curso não encontrado." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao atualizar curso", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCurso(int id)
        {
            try
            {
                if (await _cursoService.DeleteCurso(id))
                {
                    return NoContent();
                }

                return NotFound(new { message = "Curso não encontrado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao deletar curso", details = ex.Message });
            }
        }
    }
}