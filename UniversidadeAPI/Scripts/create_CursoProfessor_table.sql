-- Criar tabela CursoProfessor para relacionamento many-to-many
CREATE TABLE CursoProfessor (
    IdCursoProfessor INT AUTO_INCREMENT PRIMARY KEY,
    Cursos_IdCursos INT NOT NULL,
    Professores_IdProfessores INT NOT NULL,
    DataVinculacao DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (Cursos_IdCursos) REFERENCES Cursos(IdCursos) ON DELETE CASCADE,
    FOREIGN KEY (Professores_IdProfessores) REFERENCES Professores(IdProfessores) ON DELETE CASCADE,
    
    UNIQUE KEY uk_curso_professor (Cursos_IdCursos, Professores_IdProfessores)
);

-- Criar índice para melhorar performance nas consultas
CREATE INDEX idx_cursos_idcursos ON CursoProfessor(Cursos_IdCursos);
CREATE INDEX idx_professores_idprofessores ON CursoProfessor(Professores_IdProfessores);
