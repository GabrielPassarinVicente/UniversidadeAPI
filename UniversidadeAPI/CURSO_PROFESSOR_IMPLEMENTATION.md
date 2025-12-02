# Implementação Relação Many-to-Many: Curso ? Professor

## Resumo das Alterações

Este documento descreve a implementação da relação many-to-many entre Cursos e Professores no backend da UniversidadeAPI.

## 1. Modelos Criados/Atualizados

### Models
- **CursoProfessor.cs** - Nova entidade para tabela de junção
- **Curso.cs** - Atualizado com propriedades de navegação
- **Professor.cs** - Atualizado com propriedades de navegação
- **CreateCursoRequest.cs** - DTO para criar curso com professores
- **UpdateCursoRequest.cs** - DTO para atualizar curso com professores
- **CursoResponseComProfessores.cs** - DTO para resposta com professores

## 2. Repositórios

### ICursoProfessorRepository
Interface com métodos:
- `GetProfessoresByCursoId(int cursoId)` - Obter professores de um curso
- `GetCursosByProfessorId(int professorId)` - Obter cursos de um professor
- `AddCursoProfessor(int cursoId, int professorId)` - Vincular professor a curso
- `RemoveCursoProfessor(int cursoId, int professorId)` - Desvincular
- `RemoveAllProfessoresByCurso(int cursoId)` - Remover todos
- `ProfessorExistInCurso(int cursoId, int professorId)` - Verificar vínculo

### CursoProfessorRepository
Implementação com Dapper para operações na tabela CursoProfessor.

### Atualizações em ICursoRepository
Novos métodos:
- `GetByIdWithProfessores(int id)` - Obter curso com professores
- `GetAllWithProfessores()` - Obter todos os cursos com professores

## 3. Banco de Dados

### Tabela CursoProfessor
```sql
CREATE TABLE CursoProfessor (
    IdCursoProfessor INT AUTO_INCREMENT PRIMARY KEY,
    Cursos_IdCursos INT NOT NULL,
    Professores_IdProfessores INT NOT NULL,
    DataVinculacao DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (Cursos_IdCursos) REFERENCES Cursos(IdCursos) ON DELETE CASCADE,
    FOREIGN KEY (Professores_IdProfessores) REFERENCES Professores(IdProfessores) ON DELETE CASCADE,
    
    UNIQUE KEY uk_curso_professor (Cursos_IdCursos, Professores_IdProfessores)
);
```

**Executar o script:** `Scripts/create_CursoProfessor_table.sql`

## 4. Endpoints da API

### GET /api/cursos
Retorna todos os cursos com seus professores.

**Resposta:**
```json
[
  {
    "idCursos": 1,
    "nome": "Ciência da Computação",
    "cargaHoraria": "3600h",
    "departamentos_idDepartamentos": 1,
    "professores": [
      { "idProfessores": 1, "nome": "Prof. João Silva" },
      { "idProfessores": 2, "nome": "Prof. Maria Santos" }
    ]
  }
]
```

### GET /api/cursos/{id}
Retorna um curso específico com seus professores.

**Resposta:**
```json
{
  "idCursos": 1,
  "nome": "Ciência da Computação",
  "cargaHoraria": "3600h",
  "departamentos_idDepartamentos": 1,
  "professores": [
    { "idProfessores": 1, "nome": "Prof. João Silva" }
  ]
}
```

### POST /api/cursos
Cria um novo curso com professores vinculados.

**Request:**
```json
{
  "nome": "Ciência da Computação",
  "cargaHoraria": "3600h",
  "departamentos_idDepartamentos": 1,
  "professores": [1, 2, 3]
}
```

**Validações:**
- ? Nome obrigatório
- ? Departamento deve ser válido
- ? Todos os professorIds devem existir
- ? Se professor não existe ? 400 Bad Request
- ? Lista vazia de professores é permitida

**Resposta:** 201 Created
```json
{
  "idCursos": 1,
  "nome": "Ciência da Computação",
  "cargaHoraria": "3600h",
  "departamentos_idDepartamentos": 1,
  "professores": [
    { "idProfessores": 1, "nome": "Prof. João Silva" },
    { "idProfessores": 2, "nome": "Prof. Maria Santos" },
    { "idProfessores": 3, "nome": "Prof. Pedro Costa" }
  ]
}
```

### PUT /api/cursos/{id}
Atualiza curso e substitui a lista de professores.

**Request:**
```json
{
  "idCursos": 1,
  "nome": "Ciência da Computação",
  "cargaHoraria": "3600h",
  "departamentos_idDepartamentos": 1,
  "professores": [1, 4]
}
```

**Validações:** Mesmas do POST

**Resposta:** 200 OK com curso atualizado

### DELETE /api/cursos/{id}
Remove curso e todos seus vínculos com professores (cascade).

**Resposta:** 204 No Content

## 5. Exemplo de Uso Completo

### 1. Criar um curso com 3 professores
```bash
curl -X POST http://localhost:5000/api/cursos \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Engenharia de Software",
    "cargaHoraria": "4000h",
    "departamentos_idDepartamentos": 1,
    "professores": [1, 2, 3]
  }'
```

### 2. Obter curso com seus professores
```bash
curl -X GET http://localhost:5000/api/cursos/1 \
  -H "Authorization: Bearer {token}"
```

### 3. Atualizar curso e remover um professor
```bash
curl -X PUT http://localhost:5000/api/cursos/1 \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "idCursos": 1,
    "nome": "Engenharia de Software",
    "cargaHoraria": "4000h",
    "departamentos_idDepartamentos": 1,
    "professores": [1, 2]
  }'
```

### 4. Obter cursos com professores
```bash
curl -X GET http://localhost:5000/api/cursos \
  -H "Authorization: Bearer {token}"
```

## 6. Instalação

### Passo 1: Executar script SQL
Execute o arquivo `Scripts/create_CursoProfessor_table.sql` no seu MySQL.

### Passo 2: Compilar projeto
```bash
dotnet build
```

### Passo 3: Testar API
Use o Swagger ou um cliente HTTP para testar os endpoints.

## 7. Fluxo de Dados

```
CreateCursoRequest
    ?
CursoService.AddCursoWithProfessores()
    ?
Validações:
  - Nome obrigatório
  - Departamento válido
  - Todos professores existem
    ?
CursoRepository.Add() ? Novo Curso
    ?
CursoProfessorRepository.AddCursoProfessor() para cada professor
    ?
GetCursoByIdWithProfessores() ? CursoResponseComProfessores
```

## 8. Tratamento de Erros

| Erro | Status | Mensagem |
|------|--------|----------|
| Nome vazio | 400 | "Nome do curso é obrigatório." |
| Professor não existe | 400 | "Professor com ID {id} não existe." |
| Departamento inválido | 400 | "Departamento inválido." |
| ID mismatch | 400 | "O ID na URL não corresponde..." |
| Curso não encontrado | 404 | "Curso não encontrado." |
| Erro interno | 500 | "Erro ao criar/atualizar curso" |

## 9. Considerações de Performance

- ? Índices criados em Cursos_IdCursos e Professores_IdProfessores
- ? UNIQUE constraint previne duplicatas
- ? ON DELETE CASCADE simplifica exclusões
- ? Queries otimizadas com JOINs

## 10. Validações Implementadas

- ? Verificar se professor existe antes de vincular
- ? Retornar erro 400 se professor não existir
- ? Permitir lista vazia de professores
- ? Remover duplicatas de professorIds antes de vincular
- ? Validar nome do curso obrigatório
- ? Limpar vínculos ao deletar curso (CASCADE)
