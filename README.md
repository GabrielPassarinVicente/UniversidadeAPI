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
   sqlcmd -S localhost,1433 -U sa -P "<mesma senha do .env>" -C -f 65001 -i create_database
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
   dotnet user-secrets set "JwtSettings:SecretKey" "CHANGE_ME_JWT_SECRET_KEY_MIN_32_CHARS"
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
