# Autenticação JWT - UniversidadeAPI

## Configuração

### 1. Criar a tabela no banco de dados
Execute o script SQL localizado em `Scripts/create_usuario_table.sql` no seu banco de dados MySQL.

```sql
CREATE TABLE IF NOT EXISTS Usuario (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    DataCriacao DATETIME NOT NULL,
    INDEX idx_username (Username),
    INDEX idx_email (Email)
);
```

### 2. Configurar chave secreta (IMPORTANTE para produção)
No arquivo `appsettings.json`, altere a chave secreta em `JwtSettings.SecretKey` para uma chave única e segura.

## Endpoints de Autenticação

### Registrar novo usuário
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "usuario_teste",
  "password": "senha123",
  "email": "usuario@teste.com"
}
```

**Resposta de sucesso (201):**
```json
{
  "id": 1,
  "username": "usuario_teste",
  "email": "usuario@teste.com",
  "dataCriacao": "2025-01-15T10:30:00Z"
}
```

### Login (obter token)
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "usuario_teste",
  "password": "senha123"
}
```

**Resposta de sucesso (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2025-01-15T18:30:00Z",
  "username": "usuario_teste",
  "email": "usuario@teste.com"
}
```

## Usando o Token

Depois de fazer login, use o token retornado nas requisições protegidas:

```http
GET /api/alunos
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Protegendo Endpoints

Para proteger um endpoint, adicione o atributo `[Authorize]` no controller ou método:

```csharp
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Protege todos os métodos do controller
public class AlunosController : ControllerBase
{
    // ...
}
```

Ou proteger apenas métodos específicos:

```csharp
[HttpGet]
[Authorize] // Protege apenas este método
public async Task<ActionResult<IEnumerable<Aluno>>> GetAlunos()
{
    // ...
}
```

## Testando no Postman

1. **Registrar usuário:**
   - Método: POST
   - URL: `http://localhost:5000/api/auth/register`
   - Body (JSON):
     ```json
     {
       "username": "teste",
       "password": "senha123",
       "email": "teste@email.com"
     }
     ```

2. **Fazer Login:**
   - Método: POST
   - URL: `http://localhost:5000/api/auth/login`
   - Body (JSON):
     ```json
     {
       "username": "teste",
       "password": "senha123"
     }
     ```
   - Copie o valor do campo `token` da resposta

3. **Usar o Token:**
   - Em qualquer requisição protegida, vá em "Authorization"
   - Selecione "Bearer Token"
   - Cole o token copiado

## Integração com Frontend

No seu frontend, após o login bem-sucedido:

```javascript
// Fazer login
const response = await fetch('http://localhost:5000/api/auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    username: 'usuario',
    password: 'senha'
  })
});

const data = await response.json();
// Guardar o token (localStorage, sessionStorage, etc.)
localStorage.setItem('token', data.token);

// Usar o token em outras requisições
const alunosResponse = await fetch('http://localhost:5000/api/alunos', {
  headers: {
    'Authorization': `Bearer ${localStorage.getItem('token')}`
  }
});
```

## Configuração CORS (se necessário para o frontend)

Se você estiver conectando de um frontend em outro domínio, adicione CORS no `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // URL do seu frontend
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ...

app.UseCors("AllowFrontend");
```

## Configurações JWT (appsettings.json)

- **SecretKey**: Chave secreta para assinar o token (mínimo 32 caracteres)
- **Issuer**: Emissor do token (nome da sua API)
- **Audience**: Público-alvo do token
- **ExpirationHours**: Tempo de validade do token em horas
