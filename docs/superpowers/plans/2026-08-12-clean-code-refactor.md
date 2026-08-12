# UniversidadeAPI Clean Code Refactor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix the broken build (missing Curso↔Professor feature), standardize the project on SQL Server, remove committed secrets, harden password hashing, cut CRUD duplication, and add Docker support — without changing the overall Controllers → Services → Repositories + Dapper architecture.

**Architecture:** Same 3-layer architecture as today (Controllers, Services, Repositories, all via constructor-injected interfaces, Dapper for data access). No ORM migration, no new architectural layers — just filling in a missing feature, fixing a database-provider mismatch, and removing duplicated boilerplate via one small abstract base class and one middleware.

**Tech Stack:** .NET 8, ASP.NET Core Web API, Dapper, Microsoft.Data.SqlClient (SQL Server), JWT auth, BCrypt.Net-Next, Docker.

## Global Constraints

- Database is **SQL Server** (not MySQL) — the `create_database` script is already T-SQL; the C# code is what's wrong today.
- **No automated test project** in this pass — this was an explicit user decision. Every task is verified with `dotnet build` (must show `0 Erro(s)`) plus manual code review, not a test suite.
- CORS policy `AllowAll` stays as-is — out of scope.
- The `/api/materias` alias route on `DisciplinasController` stays as-is — it's intentional.
- No EF Core migration, no new architectural layers beyond `RepositoryBase` and `ExceptionHandlingMiddleware`.
- Spec: `docs/superpowers/specs/2026-08-12-clean-code-refactor-design.md`.

All paths below are relative to `UniversidadeAPI/` (the project root is
`C:\Users\Gabriel\OneDrive\Área de Trabalho\universidade-api`, which contains the solution file and
the `UniversidadeAPI` project folder). Run all commands from the repo root
(`C:\Users\Gabriel\OneDrive\Área de Trabalho\universidade-api`) unless stated otherwise.

---

### Task 1: Git housekeeping — stop tracking build artifacts

**Files:**
- Create: `.gitignore` (repo root)
- Modify: git index (untrack `UniversidadeAPI/bin`, `UniversidadeAPI/obj`, `.vs`)

**Interfaces:** None — this task touches no application code.

- [ ] **Step 1: Create `.gitignore` at the repo root**

```gitignore
bin/
obj/
.vs/
*.user
.env
```

- [ ] **Step 2: Untrack build artifacts already committed**

Run from the repo root:

```bash
git rm -r --cached UniversidadeAPI/bin UniversidadeAPI/obj .vs
```

Expected: a long list of `rm 'UniversidadeAPI/bin/...'` / `rm '.vs/...'` lines. This only removes
them from git's index — the files stay on disk.

- [ ] **Step 3: Verify**

Run: `git status`
Expected: `.gitignore` shown as a new file; `UniversidadeAPI/bin`, `UniversidadeAPI/obj`, `.vs`
shown as deletions (from the index) — nothing else changed.

- [ ] **Step 4: Commit**

```bash
git add .gitignore
git add -u
git commit -m "chore: stop tracking build artifacts and IDE state"
```

---

### Task 2: Implement the Curso↔Professor relationship

This is the task that fixes the 20 build errors — `CursoProfessor`, `ICursoProfessorRepository`,
`CursoResponseComProfessores`, `ProfessorDTO`, `CreateCursoRequest`, and `UpdateCursoRequest` are
referenced throughout `CursoService.cs`, `ICursoService.cs`, `CursosController.cs`, `Curso.cs`, and
`Professor.cs`, but none of them exist as files today.

**Files:**
- Modify: `UniversidadeAPI/../create_database` (repo root, add `CursoProfessor` table)
- Create: `UniversidadeAPI/Models/CursoProfessor.cs`
- Create: `UniversidadeAPI/Models/Dtos/ProfessorDTO.cs`
- Create: `UniversidadeAPI/Models/Dtos/CursoResponseComProfessores.cs`
- Create: `UniversidadeAPI/Models/Dtos/CreateCursoRequest.cs`
- Create: `UniversidadeAPI/Models/Dtos/UpdateCursoRequest.cs`
- Create: `UniversidadeAPI/Repositories/ICursoProfessorRepository.cs`
- Create: `UniversidadeAPI/Repositories/CursoProfessorRepository.cs`

**Interfaces:**
- Consumes: `ConectarBanco.CriarConexao(): DbConnection` (existing, unchanged in this task).
- Produces: `ICursoProfessorRepository` with `Task AddCursoProfessor(int cursoId, int professorId)`
  and `Task RemoveAllProfessoresByCurso(int cursoId)` — these exact names/signatures are already
  called by `CursoService.cs` today. `CursoProfessor`, `ProfessorDTO`, `CursoResponseComProfessores`,
  `CreateCursoRequest`, `UpdateCursoRequest` as plain data classes in the `UniversidadeAPI.Models`
  namespace (same namespace as other models, even though the DTO files live in a `Dtos`
  subfolder — this avoids touching `using` statements in `CursoService.cs`, `ICursoService.cs`, and
  `CursosController.cs`, which already reference these types with only `using UniversidadeAPI.Models;`).

- [ ] **Step 1: Add the `CursoProfessor` junction table to the database script**

Open `create_database` (repo root, plain text file with a `.sql`-style T-SQL script, no
extension). Find the `CREATE TABLE Cursos (...)` block and its trailing index statements:

```sql
CREATE INDEX IX_Cursos_Nome ON Cursos(Nome);
CREATE INDEX IX_Cursos_Departamento ON Cursos(Departamentos_idDepartamentos);
```

Immediately after those two lines (and before `CREATE TABLE Aluno (`), insert:

```sql
CREATE TABLE CursoProfessor (
  Cursos_IdCursos INT NOT NULL,
  Professores_IdProfessores INT NOT NULL,
  PRIMARY KEY (Cursos_IdCursos, Professores_IdProfessores),
  CONSTRAINT fk_cursoprofessor_curso FOREIGN KEY (Cursos_IdCursos) REFERENCES Cursos(IdCursos) ON DELETE CASCADE,
  CONSTRAINT fk_cursoprofessor_professor FOREIGN KEY (Professores_IdProfessores) REFERENCES Professores(IdProfessores) ON DELETE CASCADE
);
```

- [ ] **Step 2: Verify the script edit**

Run: `grep -n "CursoProfessor" "create_database"`
Expected: one match showing the new `CREATE TABLE CursoProfessor (` line, positioned after the
`Cursos` table block and before the `Aluno` table block.

(This script isn't run against a live database in this environment — there's no SQL Server
instance available here. This is a visual/structural check only; the user will run the updated
script the next time they set up their database, per Task 9's README update.)

- [ ] **Step 3: Create the `CursoProfessor` model**

Create `UniversidadeAPI/Models/CursoProfessor.cs`:

```csharp
namespace UniversidadeAPI.Models
{
    public class CursoProfessor
    {
        public int Cursos_IdCursos { get; set; }
        public int Professores_IdProfessores { get; set; }
    }
}
```

- [ ] **Step 4: Create the DTOs**

Create `UniversidadeAPI/Models/Dtos/ProfessorDTO.cs`:

```csharp
namespace UniversidadeAPI.Models
{
    public class ProfessorDTO
    {
        public int IdProfessores { get; set; }
        public string? Nome { get; set; }
    }
}
```

Create `UniversidadeAPI/Models/Dtos/CursoResponseComProfessores.cs`:

```csharp
namespace UniversidadeAPI.Models
{
    public class CursoResponseComProfessores
    {
        public int IdCursos { get; set; }
        public string? Nome { get; set; }
        public string? CargaHoraria { get; set; }
        public int Departamentos_idDepartamentos { get; set; }
        public List<ProfessorDTO> Professores { get; set; } = new List<ProfessorDTO>();
    }
}
```

Create `UniversidadeAPI/Models/Dtos/CreateCursoRequest.cs`:

```csharp
namespace UniversidadeAPI.Models
{
    public class CreateCursoRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string? CargaHoraria { get; set; }
        public int Departamentos_idDepartamentos { get; set; }
        public List<int>? Professores { get; set; }
    }
}
```

Create `UniversidadeAPI/Models/Dtos/UpdateCursoRequest.cs`:

```csharp
namespace UniversidadeAPI.Models
{
    public class UpdateCursoRequest
    {
        public int IdCursos { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? CargaHoraria { get; set; }
        public int Departamentos_idDepartamentos { get; set; }
        public List<int>? Professores { get; set; }
    }
}
```

- [ ] **Step 5: Create `ICursoProfessorRepository`**

Create `UniversidadeAPI/Repositories/ICursoProfessorRepository.cs`:

```csharp
namespace UniversidadeAPI.Repositories
{
    public interface ICursoProfessorRepository
    {
        Task AddCursoProfessor(int cursoId, int professorId);
        Task RemoveAllProfessoresByCurso(int cursoId);
    }
}
```

- [ ] **Step 6: Create `CursoProfessorRepository`**

Create `UniversidadeAPI/Repositories/CursoProfessorRepository.cs`:

```csharp
using Dapper;

namespace UniversidadeAPI.Repositories
{
    public class CursoProfessorRepository : ICursoProfessorRepository
    {
        private readonly ConectarBanco _conectarBanco;

        public CursoProfessorRepository(ConectarBanco conectarBanco)
        {
            _conectarBanco = conectarBanco;
        }

        public async Task AddCursoProfessor(int cursoId, int professorId)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    INSERT INTO CursoProfessor (Cursos_IdCursos, Professores_IdProfessores)
                    VALUES (@CursoId, @ProfessorId);";

                await conexao.ExecuteAsync(sql, new { CursoId = cursoId, ProfessorId = professorId });
            }
        }

        public async Task RemoveAllProfessoresByCurso(int cursoId)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "DELETE FROM CursoProfessor WHERE Cursos_IdCursos = @CursoId;";
                await conexao.ExecuteAsync(sql, new { CursoId = cursoId });
            }
        }
    }
}
```

(This is written in the current connection-per-call style, matching every other repository today.
Task 5 refactors it — along with all the others — onto the shared `RepositoryBase`.)

- [ ] **Step 7: Build and verify the original 20 errors are gone**

Run: `dotnet build`
Expected: `0 Erro(s)`. Before this task, the same command produced 20 `CS0246` errors, all naming
`CursoProfessor`, `ICursoProfessorRepository`, `CursoResponseComProfessores`, `ProfessorDTO`,
`CreateCursoRequest`, or `UpdateCursoRequest`. This is the project's first successful compile.

- [ ] **Step 8: Commit**

```bash
git add create_database UniversidadeAPI/Models/CursoProfessor.cs UniversidadeAPI/Models/Dtos UniversidadeAPI/Repositories/ICursoProfessorRepository.cs UniversidadeAPI/Repositories/CursoProfessorRepository.cs
git commit -m "feat: implement Curso-Professor relationship (fixes build)"
```

---

### Task 3: Standardize on SQL Server

The project's schema (`create_database`) is T-SQL for SQL Server, but `ConectarBanco` and every
repository's `INSERT` statement are written for MySQL (`MySqlConnection`,
`SELECT LAST_INSERT_ID()`). This task makes the C# code match the database it's actually meant to
run against.

**Files:**
- Modify: `UniversidadeAPI/UniversidadeAPI.csproj`
- Modify: `UniversidadeAPI/ConectarBanco.cs`
- Modify: `UniversidadeAPI/appsettings.json`
- Modify: `UniversidadeAPI/Repositories/AlunoRepository.cs`
- Modify: `UniversidadeAPI/Repositories/CursoRepository.cs`
- Modify: `UniversidadeAPI/Repositories/ProfessorRepository.cs`
- Modify: `UniversidadeAPI/Repositories/DepartamentoRepository.cs`
- Modify: `UniversidadeAPI/Repositories/DisciplinaRepository.cs`
- Modify: `UniversidadeAPI/Repositories/UsuarioRepository.cs`

**Interfaces:** No public signatures change in this task — only SQL text and the connection type
returned by `ConectarBanco.CriarConexao()` (still `DbConnection`, just a `SqlConnection` instance
instead of `MySqlConnection`).

- [ ] **Step 1: Swap the database driver package**

In `UniversidadeAPI/UniversidadeAPI.csproj`, replace:

```xml
<PackageReference Include="MySql.Data" Version="9.4.0" />
```

with:

```xml
<PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
```

Run: `dotnet restore`
Expected: restore succeeds, no errors (the build will still fail after this step — that's
expected, fixed by the next steps).

- [ ] **Step 2: Update `ConectarBanco` to create a `SqlConnection`**

Replace the full contents of `UniversidadeAPI/ConectarBanco.cs`:

```csharp
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace UniversidadeAPI
{
    public class ConectarBanco
    {
        private readonly string _connectionString;

        public ConectarBanco(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentNullException(nameof(_connectionString),
                    "A string de conexão 'DefaultConnection' não foi encontrada no appsettings.json.");
            }
        }

        public DbConnection CriarConexao()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
```

- [ ] **Step 3: Fix the connection string in `appsettings.json`**

In `UniversidadeAPI/appsettings.json`, replace the `ConnectionStrings` section:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=mydb;User Id=sa;Password=CHANGE_ME;TrustServerCertificate=True;"
}
```

(`CHANGE_ME` is a deliberate placeholder — Task 9 documents using `dotnet user-secrets` for the
real value. The current real password stays out of version control from this point on.)

- [ ] **Step 4: Fix `AlunoRepository.Add` (it currently always returns `Id = 0`)**

In `UniversidadeAPI/Repositories/AlunoRepository.cs`, the `Add` method's SQL has no `SELECT` to
return the new ID at all, so `ExecuteScalarAsync<int>` always returns `0`. Replace:

```csharp
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
```

with:

```csharp
        public async Task<Aluno> Add(Aluno aluno)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    INSERT INTO Aluno (NomeCompleto, DataNascimento, Cpf, Endereco, Telefone, Email, DataMatricula)
                    VALUES (@NomeCompleto, @DataNascimento, @Cpf, @Endereco, @Telefone, @Email, @DataMatricula);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var newId = await conexao.ExecuteScalarAsync<int>(sql, aluno);

                aluno.Id = newId;
                return aluno;
            }
        }
```

- [ ] **Step 5: Replace `LAST_INSERT_ID()` with `SCOPE_IDENTITY()` everywhere else**

In each of the following files, find `SELECT LAST_INSERT_ID();` and replace it with
`SELECT CAST(SCOPE_IDENTITY() AS INT);` (same indentation, same trailing semicolon):

- `UniversidadeAPI/Repositories/CursoRepository.cs` — inside `Add(Curso curso)`
- `UniversidadeAPI/Repositories/ProfessorRepository.cs` — inside `Add(Professor professor)`
- `UniversidadeAPI/Repositories/DepartamentoRepository.cs` — inside `Add(Departamento departamento)`
- `UniversidadeAPI/Repositories/DisciplinaRepository.cs` — inside `Add(Disciplina disciplina)`
- `UniversidadeAPI/Repositories/UsuarioRepository.cs` — inside `Add(Usuario usuario)`

- [ ] **Step 6: Remove the MySQL-specific exception handling in `DisciplinaRepository`**

`DisciplinaRepository.cs` imports `MySql.Data.MySqlClient` and catches `MySqlException` in
`Add`, `Update`, and `Delete` — that type no longer exists once the package is removed. Replace
the full contents of `UniversidadeAPI/Repositories/DisciplinaRepository.cs`:

```csharp
using Dapper;
using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public class DisciplinaRepository : IDisciplinaRepository
    {
        private readonly ConectarBanco _conectarBanco;

        public DisciplinaRepository(ConectarBanco conectarBanco)
        {
            _conectarBanco = conectarBanco;
        }

        public async Task<IEnumerable<Disciplina>> GetAll()
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
            SELECT 
                d.*, 
                c.IdCursos, c.Nome AS CursoNome, c.CargaHoraria AS CursoCargaHoraria, c.Departamentos_idDepartamentos,
                p.IdProfessores, p.Nome AS ProfessorNome
            FROM Disciplinas d
            LEFT JOIN Cursos c ON d.Curso_IdCursos = c.IdCursos
            LEFT JOIN Professores p ON d.Professor_IdProfessores = p.IdProfessores";

                var disciplinas = await conexao.QueryAsync<Disciplina, Curso, Professor, Disciplina>(
                    sql,
                    (disciplina, curso, professor) =>
                    {
                        disciplina.Curso = curso;
                        disciplina.Professor = professor;
                        return disciplina;
                    },
                    splitOn: "IdCursos,IdProfessores"
                );

                return disciplinas;
            }
        }

        public async Task<Disciplina> GetById(int id)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
            SELECT 
                d.*, 
                c.IdCursos, c.Nome AS CursoNome, c.CargaHoraria AS CursoCargaHoraria, c.Departamentos_idDepartamentos,
                p.IdProfessores, p.Nome AS ProfessorNome
            FROM Disciplinas d
            LEFT JOIN Cursos c ON d.Curso_IdCursos = c.IdCursos
            LEFT JOIN Professores p ON d.Professor_IdProfessores = p.IdProfessores
            WHERE d.IdDisciplina = @IdDisciplina";

                var disciplina = await conexao.QueryAsync<Disciplina, Curso, Professor, Disciplina>(
                    sql,
                    (disciplina, curso, professor) =>
                    {
                        disciplina.Curso = curso;
                        disciplina.Professor = professor;
                        return disciplina;
                    },
                    new { IdDisciplina = id },
                    splitOn: "IdCursos,IdProfessores"
                );

                return disciplina.FirstOrDefault();
            }
        }

        public async Task<IEnumerable<Disciplina>> GetByCurso(int cursoId)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "SELECT * FROM Disciplinas WHERE Curso_IdCursos = @CursoId";
                return await conexao.QueryAsync<Disciplina>(sql, new { CursoId = cursoId });
            }
        }

        public async Task<IEnumerable<Disciplina>> GetByProfessor(int professorId)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "SELECT * FROM Disciplinas WHERE Professor_IdProfessores = @ProfessorId";
                return await conexao.QueryAsync<Disciplina>(sql, new { ProfessorId = professorId });
            }
        }

        public async Task<Disciplina> Add(Disciplina disciplina)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    INSERT INTO Disciplinas (Nome, Codigo, CargaHoraria, Creditos, Ementa, Curso_IdCursos, Professor_IdProfessores)
                    VALUES (@Nome, @Codigo, @CargaHoraria, @Creditos, @Ementa, @Curso_IdCursos, @Professor_IdProfessores);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var newId = await conexao.ExecuteScalarAsync<int>(sql, new
                {
                    Nome = disciplina.Nome,
                    Codigo = disciplina.Codigo,
                    CargaHoraria = disciplina.CargaHoraria,
                    Creditos = disciplina.Creditos,
                    Ementa = disciplina.Ementa,
                    Curso_IdCursos = disciplina.Curso_IdCursos,
                    Professor_IdProfessores = disciplina.Professor_IdProfessores
                });
                disciplina.IdDisciplina = newId;
                return disciplina;
            }
        }

        public async Task<bool> Update(Disciplina disciplina)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    UPDATE Disciplinas SET 
                        Nome = @Nome, 
                        Codigo = @Codigo, 
                        CargaHoraria = @CargaHoraria,
                        Creditos = @Creditos,
                        Ementa = @Ementa,
                        Curso_IdCursos = @Curso_IdCursos,
                        Professor_IdProfessores = @Professor_IdProfessores
                    WHERE IdDisciplina = @IdDisciplina;";

                var affectedRows = await conexao.ExecuteAsync(sql, new
                {
                    IdDisciplina = disciplina.IdDisciplina,
                    Nome = disciplina.Nome,
                    Codigo = disciplina.Codigo,
                    CargaHoraria = disciplina.CargaHoraria,
                    Creditos = disciplina.Creditos,
                    Ementa = disciplina.Ementa,
                    Curso_IdCursos = disciplina.Curso_IdCursos,
                    Professor_IdProfessores = disciplina.Professor_IdProfessores
                });
                return affectedRows > 0;
            }
        }

        public async Task<bool> Delete(int id)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "DELETE FROM Disciplinas WHERE IdDisciplina = @IdDisciplina";
                await conexao.ExecuteAsync(sql, new { IdDisciplina = id });
                return true;
            }
        }

        public async Task<bool> CodigoExists(string codigo)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "SELECT COUNT(*) FROM Disciplinas WHERE Codigo = @Codigo";
                var count = await conexao.ExecuteScalarAsync<int>(sql, new { Codigo = codigo });
                return count > 0;
            }
        }
    }
}
```

- [ ] **Step 7: Remove the now-unused MySQL import in `AlunoRepository`**

`UniversidadeAPI/Repositories/AlunoRepository.cs` has `using MySql.Data.MySqlClient;` at the top,
which is unused (the connection type comes from `ConectarBanco`, not a direct reference). Remove
that `using` line.

- [ ] **Step 8: Build and verify**

Run: `dotnet build`
Expected: `0 Erro(s)`.

Run: `grep -rn "LAST_INSERT_ID\|MySql" UniversidadeAPI/Repositories UniversidadeAPI/ConectarBanco.cs UniversidadeAPI/UniversidadeAPI.csproj`
Expected: no matches.

- [ ] **Step 9: Commit**

```bash
git add UniversidadeAPI/UniversidadeAPI.csproj UniversidadeAPI/ConectarBanco.cs UniversidadeAPI/appsettings.json UniversidadeAPI/Repositories
git commit -m "fix: standardize database access on SQL Server"
```

---

### Task 4: Introduce `RepositoryBase` and refactor Aluno/Curso/Professor repositories

Every repository repeats the same `await using (var conexao = _conectarBanco.CriarConexao())`
boilerplate around every single query. This task introduces one small abstract base class that
owns connection lifecycle, and migrates the first three repositories onto it.

**Files:**
- Create: `UniversidadeAPI/Repositories/RepositoryBase.cs`
- Modify: `UniversidadeAPI/Repositories/AlunoRepository.cs`
- Modify: `UniversidadeAPI/Repositories/CursoRepository.cs`
- Modify: `UniversidadeAPI/Repositories/ProfessorRepository.cs`

**Interfaces:**
- Produces: `RepositoryBase` (abstract), with protected members other repositories call in Task 5:
  - `protected Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parametros = null)`
  - `protected Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parametros = null)`
  - `protected Task<int> ExecuteAsync(string sql, object? parametros = null)`
  - `protected Task<T> ExecuteScalarAsync<T>(string sql, object? parametros = null)`
  - `protected Task<T> WithConnectionAsync<T>(Func<DbConnection, Task<T>> acao)` (escape hatch for
    multi-query-per-connection or multi-mapping calls that don't fit the four helpers above)

- [ ] **Step 1: Create `RepositoryBase`**

Create `UniversidadeAPI/Repositories/RepositoryBase.cs`:

```csharp
using System.Data.Common;
using Dapper;

namespace UniversidadeAPI.Repositories
{
    public abstract class RepositoryBase
    {
        private readonly ConectarBanco _conectarBanco;

        protected RepositoryBase(ConectarBanco conectarBanco)
        {
            _conectarBanco = conectarBanco;
        }

        protected async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parametros = null)
        {
            await using var conexao = _conectarBanco.CriarConexao();
            return await conexao.QueryAsync<T>(sql, parametros);
        }

        protected async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parametros = null)
        {
            await using var conexao = _conectarBanco.CriarConexao();
            return await conexao.QueryFirstOrDefaultAsync<T>(sql, parametros);
        }

        protected async Task<int> ExecuteAsync(string sql, object? parametros = null)
        {
            await using var conexao = _conectarBanco.CriarConexao();
            return await conexao.ExecuteAsync(sql, parametros);
        }

        protected async Task<T> ExecuteScalarAsync<T>(string sql, object? parametros = null)
        {
            await using var conexao = _conectarBanco.CriarConexao();
            return await conexao.ExecuteScalarAsync<T>(sql, parametros);
        }

        protected async Task<T> WithConnectionAsync<T>(Func<DbConnection, Task<T>> acao)
        {
            await using var conexao = _conectarBanco.CriarConexao();
            return await acao(conexao);
        }
    }
}
```

- [ ] **Step 2: Refactor `AlunoRepository`**

Replace the full contents of `UniversidadeAPI/Repositories/AlunoRepository.cs`:

```csharp
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
```

- [ ] **Step 3: Refactor `CursoRepository`**

Replace the full contents of `UniversidadeAPI/Repositories/CursoRepository.cs`:

```csharp
using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public class CursoRepository : RepositoryBase, ICursoRepository
    {
        public CursoRepository(ConectarBanco conectarBanco) : base(conectarBanco)
        {
        }

        public async Task<IEnumerable<Curso>> GetAll() =>
            await QueryAsync<Curso>("SELECT * FROM Cursos");

        public async Task<Curso> GetById(int id) =>
            await QueryFirstOrDefaultAsync<Curso>("SELECT * FROM Cursos WHERE IdCursos = @IdCursos", new { IdCursos = id });

        public async Task<Curso> GetByIdWithProfessores(int id) =>
            await WithConnectionAsync(async conexao =>
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
            });

        public async Task<IEnumerable<Curso>> GetAllWithProfessores() =>
            await WithConnectionAsync(async conexao =>
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
            });

        public async Task<Curso> Add(Curso curso)
        {
            var sql = @"
                INSERT INTO Cursos (Nome, CargaHoraria, Departamentos_idDepartamentos)
                VALUES (@Nome, @CargaHoraria, @Departamentos_idDepartamentos);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            curso.IdCursos = await ExecuteScalarAsync<int>(sql, curso);
            return curso;
        }

        public async Task<bool> Update(Curso curso)
        {
            var sql = @"
                UPDATE Cursos 
                SET Nome = @Nome, 
                    CargaHoraria = @CargaHoraria, 
                    Departamentos_idDepartamentos = @Departamentos_idDepartamentos
                WHERE IdCursos = @IdCursos;";

            var affectedRows = await ExecuteAsync(sql, curso);
            return affectedRows > 0;
        }

        public async Task<bool> Delete(int id)
        {
            var affectedRows = await ExecuteAsync("DELETE FROM Cursos WHERE IdCursos = @IdCursos;", new { IdCursos = id });
            return affectedRows > 0;
        }
    }
}
```

- [ ] **Step 4: Refactor `ProfessorRepository`**

Replace the full contents of `UniversidadeAPI/Repositories/ProfessorRepository.cs`:

```csharp
using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public class ProfessorRepository : RepositoryBase, IProfessorRepository
    {
        public ProfessorRepository(ConectarBanco conectarBanco) : base(conectarBanco)
        {
        }

        public async Task<IEnumerable<Professor>> GetAll() =>
            await QueryAsync<Professor>("SELECT * FROM Professores");

        public async Task<Professor> GetById(int id) =>
            await QueryFirstOrDefaultAsync<Professor>("SELECT * FROM Professores WHERE IdProfessores = @IdProfessores", new { IdProfessores = id });

        public async Task<Professor> Add(Professor professor)
        {
            var sql = @"
                INSERT INTO Professores (Nome)
                VALUES (@Nome);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            professor.IdProfessores = await ExecuteScalarAsync<int>(sql, professor);
            return professor;
        }

        public async Task<bool> Update(Professor professor)
        {
            var sql = @"
                UPDATE Professores SET 
                    Nome = @Nome 
                WHERE IdProfessores = @IdProfessores;";

            var affectedRows = await ExecuteAsync(sql, professor);
            return affectedRows > 0;
        }

        public async Task<bool> Delete(int id)
        {
            var affectedRows = await ExecuteAsync("DELETE FROM Professores WHERE IdProfessores = @IdProfessores;", new { IdProfessores = id });
            return affectedRows > 0;
        }
    }
}
```

(This also drops the unused `using System.Xml.Linq;` that was in the original file.)

- [ ] **Step 5: Build and verify**

Run: `dotnet build`
Expected: `0 Erro(s)`.

- [ ] **Step 6: Commit**

```bash
git add UniversidadeAPI/Repositories/RepositoryBase.cs UniversidadeAPI/Repositories/AlunoRepository.cs UniversidadeAPI/Repositories/CursoRepository.cs UniversidadeAPI/Repositories/ProfessorRepository.cs
git commit -m "refactor: introduce RepositoryBase, migrate Aluno/Curso/Professor repositories"
```

---

### Task 5: Migrate remaining repositories onto `RepositoryBase`

**Files:**
- Modify: `UniversidadeAPI/Repositories/DepartamentoRepository.cs`
- Modify: `UniversidadeAPI/Repositories/DisciplinaRepository.cs`
- Modify: `UniversidadeAPI/Repositories/UsuarioRepository.cs`
- Modify: `UniversidadeAPI/Repositories/CursoProfessorRepository.cs`

**Interfaces:**
- Consumes: `RepositoryBase` protected members from Task 4 (`QueryAsync`,
  `QueryFirstOrDefaultAsync`, `ExecuteAsync`, `ExecuteScalarAsync`, `WithConnectionAsync`).

- [ ] **Step 1: Refactor `DepartamentoRepository`**

Replace the full contents of `UniversidadeAPI/Repositories/DepartamentoRepository.cs`:

```csharp
using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public class DepartamentoRepository : RepositoryBase, IDepartamentoRepository
    {
        public DepartamentoRepository(ConectarBanco conectarBanco) : base(conectarBanco)
        {
        }

        public async Task<IEnumerable<Departamento>> GetAll() =>
            await QueryAsync<Departamento>("SELECT * FROM Departamentos");

        public async Task<Departamento> GetById(int id) =>
            await QueryFirstOrDefaultAsync<Departamento>("SELECT * FROM Departamentos WHERE IdDepartamentos = @Id", new { Id = id });

        public async Task<Departamento> Add(Departamento departamento)
        {
            var sql = @"
                INSERT INTO Departamentos (Nome, Codigo, Descricao, DataCriacao)
                VALUES (@Nome, @Codigo, @Descricao, @DataCriacao);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            departamento.IdDepartamentos = await ExecuteScalarAsync<int>(sql, departamento);
            return departamento;
        }

        public async Task<bool> Update(Departamento departamento)
        {
            var sql = @"
                UPDATE Departamentos SET 
                    Nome = @Nome, 
                    Codigo = @Codigo, 
                    Descricao = @Descricao
                WHERE IdDepartamentos = @IdDepartamentos;";

            var affectedRows = await ExecuteAsync(sql, departamento);
            return affectedRows > 0;
        }

        public async Task<bool> Delete(int id)
        {
            var affectedRows = await ExecuteAsync("DELETE FROM Departamentos WHERE IdDepartamentos = @Id;", new { Id = id });
            return affectedRows > 0;
        }

        public async Task<bool> CodigoExists(string codigo)
        {
            var count = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Departamentos WHERE Codigo = @Codigo", new { Codigo = codigo });
            return count > 0;
        }
    }
}
```

- [ ] **Step 2: Refactor `DisciplinaRepository`**

Replace the full contents of `UniversidadeAPI/Repositories/DisciplinaRepository.cs`:

```csharp
using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public class DisciplinaRepository : RepositoryBase, IDisciplinaRepository
    {
        public DisciplinaRepository(ConectarBanco conectarBanco) : base(conectarBanco)
        {
        }

        public async Task<IEnumerable<Disciplina>> GetAll() =>
            await WithConnectionAsync(async conexao =>
            {
                var sql = @"
            SELECT 
                d.*, 
                c.IdCursos, c.Nome AS CursoNome, c.CargaHoraria AS CursoCargaHoraria, c.Departamentos_idDepartamentos,
                p.IdProfessores, p.Nome AS ProfessorNome
            FROM Disciplinas d
            LEFT JOIN Cursos c ON d.Curso_IdCursos = c.IdCursos
            LEFT JOIN Professores p ON d.Professor_IdProfessores = p.IdProfessores";

                return await conexao.QueryAsync<Disciplina, Curso, Professor, Disciplina>(
                    sql,
                    (disciplina, curso, professor) =>
                    {
                        disciplina.Curso = curso;
                        disciplina.Professor = professor;
                        return disciplina;
                    },
                    splitOn: "IdCursos,IdProfessores"
                );
            });

        public async Task<Disciplina> GetById(int id) =>
            await WithConnectionAsync(async conexao =>
            {
                var sql = @"
            SELECT 
                d.*, 
                c.IdCursos, c.Nome AS CursoNome, c.CargaHoraria AS CursoCargaHoraria, c.Departamentos_idDepartamentos,
                p.IdProfessores, p.Nome AS ProfessorNome
            FROM Disciplinas d
            LEFT JOIN Cursos c ON d.Curso_IdCursos = c.IdCursos
            LEFT JOIN Professores p ON d.Professor_IdProfessores = p.IdProfessores
            WHERE d.IdDisciplina = @IdDisciplina";

                var disciplinas = await conexao.QueryAsync<Disciplina, Curso, Professor, Disciplina>(
                    sql,
                    (disciplina, curso, professor) =>
                    {
                        disciplina.Curso = curso;
                        disciplina.Professor = professor;
                        return disciplina;
                    },
                    new { IdDisciplina = id },
                    splitOn: "IdCursos,IdProfessores"
                );

                return disciplinas.FirstOrDefault();
            });

        public async Task<IEnumerable<Disciplina>> GetByCurso(int cursoId) =>
            await QueryAsync<Disciplina>("SELECT * FROM Disciplinas WHERE Curso_IdCursos = @CursoId", new { CursoId = cursoId });

        public async Task<IEnumerable<Disciplina>> GetByProfessor(int professorId) =>
            await QueryAsync<Disciplina>("SELECT * FROM Disciplinas WHERE Professor_IdProfessores = @ProfessorId", new { ProfessorId = professorId });

        public async Task<Disciplina> Add(Disciplina disciplina)
        {
            var sql = @"
                INSERT INTO Disciplinas (Nome, Codigo, CargaHoraria, Creditos, Ementa, Curso_IdCursos, Professor_IdProfessores)
                VALUES (@Nome, @Codigo, @CargaHoraria, @Creditos, @Ementa, @Curso_IdCursos, @Professor_IdProfessores);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            disciplina.IdDisciplina = await ExecuteScalarAsync<int>(sql, new
            {
                disciplina.Nome,
                disciplina.Codigo,
                disciplina.CargaHoraria,
                disciplina.Creditos,
                disciplina.Ementa,
                disciplina.Curso_IdCursos,
                disciplina.Professor_IdProfessores
            });
            return disciplina;
        }

        public async Task<bool> Update(Disciplina disciplina)
        {
            var sql = @"
                UPDATE Disciplinas SET 
                    Nome = @Nome, 
                    Codigo = @Codigo, 
                    CargaHoraria = @CargaHoraria,
                    Creditos = @Creditos,
                    Ementa = @Ementa,
                    Curso_IdCursos = @Curso_IdCursos,
                    Professor_IdProfessores = @Professor_IdProfessores
                WHERE IdDisciplina = @IdDisciplina;";

            var affectedRows = await ExecuteAsync(sql, new
            {
                disciplina.IdDisciplina,
                disciplina.Nome,
                disciplina.Codigo,
                disciplina.CargaHoraria,
                disciplina.Creditos,
                disciplina.Ementa,
                disciplina.Curso_IdCursos,
                disciplina.Professor_IdProfessores
            });
            return affectedRows > 0;
        }

        public async Task<bool> Delete(int id)
        {
            await ExecuteAsync("DELETE FROM Disciplinas WHERE IdDisciplina = @IdDisciplina", new { IdDisciplina = id });
            return true;
        }

        public async Task<bool> CodigoExists(string codigo)
        {
            var count = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Disciplinas WHERE Codigo = @Codigo", new { Codigo = codigo });
            return count > 0;
        }
    }
}
```

- [ ] **Step 3: Refactor `UsuarioRepository`**

Replace the full contents of `UniversidadeAPI/Repositories/UsuarioRepository.cs`:

```csharp
using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public class UsuarioRepository : RepositoryBase, IUsuarioRepository
    {
        public UsuarioRepository(ConectarBanco conectarBanco) : base(conectarBanco)
        {
        }

        public async Task<Usuario> GetByUsername(string username) =>
            await QueryFirstOrDefaultAsync<Usuario>("SELECT * FROM Usuario WHERE Username = @Username", new { Username = username });

        public async Task<Usuario> GetById(int id) =>
            await QueryFirstOrDefaultAsync<Usuario>("SELECT * FROM Usuario WHERE Id = @Id", new { Id = id });

        public async Task<Usuario> Add(Usuario usuario)
        {
            var sql = @"
                INSERT INTO Usuario (Username, PasswordHash, Email, DataCriacao)
                VALUES (@Username, @PasswordHash, @Email, @DataCriacao);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            usuario.Id = await ExecuteScalarAsync<int>(sql, usuario);
            return usuario;
        }

        public async Task<bool> UsernameExists(string username)
        {
            var count = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Usuario WHERE Username = @Username", new { Username = username });
            return count > 0;
        }

        public async Task<bool> EmailExists(string email)
        {
            var count = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Usuario WHERE Email = @Email", new { Email = email });
            return count > 0;
        }
    }
}
```

- [ ] **Step 4: Refactor `CursoProfessorRepository`**

Replace the full contents of `UniversidadeAPI/Repositories/CursoProfessorRepository.cs`:

```csharp
namespace UniversidadeAPI.Repositories
{
    public class CursoProfessorRepository : RepositoryBase, ICursoProfessorRepository
    {
        public CursoProfessorRepository(ConectarBanco conectarBanco) : base(conectarBanco)
        {
        }

        public async Task AddCursoProfessor(int cursoId, int professorId)
        {
            var sql = @"
                INSERT INTO CursoProfessor (Cursos_IdCursos, Professores_IdProfessores)
                VALUES (@CursoId, @ProfessorId);";

            await ExecuteAsync(sql, new { CursoId = cursoId, ProfessorId = professorId });
        }

        public async Task RemoveAllProfessoresByCurso(int cursoId)
        {
            await ExecuteAsync("DELETE FROM CursoProfessor WHERE Cursos_IdCursos = @CursoId;", new { CursoId = cursoId });
        }
    }
}
```

- [ ] **Step 5: Build and verify**

Run: `dotnet build`
Expected: `0 Erro(s)`.

Run: `grep -rn "_conectarBanco.CriarConexao" UniversidadeAPI/Repositories`
Expected: matches only in `RepositoryBase.cs` (every concrete repository now goes through it).

- [ ] **Step 6: Commit**

```bash
git add UniversidadeAPI/Repositories/DepartamentoRepository.cs UniversidadeAPI/Repositories/DisciplinaRepository.cs UniversidadeAPI/Repositories/UsuarioRepository.cs UniversidadeAPI/Repositories/CursoProfessorRepository.cs
git commit -m "refactor: migrate remaining repositories onto RepositoryBase"
```

---

### Task 6: Add centralized exception-handling middleware

**Files:**
- Create: `UniversidadeAPI/Middleware/ExceptionHandlingMiddleware.cs`
- Modify: `UniversidadeAPI/Program.cs`

**Interfaces:**
- Produces: `ExceptionHandlingMiddleware`, an ASP.NET Core middleware registered via
  `app.UseMiddleware<ExceptionHandlingMiddleware>()`. Maps `ArgumentException` → 400,
  `UnauthorizedAccessException` → 401, anything else → 500, always as `{ "message": "..." }` JSON.
  Task 7 relies on this being registered before it strips `try/catch` blocks out of the
  controllers.

- [ ] **Step 1: Create the middleware**

Create `UniversidadeAPI/Middleware/ExceptionHandlingMiddleware.cs`:

```csharp
using System.Net;
using System.Text.Json;

namespace UniversidadeAPI.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentException ex)
            {
                await WriteResponseAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                await WriteResponseAsync(context, HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro não tratado ao processar {Method} {Path}", context.Request.Method, context.Request.Path);
                await WriteResponseAsync(context, HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado.");
            }
        }

        private static async Task WriteResponseAsync(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var payload = JsonSerializer.Serialize(new { message });
            await context.Response.WriteAsync(payload);
        }
    }
}
```

- [ ] **Step 2: Register the middleware in `Program.cs`**

In `UniversidadeAPI/Program.cs`, add the using at the top (alongside the existing usings):

```csharp
using UniversidadeAPI.Middleware;
```

Then find:

```csharp
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
```

and insert the middleware registration between those two lines, so it's the very first thing in
the pipeline:

```csharp
            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
```

- [ ] **Step 3: Build and verify**

Run: `dotnet build`
Expected: `0 Erro(s)`.

- [ ] **Step 4: Commit**

```bash
git add UniversidadeAPI/Middleware/ExceptionHandlingMiddleware.cs UniversidadeAPI/Program.cs
git commit -m "feat: add centralized exception-handling middleware"
```

---

### Task 7: Simplify controllers — remove duplicated try/catch

Every controller repeats the same `catch (ArgumentException ex) { return BadRequest(...); }`
pattern that the middleware from Task 6 now handles centrally. This task strips that duplication
so controllers only orchestrate service calls and HTTP results.

**Files:**
- Modify: `UniversidadeAPI/Controllers/AlunosController.cs`
- Modify: `UniversidadeAPI/Controllers/AuthController.cs`
- Modify: `UniversidadeAPI/Controllers/CursosController.cs`
- Modify: `UniversidadeAPI/Controllers/DepartamentoController.cs`
- Modify: `UniversidadeAPI/Controllers/DisciplinasController.cs`
- Modify: `UniversidadeAPI/Controllers/ProfessoresController.cs`

**Interfaces:** No signature changes — only removal of `try/catch` blocks. Every controller
method keeps the same route, verb, and return type.

- [ ] **Step 1: Simplify `AlunosController`**

Replace the full contents of `UniversidadeAPI/Controllers/AlunosController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversidadeAPI.Models;
using UniversidadeAPI.Services;

namespace UniversidadeAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AlunosController : ControllerBase
    {
        private readonly IAlunoService _alunoService;

        public AlunosController(IAlunoService alunoService)
        {
            _alunoService = alunoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Aluno>>> GetAlunos()
        {
            var alunos = await _alunoService.GetAllAlunos();
            return Ok(alunos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Aluno>> GetAluno(int id)
        {
            var aluno = await _alunoService.GetAlunoById(id);

            if (aluno == null)
            {
                return NotFound();
            }
            return Ok(aluno);
        }

        [HttpPost]
        public async Task<ActionResult<Aluno>> PostAluno(Aluno aluno)
        {
            var newAluno = await _alunoService.AddAluno(aluno);
            return CreatedAtAction(nameof(GetAluno), new { id = newAluno.Id }, newAluno);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAluno(int id, Aluno aluno)
        {
            if (id != aluno.Id)
            {
                return BadRequest("O ID na URL não corresponde ao ID do aluno no corpo da requisição.");
            }

            var updated = await _alunoService.UpdateAluno(aluno);

            if (updated)
            {
                return NoContent();
            }

            return NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAluno(int id)
        {
            if (await _alunoService.DeleteAluno(id))
            {
                return NoContent();
            }

            return NotFound();
        }
    }
}
```

- [ ] **Step 2: Simplify `AuthController`**

Replace the full contents of `UniversidadeAPI/Controllers/AuthController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using UniversidadeAPI.Models;
using UniversidadeAPI.Services;

namespace UniversidadeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.Login(request);
            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<ActionResult<Usuario>> Register([FromBody] RegistroRequest request)
        {
            var usuario = await _authService.Register(request);
            return CreatedAtAction(nameof(Register), new { id = usuario.Id }, new
            {
                id = usuario.Id,
                username = usuario.Username,
                email = usuario.Email,
                dataCriacao = usuario.DataCriacao
            });
        }
    }
}
```

- [ ] **Step 3: Simplify `CursosController`**

Replace the full contents of `UniversidadeAPI/Controllers/CursosController.cs`:

```csharp
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
```

- [ ] **Step 4: Simplify `DepartamentosController`**

Replace the full contents of `UniversidadeAPI/Controllers/DepartamentoController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversidadeAPI.Models;
using UniversidadeAPI.Services;

namespace UniversidadeAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DepartamentosController : ControllerBase
    {
        private readonly IDepartamentoService _departamentoService;

        public DepartamentosController(IDepartamentoService departamentoService)
        {
            _departamentoService = departamentoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Departamento>>> GetAllDepartamentos()
        {
            var departamentos = await _departamentoService.GetAllDepartamentos();
            return Ok(departamentos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Departamento>> GetDepartamentoById(int id)
        {
            var departamento = await _departamentoService.GetDepartamentoById(id);

            if (departamento == null)
            {
                return NotFound(new { message = $"Departamento com ID {id} não encontrado." });
            }

            return Ok(departamento);
        }

        [HttpPost]
        public async Task<ActionResult<Departamento>> AddDepartamento([FromBody] Departamento departamento)
        {
            var newDepartamento = await _departamentoService.AddDepartamento(departamento);
            return CreatedAtAction(nameof(GetDepartamentoById), new { id = newDepartamento.IdDepartamentos }, newDepartamento);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartamento(int id, [FromBody] Departamento departamento)
        {
            if (id != departamento.IdDepartamentos)
            {
                return BadRequest(new { message = "O ID na URL não corresponde ao ID do departamento no corpo da requisição." });
            }

            var updated = await _departamentoService.UpdateDepartamento(departamento);

            if (updated)
            {
                return NoContent();
            }

            return NotFound(new { message = $"Departamento com ID {id} não encontrado." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartamento(int id)
        {
            var deleted = await _departamentoService.DeleteDepartamento(id);

            if (deleted)
            {
                return NoContent();
            }

            return NotFound(new { message = $"Departamento com ID {id} não encontrado." });
        }
    }
}
```

- [ ] **Step 5: Simplify `DisciplinasController`**

Replace the full contents of `UniversidadeAPI/Controllers/DisciplinasController.cs`:

```csharp
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
```

- [ ] **Step 6: Simplify `ProfessoresController`**

Replace the full contents of `UniversidadeAPI/Controllers/ProfessoresController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversidadeAPI.Models;
using UniversidadeAPI.Services;

namespace UniversidadeAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProfessoresController : ControllerBase
    {
        private readonly IProfessorService _professorService;

        public ProfessoresController(IProfessorService professorService)
        {
            _professorService = professorService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Professor>>> GetAllProfessores()
        {
            var professores = await _professorService.GetAllProfessores();
            return Ok(professores);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Professor>> GetProfessorById(int id)
        {
            var professor = await _professorService.GetProfessorById(id);

            if (professor == null)
            {
                return NotFound();
            }
            return Ok(professor);
        }

        [HttpPost]
        public async Task<ActionResult<Professor>> AddProfessor(Professor professor)
        {
            var newProfessor = await _professorService.AddProfessor(professor);
            return CreatedAtAction(nameof(GetProfessorById), new { id = newProfessor.IdProfessores }, newProfessor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfessor(int id, Professor professor)
        {
            if (id != professor.IdProfessores)
            {
                return BadRequest("O ID na URL não corresponde ao ID do professor no corpo da requisição.");
            }

            var updated = await _professorService.UpdateProfessor(professor);

            if (updated)
            {
                return NoContent();
            }

            return NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfessor(int id)
        {
            if (await _professorService.DeleteProfessor(id))
            {
                return NoContent();
            }

            return NotFound();
        }
    }
}
```

- [ ] **Step 7: Build and verify**

Run: `dotnet build`
Expected: `0 Erro(s)`.

Run: `grep -rln "try" UniversidadeAPI/Controllers`
Expected: no matches — no controller catches exceptions anymore; the middleware from Task 6
handles all of them.

- [ ] **Step 8: Commit**

```bash
git add UniversidadeAPI/Controllers
git commit -m "refactor: remove duplicated try/catch from controllers"
```

---

### Task 8: Switch password hashing to BCrypt

**Files:**
- Modify: `UniversidadeAPI/UniversidadeAPI.csproj`
- Modify: `UniversidadeAPI/Services/AuthService.cs`

**Interfaces:** `AuthService.HashPassword`/`VerifyPassword` stay `private` with the same
signatures (`string HashPassword(string password)`, `bool VerifyPassword(string password, string
passwordHash)`) — `Login`/`Register` on `IAuthService` are unchanged.

- [ ] **Step 1: Add the BCrypt package**

In `UniversidadeAPI/UniversidadeAPI.csproj`, add inside the existing `<ItemGroup>` with the other
`PackageReference` entries:

```xml
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
```

Run: `dotnet restore`
Expected: restore succeeds.

- [ ] **Step 2: Replace the hashing implementation**

In `UniversidadeAPI/Services/AuthService.cs`, remove the `using System.Security.Cryptography;`
line, then replace:

```csharp
        private string HashPassword(string password)
        {
            // Usar SHA256 para hash simples (para produ��o, use BCrypt ou PBKDF2)
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            var hash = HashPassword(password);
            return hash == passwordHash;
        }
```

with:

```csharp
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
```

- [ ] **Step 3: Build and verify**

Run: `dotnet build`
Expected: `0 Erro(s)`.

- [ ] **Step 4: Commit**

```bash
git add UniversidadeAPI/UniversidadeAPI.csproj UniversidadeAPI/Services/AuthService.cs
git commit -m "fix: hash passwords with BCrypt instead of unsalted SHA256"
```

---

### Task 9: Docker support and README updates

**Files:**
- Create: `Dockerfile` (repo root)
- Create: `.dockerignore` (repo root)
- Create: `docker-compose.yml` (repo root)
- Create: `.env.example` (repo root)
- Modify: `.gitignore` (repo root, from Task 1 — add `.env` if not already present; it already is)
- Modify: `README.md` (repo root)

**Interfaces:** None — this is packaging/documentation only, no application code changes.

- [ ] **Step 1: Create the `Dockerfile`**

Create `Dockerfile` at the repo root (same level as `UniversidadeAPI.sln`):

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY UniversidadeAPI/UniversidadeAPI.csproj UniversidadeAPI/
RUN dotnet restore UniversidadeAPI/UniversidadeAPI.csproj

COPY UniversidadeAPI/ UniversidadeAPI/
RUN dotnet publish UniversidadeAPI/UniversidadeAPI.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "UniversidadeAPI.dll"]
```

- [ ] **Step 2: Create `.dockerignore`**

Create `.dockerignore` at the repo root:

```
**/bin/
**/obj/
**/.vs/
.git/
.env
```

- [ ] **Step 3: Create `docker-compose.yml`**

Create `docker-compose.yml` at the repo root:

```yaml
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: ${MSSQL_SA_PASSWORD}
    ports:
      - "1433:1433"
    volumes:
      - mssql-data:/var/opt/mssql

  api:
    build:
      context: .
      dockerfile: Dockerfile
    environment:
      ASPNETCORE_URLS: "http://+:8080"
      ConnectionStrings__DefaultConnection: "Server=db;Database=mydb;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=True;"
      JwtSettings__SecretKey: ${JWT_SECRET_KEY}
      JwtSettings__Issuer: UniversidadeAPI
      JwtSettings__Audience: UniversidadeAPIUsers
      JwtSettings__ExpirationHours: "8"
    ports:
      - "8080:8080"
    depends_on:
      - db

volumes:
  mssql-data:
```

- [ ] **Step 4: Create `.env.example`**

Create `.env.example` at the repo root:

```
MSSQL_SA_PASSWORD=DefinaUmaSenhaForte123!
JWT_SECRET_KEY=sua-chave-secreta-super-segura-com-pelo-menos-32-caracteres
```

- [ ] **Step 5: Verify `.gitignore` already covers `.env`**

Run: `grep -n "^\.env$" .gitignore`
Expected: one match (added in Task 1, Step 1). If missing, append `.env` to `.gitignore` now.

- [ ] **Step 6: Update the README**

Replace the full contents of `README.md` (repo root):

```markdown
# UniversidadeAPI

Projeto: UniversidadeAPI
Plataforma: .NET 8 (C# 12)
Banco: SQL Server (script: `create_database`)
ORM: Dapper
Auth: JWT

## Opção 1: Rodar com Docker (recomendado)

1. Copie `.env.example` para `.env` e defina uma senha forte para `MSSQL_SA_PASSWORD` e uma chave
   para `JWT_SECRET_KEY` (mínimo 32 caracteres):
   ```
   cp .env.example .env
   ```
2. Suba os containers:
   ```
   docker compose up -d
   ```
3. Na primeira vez, rode o script `create_database` contra o container do banco (porta `1433`
   exposta no host):
   ```
   sqlcmd -S localhost,1433 -U sa -P "<mesma senha do .env>" -i create_database
   ```
   (ou abra no Azure Data Studio / SSMS apontando para `localhost,1433`)
4. A API estará em `http://localhost:8080`, com Swagger em `http://localhost:8080/swagger`.

## Opção 2: Rodar localmente sem Docker

1. Configure a connection string via `dotnet user-secrets` (evita colocar a senha real no
   `appsettings.json`, que é versionado):
   ```
   cd UniversidadeAPI
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\SQLEXPRESS;Database=mydb;User Id=sa;Password=SuaSenha;TrustServerCertificate=True;"
   dotnet user-secrets set "JwtSettings:SecretKey" "sua-chave-secreta-super-segura-com-pelo-menos-32-caracteres"
   ```
2. Crie o banco (SQL Server):
   ```
   sqlcmd -S .\SQLEXPRESS -U sa -P "SuaSenha" -i create_database
   ```
   (ou abra no SSMS e execute)
3. Execute a API:
   ```
   cd UniversidadeAPI
   dotnet restore
   dotnet run
   ```
4. Swagger: `http://localhost:{porta}/swagger`

## Segurança

- Nunca comite senhas reais em `appsettings.json` — use `dotnet user-secrets` (local) ou `.env`
  (Docker), ambos fora do controle de versão.
- Se você clonou este repositório antes desta atualização: a senha antiga do banco ficou exposta
  no histórico do GitHub. Troque essa senha no seu SQL Server.

## Arquivos úteis

- `UniversidadeAPI/ConectarBanco.cs` — cria a conexão SQL Server
- `create_database` — script T-SQL de criação do banco (inclui a tabela `CursoProfessor`)
- `UniversidadeAPI/Controllers/` — controllers REST
- `UniversidadeAPI/Repositories/` e `UniversidadeAPI/Services/` — lógica de dados
- `UniversidadeAPI/Repositories/RepositoryBase.cs` — helpers de acesso a dados compartilhados por
  todos os repositórios
- `UniversidadeAPI/Middleware/ExceptionHandlingMiddleware.cs` — tratamento central de erros da API
```

- [ ] **Step 7: Verify the compose file and image build**

Run: `docker compose config`
Expected: prints the resolved compose configuration with no errors (this only validates syntax
and variable substitution — it does not start anything).

Run: `docker compose build`
Expected: the `api` image builds successfully (this runs `dotnet publish` inside the build stage,
which re-validates that the whole project still compiles).

(This step does not run `docker compose up` — starting the SQL Server container and binding ports
is left for the user to do when they're ready, since it consumes real resources on their machine.)

- [ ] **Step 8: Commit**

```bash
git add Dockerfile .dockerignore docker-compose.yml .env.example README.md
git commit -m "docs: add Docker support and update README with setup instructions"
```

---

## Final verification (after all tasks)

- [ ] Run `dotnet build` from the repo root — expect `0 Erro(s)`.
- [ ] Run `grep -rn "LAST_INSERT_ID\|MySql\|SHA256" UniversidadeAPI` — expect no matches.
- [ ] Run `git log --oneline -10` — expect 9 new commits, one per task, on top of the spec commit.
- [ ] Manually re-read `Program.cs` end-to-end and confirm every repository interface registered
  in DI (`IAlunoRepository` ... `ICursoProfessorRepository`) has exactly one concrete
  implementation registered, and that `ExceptionHandlingMiddleware` is registered before
  `UseAuthentication`.
- [ ] Remind the user (this cannot be done by an agent without their credentials): rotate the SQL
  Server password that was previously committed to GitHub, and run the updated `create_database`
  script — including the new `CursoProfessor` table — against their actual database before using
  the API.
