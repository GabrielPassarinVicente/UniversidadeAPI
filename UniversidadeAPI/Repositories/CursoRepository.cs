using Dapper;
using System.Collections.Generic;
using System.Linq; 
using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public class CursoRepository : ICursoRepository
    {
        private readonly ConectarBanco _conectarBanco;

        public CursoRepository(ConectarBanco conectarBanco)
        {
            _conectarBanco = conectarBanco;
        }

        public async Task<IEnumerable<Curso>> GetAll()
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "SELECT * FROM Cursos";
                return await conexao.QueryAsync<Curso>(sql);
            }
        }

        public async Task<Curso> GetById(int id)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "SELECT * FROM Cursos WHERE IdCursos = @IdCursos";
                return await conexao.QueryFirstOrDefaultAsync<Curso>(sql, new { IdCursos = id });
            }
        }

        public async Task<Curso> GetByIdWithProfessores(int id)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    SELECT DISTINCT c.IdCursos, c.Nome, c.CargaHoraria, c.Departamentos_idDepartamentos
                    FROM Cursos c
                    LEFT JOIN CursoProfessor cp ON c.IdCursos = cp.Cursos_IdCursos
                    WHERE c.IdCursos = @IdCursos";

                var curso = await conexao.QueryFirstOrDefaultAsync<Curso>(sql, new { IdCursos = id });

                if (curso != null)
                {
                    var sqlProfessores = @"
                        SELECT p.IdProfessores, p.Nome
                        FROM Professores p
                        INNER JOIN CursoProfessor cp ON p.IdProfessores = cp.Professores_IdProfessores
                        WHERE cp.Cursos_IdCursos = @CursoId
                        ORDER BY p.Nome";

                    var professores = await conexao.QueryAsync<Professor>(sqlProfessores, new { CursoId = id });
                    curso.Professores = professores.ToList();
                }

                return curso;
            }
        }

        public async Task<IEnumerable<Curso>> GetAllWithProfessores()
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    SELECT DISTINCT c.IdCursos, c.Nome, c.CargaHoraria, c.Departamentos_idDepartamentos
                    FROM Cursos c
                    LEFT JOIN CursoProfessor cp ON c.IdCursos = cp.Cursos_IdCursos
                    ORDER BY c.Nome";

                var cursos = await conexao.QueryAsync<Curso>(sql);

                foreach (var curso in cursos)
                {
                    var sqlProfessores = @"
                        SELECT p.IdProfessores, p.Nome
                        FROM Professores p
                        INNER JOIN CursoProfessor cp ON p.IdProfessores = cp.Professores_IdProfessores
                        WHERE cp.Cursos_IdCursos = @CursoId
                        ORDER BY p.Nome";

                    var professores = await conexao.QueryAsync<Professor>(sqlProfessores, new { CursoId = curso.IdCursos });
                    curso.Professores = professores.ToList();
                }

                return cursos;
            }
        }

        public async Task<Curso> Add(Curso curso)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    INSERT INTO Cursos (Nome, CargaHoraria, Departamentos_idDepartamentos)
                    VALUES (@Nome, @CargaHoraria, @Departamentos_idDepartamentos);
                    SELECT LAST_INSERT_ID();";

                var newId = await conexao.ExecuteScalarAsync<int>(sql, curso);
                curso.IdCursos = newId;
                return curso;
            }
        }

        public async Task<bool> Update(Curso curso)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    UPDATE Cursos 
                    SET Nome = @Nome, 
                        CargaHoraria = @CargaHoraria, 
                        Departamentos_idDepartamentos = @Departamentos_idDepartamentos
                    WHERE IdCursos = @IdCursos;";

                var affectedRows = await conexao.ExecuteAsync(sql, curso);
                return affectedRows > 0;
            }
        }

        public async Task<bool> Delete(int id)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "DELETE FROM Cursos WHERE IdCursos = @IdCursos;";
                var affectedRows = await conexao.ExecuteAsync(sql, new { IdCursos = id });
                return affectedRows > 0;
            }
        }
    }
}