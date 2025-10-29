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
            using (var conexao = _conectarBanco.CriarConexao())
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
            using (var conexao = _conectarBanco.CriarConexao())
            {
                await conexao.OpenAsync();
                using (IDbTransaction transaction = conexao.BeginTransaction())
                {
                    try
                    {
                        var sql = @"
                    INSERT INTO Aluno (idAluno, Nome, Cpf, Matrículas_idMatrículas)
                    VALUES (@idAluno, @Nome, @Cpf, @Matrículas_idMatrículas);";
                 
                        conexao.Execute(sql, new
                        {
                            aluno.IdAluno,
                            aluno.Nome,
                            aluno.Cpf,
                            aluno.Matrículas_idMatrículas
                        
                        }, transaction: transaction);
                        transaction.Commit();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Erro na transação: {ex.Message}");
                        transaction.Rollback();
                        throw;
                    }



                    return aluno;


                }
            }
        }

        public async Task<bool> Update(Aluno aluno)
        {
             using (var conexao = _conectarBanco.CriarConexao())
            {
                await conexao.OpenAsync();
                using (IDbTransaction transaction = conexao.BeginTransaction())
                {
                    try
                    {
                        var sql = @"
                    UPDATE Aluno SET 
                        Nome = @Nome,
                        Cpf = @Cpf,
                        Matrículas_idMatrículas = @Matrículas_idMatrículas                 
                    WHERE IdAluno = @IdAluno;";

                        conexao.Execute(sql, new
                        {
                            aluno.IdAluno,
                            aluno.Nome,
                            aluno.Cpf,
                            aluno.Matrículas_idMatrículas
                        }, transaction: transaction);
                        transaction.Commit();
                    }catch(Exception ex)
                    {
                        Console.WriteLine($"Erro na transação: {ex.Message}");
                        transaction.Rollback();
                        throw;
                    }

                    return true;

                }



            }
        }


        public async Task<bool> Delete(int idAluno)
        {
             using (var conexao = _conectarBanco.CriarConexao())
            {
                await conexao.OpenAsync();
                using (IDbTransaction transaction = conexao.BeginTransaction())
                {
                    try
                    {
                       
                         var sql = "ON DELETE CASCADE FROM Aluno WHERE IdAluno = @IdAluno;";
                        conexao.Execute(sql, new { IdAluno = idAluno }, transaction: transaction);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro na transação: {ex.Message}");
                        transaction.Rollback();
                        throw;
                    }


                    return true;
                }
            }
        }
    }
}