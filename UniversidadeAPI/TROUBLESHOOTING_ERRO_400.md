# ?? TROUBLESHOOTING: Erro 400 (Bad Request) ao Atualizar Curso

## ? PROGRESSO

Mudou de **500 ? 400**, o que significa:
- ? Tabela `CursoProfessor` foi criada com sucesso
- ? Conexão com banco funcionando
- ?? Problema agora está na **validação dos dados**

---

## ?? ERRO 400: O que significa?

**Bad Request** indica que os dados enviados pelo frontend não passaram nas validações do backend.

Possíveis causas:
1. Professor com ID inválido (não existe no banco)
2. Departamento com ID inválido
3. Campos obrigatórios faltando
4. Formato de dados incorreto

---

## ?? VERIFICAR DADOS NO FRONTEND

### 1. Ver o que está sendo enviado

No arquivo `curso.component.ts`, adicione este log **ANTES** do `this.http.put`:

```typescript
onSubmit() {
  if (this.cursoForm.valid) {
    const formValue = this.cursoForm.value;
    
    // ?? LOG PARA DEBUG - ADICIONE ISTO
    console.log('?? Dados enviados para API:', JSON.stringify(formValue, null, 2));
    console.log('?? Curso ID:', this.cursoId);
    console.log('????? Professores selecionados:', formValue.professores);
    
    const cursoAtualizado = {
      idCursos: this.cursoId,
      nome: formValue.nome,
      cargaHoraria: formValue.cargaHoraria,
      departamentos_idDepartamentos: formValue.departamento,
      professores: formValue.professores || []
    };

    console.log('?? Payload final:', JSON.stringify(cursoAtualizado, null, 2));
    
    this.http.put(`${this.apiUrl}/${this.cursoId}`, cursoAtualizado)
      .subscribe({
        next: (response) => { /* ... */ },
        error: (error) => {
          console.error('? Erro ao atualizar curso', error);
          console.error('?? Detalhes do erro:', error.error);
          console.error('?? Status da resposta:', error.status);
          console.error('?? URL da requisição:', error.url);
          
          // ?? MOSTRAR MENSAGEM DE VALIDAÇÃO
          if (error.error && error.error.message) {
            alert(`Erro de validação: ${error.error.message}`);
          }
        }
      });
  }
}
```

---

## ?? VERIFICAR DADOS NO BACKEND

### 1. Verificar se Professores existem

No MySQL Workbench, execute:

```sql
-- Ver todos os professores disponíveis
SELECT IdProfessores, Nome FROM Professores ORDER BY IdProfessores;

-- Verificar se os IDs que você está usando existem
SELECT IdProfessores, Nome 
FROM Professores 
WHERE IdProfessores IN (1, 2, 3);  -- IDs que você está tentando vincular
```

### 2. Verificar se Departamento existe

```sql
-- Ver todos os departamentos
SELECT IdDepartamentos, Nome FROM Departamentos ORDER BY IdDepartamentos;

-- Verificar departamento específico
SELECT IdDepartamentos, Nome 
FROM Departamentos 
WHERE IdDepartamentos = 1;  -- ID do departamento do curso
```

### 3. Verificar se Curso existe

```sql
-- Verificar curso que você está tentando atualizar
SELECT IdCursos, Nome, Departamentos_idDepartamentos 
FROM Cursos 
WHERE IdCursos = 4;  -- ID do curso da URL
```

---

## ?? SOLUÇÕES PARA PROBLEMAS COMUNS

### Problema 1: "Professor com ID X não existe"

**Causa:** Você está tentando vincular um professor que não está no banco.

**Solução:**
```sql
-- Ver IDs disponíveis
SELECT IdProfessores, Nome FROM Professores;

-- Se necessário, adicionar professor
INSERT INTO Professores (Nome) VALUES ('Prof. Novo Nome');
```

### Problema 2: "Departamento inválido"

**Causa:** O ID do departamento não existe ou é 0.

**Solução:**
```sql
-- Ver departamentos disponíveis
SELECT IdDepartamentos, Nome, Codigo FROM Departamentos;

-- Atualizar curso para usar departamento válido
UPDATE Cursos 
SET Departamentos_idDepartamentos = 1 
WHERE IdCursos = 4;
```

### Problema 3: "Nome do curso é obrigatório"

**Causa:** Campo `nome` vazio ou nulo.

**Solução no Frontend:**
```typescript
// Verificar se nome não está vazio
if (!formValue.nome || formValue.nome.trim() === '') {
  alert('Nome do curso é obrigatório!');
  return;
}
```

### Problema 4: Array de professores vazio causa erro

**Causa:** Backend não aceita array vazio.

**Solução Temporária:**
```typescript
professores: formValue.professores && formValue.professores.length > 0 
  ? formValue.professores 
  : []
```

---

## ?? CHECKLIST DE VERIFICAÇÃO

Execute cada item e marque se passou:

### No MySQL:
- [ ] Curso com ID 4 existe
- [ ] Departamento vinculado ao curso existe
- [ ] Todos os professores que você quer vincular existem
- [ ] Tabela `CursoProfessor` existe

### No Frontend:
- [ ] `cursoForm.valid` retorna `true`
- [ ] Campo `nome` tem valor preenchido
- [ ] Campo `cargaHoraria` tem valor
- [ ] Campo `departamento` tem ID válido (não 0 ou null)
- [ ] Array `professores` tem IDs válidos

### No Backend:
- [ ] API está rodando (http://localhost:5019)
- [ ] Endpoint PUT /api/cursos/{id} existe
- [ ] Não há erros nos logs do Visual Studio

---

## ?? TESTE MANUAL NO SWAGGER

1. Acesse: `http://localhost:5019/swagger`
2. Faça login e obtenha token
3. Clique em **Authorize** e cole o token
4. Expanda: `PUT /api/cursos/{id}`
5. Clique: **Try it out**
6. Cole este JSON de teste:

```json
{
  "idCursos": 4,
  "nome": "Teste de Atualização",
  "cargaHoraria": "3000h",
  "departamentos_idDepartamentos": 1,
  "professores": [1, 2]
}
```

7. Clique: **Execute**
8. Verifique a resposta:
   - **200 OK** = Sucesso
   - **400 Bad Request** = Ver mensagem de erro retornada
   - **404 Not Found** = Curso não existe

---

## ?? EXEMPLO DE RESPOSTA 400 COM DETALHES

Quando há erro 400, o backend retorna:

```json
{
  "message": "Professor com ID 99 não existe."
}
```

ou

```json
{
  "message": "Nome do curso é obrigatório."
}
```

**No console do navegador você verá:**
```
Erro de validação: Professor com ID 99 não existe.
```

---

## ?? SOLUÇÃO DEFINITIVA

### PASSO 1: Verificar Dados

```sql
-- Execute no MySQL
SELECT 'Curso' AS Tipo, IdCursos AS ID, Nome FROM Cursos WHERE IdCursos = 4
UNION ALL
SELECT 'Departamento', IdDepartamentos, Nome FROM Departamentos WHERE IdDepartamentos = 1
UNION ALL
SELECT 'Professor', IdProfessores, Nome FROM Professores WHERE IdProfessores IN (1,2,3,4,5);
```

### PASSO 2: Ajustar Frontend

Certifique-se que o formulário envia dados corretos:

```typescript
// curso.component.ts - método onSubmit()
const cursoAtualizado = {
  idCursos: Number(this.cursoId),  // Garantir que é número
  nome: formValue.nome?.trim() || '',
  cargaHoraria: formValue.cargaHoraria?.trim() || '',
  departamentos_idDepartamentos: Number(formValue.departamento) || 1,
  professores: Array.isArray(formValue.professores) 
    ? formValue.professores.map(id => Number(id))
    : []
};

// Validar antes de enviar
if (!cursoAtualizado.nome) {
  alert('Nome do curso é obrigatório!');
  return;
}

if (cursoAtualizado.departamentos_idDepartamentos === 0) {
  alert('Departamento é obrigatório!');
  return;
}
```

### PASSO 3: Testar Novamente

1. Recarregue a página do formulário (F5)
2. Preencha todos os campos
3. Selecione professores que existem no banco
4. Clique em Salvar
5. Verifique o console do navegador (F12)
6. Veja a mensagem de erro específica

---

## ?? PRÓXIMOS PASSOS

1. ? Execute os SQLs de verificação acima
2. ? Adicione os logs no frontend
3. ? Tente atualizar o curso novamente
4. ? **Copie a mensagem de erro exata** que aparecer
5. ? Me envie a mensagem para eu te ajudar especificamente

---

## ?? DICA IMPORTANTE

O erro 400 sempre vem com uma **mensagem específica** do backend.

Para ver essa mensagem:
1. Abra o DevTools (F12)
2. Aba **Network**
3. Encontre a requisição PUT que falhou (vermelho)
4. Clique nela
5. Aba **Response**
6. **Copie e cole a mensagem exata aqui!**

Isso me ajudará a identificar o problema específico.
