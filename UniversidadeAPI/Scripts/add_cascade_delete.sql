-- ============================================
-- SCRIPT: Adicionar CASCADE DELETE nas Foreign Keys
-- Descrição: Permite deletar professor/curso automaticamente
-- ============================================

USE mydb;

-- ============================================
-- PASSO 1: Remover constraints antigas da tabela CursoProfessor
-- ============================================

-- Verificar constraints existentes
SELECT 
    CONSTRAINT_NAME, 
    TABLE_NAME, 
    COLUMN_NAME, 
    REFERENCED_TABLE_NAME, 
    REFERENCED_COLUMN_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = 'mydb' 
  AND TABLE_NAME = 'CursoProfessor'
  AND REFERENCED_TABLE_NAME IS NOT NULL;

-- Remover constraint antiga do curso
ALTER TABLE CursoProfessor 
DROP FOREIGN KEY fk_cursoprof_curso;

-- Remover constraint antiga do professor
ALTER TABLE CursoProfessor 
DROP FOREIGN KEY fk_cursoprof_professor;

-- ============================================
-- PASSO 2: Adicionar constraints COM CASCADE
-- ============================================

-- Adicionar FK para Cursos com CASCADE
ALTER TABLE CursoProfessor
ADD CONSTRAINT fk_cursoprof_curso
    FOREIGN KEY (Cursos_IdCursos) 
    REFERENCES Cursos(IdCursos)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

-- Adicionar FK para Professores com CASCADE
ALTER TABLE CursoProfessor
ADD CONSTRAINT fk_cursoprof_professor
    FOREIGN KEY (Professores_IdProfessores) 
    REFERENCES Professores(IdProfessores)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

-- ============================================
-- PASSO 3: Atualizar constraint da tabela Disciplina
-- ============================================

-- Verificar constraints de Disciplina
SELECT 
    CONSTRAINT_NAME, 
    TABLE_NAME, 
    COLUMN_NAME, 
    REFERENCED_TABLE_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = 'mydb' 
  AND TABLE_NAME = 'Disciplina'
  AND REFERENCED_TABLE_NAME IS NOT NULL;

-- Remover constraint antiga de Professor em Disciplina
ALTER TABLE Disciplina 
DROP FOREIGN KEY fk_disciplina_professor;

-- Adicionar FK com SET NULL (não deleta disciplina, apenas remove professor)
ALTER TABLE Disciplina
ADD CONSTRAINT fk_disciplina_professor
    FOREIGN KEY (Professor_IdProfessores) 
    REFERENCES Professores(IdProfessores)
    ON DELETE SET NULL
    ON UPDATE CASCADE;

-- ============================================
-- PASSO 4: Verificar configuração final
-- ============================================

-- Mostrar todas as FKs com suas ações
SELECT 
    CONSTRAINT_NAME AS 'Constraint',
    TABLE_NAME AS 'Tabela',
    COLUMN_NAME AS 'Coluna',
    REFERENCED_TABLE_NAME AS 'Tabela Referenciada',
    REFERENCED_COLUMN_NAME AS 'Coluna Referenciada'
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = 'mydb'
  AND REFERENCED_TABLE_NAME IS NOT NULL
ORDER BY TABLE_NAME, CONSTRAINT_NAME;

-- Ver detalhes das ações ON DELETE
SELECT 
    CONSTRAINT_NAME,
    TABLE_NAME,
    REFERENCED_TABLE_NAME,
    DELETE_RULE,
    UPDATE_RULE
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
WHERE CONSTRAINT_SCHEMA = 'mydb'
ORDER BY TABLE_NAME;

-- ============================================
-- TESTE: Deletar professor com e sem vínculos
-- ============================================

-- Ver professores disponíveis
SELECT IdProfessores, Nome FROM Professores ORDER BY IdProfessores;

-- Ver vínculos de um professor específico
SELECT 
    p.IdProfessores,
    p.Nome AS Professor,
    c.IdCursos,
    c.Nome AS Curso,
    cp.DataVinculacao
FROM Professores p
LEFT JOIN CursoProfessor cp ON p.IdProfessores = cp.Professores_IdProfessores
LEFT JOIN Cursos c ON cp.Cursos_IdCursos = c.IdCursos
WHERE p.IdProfessores = 1;  -- Ajuste o ID conforme necessário

-- Verificar disciplinas vinculadas
SELECT 
    d.IdDisciplina,
    d.Nome AS Disciplina,
    p.IdProfessores,
    p.Nome AS Professor
FROM Disciplina d
LEFT JOIN Professores p ON d.Professor_IdProfessores = p.IdProfessores
WHERE p.IdProfessores = 1;  -- Ajuste o ID conforme necessário

-- ============================================
-- EXEMPLO: Deletar professor
-- ============================================

-- ATENÇÃO: Remova o comentário abaixo apenas para TESTAR

-- DELETE FROM Professores WHERE IdProfessores = 10;

-- Verificar se foi deletado e se vínculos foram removidos
-- SELECT * FROM CursoProfessor WHERE Professores_IdProfessores = 10;
-- SELECT * FROM Disciplina WHERE Professor_IdProfessores = 10;

-- ============================================
-- RESULTADO ESPERADO
-- ============================================

/*
Após executar este script:

1. ? Deletar PROFESSOR:
   - Remove automaticamente vínculos em CursoProfessor (CASCADE)
   - Seta Professor = NULL nas Disciplinas (SET NULL)
   - Deleta o professor

2. ? Deletar CURSO:
   - Remove automaticamente vínculos em CursoProfessor (CASCADE)
   - Remove disciplinas do curso (já tinha CASCADE)
   - Deleta o curso

3. ? Deletar DEPARTAMENTO:
   - BLOQUEADO se houver cursos vinculados (RESTRICT)
   - Evita perda acidental de dados

IMPORTANTE:
- CursoProfessor: DELETE CASCADE (remove vínculos automaticamente)
- Disciplina: SET NULL (mantém disciplina, remove professor)
- Cursos: RESTRICT em departamento (protege dados)
*/

SELECT '? SCRIPT EXECUTADO COM SUCESSO!' AS Status;
SELECT '?? Foreign Keys atualizadas com CASCADE' AS Info;
SELECT '?? Agora você pode deletar professores e cursos livremente!' AS Info2;
