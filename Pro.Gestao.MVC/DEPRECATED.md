# ⚠️ PROJETO DEPRECIADO

> **Este projeto está depreciado e não deve receber novos desenvolvimentos.**

## Informações

| Campo | Valor |
|-------|-------|
| **Projeto** | Pro.Gestao.MVC |
| **Status** | Depreciado |
| **Data de depreciação** | Junho/2025 |
| **Projeto ativo** | [`src/ProGestao`](../src/ProGestao/) (ASP.NET Core 9 — Razor Pages) |

## Motivo da Depreciação

O projeto `Pro.Gestao.MVC` foi a implementação inicial do sistema ProGestao utilizando o padrão MVC com Controllers. Durante o desenvolvimento, o projeto foi migrado para uma arquitetura baseada em **Razor Pages** no diretório `src/ProGestao`, que é o projeto ativo e mantido.

Este projeto legado apresenta os seguintes problemas que justificam a depreciação:

1. **Repositórios não implementados** — Múltiplos repositórios e services contêm apenas `throw new NotImplementedException()`, indicando que a implementação nunca foi concluída neste projeto.
2. **Duplicação de Dependency Injection** — O `Program.cs` contém registros duplicados de DI (ex: `AgendamentoAppService` registrado tanto via interface quanto diretamente).
3. **Arquitetura substituída** — O projeto ativo `src/ProGestao` utiliza Razor Pages com padrão CQRS, EF Core 9 e uma arquitetura mais robusta e completa.

## Repositórios e Services com NotImplementedException

### Pro.Gestao.Infra.Data — Repositórios

#### `Repository/AgendamentoRepository.cs`

| Método | Status |
|--------|--------|
| `AddAgendamento(Agendamento)` | ❌ NotImplementedException |
| `AddAgendamento(TEntity)` | ❌ NotImplementedException |
| `DeleteAgendamento(string)` | ❌ NotImplementedException |
| `GetAgendamentosPessoa(string)` | ❌ NotImplementedException |
| `GetAllAgendamentos()` | ❌ NotImplementedException |
| `UpdateAgendamento(TEntity)` | ❌ NotImplementedException |
| `GetAgendamento()` | ❌ NotImplementedException |

#### `Repository/PessoaRepository.cs`

| Método | Status |
|--------|--------|
| `AddPessoa(TEntity)` | ❌ NotImplementedException |
| `DeletePessoa(string)` | ❌ NotImplementedException |
| `GetAllPessoas()` | ❌ NotImplementedException |
| `GetPessoa(string)` | ❌ NotImplementedException |
| `UpdatePessoa(TEntity)` | ❌ NotImplementedException |

### Pro.Gestao.Domain — Services

#### `Services/AgendamentoService.cs`

| Método | Status |
|--------|--------|
| `GetAllAgendamentos()` | ❌ NotImplementedException |
| `GetAllAgendamentosPessoas()` | ❌ NotImplementedException |
| `GetAllAgendamentosPessoas(string)` | ❌ NotImplementedException |
| `GetAgendamentosPessoa(string)` | ❌ NotImplementedException |

#### `Services/PessoaService.cs`

| Método | Status |
|--------|--------|
| `GetAllPessoas()` | ❌ NotImplementedException |

### Pro.Gestao.Application — Application Services

#### `Services/AgendamentoAppService.cs`

| Método | Status |
|--------|--------|
| `GetAllAgendamentos()` | ❌ NotImplementedException |

## Projeto Ativo

Todo novo desenvolvimento deve ser feito no projeto **`src/ProGestao`**, que contém:

- ASP.NET Core 9 com Razor Pages
- Entity Framework Core 9 com SQL Server
- Padrão CQRS (Query/Command/Validation Services)
- Bootstrap 5.3.2
- Cadastro completo de Atividades, Usuários, Equipes e Projetos
- Cadastro de Ausências e Tipos de Ausência
- Grid Semanal e Mensal
- Diagrama de Gantt
- Dashboard com métricas

## Orientações

- **NÃO** inicie novos desenvolvimentos neste projeto.
- **NÃO** corrija bugs neste projeto — as correções devem ser feitas em `src/ProGestao`.
- Este projeto é mantido no repositório apenas para referência histórica.
- Em caso de dúvidas, consulte a documentação do projeto ativo em `src/ProGestao`.
