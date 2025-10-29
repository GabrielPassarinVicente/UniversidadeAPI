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
             using (var conexao = _conectarBanco.CriarConexao())
            {
                
                var sql = "SELECT * FROM Cursos WHERE IdCursos = @IdCursos";

                return await conexao.QueryFirstOrDefaultAsync<Curso>(sql, new { Id = id });
            }
        }


        public async Task<Curso> Add(Curso curso)
        {
             using (var conexao = _conectarBanco.CriarConexao())
            {
                
                await conexao.OpenAsync(); 
                

                var sql = @"
                    INSERT INTO Cursos (Nome, CargaHoraria, Departamentos_idDepartamentos)
                    VALUES (@Nome, @CargaHoraria, @Departamentos_idDepartamentos);";
                    

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
                    UPDATE Cursos SET 
                        Nome = @Nome, 
                        CargaHoraria = @CargaHoraria, 
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

                var affectedRows = await conexao.ExecuteAsync(sql, new { Id = id });

                return affectedRows > 0;
            }
        }
    }
}