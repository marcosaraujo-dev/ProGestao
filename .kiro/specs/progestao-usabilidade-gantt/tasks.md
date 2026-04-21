# Tasks

## Phase 1: Correções de Usabilidade Urgentes

- [ ] 1. Correção de encoding e sintaxe CSS
  - [~] 1.1 Corrigir encoding UTF-8 do arquivo `site.css`, substituindo caracteres corrompidos (Vers�o → Versão, Descri��o → Descrição, etc.) e adicionando `@charset "UTF-8"` no início
  - [~] 1.2 Corrigir erro de sintaxe na regra `.activity-title span` em `grid.css`, substituindo `display: blockactivity-item white-space: nowrap;` por `display: block; white-space: nowrap;`

- [ ] 2. Consolidação de CSS duplicado e hierarquia de z-index
  - [~] 2.1 Consolidar todas as variáveis de z-index em `:root` no `site.css`, removendo declarações duplicadas de `dropdown.css` e alinhando com valores padrão do Bootstrap
  - [~] 2.2 Remover declarações CSS duplicadas para `.activity-cell`, `.activity-item` e `.activity-title` em `grid.css`, mantendo apenas uma definição consolidada
  - [~] 2.3 Migrar estilos necessários de `dropdown.css` para `site.css` (globais) e `grid.css` (específicos do grid), e remover os arquivos `dropdown.css` e `dropdown-fix.css`
  - [~] 2.4 Reduzir uso de `!important` nos arquivos CSS, mantendo apenas os estritamente necessários para sobrescrever Bootstrap

- [ ] 3. Correção do dropdown de período e tooltips no Grid
  - [~] 3.1 Remover do `Pages/Grid/Index.cshtml` todo o JavaScript de workaround do dropdown: MutationObserver, setTimeout para posicionamento, manipulação de estilos inline do dropdownMenu, e instanciação manual do `bootstrap.Dropdown`
  - [ ] 3.2 Configurar tooltips do Grid com `container: 'body'` na inicialização Bootstrap para evitar corte pelo overflow do `grid-wrapper`
  - [ ] 3.3 Remover a referência ao CSS `dropdown-fix.css` no `@section Styles` do `Pages/Grid/Index.cshtml`

- [ ] 4. Remoção de scroll forçado e correção de truncamento
  - [ ] 4.1 Remover todas as chamadas `window.scrollTo(0, 0)` e lógica de remoção de `autofocus`/`blur()` dos scripts do `Pages/Grid/Index.cshtml` (eventos DOMContentLoaded, load e setTimeout)
  - [ ] 4.2 Substituir truncamento server-side `atividade.Nome.Substring(0, 10)` por exibição do nome completo no Razor, delegando truncamento visual ao CSS existente (`-webkit-line-clamp: 2` e `text-overflow: ellipsis`)

- [ ] 5. Correção de HTML inválido no Dashboard
  - [ ] 5.1 Corrigir a tabela de Performance da Equipe em `Pages/Dashboard/Index.cshtml`, removendo o `<td>` aninhado dentro de outro `<td>` na coluna de progresso, mantendo apenas uma célula com a barra de progresso

## Phase 2: Modelo de Ausências

- [ ] 6. Criar entidades TipoAusencia e Ausencia
  - [ ] 6.1 Criar modelo `Models/TipoAusencia.cs` com propriedades Id, Nome, Cor, Descricao, Ativo e coleção de navegação Ausencias
  - [ ] 6.2 Criar modelo `Models/Ausencia.cs` com propriedades Id, UsuarioId, TipoAusenciaId, DataInicio, DataFim, Observacao, DataCriacao, Ativo e navigation properties
  - [ ] 6.3 Adicionar `ICollection<Ausencia> Ausencias` ao modelo `Usuario.cs`
  - [ ] 6.4 Adicionar DbSets `TiposAusencia` e `Ausencias` ao `ProGestaoContext.cs` com configurações de relacionamento, índices e constraint de datas
  - [ ] 6.5 Criar migration EF Core para as novas tabelas com seed data dos 5 tipos de ausência padrão (Férias, Folga, Afastamento, Day-off, Licença Médica)

- [ ] 7. Criar services de TipoAusencia (CQRS)
  - [ ] 7.1 Criar interface `Services/Interfaces/ITipoAusenciaQueryService.cs` com métodos GetAllAsync, GetByIdAsync, GetAtivosAsync
  - [ ] 7.2 Criar interface `Services/Interfaces/ITipoAusenciaCommandService.cs` com métodos CreateAsync, UpdateAsync, DesativarAsync
  - [ ] 7.3 Criar implementação `Services/TiposAusencia/TipoAusenciaQueryService.cs`
  - [ ] 7.4 Criar implementação `Services/TiposAusencia/TipoAusenciaCommandService.cs` com validação de nome obrigatório e formato de cor
  - [ ] 7.5 Criar ViewModel `ViewModels/TipoAusencia/TipoAusenciaViewModel.cs`

- [ ] 8. Criar páginas Razor de TipoAusencia
  - [ ] 8.1 Criar página de listagem `Pages/TiposAusencia/Index.cshtml` com tabela de tipos ativos e opções de criar, editar e desativar
  - [ ] 8.2 Criar página de criação `Pages/TiposAusencia/Create.cshtml` com formulário validado
  - [ ] 8.3 Criar página de edição `Pages/TiposAusencia/Edit.cshtml` com formulário validado
  - [ ] 8.4 Registrar services de TipoAusencia no `ServiceConfiguration.cs`

- [ ] 9. Criar services de Ausencia (CQRS)
  - [ ] 9.1 Criar interface `Services/Interfaces/IAusenciaQueryService.cs` com métodos GetByFiltroAsync, GetByIdAsync, GetPorPeriodoAsync, GetPorUsuarioAsync
  - [ ] 9.2 Criar interface `Services/Interfaces/IAusenciaCommandService.cs` com métodos CreateAsync, UpdateAsync, DesativarAsync
  - [ ] 9.3 Criar interface `Services/Interfaces/IAusenciaValidationService.cs` com métodos ValidateCreateAsync, ValidateUpdateAsync, ValidateSobreposicaoAsync
  - [ ] 9.4 Criar implementação `Services/Ausencias/AusenciaValidationService.cs` com validação de datas (DataInicio <= DataFim) e sobreposição de períodos
  - [ ] 9.5 Criar implementação `Services/Ausencias/AusenciaQueryService.cs`
  - [ ] 9.6 Criar implementação `Services/Ausencias/AusenciaCommandService.cs` usando Result Pattern
  - [ ] 9.7 Criar ViewModels `ViewModels/Ausencia/AusenciaViewModel.cs` e `ViewModels/Ausencia/AusenciaGridViewModel.cs`

- [ ] 10. Criar páginas Razor de Ausencia
  - [ ] 10.1 Criar página de listagem `Pages/Ausencias/Index.cshtml` com filtros por usuário, tipo e período
  - [ ] 10.2 Criar página de criação `Pages/Ausencias/Create.cshtml` com seleção de usuário, tipo e datas
  - [ ] 10.3 Criar página de edição `Pages/Ausencias/Edit.cshtml`
  - [ ] 10.4 Registrar services de Ausencia no `ServiceConfiguration.cs`
  - [ ] 10.5 Adicionar itens de menu "Ausências" e "Tipos de Ausência" no `_Layout.cshtml`

- [ ] 11. Integrar ausências no Grid Semanal
  - [ ] 11.1 Adicionar método `GetAusenciasPorPeriodoAsync` ao `IGridService` e implementar no `GridService.cs`
  - [ ] 11.2 Expandir `UsuarioGridViewModel` com propriedade `AusenciasPorDia` (Dictionary<DateTime, List<AusenciaGridViewModel>>)
  - [ ] 11.3 Modificar `GridService.GetGridDataAsync` para carregar e mapear ausências junto com atividades
  - [ ] 11.4 Modificar `Pages/Grid/Index.cshtml` para renderizar indicadores visuais de ausência nas células (cor do tipo, nome, fundo diferenciado)
  - [ ] 11.5 Adicionar estilos CSS para indicadores de ausência em `grid.css` (fundo listrado, badge com cor do tipo)

## Phase 3: Diagrama de Gantt

- [ ] 12. Criar service e ViewModels do Gantt
  - [ ] 12.1 Criar ViewModels em `ViewModels/Gantt/`: GanttFilterViewModel, GanttDataViewModel, GanttUsuarioViewModel, GanttBarraViewModel
  - [ ] 12.2 Criar interface `Services/Interfaces/IGanttService.cs` com método GetGanttDataAsync
  - [ ] 12.3 Criar implementação `Services/Gantt/GanttService.cs` que calcula barras (offset, duração, cor, atraso) a partir de atividades e ausências
  - [ ] 12.4 Registrar GanttService no `ServiceConfiguration.cs`

- [ ] 13. Criar página do Diagrama de Gantt
  - [ ] 13.1 Criar `Pages/Gantt/Index.cshtml.cs` (PageModel) com carregamento de dados, filtros por equipe/projeto e navegação temporal (semanal/mensal, anterior/próximo, hoje)
  - [ ] 13.2 Criar `Pages/Gantt/Index.cshtml` com renderização de barras horizontais usando CSS Grid, tooltips, links para detalhes, destaque do dia atual e indicadores de atraso
  - [ ] 13.3 Criar `wwwroot/css/pages/gantt.css` com estilos do diagrama: container CSS Grid, barras de atividade, barras de ausência (hachurado), linha do dia atual, responsividade
  - [ ] 13.4 Adicionar item "Gantt" no menu dropdown "Atividades" do `_Layout.cshtml`, após "Grid Semanal", com ícone `fa-chart-gantt`

- [ ] 14. Adicionar visão mensal ao Grid Semanal
  - [ ] 14.1 Adicionar opção "Mensal" (30 dias) ao `PeriodosDisponiveis` no PageModel do Grid e ajustar a lógica de cálculo de período
  - [ ] 14.2 Modificar `Pages/Grid/Index.cshtml` para renderizar visão compacta quando período mensal está ativo: cabeçalhos com número do dia, células com badges coloridos e contagem, popover ao clicar
  - [ ] 14.3 Adicionar estilos CSS para visão compacta mensal em `grid.css` (colunas estreitas, badges, popover)

## Phase 4: Consolidação

- [ ] 15. Depreciar projeto legado Pro.Gestao.MVC
  - [ ] 15.1 Criar arquivo `DEPRECATED.md` na raiz do projeto `Pro.Gestao.MVC` documentando a depreciação, listando repositórios com NotImplementedException e direcionando para `src/ProGestao`
