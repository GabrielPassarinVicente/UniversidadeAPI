# ? SISTEMA COMPLETO - UNIVERSIDADE API

## ?? O QUE FOI IMPLEMENTADO

### ?? Autenticação JWT
- ? Login e Registro
- ? Token JWT com 8h de validade
- ? Swagger com botão de autorização (??)
- ? Todos os endpoints protegidos

### ?? CRUD Completo
- ? **Departamentos** (Nome, Código, Descrição)
- ? **Cursos** (vinculado a Departamento)
- ? **Professores**
- ? **Disciplinas/Matérias** (vinculado a Curso e Professor)
- ? **Alunos**
- ? **Matrículas**
- ? **Notas**

### ??? DELETE CASCADE
- ? **Deletar Departamento** ? Deleta Cursos, Disciplinas, Matrículas, Notas
- ? **Deletar Curso** ? Deleta Disciplinas, Matrículas, Notas
- ? **Deletar Professor** ? Disciplinas ficam sem professor (SET NULL)
- ? **Deletar Aluno** ? Deleta Matrículas e Notas
- ? **Deletar Disciplina** ? Deleta Notas
- ? **Pode deletar qualquer coisa sem erro de constraint!**

---

## ?? COMO USAR

### 1?? Executar Script SQL
```bash
# No MySQL Workbench ou linha de comando:
mysql -u root -p < "Scripts/recreate_database_with_cascade.sql"
```

### 2?? Rodar a API
```bash
cd UniversidadeAPI
dotnet run
```

### 3?? Acessar Swagger
```
http://localhost:5000/swagger
```

### 4?? Fazer Login
- Endpoint: `POST /api/auth/login`
- Body:
```json
{
  "username": "admin",
  "password": "admin123"
}
```

### 5?? Autorizar no Swagger
1. Clique no botão **?? Authorize**
2. Cole o token retornado
3. Clique em **Authorize**

### 6?? Usar os Endpoints
Todos os endpoints estão disponíveis e protegidos:
- `/api/departamentos`
- `/api/cursos`
- `/api/professores`
- `/api/disciplinas`
- `/api/alunos`

---

## ?? ENDPOINTS PRINCIPAIS

### Departamentos
```http
GET    /api/departamentos
POST   /api/departamentos  {"nome": "Artes"}
PUT    /api/departamentos/1
DELETE /api/departamentos/1  ? CASCADE DELETE
```

### Disciplinas (Matérias)
```http
GET    /api/disciplinas
GET    /api/disciplinas/curso/1        ? Por curso
GET    /api/disciplinas/professor/1    ? Por professor
POST   /api/disciplinas
PUT    /api/disciplinas/1
DELETE /api/disciplinas/1  ? CASCADE DELETE
```

**Criar Disciplina:**
```json
{
  "nome": "Programação Web",
  "codigo": "CC301",
  "cargaHoraria": 80,
  "creditos": 4,
  "curso_IdCursos": 1,
  "professor_IdProfessores": 1
}
```

---

## ?? PRONTO!

? **API Completa**  
? **JWT Funcionando**  
? **DELETE CASCADE Configurado**  
? **Banco com Dados de Exemplo**  
? **Swagger Configurado**  
? **Frontend Pode Conectar**  

**TUDO FUNCIONANDO! ??**
