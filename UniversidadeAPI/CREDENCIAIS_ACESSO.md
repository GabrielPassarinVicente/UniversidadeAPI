# ?? CREDENCIAIS DE ACESSO - UniversidadeAPI

## ?? USUÁRIOS DE TESTE

Após executar o script SQL `criar_banco_completo.sql`, você terá 3 usuários criados:

---

### ?? USUÁRIO 1: Administrador
```
Username: admin
Password: Admin@123
Email: admin@universidade.com
```

**Como fazer login:**
```json
POST http://localhost:5019/api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

---

### ?? USUÁRIO 2: Aluno
```
Username: aluno1
Password: Aluno@123
Email: aluno1@universidade.com
```

**Como fazer login:**
```json
POST http://localhost:5019/api/auth/login
Content-Type: application/json

{
  "username": "aluno1",
  "password": "Aluno@123"
}
```

---

### ?? USUÁRIO 3: Professor
```
Username: professor1
Password: Prof@123
Email: professor1@universidade.com
```

**Como fazer login:**
```json
POST http://localhost:5019/api/auth/login
Content-Type: application/json

{
  "username": "professor1",
  "password": "Prof@123"
}
```

---

## ?? COMO USAR

### 1. Fazer Login (obter token)

**Request:**
```bash
POST http://localhost:5019/api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

**Response esperada:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2024-12-20T10:00:00Z",
  "username": "admin",
  "email": "admin@universidade.com"
}
```

### 2. Usar o Token nos Endpoints Protegidos

Copie o `token` recebido e use nos próximos requests:

```bash
GET http://localhost:5019/api/cursos
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## ?? CRIAR NOVO USUÁRIO

Se quiser criar um novo usuário:

**Request:**
```bash
POST http://localhost:5019/api/auth/register
Content-Type: application/json

{
  "username": "novoaluno",
  "password": "SenhaForte@123",
  "email": "novoaluno@universidade.com"
}
```

**Response esperada:**
```json
{
  "id": 4,
  "username": "novoaluno",
  "email": "novoaluno@universidade.com",
  "dataCriacao": "2024-12-19T10:00:00Z"
}
```

Depois faça login com as credenciais criadas.

---

## ?? TESTE RÁPIDO COM cURL

### Login:
```bash
curl -X POST http://localhost:5019/api/auth/login \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"admin\",\"password\":\"Admin@123\"}"
```

### Usar Token:
```bash
# Substitua YOUR_TOKEN pelo token recebido
curl -X GET http://localhost:5019/api/cursos \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## ?? RESUMO DAS SENHAS

| Username | Password | Email |
|----------|----------|-------|
| admin | Admin@123 | admin@universidade.com |
| aluno1 | Aluno@123 | aluno1@universidade.com |
| professor1 | Prof@123 | professor1@universidade.com |

---

## ?? IMPORTANTE

1. **O token expira em 8 horas** (configurado em `appsettings.json`)
2. **Todos os endpoints exceto `/api/auth/*` requerem autenticação**
3. **O token deve ser enviado no header:** `Authorization: Bearer {token}`

---

## ?? PROBLEMAS COMUNS

### Erro: "Usuário ou senha inválidos"
- Verifique se o username e password estão corretos (case-sensitive)
- Confirme que o usuário foi criado no banco de dados

### Erro: "Unauthorized" (401)
- O token está expirado (8h)
- Faça login novamente para obter novo token
- Verifique se está enviando o header `Authorization` corretamente

### Erro: "Token inválido"
- Token malformado ou corrompido
- Faça login novamente

---

## ?? TESTE NO SWAGGER

1. Acesse: `http://localhost:5019/swagger`
2. Clique no endpoint `/api/auth/login`
3. Clique em **Try it out**
4. Cole o JSON de login:
   ```json
   {
     "username": "admin",
     "password": "Admin@123"
   }
   ```
5. Clique em **Execute**
6. Copie o `token` da resposta
7. Clique no botão **Authorize** (cadeado) no topo da página
8. Cole: `Bearer {seu_token}`
9. Clique em **Authorize**
10. Agora você pode testar todos os endpoints protegidos!

---

## ?? PRÓXIMOS PASSOS

Após fazer login com sucesso:

1. ? Testar GET /api/cursos (listar cursos com professores)
2. ? Testar POST /api/cursos (criar novo curso com professores)
3. ? Testar PUT /api/cursos/{id} (atualizar curso)
4. ? Testar DELETE /api/cursos/{id} (deletar curso)
5. ? Integrar com o frontend Angular
