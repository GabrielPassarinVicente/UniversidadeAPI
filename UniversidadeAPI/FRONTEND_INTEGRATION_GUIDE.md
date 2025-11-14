# Guia de Integração Frontend - Autenticação JWT

## Passo a Passo para Conectar com o Frontend

### 1. Executar a API
```bash
cd UniversidadeAPI
dotnet run
```
A API estará disponível em: `http://localhost:5000` ou `https://localhost:5001`

### 2. Criar a Tabela de Usuários
Execute o script SQL no seu banco de dados MySQL:
```sql
CREATE TABLE IF NOT EXISTS Usuario (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    DataCriacao DATETIME NOT NULL,
    INDEX idx_username (Username),
    INDEX idx_email (Email)
);
```

### 3. Fluxo de Autenticação no Frontend

#### A) Criar Página de Login

**HTML/React Example:**
```jsx
import React, { useState } from 'react';
import axios from 'axios';

function Login() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  const handleLogin = async (e) => {
    e.preventDefault();
    setError('');

    try {
      const response = await axios.post('http://localhost:5000/api/auth/login', {
        username: username,
        password: password
      });

      // Salvar token no localStorage
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('username', response.data.username);
      localStorage.setItem('email', response.data.email);

      // Redirecionar para página principal
      window.location.href = '/dashboard';
    } catch (error) {
      if (error.response) {
        setError(error.response.data.message || 'Erro ao fazer login');
      } else {
        setError('Erro de conexão com o servidor');
      }
    }
  };

  return (
    <div>
      <h2>Login</h2>
      <form onSubmit={handleLogin}>
        <input
          type="text"
          placeholder="Username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          required
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
        <button type="submit">Entrar</button>
      </form>
      {error && <p style={{color: 'red'}}>{error}</p>}
    </div>
  );
}

export default Login;
```

#### B) Criar Página de Registro

```jsx
import React, { useState } from 'react';
import axios from 'axios';

function Register() {
  const [formData, setFormData] = useState({
    username: '',
    password: '',
    email: ''
  });
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const handleRegister = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    try {
      const response = await axios.post('http://localhost:5000/api/auth/register', formData);
      setSuccess('Usuário registrado com sucesso! Faça login.');
      // Opcional: redirecionar para login
      setTimeout(() => {
        window.location.href = '/login';
      }, 2000);
    } catch (error) {
      if (error.response) {
        setError(error.response.data.message || 'Erro ao registrar');
      } else {
        setError('Erro de conexão com o servidor');
      }
    }
  };

  return (
    <div>
      <h2>Registrar</h2>
      <form onSubmit={handleRegister}>
        <input
          type="text"
          placeholder="Username"
          value={formData.username}
          onChange={(e) => setFormData({...formData, username: e.target.value})}
          required
        />
        <input
          type="password"
          placeholder="Password"
          value={formData.password}
          onChange={(e) => setFormData({...formData, password: e.target.value})}
          required
        />
        <input
          type="email"
          placeholder="Email"
          value={formData.email}
          onChange={(e) => setFormData({...formData, email: e.target.value})}
          required
        />
        <button type="submit">Registrar</button>
      </form>
      {error && <p style={{color: 'red'}}>{error}</p>}
      {success && <p style={{color: 'green'}}>{success}</p>}
    </div>
  );
}

export default Register;
```

#### C) Configurar Axios para Usar o Token

Crie um arquivo `api.js` ou `axiosConfig.js`:

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5000/api'
});

// Interceptor para adicionar token em todas as requisições
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Interceptor para tratar erros de autenticação
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      // Token inválido ou expirado
      localStorage.removeItem('token');
      localStorage.removeItem('username');
      localStorage.removeItem('email');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
```

#### D) Usar a API nas Requisições

```javascript
import api from './api';

// Buscar todos os alunos
export const getAlunos = async () => {
  try {
    const response = await api.get('/alunos');
    return response.data;
  } catch (error) {
    console.error('Erro ao buscar alunos:', error);
    throw error;
  }
};

// Adicionar novo aluno
export const addAluno = async (aluno) => {
  try {
    const response = await api.post('/alunos', aluno);
    return response.data;
  } catch (error) {
    console.error('Erro ao adicionar aluno:', error);
    throw error;
  }
};

// Atualizar aluno
export const updateAluno = async (id, aluno) => {
  try {
    const response = await api.put(`/alunos/${id}`, aluno);
    return response.data;
  } catch (error) {
    console.error('Erro ao atualizar aluno:', error);
    throw error;
  }
};

// Deletar aluno
export const deleteAluno = async (id) => {
  try {
    await api.delete(`/alunos/${id}`);
  } catch (error) {
    console.error('Erro ao deletar aluno:', error);
    throw error;
  }
};
```

#### E) Criar um Context para Autenticação (React)

```jsx
import React, { createContext, useState, useContext, useEffect } from 'react';
import axios from 'axios';

const AuthContext = createContext();

export const useAuth = () => {
  return useContext(AuthContext);
};

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Verificar se existe token ao carregar
    const token = localStorage.getItem('token');
    const username = localStorage.getItem('username');
    const email = localStorage.getItem('email');
    
    if (token && username && email) {
      setUser({ username, email });
    }
    setLoading(false);
  }, []);

  const login = async (username, password) => {
    try {
      const response = await axios.post('http://localhost:5000/api/auth/login', {
        username,
        password
      });

      localStorage.setItem('token', response.data.token);
      localStorage.setItem('username', response.data.username);
      localStorage.setItem('email', response.data.email);

      setUser({
        username: response.data.username,
        email: response.data.email
      });

      return { success: true };
    } catch (error) {
      return {
        success: false,
        message: error.response?.data?.message || 'Erro ao fazer login'
      };
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('username');
    localStorage.removeItem('email');
    setUser(null);
  };

  const value = {
    user,
    login,
    logout,
    isAuthenticated: !!user
  };

  return (
    <AuthContext.Provider value={value}>
      {!loading && children}
    </AuthContext.Provider>
  );
};
```

#### F) Proteger Rotas no Frontend

```jsx
import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from './AuthContext';

const PrivateRoute = ({ children }) => {
  const { isAuthenticated } = useAuth();

  return isAuthenticated ? children : <Navigate to="/login" />;
};

export default PrivateRoute;

// Uso:
// <Route path="/dashboard" element={<PrivateRoute><Dashboard /></PrivateRoute>} />
```

### 4. Exemplo de Uso Completo em um Componente

```jsx
import React, { useEffect, useState } from 'react';
import { useAuth } from './AuthContext';
import api from './api';

function Dashboard() {
  const { user, logout } = useAuth();
  const [alunos, setAlunos] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadAlunos();
  }, []);

  const loadAlunos = async () => {
    try {
      const response = await api.get('/alunos');
      setAlunos(response.data);
    } catch (error) {
      console.error('Erro ao carregar alunos:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h1>Dashboard</h1>
      <p>Bem-vindo, {user?.username}!</p>
      <button onClick={logout}>Sair</button>

      <h2>Lista de Alunos</h2>
      {loading ? (
        <p>Carregando...</p>
      ) : (
        <ul>
          {alunos.map(aluno => (
            <li key={aluno.id}>{aluno.nomeCompleto}</li>
          ))}
        </ul>
      )}
    </div>
  );
}

export default Dashboard;
```

### 5. Configurar CORS (se necessário)

Se o frontend estiver em outra porta/domínio, adicione no `Program.cs` (antes de `var app = builder.Build();`):

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // URLs do seu frontend
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Depois de app.Build(), antes de app.UseAuthentication():
app.UseCors("AllowFrontend");
```

### 6. Testando o Fluxo

1. **Registrar um usuário:**
   - Acesse a página de registro
   - Preencha: username, password, email
   - Clique em Registrar

2. **Fazer Login:**
   - Acesse a página de login
   - Entre com as credenciais
   - O token será salvo automaticamente

3. **Acessar recursos protegidos:**
   - Todas as requisições subsequentes incluirão o token
   - O usuário permanecerá logado até fazer logout ou o token expirar

### 7. Dicas Importantes

- **Token expira em 8 horas** (configurado em appsettings.json)
- **Armazene o token com segurança** (localStorage para desenvolvimento, httpOnly cookies para produção)
- **Trate erros 401** (token inválido/expirado) redirecionando para login
- **Nunca exponha a SecretKey** do JWT no frontend
- **Use HTTPS em produção**

### 8. Exemplo Fetch (sem biblioteca)

Se não usar Axios:

```javascript
// Login
const login = async (username, password) => {
  const response = await fetch('http://localhost:5000/api/auth/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ username, password })
  });

  if (!response.ok) {
    throw new Error('Login falhou');
  }

  const data = await response.json();
  localStorage.setItem('token', data.token);
  return data;
};

// Requisição autenticada
const getAlunos = async () => {
  const token = localStorage.getItem('token');
  
  const response = await fetch('http://localhost:5000/api/alunos', {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });

  if (!response.ok) {
    throw new Error('Erro ao buscar alunos');
  }

  return await response.json();
};
```

## Resumo

? Backend pronto com JWT
? Endpoints de login e registro funcionando
? Token é retornado no login
? Use o token no header `Authorization: Bearer {token}`
? Configure CORS se necessário
? Implemente o frontend conforme exemplos acima

**O token JWT está funcionando! Agora você pode conectar seu frontend e usar a autenticação!** ??
