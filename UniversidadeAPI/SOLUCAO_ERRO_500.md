# ?? SOLUÇÃO DEFINITIVA: Erro 500 "Unknown column 'Id'"

## ?? PROBLEMA RAIZ

O erro `Unknown column 'Id' in 'where clause'` ocorre porque:

1. **A tabela `CursoProfessor` NÃO foi criada** no MySQL
2. As queries tentam acessar uma tabela que não existe
3. MySQL retorna erro de coluna desconhecida

## ? SOLUÇÃO (3 PASSOS)

### PASSO 1: Verificar/Criar Tabela no MySQL

Abra o **MySQL Workbench** e execute:

```sql
-- Verificar se tabela existe
SHOW TABLES LIKE 'CursoProfessor';

-- Se não existir, criar agora:
CREATE TABLE IF NOT EXISTS CursoProfessor (
    IdCursoProfessor INT AUTO_INCREMENT PRIMARY KEY,
    Cursos_IdCursos INT NOT NULL,
    Professores_IdProfessores INT NOT NULL,
    DataVinculacao DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_curso FOREIGN KEY (Cursos_IdCursos) 
        REFERENCES Cursos(IdCursos) ON DELETE CASCADE,
    CONSTRAINT fk_professor FOREIGN KEY (Professores_IdProfessores) 
        REFERENCES Professores(IdProfessores) ON DELETE CASCADE,
    
    UNIQUE KEY uk_curso_professor (Cursos_IdCursos, Professores_IdProfessores)
);

-- Criar índices
CREATE INDEX idx_cursos_idcursos ON CursoProfessor(Cursos_IdCursos);
CREATE INDEX idx_professores_idprofessores ON CursoProfessor(Professores_IdProfessores);

-- Verificar se foi criada
DESC CursoProfessor;
SELECT * FROM CursoProfessor LIMIT 1;
```

### PASSO 2: Restartae a API

1. No Visual Studio, pressione **Shift + F5** para parar a aplicação
2. Aguarde 2-3 segundos
3. Pressione **F5** para iniciar novamente

### PASSO 3: Testar Atualização de Curso

Use este request:

```bash
PUT http://localhost:5019/api/cursos/1
Authorization: Bearer YOUR_TOKEN_HERE
Content-Type: application/json

{
  "idCursos": 1,
  "nome": "Ciência da Computação",
  "cargaHoraria": "3600h",
  "departamentos_idDepartamentos": 1,
  "professores": [1, 2]
}
```

**Esperado:** 200 OK com o curso atualizado

---

## ?? VERIFICAÇÕES ADICONAIS

Se ainda receber erro 500, execute:

```sql
-- 1. Verificar que tabela Cursos existe
SELECT COUNT(*) FROM Cursos;

-- 2. Verificar que tabela Professores existe
SELECT COUNT(*) FROM Professores;

-- 3. Verificar que tabela CursoProfessor existe
SHOW TABLES LIKE 'CursoProfessor';

-- 4. Verificar estrutura da tabela
DESC CursoProfessor;

-- 5. Testar insert manual
INSERT INTO CursoProfessor (Cursos_IdCursos, Professores_IdProfessores, DataVinculacao)
VALUES (1, 1, NOW());

SELECT * FROM CursoProfessor;
```

---

## ?? CHECKLIST DE RESOLUÇÃO

- [ ] Tabela `CursoProfessor` foi criada no MySQL
- [ ] Foram executados os scripts de criação de índices
- [ ] A API foi reiniciada (Shift+F5, depois F5)
- [ ] Testei um PUT em `/api/cursos/{id}` e recebi 200 OK
- [ ] Não aparecem mais erros MySqlException nos logs

---

## ?? ERROS COMUNS

| Erro | Solução |
|------|---------|
| `Unknown column 'Id'` | Tabela não foi criada. Execute o script SQL acima |
| `Cannot add or update a child row` | Um curso ou professor não existe. Verifique IDs |
| `Duplicate entry` | Um vínculo já existe. Verifique dados no banco |
| `Access denied` | Usuário MySQL não tem permissão. Altere `root`/senha |

---

## ?? CONFIRMAÇÃO DE SUCESSO

Quando tudo estiver funcionando:

1. **PUT /api/cursos/1** retorna 200 OK
2. **GET /api/cursos/1** mostra os professores vinculados
3. Não há mais erros 500 nos logs
4. Tabela `CursoProfessor` tem registros

---

## ?? PRÓXIMAS AÇÕES

Após resolver este erro:

1. Testar POST /api/cursos (criar novo com professores)
2. Testar DELETE /api/cursos/{id} (remover cascade)
3. Atualizar frontend para usar novos endpoints
4. Documentar no README final

