# UniversidadeAPI — Correção e Clean Code

## Contexto

O projeto (.NET 8, ASP.NET Core Web API, Dapper, JWT) hoje **não compila**: 20 erros de build,
todos originados da relação Curso↔Professor, que foi referenciada no código (modelos, service,
controller, DI) mas nunca implementada de fato (sem tabela no banco, sem classe `CursoProfessor`,
sem os DTOs `CreateCursoRequest`, `UpdateCursoRequest`, `CursoResponseComProfessores`, `ProfessorDTO`).

Além disso, o projeto está inconsistente entre dois bancos: `ConectarBanco` e todos os
repositórios usam `MySqlConnection`/sintaxe MySQL (`SELECT LAST_INSERT_ID()`), mas o script
`create_database` já é T-SQL puro (SQL Server: `IDENTITY`, `DATETIME2`, `GETDATE()`, `CHECK`,
`GO`). O README e o `appsettings.json` também apontam para SQL Server. Foi decidido padronizar
em **SQL Server**, que é o banco real do schema.

Este documento cobre a correção desses problemas e uma limpeza de "clean code" na base existente
(Controllers → Services → Repositories), sem trocar a arquitetura geral nem migrar para EF Core.

## Objetivos

1. Fazer o projeto compilar e funcionar corretamente contra SQL Server.
2. Implementar de verdade a relação muitos-para-muitos Curso↔Professor.
3. Remover segredos do controle de versão (senha do banco exposta no GitHub).
4. Trocar o hash de senha (SHA256 puro) por BCrypt.
5. Reduzir a duplicação de código entre os 5 conjuntos de Controller/Service/Repository.
6. Adicionar suporte a Docker para facilitar rodar a API + banco localmente.

## Fora de escopo

- CORS `AllowAll` (mantido como está).
- Testes automatizados (decisão do usuário: não incluir nesta rodada).
- A rota alias `/api/materias` em `DisciplinasController` (é intencional, mantida).
- Migração para EF Core ou qualquer outro ORM.

## 1. Relação Curso↔Professor

**Banco** — adicionar ao `create_database`, após a criação de `Cursos` e `Professores`:

```sql
CREATE TABLE CursoProfessor (
  Cursos_IdCursos INT NOT NULL,
  Professores_IdProfessores INT NOT NULL,
  PRIMARY KEY (Cursos_IdCursos, Professores_IdProfessores),
  CONSTRAINT fk_cursoprofessor_curso FOREIGN KEY (Cursos_IdCursos) REFERENCES Cursos(IdCursos) ON DELETE CASCADE,
  CONSTRAINT fk_cursoprofessor_professor FOREIGN KEY (Professores_IdProfessores) REFERENCES Professores(IdProfessores) ON DELETE CASCADE
);
```

**Modelo** — `Models/CursoProfessor.cs`, com `Cursos_IdCursos` e `Professores_IdProfessores`,
satisfazendo as propriedades `ICollection<CursoProfessor>` já declaradas em `Curso` e `Professor`.

**Repositório** — `Repositories/ICursoProfessorRepository.cs` / `CursoProfessorRepository.cs`:
- `Task AddCursoProfessor(int cursoId, int professorId)`
- `Task RemoveAllProfessoresByCurso(int cursoId)`

(a interface já está registrada em `Program.cs`; só faltam os arquivos)

**DTOs** — `Models/Dtos/`:
- `CreateCursoRequest` (Nome, CargaHoraria, Departamentos_idDepartamentos, `List<int>? Professores`)
- `UpdateCursoRequest` (idem + IdCursos)
- `ProfessorDTO` (IdProfessores, Nome)
- `CursoResponseComProfessores` (IdCursos, Nome, CargaHoraria, Departamentos_idDepartamentos, `List<ProfessorDTO> Professores`)

`CursoService`, `ICursoService` e `CursosController` já usam esses tipos — nenhuma mudança de
assinatura é necessária além de criar os arquivos.

## 2. Padronização em SQL Server

- `UniversidadeAPI.csproj`: remover `PackageReference` de `MySql.Data`, adicionar
  `Microsoft.Data.SqlClient`.
- `ConectarBanco.CriarConexao()`: retorna `new SqlConnection(_connectionString)`.
- Em todos os repositórios (`Aluno`, `Curso`, `Professor`, `Departamento`, `Disciplina`,
  `Usuario`), trocar `SELECT LAST_INSERT_ID();` por `SELECT CAST(SCOPE_IDENTITY() AS INT);`.
- Corrigir `AlunoRepository.Add`, que hoje faz só o `INSERT` sem `SELECT` de retorno — por isso
  `ExecuteScalarAsync<int>` sempre devolve `0` e o `Aluno.Id` nunca é preenchido corretamente.
- `DisciplinaRepository`: remover os blocos `try/catch (MySqlException)` — o tipo deixa de existir
  e o tratamento de erro passa a ser responsabilidade do middleware (seção 5).
- `appsettings.json`: connection string ajustada para o formato SQL Server, com placeholder no
  lugar da senha real.

## 3. Segredos

- `appsettings.json` (versionado) passa a ter placeholders: `"DefaultConnection":
  "Server=localhost\\SQLEXPRESS;Database=mydb;User Id=sa;Password=CHANGE_ME;TrustServerCertificate=True;"`.
- README documenta o uso de `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."`
  para desenvolvimento local, em vez de editar o `appsettings.json` versionado.
- Novo `.gitignore` na raiz cobrindo `bin/`, `obj/`, `.vs/`, `*.user`.
- **Ação recomendada para o usuário** (fora do que o Claude pode fazer): trocar a senha do SQL
  Server que ficou exposta no histórico público do GitHub.

## 4. Hash de senha

- Adicionar pacote `BCrypt.Net-Next`.
- `AuthService.HashPassword(password)` → `BCrypt.Net.BCrypt.HashPassword(password)`.
- `AuthService.VerifyPassword(password, hash)` → `BCrypt.Net.BCrypt.Verify(password, hash)`.
- Sem mudança de assinatura pública — `Login`/`Register` continuam iguais.
- **Nota de compatibilidade**: hashes SHA256 já gravados no banco não são compatíveis com
  `BCrypt.Verify`. Usuários cadastrados antes desta mudança precisarão se registrar novamente
  (aceitável — o `create_database` só insere dados de teste, não há usuários reais em produção).

## 5. Redução de duplicação

**`Repositories/RepositoryBase.cs`** (classe abstrata):
- Recebe `ConectarBanco` no construtor.
- Implementa uma vez `GetAllAsync<T>(string sql)`, `GetByIdAsync<T>(string sql, object parametros)`
  e `ExecuteAsync(string sql, object parametros)`, encapsulando o `await using (var conexao =
  _conectarBanco.CriarConexao())` que hoje se repete em ~30 métodos.
- `Add`/`Update` continuam implementados em cada repositório concreto (colunas diferentes por
  entidade — generalizar isso forçaria uma abstração artificial).
- Todos os repositórios (`Aluno`, `Curso`, `Professor`, `Departamento`, `Disciplina`, `Usuario`,
  `CursoProfessor`) passam a herdar de `RepositoryBase`.

**`Middleware/ExceptionHandlingMiddleware.cs`**:
- `ArgumentException` → 400 com `{ message }`.
- `UnauthorizedAccessException` → 401 com `{ message }`.
- Qualquer outra exceção → log via `ILogger<ExceptionHandlingMiddleware>` + 500 com mensagem
  genérica (não expor detalhes internos ao cliente).
- Registrado em `Program.cs` via `app.UseMiddleware<ExceptionHandlingMiddleware>()`, antes de
  `UseAuthentication`.
- Todos os `try/catch` hoje presentes em `AlunosController`, `AuthController`, `CursosController`,
  `DepartamentosController`, `DisciplinasController`, `ProfessoresController` são removidos —
  controllers passam a só orquestrar chamada ao service e devolver o resultado HTTP.

## 6. Docker

- `Dockerfile` multi-stage: estágio `build` com SDK .NET 8 (`dotnet restore` + `dotnet publish`),
  estágio final com `aspnet:8.0` rodando o binário publicado.
- `docker-compose.yml` com dois serviços:
  - `api`: build do `Dockerfile`, expõe a porta da API, `DefaultConnection` apontando para o
    serviço `db` via variável de ambiente/user-secrets do compose.
  - `db`: imagem `mcr.microsoft.com/mssql/server:2022-latest`, com `SA_PASSWORD` via variável de
    ambiente do compose (não hardcoded no arquivo).
- README documenta `docker compose up` e o passo único de rodar `create_database.sql` contra o
  container `db` na primeira vez (via `sqlcmd` local, `docker exec` ou SSMS apontando pra porta
  exposta).

## Verificação

- `dotnet build` sem erros (hoje falha com 20 erros — critério objetivo de sucesso).
- Revisão manual de cada fluxo: login/registro, CRUD de Aluno/Curso/Professor/Departamento/
  Disciplina, criação/atualização de Curso com lista de professores.
- **Limitação conhecida**: não há uma instância real de SQL Server neste ambiente de execução do
  Claude Code, então não é possível rodar a API de ponta a ponta contra o banco aqui. Isso precisa
  ser validado localmente (ou via `docker compose up`) pelo usuário após a implementação.
