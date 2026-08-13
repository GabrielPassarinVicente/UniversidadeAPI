using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public class AlunoRepository : RepositoryBase, IAlunoRepository
    {
        public AlunoRepository(ConectarBanco conectarBanco) : base(conectarBanco)
        {
        }

        public async Task<IEnumerable<Aluno>> GetAll() =>
            await QueryAsync<Aluno>("SELECT * FROM Aluno");

        public async Task<Aluno> GetById(int id) =>
            await QueryFirstOrDefaultAsync<Aluno>("SELECT * FROM Aluno WHERE Id = @Id", new { Id = id });

        public async Task<Aluno> Add(Aluno aluno)
        {
            var sql = @"
                INSERT INTO Aluno (NomeCompleto, DataNascimento, Cpf, Endereco, Telefone, Email, DataMatricula)
                VALUES (@NomeCompleto, @DataNascimento, @Cpf, @Endereco, @Telefone, @Email, @DataMatricula);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            aluno.Id = await ExecuteScalarAsync<int>(sql, aluno);
            return aluno;
        }

        public async Task<bool> Update(Aluno aluno)
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

            var affectedRows = await ExecuteAsync(sql, aluno);
            return affectedRows > 0;
        }

        public async Task<bool> Delete(int id)
        {
            var affectedRows = await ExecuteAsync("DELETE FROM Aluno WHERE Id = @Id;", new { Id = id });
            return affectedRows > 0;
        }
    }
}
