# ??? Guia: Recriar Banco de Dados com DELETE CASCADE

## ?? ATENÇÃO
Este script **DELETA E RECRIA** todo o banco de dados `mydb`.
**TODOS OS DADOS SERÃO PERDIDOS!**

---

## ? O que este script faz:

1. **Deleta o banco de dados** `mydb` (se existir)
2. **Cria o banco de dados** do zero
3. **Cria todas as tabelas**:
   - Departamentos
   - Usuario
   - Professores
   - Cursos
   - Aluno
   - Disciplinas
   - Matricula
   - Notas

4. **Configura DELETE CASCADE em TODOS os relacionamentos**
5. **Insere dados de exemplo**
6. **Cria Views úteis**

---

## ?? Como Executar

### Opção 1: MySQL Workbench

1. Abra o **MySQL Workbench**
2. Conecte-se ao seu servidor MySQL
3. Abra o arquivo:
   ```
   UniversidadeAPI\Scripts\recreate_database_with_cascade.sql
   ```
4. Clique em ? **Execute** (ou pressione `Ctrl+Shift+Enter`)
5. Aguarde a execução (pode demorar alguns segundos)

### Opção 2: Linha de Comando

```bash
mysql -u root -p < "UniversidadeAPI/Scripts/recreate_database_with_cascade.sql"
# Digite sua senha quando solicitado
```

### Opção 3: MySQL Command Line Client

```bash
mysql -u root -p

# Digite sua senha

USE mydb;
SOURCE C:/Users/Gabriel/Desktop/Curso FTI/UniversidadeAPI/UniversidadeAPI/Scripts/recreate_database_with_cascade.sql;
```

---

## ?? Usuários Criados

| Username | Password | Email |
|----------|----------|-------|
| admin | admin123 | admin@universidade.com |
| professor1 | admin123 | professor@universidade.com |
| aluno1 | admin123 | aluno@universidade.com |

---

## ??? Comportamento do DELETE CASCADE

### ? Todos os deletes estão configurados!

#### 1. **Deletar DEPARTAMENTO**
```sql
DELETE FROM Departamentos WHERE idDepartamentos = 1;
```
**Efeito em CASCATA:**
- ? Deleta todos os **Cursos** do departamento
  - ? Deleta todas as **Disciplinas** dos cursos
    - ? Deleta todas as **Notas** das disciplinas
  - ? Deleta todas as **Matrículas** dos cursos

---

#### 2. **Deletar CURSO**
```sql
DELETE FROM Cursos WHERE IdCursos = 1;
```
**Efeito em CASCATA:**
- ? Deleta todas as **Disciplinas** do curso
  - ? Deleta todas as **Notas** das disciplinas
- ? Deleta todas as **Matrículas** do curso

---

#### 3. **Deletar PROFESSOR**
```sql
DELETE FROM Professores WHERE IdProfessores = 1;
```
**Efeito:**
- ? Define `Professor_IdProfessores = NULL` nas disciplinas
- ?? **Disciplinas NÃO são deletadas**, apenas ficam sem professor

---

#### 4. **Deletar ALUNO**
```sql
DELETE FROM Aluno WHERE Id = 1;
```
**Efeito em CASCATA:**
- ? Deleta todas as **Matrículas** do aluno
- ? Deleta todas as **Notas** do aluno

---

#### 5. **Deletar DISCIPLINA**
```sql
DELETE FROM Disciplinas WHERE IdDisciplina = 1;
```
**Efeito em CASCATA:**
- ? Deleta todas as **Notas** da disciplina

---

## ?? Testando o DELETE CASCADE

### Teste 1: Deletar um Departamento
```sql
-- Ver cursos do departamento 2 (Computação)
SELECT * FROM Cursos WHERE Departamentos_idDepartamentos = 2;

-- Ver disciplinas desses cursos
SELECT * FROM Disciplinas WHERE Curso_IdCursos IN (SELECT IdCursos FROM Cursos WHERE Departamentos_idDepartamentos = 2);

-- DELETAR O DEPARTAMENTO
DELETE FROM Departamentos WHERE idDepartamentos = 2;

-- Verificar: Cursos e Disciplinas foram deletados automaticamente!
SELECT COUNT(*) AS CursosRestantes FROM Cursos WHERE Departamentos_idDepartamentos = 2;
-- Resultado: 0
```

### Teste 2: Deletar um Curso
```sql
-- Ver disciplinas do curso 1
SELECT * FROM Disciplinas WHERE Curso_IdCursos = 1;

-- DELETAR O CURSO
DELETE FROM Cursos WHERE IdCursos = 1;

-- Verificar: Disciplinas foram deletadas automaticamente!
SELECT COUNT(*) AS DisciplinasRestantes FROM Disciplinas WHERE Curso_IdCursos = 1;
-- Resultado: 0
```

### Teste 3: Deletar um Aluno
```sql
-- Ver matrículas e notas do aluno 1
SELECT * FROM Matricula WHERE Aluno_Id = 1;
SELECT * FROM Notas WHERE Aluno_Id = 1;

-- DELETAR O ALUNO
DELETE FROM Aluno WHERE Id = 1;

-- Verificar: Matrículas e Notas foram deletadas automaticamente!
SELECT COUNT(*) AS MatriculasRestantes FROM Matricula WHERE Aluno_Id = 1;
SELECT COUNT(*) AS NotasRestantes FROM Notas WHERE Aluno_Id = 1;
-- Resultado: 0 e 0
```

### Teste 4: Deletar um Professor
```sql
-- Ver disciplinas do professor 1
SELECT * FROM Disciplinas WHERE Professor_IdProfessores = 1;

-- DELETAR O PROFESSOR
DELETE FROM Professores WHERE IdProfessores = 1;

-- Verificar: Disciplinas CONTINUAM existindo, mas sem professor
SELECT * FROM Disciplinas WHERE Professor_IdProfessores IS NULL;
```

---

## ?? Verificar Constraints Configuradas

```sql
SELECT 
    TABLE_NAME AS Tabela,
    CONSTRAINT_NAME AS Constraint,
    REFERENCED_TABLE_NAME AS TabelaReferenciada,
    DELETE_RULE AS RegraDelete,
    UPDATE_RULE AS RegraUpdate
FROM 
    INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
WHERE 
    CONSTRAINT_SCHEMA = 'mydb'
ORDER BY 
    TABLE_NAME;
```

**Resultado esperado:**
| Tabela | Constraint | TabelaReferenciada | RegraDelete | RegraUpdate |
|--------|-----------|-------------------|-------------|-------------|
| Cursos | fk_curso_departamento | Departamentos | **CASCADE** | CASCADE |
| Disciplinas | fk_disciplina_curso | Cursos | **CASCADE** | CASCADE |
| Disciplinas | fk_disciplina_professor | Professores | **SET NULL** | CASCADE |
| Matricula | fk_matricula_aluno | Aluno | **CASCADE** | CASCADE |
| Matricula | fk_matricula_curso | Cursos | **CASCADE** | CASCADE |
| Notas | fk_nota_aluno | Aluno | **CASCADE** | CASCADE |
| Notas | fk_nota_disciplina | Disciplinas | **CASCADE** | CASCADE |

---

## ? Dados de Exemplo Inseridos

Após executar o script, você terá:

- ? **5 Departamentos**
- ? **3 Usuários** (admin, professor1, aluno1)
- ? **5 Professores**
- ? **5 Cursos**
- ? **5 Alunos**
- ? **6 Disciplinas**
- ? **5 Matrículas**
- ? **5 Registros de Notas**
- ? **4 Views** úteis

---

## ?? Views Criadas

### 1. vw_alunos_cursos
Lista alunos com seus cursos matriculados
```sql
SELECT * FROM vw_alunos_cursos;
```

### 2. vw_cursos_departamentos
Lista cursos com informações dos departamentos
```sql
SELECT * FROM vw_cursos_departamentos;
```

### 3. vw_boletim_alunos
Boletim completo com notas dos alunos
```sql
SELECT * FROM vw_boletim_alunos WHERE AlunoId = 1;
```

### 4. vw_disciplinas_curso
Lista disciplinas por curso com professores
```sql
SELECT * FROM vw_disciplinas_curso WHERE IdCursos = 1;
```

---

## ?? Próximos Passos

1. ? Execute o script
2. ? Verifique se as constraints foram criadas
3. ? Teste a exclusão em cascata
4. ? Inicie sua API: `dotnet run`
5. ? Faça login no Swagger com: `admin` / `admin123`
6. ? Teste os endpoints da API

---

## ?? Importante

- **Backup**: Este script deleta todo o banco! Faça backup se necessário
- **Produção**: Em produção, use migrações ao invés de recriar o banco
- **Dados**: Todos os dados de exemplo serão recriados
- **Senha padrão**: Todos os usuários têm senha `admin123` (hash SHA256)

---

## ?? Resultado Final

? **Banco de dados completo e funcional**  
? **DELETE CASCADE configurado em TODOS os relacionamentos**  
? **Você pode deletar qualquer registro sem erro de constraint**  
? **Dados de exemplo prontos para teste**  
? **Views úteis para consultas**  

**Seu banco está pronto para uso com a API!** ??
