# ?? GUIA DE IMPLEMENTAÇÃO: Relação Many-to-Many Curso ? Professor

## ?? REQUISITOS CRÍTICOS

Antes de qualquer coisa, você precisa ter criado a tabela `CursoProfessor` no banco de dados MySQL.

---

## ?? PASSO 1: Criar Tabela no Banco de Dados

Abra o **MySQL Workbench** ou qualquer cliente MySQL e execute este script:

```sql
-- Criar tabela CursoProfessor para relacionamento many-to-many
CREATE TABLE IF NOT EXISTS CursoProfessor (
    IdCursoProfessor INT AUTO_INCREMENT PRIMARY KEY,
    Cursos_IdCursos INT NOT NULL,
    Professores_IdProfessores INT NOT NULL,
    DataVinculacao DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (Cursos_IdCursos) REFERENCES Cursos(IdCursos) ON DELETE CASCADE,
    FOREIGN KEY (Professores_IdProfessores) REFERENCES Professores(IdProfessores) ON DELETE CASCADE,
    
    UNIQUE KEY uk_curso_professor (Cursos_IdCursos, Professores_IdProfessores)
);

-- Criar índices para melhorar performance
CREATE INDEX idx_cursos_idcursos ON CursoProfessor(Cursos_IdCursos);
CREATE INDEX idx_professores_idprofessores ON CursoProfessor(Professores_IdProfessores);
```

**Verificar se foi criado:**
```sql
SELECT * FROM CursoProfessor;
DESC CursoProfessor;
```

---

## ?? PASSO 2: Verificar Arquivos Criados

Os seguintes arquivos foram criados/atualizados automaticamente:

### ? MODELOS (Models)
- `Models/CursoProfessor.cs` - Nova entidade
- `Models/Curso.cs` - ?? Atualizado
- `Models/Professor.cs` - ?? Atualizado
- `Models/CreateCursoRequest.cs` - Nova DTO
- `Models/UpdateCursoRequest.cs` - Nova DTO
- `Models/CursoResponseComProfessores.cs` - Nova DTO

### ? REPOSITÓRIOS (Repositories)
- `Repositories/ICursoProfessorRepository.cs` - Nova interface
- `Repositories/CursoProfessorRepository.cs` - Nova implementação
- `Repositories/ICursoRepository.cs` - ?? Atualizado
- `Repositories/CursoRepository.cs` - ?? Atualizado

### ? SERVIÇOS (Services)
- `Services/ICursoService.cs` - ?? Atualizado
- `Services/CursoService.cs` - ?? Atualizado

### ? CONTROLLERS
- `Controllers/CursosController.cs` - ?? Atualizado

### ? CONFIGURAÇÃO
- `Program.cs` - ?? Atualizado (registrado `CursoProfessorRepository`)

---

## ?? PASSO 3: Compilar o Projeto

1. No Visual Studio, pressione **Ctrl + Shift + B** ou vá em **Build ? Build Solution**
2. Confirme que não há erros de compilação
3. Se houver erros, verifique se todos os arquivos foram criados corretamente

---

## ?? PASSO 4: Testar a API

### Opção A: Usar o arquivo `.http` incluído

Abra o arquivo `test-api-cursos-professores.http` e teste cada endpoint

### Opção B: Usar Swagger/OpenAPI

1. Inicie a API: **F5** ou **Ctrl + F5**
2. Acesse: `http://localhost:5019/swagger/index.html`
3. Teste os endpoints na interface do Swagger

### Opção C: Usar Postman ou cURL

---

## ?? ENDPOINTS DISPONÍVEIS

### 1?? GET /api/cursos
**Retorna todos os cursos com seus professores**

```bash
curl -X GET "http://localhost:5019/api/cursos" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Resposta Esperada:**
```json
[
  {
    "idCursos": 1,
    "nome": "Ciência da Computação",
    "cargaHoraria": "3600h",
    "departamentos_idDepartamentos": 1,
    "professores": [
      { "idProfessores": 1, "nome": "Prof. João" },
      { "idProfessores": 2, "nome": "Prof. Maria" }
    ]
  }
]
```

---

### 2?? GET /api/cursos/{id}
**Retorna um curso específico com seus professores**

```bash
curl -X GET "http://localhost:5019/api/cursos/1" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

### 3?? POST /api/cursos
**Cria um novo curso com professores vinculados**

```bash
curl -X POST "http://localhost:5019/api/cursos" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Engenharia de Software",
    "cargaHoraria": "4000h",
    "departamentos_idDepartamentos": 1,
    "professores": [1, 2, 3]
  }'
```

**Validações:**
- ? Nome é obrigatório
- ? Se algum professor não existe ? Erro 400
- ? Lista vazia de professores é permitida
- ? IDs duplicados são removidos automaticamente

**Resposta Esperada:** 201 Created
```json
{
  "idCursos": 2,
  "nome": "Engenharia de Software",
  "cargaHoraria": "4000h",
  "departamentos_idDepartamentos": 1,
  "professores": [
    { "idProfessores": 1, "nome": "Prof. João" },
    { "idProfessores": 2, "nome": "Prof. Maria" },
    { "idProfessores": 3, "nome": "Prof. Pedro" }
  ]
}
```

---

### 4?? PUT /api/cursos/{id}
**Atualiza curso e substitui lista de professores**

```bash
curl -X PUT "http://localhost:5019/api/cursos/1" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "idCursos": 1,
    "nome": "Ciência da Computação Avançada",
    "cargaHoraria": "4000h",
    "departamentos_idDepartamentos": 1,
    "professores": [1, 3]
  }'
```

**Comportamento:**
- Remove TODOS os professores antigos
- Adiciona os novos professores da lista

**Resposta Esperada:** 200 OK com curso atualizado

---

### 5?? DELETE /api/cursos/{id}
**Remove curso e todos seus vínculos com professores**

```bash
curl -X DELETE "http://localhost:5019/api/cursos/1" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Resposta Esperada:** 204 No Content

---

## ?? TRATAMENTO DE ERROS

| Cenário | Status | Mensagem |
|---------|--------|----------|
| Nome vazio | 400 | `"Nome do curso é obrigatório."` |
| Professor não existe | 400 | `"Professor com ID {id} não existe."` |
| Departamento inválido | 400 | `"Departamento inválido."` |
| ID mismatch (PUT) | 400 | `"O ID na URL não corresponde..."` |
| Curso não encontrado | 404 | `"Curso não encontrado."` |
| Erro interno | 500 | `"Erro ao criar/atualizar curso"` + detalhes |

---

## ?? SE RECEBER ERRO 500

### Verifique:

1. **Tabela CursoProfessor existe?**
   ```sql
   SELECT * FROM information_schema.TABLES WHERE TABLE_NAME = 'CursoProfessor';
   ```

2. **Foreign keys estão corretas?**
   ```sql
   DESC CursoProfessor;
   ```

3. **Dados de teste existem?**
   ```sql
   SELECT * FROM Cursos LIMIT 5;
   SELECT * FROM Professores LIMIT 5;
   SELECT * FROM Departamentos LIMIT 5;
   ```

4. **Logs do servidor**
   Verifique a aba "Output" do Visual Studio na seção "Debug" para ver erros detalhados

---

## ?? FLUXO DE DADOS

```
Frontend (Angular)
       ?
CursosController
       ?
CursoService
  ?? Valida dados
  ?? Verifica se professores existem
  ?? Chama CursoRepository
  ?? Chama CursoProfessorRepository
       ?
CursoRepository + CursoProfessorRepository
       ?
MySQL (Tabelas: Cursos, Professores, CursoProfessor)
```

---

## ? FEATURES IMPLEMENTADAS

- ? Criar curso com múltiplos professores
- ? Atualizar curso e substituir professores
- ? Listar cursos com professores
- ? Buscar curso específico com professores
- ? Deletar curso (e desvincula professores automaticamente)
- ? Validações completas
- ? Tratamento de erros apropriado
- ? DTOs específicas para requisição/resposta
- ? Índices no banco para performance
- ? Constraints UNIQUE para evitar duplicatas

---

## ?? VERIFICAR DADOS NO BANCO

```sql
-- Ver todos os cursos com seus professores
SELECT 
    c.IdCursos,
    c.Nome as CursoNome,
    p.IdProfessores,
    p.Nome as ProfessorNome,
    cp.DataVinculacao
FROM Cursos c
LEFT JOIN CursoProfessor cp ON c.IdCursos = cp.Cursos_IdCursos
LEFT JOIN Professores p ON p.IdProfessores = cp.Professores_IdProfessores
ORDER BY c.IdCursos, p.Nome;

-- Ver quantos professores cada curso tem
SELECT 
    c.IdCursos,
    c.Nome,
    COUNT(p.IdProfessores) as TotalProfessores
FROM Cursos c
LEFT JOIN CursoProfessor cp ON c.IdCursos = cp.Cursos_IdCursos
LEFT JOIN Professores p ON p.IdProfessores = cp.Professores_IdProfessores
GROUP BY c.IdCursos, c.Nome;
```

---

## ?? RESUMO DO QUE FOI IMPLEMENTADO

1. **Nova tabela:** `CursoProfessor` com relacionamento many-to-many
2. **Novos modelos:** `CursoProfessor`, DTOs para requisição/resposta
3. **Novo repositório:** `CursoProfessorRepository` com operações CRUD
4. **Serviço atualizado:** `CursoService` com validações e orquestração
5. **Controller atualizado:** `CursosController` com endpoints novos/atualizados
6. **Injeção de dependência:** Registrado novo repositório no `Program.cs`

---

## ? PRÓXIMOS PASSOS

Depois de validar que a API funciona:

1. Atualizar frontend para usar os novos endpoints
2. Adicionar testes unitários
3. Documentar na wiki ou README
4. Deploy em produção

---

## ?? SUPORTE

Se tiver problemas:
- Verifique que a tabela foi criada no MySQL
- Confirme que há professores e departamentos cadastrados
- Verifique os logs do Visual Studio (Debug output)
- Teste endpoints com Swagger antes de usar no frontend
