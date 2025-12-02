# ? CHECKLIST COMPLETO - API PRONTA PARA FRONTEND

## ?? RESUMO: TUDO IMPLEMENTADO!

### ? 1. API de Departamentos
**Controller:** `DepartamentosController.cs`  
**Endpoints disponíveis:**
```
GET    /api/departamentos           - Listar todos
GET    /api/departamentos/{id}      - Buscar por ID
POST   /api/departamentos           - Criar
PUT    /api/departamentos/{id}      - Atualizar
DELETE /api/departamentos/{id}      - Deletar (CASCADE)
```

**Exemplo POST:**
```json
{
  "nome": "Departamento de Artes",
  "codigo": "ART",
  "descricao": "Departamento de artes visuais"
}
```

---

### ? 2. API de Disciplinas/Matérias
**Controller:** `DisciplinasController.cs`  
**Endpoints disponíveis:**
```
GET    /api/disciplinas                  - Listar todas
GET    /api/disciplinas/{id}             - Buscar por ID
GET    /api/disciplinas/curso/{cursoId}  - Por curso
GET    /api/disciplinas/professor/{profId} - Por professor
POST   /api/disciplinas                  - Criar
PUT    /api/disciplinas/{id}             - Atualizar
DELETE /api/disciplinas/{id}             - Deletar (CASCADE)
```

**Exemplo POST:**
```json
{
  "nome": "Programação Web",
  "codigo": "CC301",
  "cargaHoraria": 80,
  "creditos": 4,
  "ementa": "HTML, CSS, JavaScript, React",
  "curso_IdCursos": 1,
  "professor_IdProfessores": 1
}
```

---

### ? 3. CASCADE DELETE Configurado
**Script:** `Scripts/recreate_database_with_cascade.sql`

**Comportamento:**
- ? Deletar Departamento ? Deleta Cursos, Disciplinas, Matrículas, Notas
- ? Deletar Curso ? Deleta Disciplinas, Matrículas, Notas
- ? Deletar Professor ? Disciplinas ficam sem professor (SET NULL)
- ? Deletar Aluno ? Deleta Matrículas e Notas
- ? Deletar Disciplina ? Deleta Notas

---

### ? 4. Erro de SQL Corrigido
**Arquivo:** `CursoRepository.cs`

**Correções feitas:**
1. ? Parâmetro `@Id` corrigido no método `GetById`
2. ? Adicionado `SELECT LAST_INSERT_ID()` no método `Add`
3. ? Removido using desnecessário
4. ? Formatação e organização do código

---

### ? 5. CORS Configurado
**Arquivo:** `Program.cs`

**Configuração adicionada:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ...

app.UseCors("AllowAll");
```

**Isso resolve o erro:** `Http failure response for http://localhost:5019/api/professores: 0 Unknown Error`

---

## ?? TODOS OS ENDPOINTS DISPONÍVEIS

### ?? Autenticação
```
POST /api/auth/login      - Fazer login
POST /api/auth/register   - Registrar usuário
```

### ?? Departamentos
```
GET    /api/departamentos
GET    /api/departamentos/{id}
POST   /api/departamentos
PUT    /api/departamentos/{id}
DELETE /api/departamentos/{id}
```

### ?? Cursos
```
GET    /api/cursos
GET    /api/cursos/{id}
POST   /api/cursos
PUT    /api/cursos/{id}
DELETE /api/cursos/{id}
```

### ????? Professores
```
GET    /api/professores
GET    /api/professores/{id}
POST   /api/professores
PUT    /api/professores/{id}
DELETE /api/professores/{id}
```

### ?? Disciplinas/Matérias
```
GET    /api/disciplinas
GET    /api/disciplinas/{id}
GET    /api/disciplinas/curso/{cursoId}
GET    /api/disciplinas/professor/{professorId}
POST   /api/disciplinas
PUT    /api/disciplinas/{id}
DELETE /api/disciplinas/{id}
```

### ????? Alunos
```
GET    /api/alunos
GET    /api/alunos/{id}
POST   /api/alunos
PUT    /api/alunos/{id}
DELETE /api/alunos/{id}
```

---

## ?? COMO USAR NO FRONTEND

### 1. Configurar URL Base
```typescript
// src/services/api.ts
const API_URL = 'http://localhost:5019/api';
```

### 2. Fazer Login
```typescript
const response = await fetch(`${API_URL}/auth/login`, {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    username: 'admin',
    password: 'admin123'
  })
});

const { token } = await response.json();
localStorage.setItem('token', token);
```

### 3. Criar Professor (exemplo do erro)
```typescript
const token = localStorage.getItem('token');

const response = await fetch(`${API_URL}/professores`, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`  // ? Incluir token
  },
  body: JSON.stringify({
    nome: 'Prof. João Silva'
  })
});

if (response.ok) {
  const professor = await response.json();
  console.log('Professor criado:', professor);
} else {
  const error = await response.json();
  console.error('Erro:', error);
}
```

### 4. Criar Disciplina
```typescript
const response = await fetch(`${API_URL}/disciplinas`, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({
    nome: 'Programação Web',
    codigo: 'CC301',
    cargaHoraria: 80,
    creditos: 4,
    ementa: 'HTML, CSS, JS, React',
    curso_IdCursos: 1,
    professor_IdProfessores: 1
  })
});
```

---

## ?? COMO RODAR A API

### 1. Execute o script SQL
```bash
mysql -u root -p < "Scripts/recreate_database_with_cascade.sql"
# Senha: 862945
```

### 2. Rode a API
```bash
cd UniversidadeAPI
dotnet run
```

### 3. Acesse o Swagger
```
http://localhost:5019/swagger
```

### 4. Teste no Postman ou Frontend
- **URL Base:** `http://localhost:5019/api`
- **Login:** `admin` / `admin123`
- **Use o token em todas as requisições**

---

## ? CHECKLIST FINAL

- ? **API de Departamentos** - IMPLEMENTADA
- ? **API de Disciplinas/Matérias** - IMPLEMENTADA
- ? **CASCADE DELETE** - CONFIGURADO
- ? **Erro SQL CursoRepository** - CORRIGIDO
- ? **CORS** - CONFIGURADO (resolve erro "0 Unknown Error")
- ? **JWT** - FUNCIONANDO
- ? **Swagger** - CONFIGURADO com ?? Authorize

---

## ?? CAMPOS OBRIGATÓRIOS POR ENTIDADE

### Departamento
```json
{
  "nome": "string" // ? OBRIGATÓRIO
}
```

### Curso
```json
{
  "nome": "string", // ? OBRIGATÓRIO
  "cargaHoraria": "string",
  "departamentos_idDepartamentos": 1 // ? OBRIGATÓRIO
}
```

### Professor
```json
{
  "nome": "string" // ? OBRIGATÓRIO
}
```

### Disciplina
```json
{
  "nome": "string", // ? OBRIGATÓRIO
  "cargaHoraria": 80, // ? OBRIGATÓRIO (int)
  "curso_IdCursos": 1, // ? OBRIGATÓRIO
  "professor_IdProfessores": 1 // Opcional
}
```

### Aluno
```json
{
  "nomeCompleto": "string", // ? OBRIGATÓRIO
  "dataNascimento": "2000-01-15", // ? OBRIGATÓRIO
  "cpf": "12345678901", // ? OBRIGATÓRIO
  "email": "aluno@email.com", // ? OBRIGATÓRIO
  "dataMatricula": "2025-01-15" // ? OBRIGATÓRIO
}
```

---

## ?? USUÁRIOS DE TESTE

| Username | Password | Email |
|----------|----------|-------|
| admin | admin123 | admin@universidade.com |
| professor1 | admin123 | professor@universidade.com |
| aluno1 | admin123 | aluno@universidade.com |

---

## ?? TUDO PRONTO!

? **CORS configurado** - Frontend pode conectar  
? **Todas as APIs implementadas**  
? **CASCADE DELETE funcionando**  
? **Erros corrigidos**  
? **Documentação completa**  

**Sua API está 100% funcional e pronta para integração com o frontend!** ??
