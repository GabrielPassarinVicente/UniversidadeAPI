-- =====================================================
-- SCRIPT: CONFIGURAR EXCLUSÃO EM CASCATA
-- Permite deletar registros independente de vínculos
-- =====================================================

USE mydb;

-- =====================================================
-- 1. REMOVER CONSTRAINTS ANTIGAS (SE EXISTIREM)
-- =====================================================

-- Remover constraints de Cursos
SET FOREIGN_KEY_CHECKS = 0;

ALTER TABLE Cursos DROP FOREIGN KEY IF EXISTS fk_curso_departamento;
ALTER TABLE Disciplinas DROP FOREIGN KEY IF EXISTS fk_disciplina_curso;
ALTER TABLE Disciplinas DROP FOREIGN KEY IF EXISTS fk_disciplina_professor;
ALTER TABLE Matricula DROP FOREIGN KEY IF EXISTS fk_matricula_aluno;
ALTER TABLE Matricula DROP FOREIGN KEY IF EXISTS fk_matricula_curso;
ALTER TABLE Notas DROP FOREIGN KEY IF EXISTS fk_nota_aluno;
ALTER TABLE Notas DROP FOREIGN KEY IF EXISTS fk_nota_disciplina;

SET FOREIGN_KEY_CHECKS = 1;

-- =====================================================
-- 2. ADICIONAR CONSTRAINTS COM DELETE CASCADE
-- =====================================================

-- Cursos -> Departamentos (CASCADE)
ALTER TABLE Cursos
ADD CONSTRAINT fk_curso_departamento 
    FOREIGN KEY (Departamentos_idDepartamentos) 
    REFERENCES Departamentos(idDepartamentos)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

-- Disciplinas -> Cursos (CASCADE)
ALTER TABLE Disciplinas
ADD CONSTRAINT fk_disciplina_curso 
    FOREIGN KEY (Curso_IdCursos) 
    REFERENCES Cursos(IdCursos)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

-- Disciplinas -> Professores (SET NULL - se o professor for deletado, disciplina fica sem professor)
ALTER TABLE Disciplinas
ADD CONSTRAINT fk_disciplina_professor 
    FOREIGN KEY (Professor_IdProfessores) 
    REFERENCES Professores(IdProfessores)
    ON DELETE SET NULL
    ON UPDATE CASCADE;

-- Matricula -> Aluno (CASCADE)
ALTER TABLE Matricula
ADD CONSTRAINT fk_matricula_aluno 
    FOREIGN KEY (Aluno_Id) 
    REFERENCES Aluno(Id)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

-- Matricula -> Curso (CASCADE)
ALTER TABLE Matricula
ADD CONSTRAINT fk_matricula_curso 
    FOREIGN KEY (Curso_IdCursos) 
    REFERENCES Cursos(IdCursos)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

-- Notas -> Aluno (CASCADE)
ALTER TABLE Notas
ADD CONSTRAINT fk_nota_aluno 
    FOREIGN KEY (Aluno_Id) 
    REFERENCES Aluno(Id)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

-- Notas -> Disciplina (CASCADE)
ALTER TABLE Notas
ADD CONSTRAINT fk_nota_disciplina 
    FOREIGN KEY (Disciplina_IdDisciplina) 
    REFERENCES Disciplinas(IdDisciplina)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

-- =====================================================
-- 3. VERIFICAR CONSTRAINTS CRIADAS
-- =====================================================

SELECT 
    TABLE_NAME,
    CONSTRAINT_NAME,
    REFERENCED_TABLE_NAME,
    DELETE_RULE,
    UPDATE_RULE
FROM 
    INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
WHERE 
    CONSTRAINT_SCHEMA = 'mydb'
ORDER BY 
    TABLE_NAME;

-- =====================================================
-- RESUMO DO COMPORTAMENTO:
-- =====================================================

/*
DELETAR DEPARTAMENTO:
- Deleta todos os cursos vinculados
- Deleta todas as disciplinas dos cursos
- Deleta todas as matrículas dos cursos
- Deleta todas as notas das disciplinas

DELETAR CURSO:
- Deleta todas as disciplinas vinculadas
- Deleta todas as matrículas vinculadas
- Deleta todas as notas das disciplinas

DELETAR PROFESSOR:
- Define Professor_IdProfessores = NULL nas disciplinas
- Disciplinas continuam existindo, mas sem professor

DELETAR ALUNO:
- Deleta todas as matrículas do aluno
- Deleta todas as notas do aluno

DELETAR DISCIPLINA:
- Deleta todas as notas vinculadas
*/

SELECT 'Constraints configuradas com sucesso!' AS Status;
SELECT 'Agora você pode deletar registros independente de vínculos!' AS Mensagem;
