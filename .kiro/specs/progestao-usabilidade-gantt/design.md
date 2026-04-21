# Design Document

## Overview

Este documento descreve o design técnico para as melhorias de usabilidade, cadastro de ausências e implementação do diagrama de Gantt no ProGestao. O projeto segue a arquitetura existente: ASP.NET Core 9 com Razor Pages, EF Core 9 com SQL Server, padrão CQRS com Query/Command/Validation services, e frontend com Bootstrap 5.3.2.

As mudanças estão organizadas em 4 fases incrementais, cada uma entregando valor independente.

---

## Architecture

### Fase 1: Correções de Usabilidade (Req 1-8)

Alterações exclusivamente em arquivos frontend (CSS, Razor, JavaScript). Nenhuma mudança no backend.

**Arquivos impactados:**
- `wwwroot/css/site.css` — Consolidação de z-index, correção de encoding, remoção de duplicações
- `wwwroot/css/pages/grid.css` — Correção de sintaxe, remoção de duplicações, truncamento CSS
- `wwwroot/css/components/dropdown.css` — Será removido (estilos migrados)
- `Pages/Grid/Index.cshtml` — Remoção de JS workarounds, truncamento server-side, scroll forçado
- `Pages/Dashboard/Index.cshtml` — Correção de HTML inválido (td aninhado)

**Estratégia de z-index consolidada:**
```
:root {
    --z-base: 1;
    --z-card: 1;
    --z-table: 2;
    --z-grid: 3;
    --z-sticky: 10;
    --z-navbar: 1030;        /* Bootstrap padrão */
    --z-dropdown: 1050;      /* Bootstrap padrão */
    --z-modal: 1055;         /* Bootstrap padrão */
    --z-tooltip: 1070;       /* Bootstrap padrão */
}
```

A estratégia é alinhar com os valores padrão do Bootstrap para dropdowns, modals e tooltips, evitando conflitos. Elementos internos da página (cards, tabelas, grid) usam valores baixos relativos entre si.

### Fase 2: Modelo de Ausências (Req 9-11)

**Novas entidades:**

```
TipoAusencia
├── Id: int (PK)
├── Nome: string (required, max 50)
├── Cor: string (max 7, hex color)
├── Descricao: string? (max 200)
├── Ativo: bool (default true)
└── Ausencias: ICollection<Ausencia>

Ausencia
├── Id: int (PK)
├── UsuarioId: int (FK → Usuario)
├── TipoAusenciaId: int (FK → TipoAusencia)
├── DataInicio: DateTime (required)
├── DataFim: DateTime (required)
├── Observacao: string? (max 500)
├── DataCriacao: DateTime
└── Ativo: bool (default true)
```

**Relacionamentos:**
- `Usuario` 1:N `Ausencia`
- `TipoAusencia` 1:N `Ausencia`

**Services (padrão CQRS existente):**
- `IAusenciaQueryService` / `AusenciaQueryService` — Consultas de ausências por período, usuário, tipo
- `IAusenciaCommandService` / `AusenciaCommandService` — CRUD com validação
- `IAusenciaValidationService` / `AusenciaValidationService` — Validação de datas e sobreposição
- `ITipoAusenciaQueryService` / `TipoAusenciaQueryService` — Consultas de tipos
- `ITipoAusenciaCommandService` / `TipoAusenciaCommandService` — CRUD de tipos

**Páginas Razor:**
- `Pages/Ausencias/Index.cshtml` — Listagem com filtros
- `Pages/Ausencias/Create.cshtml` — Criação
- `Pages/Ausencias/Edit.cshtml` — Edição
- `Pages/TiposAusencia/Index.cshtml` — Listagem de tipos
- `Pages/TiposAusencia/Create.cshtml` — Criação de tipo
- `Pages/TiposAusencia/Edit.cshtml` — Edição de tipo

**Integração no GridService:**
- Método `GetAusenciasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, int? equipeId)` adicionado ao `IGridService`
- `GridDataViewModel` expandido com `AusenciasPorDia` no `UsuarioGridViewModel`
- Nova ViewModel `AusenciaGridViewModel` com Id, TipoNome, TipoCor, DataInicio, DataFim

### Fase 3: Diagrama de Gantt (Req 12-15)

**Abordagem técnica:**
O Diagrama de Gantt será implementado como uma página Razor com renderização server-side usando HTML/CSS puro (divs posicionadas com CSS Grid ou Flexbox). Não será utilizada biblioteca JavaScript de Gantt externa para manter consistência com o stack existente e evitar dependências adicionais.

**Estrutura da página:**
```
Pages/Gantt/
├── Index.cshtml          — Página principal do Gantt
└── Index.cshtml.cs       — PageModel com lógica de carregamento
```

**Service:**
- `IGanttService` / `GanttService` — Carrega dados formatados para o Gantt
  - `GetGanttDataAsync(GanttFilterViewModel filter)` → `GanttDataViewModel`
  - Reutiliza `GridService.GetAtividadesPorPeriodoAsync` e `GetAusenciasPorPeriodoAsync`

**ViewModels:**
```
GanttFilterViewModel
├── DataInicio: DateTime
├── DataFim: DateTime
├── EquipeId: int?
├── ProjetoId: int?
├── Visao: string ("semanal" | "mensal")

GanttDataViewModel
├── Usuarios: List<GanttUsuarioViewModel>
├── Dias: List<DiaGridViewModel>
├── DataInicio: DateTime
├── DataFim: DateTime

GanttUsuarioViewModel
├── Id: int
├── Nome: string
├── Iniciais: string
├── Barras: List<GanttBarraViewModel>

GanttBarraViewModel
├── Id: int
├── Tipo: string ("atividade" | "ausencia")
├── Nome: string
├── Cor: string
├── DataInicio: DateTime
├── DataFim: DateTime
├── DiaInicioOffset: int    — Coluna de início (0-based)
├── DuracaoDias: int        — Largura em colunas
├── EstaAtrasada: bool
├── Url: string             — Link para detalhes
├── TooltipHtml: string     — Conteúdo do tooltip
```

**Renderização CSS:**
```css
.gantt-container {
    display: grid;
    grid-template-columns: 200px repeat(var(--gantt-dias), 1fr);
}

.gantt-barra {
    grid-column: calc(var(--inicio) + 2) / span var(--duracao);
    /* +2 porque coluna 1 é o nome do usuário */
}
```

Cada barra usa CSS custom properties (`--inicio`, `--duracao`) definidas inline no Razor para posicionamento. Isso evita JavaScript complexo e mantém a renderização server-side.

**Visão mensal no Grid existente (Req 14):**
- Novo período "Mensal" adicionado ao `PeriodosDisponiveis` no `GridPageModel`
- Quando visão mensal ativa, Grid renderiza com CSS class `grid-compact` que reduz largura das colunas
- Células compactas mostram apenas badges coloridos com contagem
- Click na célula abre popover Bootstrap com lista de atividades

### Fase 4: Consolidação (Req 16)

- Criação de `DEPRECATED.md` no projeto legado
- Documentação dos repositórios não implementados
- Nenhuma alteração de código no projeto legado

---

## Database Changes

### Nova Migration: AddAusencias

```sql
CREATE TABLE TipoAusencia (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nome NVARCHAR(50) NOT NULL,
    Cor NVARCHAR(7) NOT NULL DEFAULT '#007bff',
    Descricao NVARCHAR(200) NULL,
    Ativo BIT NOT NULL DEFAULT 1
);

CREATE TABLE Ausencia (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL,
    TipoAusenciaId INT NOT NULL,
    DataInicio DATETIME2 NOT NULL,
    DataFim DATETIME2 NOT NULL,
    Observacao NVARCHAR(500) NULL,
    DataCriacao DATETIME2 NOT NULL DEFAULT GETDATE(),
    Ativo BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Ausencia_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    CONSTRAINT FK_Ausencia_TipoAusencia FOREIGN KEY (TipoAusenciaId) REFERENCES TipoAusencia(Id),
    CONSTRAINT CK_Ausencia_Datas CHECK (DataInicio <= DataFim)
);

CREATE INDEX IX_Ausencia_UsuarioId ON Ausencia(UsuarioId);
CREATE INDEX IX_Ausencia_TipoAusenciaId ON Ausencia(TipoAusenciaId);
CREATE INDEX IX_Ausencia_Periodo ON Ausencia(DataInicio, DataFim);
```

### Seed Data

```sql
INSERT INTO TipoAusencia (Nome, Cor, Descricao, Ativo) VALUES
('Férias', '#4CAF50', 'Período de férias do colaborador', 1),
('Folga', '#2196F3', 'Dia de folga compensatória', 1),
('Afastamento', '#FF9800', 'Afastamento por motivos diversos', 1),
('Day-off', '#9C27B0', 'Dia de folga por aniversário ou benefício', 1),
('Licença Médica', '#F44336', 'Afastamento por motivos de saúde', 1);
```

---

## Correctness Properties

### Property 1: Validação de datas da Ausência (Req 10.2)
**Tipo:** Invariante
**Descrição:** Para qualquer Ausencia, DataInicio deve ser menor ou igual a DataFim. A validação deve rejeitar qualquer Ausencia onde DataInicio > DataFim.
**Gerador:** Gerar pares de DateTime aleatórios. Quando DataInicio > DataFim, a validação deve retornar falha.

### Property 2: Validação de sobreposição de Ausências (Req 10.3)
**Tipo:** Invariante
**Descrição:** Para qualquer Usuario, não devem existir duas Ausencias ativas com períodos sobrepostos. Dois períodos [A1, A2] e [B1, B2] se sobrepõem quando A1 <= B2 AND B1 <= A2.
**Gerador:** Gerar duas Ausencias para o mesmo Usuario com períodos aleatórios. Se os períodos se sobrepõem, a segunda criação deve falhar.

### Property 3: Round-trip de TipoAusencia (Req 9.1)
**Tipo:** Round-trip
**Descrição:** Para qualquer TipoAusencia válido, salvar no banco e recuperar deve preservar todos os campos (Nome, Cor, Descricao, Ativo).
**Gerador:** Gerar TipoAusencia com Nome (1-50 chars alfanuméricos), Cor (formato #XXXXXX), Descricao (0-200 chars).

### Property 4: Consulta de Ausências por período (Req 11.1)
**Tipo:** Metamorphic
**Descrição:** Para qualquer conjunto de Ausencias e qualquer período [P1, P2], o GridService deve retornar exatamente as Ausencias que intersectam o período. Uma Ausencia [A1, A2] intersecta [P1, P2] quando A1 <= P2 AND A2 >= P1.
**Gerador:** Gerar lista de Ausencias com datas aleatórias e um período de consulta aleatório. Verificar que o resultado contém exatamente as ausências que satisfazem a condição de interseção.

### Property 5: Cálculo de duração de barras do Gantt (Req 12.2)
**Tipo:** Metamorphic
**Descrição:** Para qualquer Atividade com DataInicio e DataFim, a duração em dias da barra no Gantt deve ser igual a (DataFim - DataInicio).Days + 1. Uma atividade com duração maior deve sempre gerar uma barra com mais colunas.
**Gerador:** Gerar pares de Atividades com durações diferentes. A atividade com maior duração deve ter DuracaoDias maior.

### Property 6: Detecção de atraso no Gantt (Req 12.8)
**Tipo:** Invariante
**Descrição:** Para qualquer Atividade onde DataFimPrevista < DateTime.Today e DataFimReal é null, a propriedade EstaAtrasada deve ser true. Para atividades com DataFimReal preenchida ou DataFimPrevista >= hoje, EstaAtrasada deve ser false.
**Gerador:** Gerar Atividades com combinações aleatórias de DataFimPrevista e DataFimReal. Verificar que EstaAtrasada é consistente com a regra.

### Property 7: Geração de dias do período (Req 13.3, 13.4)
**Tipo:** Invariante
**Descrição:** Para qualquer período [DataInicio, DataFim], o número de dias gerados deve ser exatamente (DataFim - DataInicio).Days + 1. Para visão semanal, deve ser 7. Para visão mensal, deve ser entre 28 e 31.
**Gerador:** Gerar datas de início aleatórias. Para visão semanal, DataFim = DataInicio + 6 dias. Para visão mensal, DataFim = último dia do mês.

---

## Test Strategy

### Testes Unitários (xUnit + Moq + Shouldly)
- **AusenciaValidationService**: Validação de datas, sobreposição, campos obrigatórios
- **TipoAusenciaCommandService**: CRUD com validação
- **AusenciaCommandService**: CRUD com validação de sobreposição
- **GanttService**: Cálculo de barras, duração, offset, detecção de atraso
- **GridService** (extensão): Consulta de ausências por período

### Property-Based Tests (FsCheck)
- Properties 1-7 conforme seção Correctness Properties
- Mínimo 100 iterações por propriedade
- Geradores customizados para datas, nomes e cores válidas

### Testes de Integração
- Páginas Razor de TipoAusencia e Ausencia renderizam corretamente
- Migration aplica sem erros
- Seed data é inserida corretamente

### Testes Manuais (Fase 1 — CSS/HTML)
- Verificação visual do dropdown, tooltips, truncamento
- Validação de HTML com W3C validator
- Verificação de encoding UTF-8 nos arquivos CSS

---

## File Changes

### Fase 1: Correções de Usabilidade

| Arquivo | Ação | Descrição |
|---------|------|-----------|
| `wwwroot/css/site.css` | Modificar | Corrigir encoding UTF-8, consolidar z-index, adicionar @charset |
| `wwwroot/css/pages/grid.css` | Modificar | Corrigir sintaxe, remover duplicações, melhorar truncamento CSS |
| `wwwroot/css/components/dropdown.css` | Remover | Migrar estilos necessários para site.css e grid.css |
| `wwwroot/css/components/dropdown-fix.css` | Remover | Arquivo de fix redundante |
| `Pages/Grid/Index.cshtml` | Modificar | Remover JS workarounds, scroll forçado, truncamento server-side |
| `Pages/Dashboard/Index.cshtml` | Modificar | Corrigir td aninhado na tabela de Performance |

### Fase 2: Modelo de Ausências

| Arquivo | Ação | Descrição |
|---------|------|-----------|
| `Models/TipoAusencia.cs` | Criar | Entidade TipoAusencia |
| `Models/Ausencia.cs` | Criar | Entidade Ausencia |
| `Models/Usuario.cs` | Modificar | Adicionar navigation property Ausencias |
| `Data/ProGestaoContext.cs` | Modificar | Adicionar DbSets e configurações |
| `Services/Interfaces/IAusenciaQueryService.cs` | Criar | Interface de consulta |
| `Services/Interfaces/IAusenciaCommandService.cs` | Criar | Interface de comando |
| `Services/Interfaces/IAusenciaValidationService.cs` | Criar | Interface de validação |
| `Services/Interfaces/ITipoAusenciaQueryService.cs` | Criar | Interface de consulta de tipos |
| `Services/Interfaces/ITipoAusenciaCommandService.cs` | Criar | Interface de comando de tipos |
| `Services/Ausencias/AusenciaQueryService.cs` | Criar | Implementação de consulta |
| `Services/Ausencias/AusenciaCommandService.cs` | Criar | Implementação de comando |
| `Services/Ausencias/AusenciaValidationService.cs` | Criar | Implementação de validação |
| `Services/TiposAusencia/TipoAusenciaQueryService.cs` | Criar | Implementação de consulta |
| `Services/TiposAusencia/TipoAusenciaCommandService.cs` | Criar | Implementação de comando |
| `ViewModels/Ausencia/AusenciaViewModel.cs` | Criar | ViewModel de ausência |
| `ViewModels/Ausencia/AusenciaGridViewModel.cs` | Criar | ViewModel para grid |
| `ViewModels/TipoAusencia/TipoAusenciaViewModel.cs` | Criar | ViewModel de tipo |
| `Pages/Ausencias/Index.cshtml` | Criar | Listagem de ausências |
| `Pages/Ausencias/Index.cshtml.cs` | Criar | PageModel |
| `Pages/Ausencias/Create.cshtml` | Criar | Criação de ausência |
| `Pages/Ausencias/Create.cshtml.cs` | Criar | PageModel |
| `Pages/Ausencias/Edit.cshtml` | Criar | Edição de ausência |
| `Pages/Ausencias/Edit.cshtml.cs` | Criar | PageModel |
| `Pages/TiposAusencia/Index.cshtml` | Criar | Listagem de tipos |
| `Pages/TiposAusencia/Index.cshtml.cs` | Criar | PageModel |
| `Pages/TiposAusencia/Create.cshtml` | Criar | Criação de tipo |
| `Pages/TiposAusencia/Create.cshtml.cs` | Criar | PageModel |
| `Pages/TiposAusencia/Edit.cshtml` | Criar | Edição de tipo |
| `Pages/TiposAusencia/Edit.cshtml.cs` | Criar | PageModel |
| `Services/GridService.cs` | Modificar | Adicionar consulta de ausências |
| `ViewModels/Grid/UsuarioGridViewModel.cs` | Modificar | Adicionar AusenciasPorDia |
| `Pages/Grid/Index.cshtml` | Modificar | Renderizar ausências nas células |
| `wwwroot/css/pages/grid.css` | Modificar | Estilos para indicadores de ausência |
| `Configuration/ServiceConfiguration.cs` | Modificar | Registrar novos services |
| `Pages/Shared/_Layout.cshtml` | Modificar | Adicionar itens de menu Ausências e Tipos |

### Fase 3: Diagrama de Gantt

| Arquivo | Ação | Descrição |
|---------|------|-----------|
| `Services/Interfaces/IGanttService.cs` | Criar | Interface do service |
| `Services/Gantt/GanttService.cs` | Criar | Implementação |
| `ViewModels/Gantt/GanttFilterViewModel.cs` | Criar | Filtros |
| `ViewModels/Gantt/GanttDataViewModel.cs` | Criar | Dados do Gantt |
| `ViewModels/Gantt/GanttUsuarioViewModel.cs` | Criar | Usuário no Gantt |
| `ViewModels/Gantt/GanttBarraViewModel.cs` | Criar | Barra do Gantt |
| `Pages/Gantt/Index.cshtml` | Criar | Página do Gantt |
| `Pages/Gantt/Index.cshtml.cs` | Criar | PageModel |
| `wwwroot/css/pages/gantt.css` | Criar | Estilos do Gantt |
| `Pages/Shared/_Layout.cshtml` | Modificar | Adicionar item Gantt no menu |
| `Configuration/ServiceConfiguration.cs` | Modificar | Registrar GanttService |
| `Pages/Grid/Index.cshtml` | Modificar | Adicionar visão mensal compacta |
| `Pages/Grid/Index.cshtml.cs` | Modificar | Suportar período mensal |
| `wwwroot/css/pages/grid.css` | Modificar | Estilos para visão compacta |

### Fase 4: Consolidação

| Arquivo | Ação | Descrição |
|---------|------|-----------|
| `Pro.Gestao.MVC/DEPRECATED.md` | Criar | Documentação de depreciação |
