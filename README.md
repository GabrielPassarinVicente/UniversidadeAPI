UniversidadeAPI

Projeto: UniversidadeAPI
Plataforma: .NET 8 (C# 12)
Banco: SQL Server (script: `UniversidadeAPI/Scripts/create_database.sql`)
ORM: Dapper
Auth: JWT

Rápido setup:
1. Ajuste a connection string em `UniversidadeAPI/appsettings.json`:
   "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=mydb;User Id=sa;Password=SuaSenha;TrustServerCertificate=True;"
2. Crie o banco (SQL Server):
   sqlcmd -S .\SQLEXPRESS -U sa -P "SuaSenha" -i "UniversidadeAPI\Scripts\create_database.sql"
   (ou abra no SSMS e execute)
3. Execute a API:
   cd UniversidadeAPI
   dotnet restore
   dotnet run
4. Swagger: http://localhost:{porta}/swagger

Observações rápidas:
- Se usar MySQL, use o script MySQL (se disponível) e ajuste a connection string.
- Não execute o script em produção (ele recria o banco).
- Para problemas com FK ou colunas, verifique nomes de colunas no modelo (ex.: IdDisciplina).

Arquivos úteis:
- `UniversidadeAPI/ConectarBanco.cs` — cria DbConnection
- `UniversidadeAPI/Scripts/create_database.sql` — script T-SQL
- `UniversidadeAPI/Controllers/` — controllers REST
- `UniversidadeAPI/Repositories/` e `UniversidadeAPI/Services/` — lógica de dados


