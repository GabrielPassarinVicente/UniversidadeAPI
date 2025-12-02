using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public class ProfessorRepository : IProfessorRepository
    {
        private readonly ConectarBanco _conectarBanco;

        public ProfessorRepository(ConectarBanco conectarBanco)
        {
            _conectarBanco = conectarBanco;
        }


        public async Task<IEnumerable<Professor>> GetAll()
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "SELECT * FROM Professores";

                return await conexao.QueryAsync<Professor>(sql);
            }
        }

        public async Task<Professor> GetById(int id)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "SELECT * FROM Professores WHERE Id = @Id";

                return await conexao.QueryFirstOrDefaultAsync<Professor>(sql, new { Id = id });
            }
        }


        public async Task<Professor> Add(Professor professor)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {

                var sql = @"
                    INSERT INTO Professores (Nome)
                    VALUES (@Nome);";
                    

                var newId = await conexao.ExecuteScalarAsync<int>(sql, professor);

                professor.IdProfessores = newId;
                return professor;
            }
        }


        public async Task<bool> Update(Professor professor)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    UPDATE Professores SET 
                        Nome = @Nome 
                    WHERE Id = @Id;";

                var affectedRows = await conexao.ExecuteAsync(sql, professor);

                return affectedRows > 0;
            }
        }


        public async Task<bool> Delete(int id)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "DELETE FROM Professores WHERE Id = @Id;";

                var affectedRows = await conexao.ExecuteAsync(sql, new { Id = id });

                return affectedRows > 0;
            }
        }
    }
}