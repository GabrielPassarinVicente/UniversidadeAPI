# ?? GUIA COMPLETO: Criar Banco de Dados do Zero

## ?? PASSO CRÍTICO: Executar Script SQL

### ?? PASSO 1: Abrir MySQL Workbench

1. Abra o **MySQL Workbench**
2. Conecte-se ao seu servidor MySQL:
   - Host: `localhost`
   - Port: `3306`
   - User: `root`
   - Password: `862945` (conforme seu `appsettings.json`)

---

### ?? PASSO 2: Executar Script Completo

1. No MySQL Workbench, clique em **File ? Open SQL Script**
2. Navegue até: `UniversidadeAPI\Scripts\criar_banco_completo.sql`
3. Clique em **Open**
4. Clique no ícone de **raio** (?) ou pressione **Ctrl + Shift + Enter**
5. Aguarde a execução (pode demorar 10-30 segundos)

**Ou copie e cole este script diretamente:**

```sql
-- ============================================
-- SCRIPT COMPLETO: Criar Banco de Dados UniversidadeAPI
-- ============================================

DROP DATABASE IF EXISTS mydb;
CREATE DATABASE mydb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE mydb;

-- Criar tabela Usuario
CREATE TABLE Usuario (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    DataCriacao DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_username (Username),
    INDEX idx_email (Email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Criar tabela Departamentos
CREATE TABLE Departamentos (
    IdDepartamentos INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(100) NOT NULL,
    Codigo VARCHAR(20) UNIQUE,
    Descricao TEXT,
    DataCriacao DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_nome (Nome),
    INDEX idx_codigo (Codigo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Criar tabela Professores
CREATE TABLE Professores (
    IdProfessores INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(100) NOT NULL,
    INDEX idx_nome (Nome)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Criar tabela Aluno
CREATE TABLE Aluno (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    NomeCompleto VARCHAR(150) NOT NULL,
    DataNascimento DATE NOT NULL,
    Cpf VARCHAR(14) NOT NULL UNIQUE,
    Endereco VARCHAR(255),
    Telefone VARCHAR(20),
    Email VARCHAR(100) UNIQUE,
    DataMatricula DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_nome (NomeCompleto),
    INDEX idx_cpf (Cpf),
    INDEX idx_email (Email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Criar tabela Cursos
CREATE TABLE Cursos (
    IdCursos INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(100) NOT NULL,
    CargaHoraria VARCHAR(20),
    Departamentos_idDepartamentos INT NOT NULL,
    INDEX idx_nome (Nome),
    INDEX idx_departamento (Departamentos_idDepartamentos),
    CONSTRAINT fk_cursos_departamentos 
        FOREIGN KEY (Departamentos_idDepartamentos) 
        REFERENCES Departamentos(IdDepartamentos)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Criar tabela Disciplina
CREATE TABLE Disciplina (
    IdDisciplina INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(100) NOT NULL,
    Codigo VARCHAR(20) UNIQUE,
    CargaHoraria INT NOT NULL,
    Creditos INT,
    Ementa TEXT,
    Curso_IdCursos INT NOT NULL,
    Professor_IdProfessores INT,
    DataCriacao DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_nome (Nome),
    INDEX idx_codigo (Codigo),
    INDEX idx_curso (Curso_IdCursos),
    INDEX idx_professor (Professor_IdProfessores),
    CONSTRAINT fk_disciplina_curso 
        FOREIGN KEY (Curso_IdCursos) 
        REFERENCES Cursos(IdCursos)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT fk_disciplina_professor 
        FOREIGN KEY (Professor_IdProfessores) 
        REFERENCES Professores(IdProfessores)
        ON DELETE SET NULL
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Criar tabela CursoProfessor (MANY-TO-MANY)
CREATE TABLE CursoProfessor (
    IdCursoProfessor INT AUTO_INCREMENT PRIMARY KEY,
    Cursos_IdCursos INT NOT NULL,
    Professores_IdProfessores INT NOT NULL,
    DataVinculacao DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_curso (Cursos_IdCursos),
    INDEX idx_professor (Professores_IdProfessores),
    UNIQUE KEY uk_curso_professor (Cursos_IdCursos, Professores_IdProfessores),
    CONSTRAINT fk_cursoprof_curso 
        FOREIGN KEY (Cursos_IdCursos) 
        REFERENCES Cursos(IdCursos)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT fk_cursoprof_professor 
        FOREIGN KEY (Professores_IdProfessores) 
        REFERENCES Professores(IdProfessores)
        ON DELETE CASCADE
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Inserir Departamentos
INSERT INTO Departamentos (Nome, Codigo, Descricao, DataCriacao) VALUES
('Ciências Exatas', 'DEXT', 'Departamento de Ciências Exatas e Tecnologia', NOW()),
('Ciências Humanas', 'DHUM', 'Departamento de Ciências Humanas', NOW()),
('Ciências da Saúde', 'DSAU', 'Departamento de Ciências da Saúde', NOW()),
('Engenharias', 'DENG', 'Departamento de Engenharias', NOW()),
('Administração', 'DADM', 'Departamento de Administração e Negócios', NOW());

-- Inserir Professores
INSERT INTO Professores (Nome) VALUES
('Prof. Dr. João Silva'),
('Prof. Dra. Maria Santos'),
('Prof. Dr. Pedro Costa'),
('Prof. Dra. Ana Oliveira'),
('Prof. Dr. Carlos Ferreira'),
('Prof. Dra. Juliana Lima'),
('Prof. Dr. Roberto Alves'),
('Prof. Dra. Fernanda Rocha'),
('Prof. Dr. Lucas Martins'),
('Prof. Dra. Patricia Souza');

-- Inserir Alunos
INSERT INTO Aluno (NomeCompleto, DataNascimento, Cpf, Endereco, Telefone, Email, DataMatricula) VALUES
('Gabriel Passarin Vicente', '2000-05-15', '123.456.789-01', 'Rua das Flores, 123', '(11) 98765-4321', 'gabriel@email.com', NOW()),
('Mariana Silva Costa', '1999-08-20', '234.567.890-12', 'Av. Principal, 456', '(11) 98765-4322', 'mariana@email.com', NOW()),
('Carlos Eduardo Santos', '2001-03-10', '345.678.901-23', 'Rua do Comércio, 789', '(11) 98765-4323', 'carlos@email.com', NOW()),
('Juliana Ferreira Lima', '2000-11-25', '456.789.012-34', 'Av. das Nações, 321', '(11) 98765-4324', 'juliana@email.com', NOW()),
('Fernando Oliveira Rocha', '1998-07-30', '567.890.123-45', 'Rua dos Pinheiros, 654', '(11) 98765-4325', 'fernando@email.com', NOW());

-- Inserir Cursos
INSERT INTO Cursos (Nome, CargaHoraria, Departamentos_idDepartamentos) VALUES
('Ciência da Computação', '3600h', 1),
('Engenharia de Software', '4000h', 1),
('Sistemas de Informação', '3200h', 1),
('Administração', '3000h', 5),
('Medicina', '7200h', 3),
('Psicologia', '4000h', 2),
('Engenharia Civil', '5000h', 4),
('Direito', '4000h', 2),
('Enfermagem', '4000h', 3),
('Arquitetura', '5000h', 4);

-- Inserir Disciplinas
INSERT INTO Disciplina (Nome, Codigo, CargaHoraria, Creditos, Ementa, Curso_IdCursos, Professor_IdProfessores, DataCriacao) VALUES
('Algoritmos e Estruturas de Dados', 'AED001', 80, 4, 'Estudo de algoritmos e estruturas de dados fundamentais', 1, 1, NOW()),
('Banco de Dados', 'BD001', 80, 4, 'Modelagem e implementação de bancos de dados relacionais', 1, 2, NOW()),
('Programação Orientada a Objetos', 'POO001', 80, 4, 'Conceitos e práticas de programação orientada a objetos', 1, 1, NOW()),
('Engenharia de Requisitos', 'ER001', 60, 3, 'Técnicas de levantamento e análise de requisitos', 2, 3, NOW()),
('Arquitetura de Software', 'AS001', 80, 4, 'Padrões e práticas de arquitetura de software', 2, 3, NOW()),
('Gestão de Projetos', 'GP001', 60, 3, 'Fundamentos de gestão de projetos de software', 2, 4, NOW()),
('Redes de Computadores', 'RC001', 80, 4, 'Fundamentos de redes e protocolos de comunicação', 3, 5, NOW()),
('Segurança da Informação', 'SI001', 60, 3, 'Conceitos e práticas de segurança da informação', 3, 5, NOW()),
('Teoria Geral da Administração', 'TGA001', 80, 4, 'Fundamentos da administração e gestão empresarial', 4, 6, NOW()),
('Marketing', 'MKT001', 60, 3, 'Princípios e estratégias de marketing', 4, 6, NOW());

-- Inserir relacionamentos Curso-Professor (many-to-many)
INSERT INTO CursoProfessor (Cursos_IdCursos, Professores_IdProfessores, DataVinculacao) VALUES
(1, 1, NOW()), (1, 2, NOW()), (1, 5, NOW()),
(2, 1, NOW()), (2, 3, NOW()), (2, 4, NOW()),
(3, 2, NOW()), (3, 5, NOW()),
(4, 6, NOW()), (4, 7, NOW()),
(5, 8, NOW()), (5, 9, NOW()),
(6, 9, NOW()), (6, 10, NOW()),
(7, 4, NOW()), (7, 7, NOW()),
(8, 10, NOW()),
(9, 8, NOW()),
(10, 4, NOW()), (10, 7, NOW());

-- Verificar criação
SELECT 'Tabelas criadas:' AS Status;
SHOW TABLES;

SELECT 'Total de registros:' AS Status;
SELECT 'Departamentos' AS Tabela, COUNT(*) AS Total FROM Departamentos
UNION ALL SELECT 'Professores', COUNT(*) FROM Professores
UNION ALL SELECT 'Alunos', COUNT(*) FROM Aluno
UNION ALL SELECT 'Cursos', COUNT(*) FROM Cursos
UNION ALL SELECT 'Disciplinas', COUNT(*) FROM Disciplina
UNION ALL SELECT 'CursoProfessor', COUNT(*) FROM CursoProfessor;
```

---

### ?? PASSO 3: Criar Usuários com Endpoint de Registro

Como os hashes de senha são complexos, vamos criar usuários usando a própria API:

1. **Reinicie a API** no Visual Studio (Shift+F5, depois F5)
2. Aguarde até ver: `Now listening on: http://localhost:5019`
3. Use o Swagger ou Postman para criar usuários

**Acessar Swagger:**
- Abra: `http://localhost:5019/swagger`

---

### ?? PASSO 4: Criar Usuário Admin

No Swagger ou Postman:

```bash
POST http://localhost:5019/api/auth/register
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123",
  "email": "admin@universidade.com"
}
```

**Resposta esperada:**
```json
{
  "id": 1,
  "username": "admin",
  "email": "admin@universidade.com",
  "dataCriacao": "2024-12-19T..."
}
```

---

### ?? PASSO 5: Criar Mais Usuários

**Usuário 2:**
```bash
POST http://localhost:5019/api/auth/register
Content-Type: application/json

{
  "username": "aluno1",
  "password": "Aluno@123",
  "email": "aluno1@universidade.com"
}
```

**Usuário 3:**
```bash
POST http://localhost:5019/api/auth/register
Content-Type: application/json

{
  "username": "professor1",
  "password": "Prof@123",
  "email": "professor1@universidade.com"
}
```

---

### ?? PASSO 6: Fazer Login

Agora teste o login com qualquer usuário criado:

```bash
POST http://localhost:5019/api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

**Resposta esperada:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2024-12-20T10:00:00Z",
  "username": "admin",
  "email": "admin@universidade.com"
}
```

---

### ?? PASSO 7: Testar Endpoints com Token

Copie o `token` e use nos próximos requests:

```bash
GET http://localhost:5019/api/cursos
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## ?? TESTE COMPLETO NO SWAGGER

### 1. Criar usuário admin
1. Acesse: `http://localhost:5019/swagger`
2. Expanda: `POST /api/auth/register`
3. Clique: **Try it out**
4. Cole:
   ```json
   {
     "username": "admin",
     "password": "Admin@123",
     "email": "admin@universidade.com"
   }
   ```
5. Clique: **Execute**
6. Verifique resposta: **200 OK**

### 2. Fazer login
1. Expanda: `POST /api/auth/login`
2. Clique: **Try it out**
3. Cole:
   ```json
   {
     "username": "admin",
     "password": "Admin@123"
   }
   ```
4. Clique: **Execute**
5. **COPIE O TOKEN** da resposta

### 3. Autorizar no Swagger
1. Clique no botão **Authorize** (?? cadeado verde) no topo
2. Cole: `Bearer {seu_token_aqui}`
3. Clique: **Authorize**
4. Clique: **Close**

### 4. Testar GET /api/cursos
1. Expanda: `GET /api/cursos`
2. Clique: **Try it out**
3. Clique: **Execute**
4. Verifique resposta: Lista de cursos com professores

---

## ? CHECKLIST DE SUCESSO

- [ ] Script SQL executado sem erros
- [ ] 7 tabelas criadas (SHOW TABLES mostra todas)
- [ ] Dados inseridos (SELECT COUNT mostra totais)
- [ ] API reiniciada (Shift+F5, depois F5)
- [ ] Usuário admin criado via /api/auth/register
- [ ] Login funcionando (retorna token)
- [ ] Token autorizado no Swagger
- [ ] GET /api/cursos retorna lista com professores
- [ ] Sem erros MySqlException nos logs

---

## ?? PROBLEMAS E SOLUÇÕES

### Problema: "Username já está em uso"
**Solução:** Já existe um usuário com esse username. Use outro ou delete no banco:
```sql
DELETE FROM Usuario WHERE Username = 'admin';
```

### Problema: "Table already exists"
**Solução:** Execute o DROP DATABASE primeiro:
```sql
DROP DATABASE IF EXISTS mydb;
```

### Problema: "Cannot add foreign key constraint"
**Solução:** Execute as tabelas na ordem correta (o script já faz isso)

### Problema: Login retorna "Usuário ou senha inválidos"
**Solução:** 
1. Verifique se o username está correto (case-sensitive)
2. Verifique se a senha está correta
3. Confirme que o usuário foi criado:
```sql
SELECT * FROM Usuario WHERE Username = 'admin';
```

---

## ?? RESUMO RÁPIDO

```bash
# 1. Executar script SQL no MySQL Workbench
# (copia/cola o script acima)

# 2. Reiniciar API
# Shift+F5, depois F5 no Visual Studio

# 3. Criar usuário
POST http://localhost:5019/api/auth/register
{
  "username": "admin",
  "password": "Admin@123",
  "email": "admin@universidade.com"
}

# 4. Fazer login
POST http://localhost:5019/api/auth/login
{
  "username": "admin",
  "password": "Admin@123"
}

# 5. Usar token nos endpoints protegidos
GET http://localhost:5019/api/cursos
Authorization: Bearer {token_aqui}
```

---

## ?? PRONTO!

Agora você tem:
- ? Banco de dados completo
- ? Usuários funcionais
- ? Login com token JWT
- ? Endpoints protegidos
- ? Relação many-to-many Curso ? Professor funcionando

**Próximo passo:** Testar no frontend Angular!
