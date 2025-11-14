using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
using UniversidadeAPI.Models;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniversidadeAPI.Repositories
{
    public class AlunoRepository : IAlunoRepository
    {
        private readonly ConectarBanco _conectarBanco;

        public AlunoRepository(ConectarBanco conectarBanco)
        {
            _conectarBanco = conectarBanco;
        }


        public async Task<IEnumerable<Aluno>> GetAll()
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "SELECT * FROM Aluno";

                return await conexao.QueryAsync<Aluno>(sql);
            }
        }

        public async Task<Aluno> GetById(int id)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "SELECT * FROM Aluno WHERE Id = @Id";

                return await conexao.QueryFirstOrDefaultAsync<Aluno>(sql, new { Id = id });
            }
        }


        public async Task<Aluno> Add(Aluno aluno)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    INSERT INTO Aluno (NomeCompleto, DataNascimento, Cpf, Endereco, Telefone, Email, DataMatricula)
                    VALUES (@NomeCompleto, @DataNascimento, @Cpf, @Endereco, @Telefone, @Email, @DataMatricula);";
                    
                    

                var newId = await conexao.ExecuteScalarAsync<int>(sql, aluno);

                aluno.Id = newId;
                return aluno;
            }
        }


        public async Task<bool> Update(Aluno aluno)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    UPDATE Aluno SET 
                        NomeCompleto = @NomeCompleto, 
                        DataNascimento = @DataNascimento, 
                        Cpf = @Cpf, 
                        Endereco = @Endereco, 
                        Telefone = @Telefone, 
                        Email = @Email, 
                        DataMatricula = @DataMatricula
                    WHERE Id = @Id;";

                var affectedRows = await conexao.ExecuteAsync(sql, aluno);

                return affectedRows > 0;
            }
        }


        public async Task<bool> Delete(int id)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "DELETE FROM Aluno WHERE Id = @Id;";

                var affectedRows = await conexao.ExecuteAsync(sql, new { Id = id });

                return affectedRows > 0;
            }
        }
    }
}