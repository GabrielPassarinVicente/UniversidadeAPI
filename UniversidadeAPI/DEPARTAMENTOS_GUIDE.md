# ?? Guia de Uso - API de Departamentos

## ? CRUD Completo Implementado!

Agora você tem um **CRUD completo de Departamentos** com autenticação JWT.

---

## ?? Endpoints Disponíveis

### 1. **Listar todos os departamentos**
```http
GET /api/departamentos
Authorization: Bearer {seu_token}
```

**Resposta (200 OK):**
```json
[
  {
    "idDepartamentos": 1,
    "nome": "Ciências Exatas",
    "codigo": "CEX",
    "descricao": "Departamento de Matemática, Física e Química",
    "dataCriacao": "2025-01-15T10:00:00Z"
  },
  {
    "idDepartamentos": 2,
    "nome": "Computação",
    "codigo": "DCC",
    "descricao": "Departamento de Ciência da Computação",
    "dataCriacao": "2025-01-15T10:00:00Z"
  }
]
```

---

### 2. **Buscar departamento por ID**
```http
GET /api/departamentos/1
Authorization: Bearer {seu_token}
```

**Resposta (200 OK):**
```json
{
  "idDepartamentos": 1,
  "nome": "Ciências Exatas",
  "codigo": "CEX",
  "descricao": "Departamento de Matemática, Física e Química",
  "dataCriacao": "2025-01-15T10:00:00Z"
}
```

---

### 3. **Adicionar novo departamento (APENAS NOME)**
```http
POST /api/departamentos
Authorization: Bearer {seu_token}
Content-Type: application/json
```

**Body (SIMPLES - só o nome):**
```json
{
  "nome": "Departamento de Artes"
}
```

**Resposta (201 Created):**
```json
{
  "idDepartamentos": 6,
  "nome": "Departamento de Artes",
  "codigo": null,
  "descricao": null,
  "dataCriacao": "2025-01-15T14:30:00Z"
}
```

---

### 4. **Adicionar departamento COMPLETO**
```http
POST /api/departamentos
Authorization: Bearer {seu_token}
Content-Type: application/json
```

**Body (COMPLETO):**
```json
{
  "nome": "Departamento de Biologia",
  "codigo": "BIO",
  "descricao": "Departamento de ciências biológicas e ambientais"
}
```

**Resposta (201 Created):**
```json
{
  "idDepartamentos": 7,
  "nome": "Departamento de Biologia",
  "codigo": "BIO",
  "descricao": "Departamento de ciências biológicas e ambientais",
  "dataCriacao": "2025-01-15T14:35:00Z"
}
```

---

### 5. **Atualizar departamento**
```http
PUT /api/departamentos/1
Authorization: Bearer {seu_token}
Content-Type: application/json
```

**Body:**
```json
{
  "idDepartamentos": 1,
  "nome": "Ciências Exatas - Atualizado",
  "codigo": "CEX-NEW",
  "descricao": "Descrição atualizada do departamento"
}
```

**Resposta (204 No Content)** - Sucesso, sem corpo de resposta

---

### 6. **Deletar departamento**
```http
DELETE /api/departamentos/1
Authorization: Bearer {seu_token}
```

**Resposta (204 No Content)** - Sucesso, sem corpo de resposta

---

## ?? Como Testar no Swagger

1. **Inicie a API:**
   ```bash
   dotnet run
   ```

2. **Acesse o Swagger:**
   ```
   http://localhost:5000/swagger
   ```

3. **Faça Login:**
   - Vá em `POST /api/auth/login`
   - Use: `username: admin`, `password: admin123`
   - **Copie o token** retornado

4. **Autorize no Swagger:**
   - Clique no botão **?? Authorize** (cadeado no topo)
   - Cole o token (só o token, sem "Bearer")
   - Clique em **Authorize**

5. **Teste os Endpoints:**
   - Agora você pode testar todos os endpoints de Departamentos!

---

## ?? Exemplos Práticos

### Exemplo 1: Criar departamento simples (só nome)
```bash
curl -X POST http://localhost:5000/api/departamentos \
  -H "Authorization: Bearer SEU_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"nome":"Marketing"}'
```

### Exemplo 2: Criar departamento completo
```bash
curl -X POST http://localhost:5000/api/departamentos \
  -H "Authorization: Bearer SEU_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Recursos Humanos",
    "codigo": "RH",
    "descricao": "Departamento de gestão de pessoas"
  }'
```

### Exemplo 3: Listar todos
```bash
curl -X GET http://localhost:5000/api/departamentos \
  -H "Authorization: Bearer SEU_TOKEN"
```

---

## ?? Integração com Frontend

### JavaScript/Fetch

```javascript
// 1. Fazer login e obter token
const loginResponse = await fetch('http://localhost:5000/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    username: 'admin',
    password: 'admin123'
  })
});

const { token } = await loginResponse.json();

// 2. Criar departamento (só nome)
const createResponse = await fetch('http://localhost:5000/api/departamentos', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({
    nome: 'Novo Departamento'
  })
});

const newDepartamento = await createResponse.json();
console.log('Departamento criado:', newDepartamento);

// 3. Listar todos os departamentos
const listResponse = await fetch('http://localhost:5000/api/departamentos', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

const departamentos = await listResponse.json();
console.log('Departamentos:', departamentos);
```

### React Example

```jsx
import React, { useState } from 'react';
import api from './services/api'; // axios configurado com token

function AddDepartamento() {
  const [nome, setNome] = useState('');
  const [codigo, setCodigo] = useState('');
  const [descricao, setDescricao] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    try {
      const response = await api.post('/departamentos', {
        nome,
        codigo,
        descricao
      });
      
      alert('Departamento criado com sucesso!');
      console.log(response.data);
      
      // Limpar form
      setNome('');
      setCodigo('');
      setDescricao('');
    } catch (error) {
      alert('Erro: ' + error.response?.data?.message);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label>Nome do Departamento *</label>
        <input
          type="text"
          value={nome}
          onChange={(e) => setNome(e.target.value)}
          required
        />
      </div>
      
      <div>
        <label>Código (opcional)</label>
        <input
          type="text"
          value={codigo}
          onChange={(e) => setCodigo(e.target.value)}
        />
      </div>
      
      <div>
        <label>Descrição (opcional)</label>
        <textarea
          value={descricao}
          onChange={(e) => setDescricao(e.target.value)}
        />
      </div>
      
      <button type="submit">Adicionar Departamento</button>
    </form>
  );
}

export default AddDepartamento;
```

---

## ? Validações Implementadas

- ? **Nome é obrigatório** - Não pode ser vazio
- ? **Código único** - Se fornecido, não pode duplicar
- ? **Data de criação automática** - Definida automaticamente
- ? **Autenticação obrigatória** - Todos os endpoints exigem JWT

---

## ?? Resumo

Você agora tem:
- ? CRUD completo de Departamentos
- ? Pode adicionar departamento **só com o nome**
- ? Pode adicionar campos opcionais (código, descrição)
- ? Totalmente integrado com JWT
- ? Pronto para usar no frontend

---

**Pronto para usar! ??**
