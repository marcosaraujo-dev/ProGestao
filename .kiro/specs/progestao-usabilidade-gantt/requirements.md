# Requirements Document

## Introduction

O ProGestao é um sistema ASP.NET Core 9 com Razor Pages para gestão de atividades de equipes de desenvolvimento. Este documento especifica os requisitos para melhorias de usabilidade, correções de bugs de UI, implementação de cadastro de ausências e construção de um diagrama de Gantt real. As melhorias estão organizadas em 4 fases: correções urgentes de usabilidade, modelo de ausências, diagrama de Gantt e consolidação do projeto legado.

## Glossary

- **Grid_Semanal**: Página que exibe atividades dos usuários organizadas em colunas por dia da semana, localizada em `Pages/Grid/Index.cshtml`
- **Diagrama_Gantt**: Visualização de barras horizontais representando a duração de atividades ao longo do tempo, com eixo X temporal e eixo Y por usuário
- **Timeline**: Página que exibe eventos de atividades em ordem cronológica, localizada em `Pages/Timeline/Index.cshtml`
- **Dashboard**: Página principal com métricas e gráficos de performance da equipe, localizada em `Pages/Dashboard/Index.cshtml`
- **Dropdown_Periodo**: Componente dropdown Bootstrap para seleção de período no Grid_Semanal
- **Grid_Wrapper**: Container `div.grid-wrapper` com `overflow: auto` e `max-height: 70vh` que envolve a tabela do Grid_Semanal
- **Ausencia**: Registro de período em que um Usuário está indisponível (férias, folga, afastamento, day-off)
- **TipoAusencia**: Classificação de uma Ausencia (Férias, Folga, Afastamento, Day-off, Licença Médica)
- **Atividade**: Entidade principal do sistema representando uma tarefa atribuída a um Usuário, com datas de início/fim, status e prioridade
- **Usuario**: Membro de uma Equipe que executa Atividades
- **Equipe**: Agrupamento de Usuarios
- **Projeto**: Agrupamento de Atividades com responsável e datas
- **ProGestaoContext**: DbContext do Entity Framework Core que gerencia o acesso ao banco de dados SQL Server
- **GridService**: Service responsável por carregar dados do Grid_Semanal (usuários, atividades por período, métricas)
- **CSS_Z_Index**: Propriedade CSS que controla a ordem de empilhamento de elementos sobrepostos
- **Projeto_Legado_MVC**: Projeto `Pro.Gestao.MVC` com repositórios não implementados (NotImplementedException) e DI duplicada

---

## Requirements

### Requirement 1: Correção do posicionamento do Dropdown de Período no Grid

**User Story:** Como um gestor de equipe, eu quero que o dropdown de seleção de período no Grid_Semanal abra corretamente posicionado abaixo do botão, para que eu consiga selecionar o período sem problemas visuais.

#### Acceptance Criteria

1. WHEN o Usuário clica no Dropdown_Periodo no Grid_Semanal, THE Grid_Semanal SHALL exibir o menu dropdown posicionado diretamente abaixo do botão de toggle, sem deslocamento horizontal ou vertical inesperado
2. WHILE o Dropdown_Periodo está aberto, THE Grid_Semanal SHALL exibir o menu dropdown acima de todos os outros elementos da página, incluindo o header-blue, o grid-container e os cabeçalhos sticky da tabela
3. WHEN o Dropdown_Periodo é aberto, THE Grid_Semanal SHALL posicionar o menu usando exclusivamente CSS sem necessidade de MutationObserver, setTimeout ou manipulação JavaScript de estilos inline
4. THE Grid_Semanal SHALL definir uma hierarquia de CSS_Z_Index consolidada em um único arquivo CSS, eliminando declarações `!important` duplicadas entre `site.css`, `dropdown.css` e `grid.css`

---

### Requirement 2: Correção de tooltips e popovers cortados pelo overflow do Grid

**User Story:** Como um desenvolvedor visualizando o Grid_Semanal, eu quero que os tooltips das atividades sejam exibidos por completo, para que eu consiga ler as informações detalhadas sem que o texto seja cortado.

#### Acceptance Criteria

1. WHEN o Usuário passa o mouse sobre uma Atividade no Grid_Semanal, THE Grid_Semanal SHALL exibir o tooltip completo visível acima do Grid_Wrapper, sem corte por overflow
2. THE Grid_Semanal SHALL configurar os tooltips Bootstrap com a opção `boundary` definida como `'window'` ou `container` definido como `'body'`, garantindo que o tooltip seja renderizado fora do Grid_Wrapper
3. IF o tooltip de uma Atividade ultrapassar os limites visíveis da viewport, THEN THE Grid_Semanal SHALL reposicionar o tooltip automaticamente para um lado visível usando o comportamento `flip` do Bootstrap

---

### Requirement 3: Remoção de scroll forçado para o topo no Grid

**User Story:** Como um usuário navegando no Grid_Semanal, eu quero que a página mantenha minha posição de scroll ao carregar, para que eu não perca o contexto de onde estava visualizando.

#### Acceptance Criteria

1. WHEN a página do Grid_Semanal é carregada, THE Grid_Semanal SHALL renderizar sem executar chamadas `window.scrollTo(0, 0)` forçadas
2. THE Grid_Semanal SHALL remover todas as chamadas redundantes de `window.scrollTo(0, 0)` dos eventos `DOMContentLoaded`, `load` e `setTimeout` presentes no script da página
3. THE Grid_Semanal SHALL remover a lógica de remoção de atributos `autofocus` e chamadas `blur()` que forçam o foco para o topo

---

### Requirement 4: Correção de HTML inválido na tabela de Performance da Equipe no Dashboard

**User Story:** Como um gestor visualizando o Dashboard, eu quero que a tabela de Performance da Equipe seja renderizada com HTML válido, para que o layout não quebre e os dados sejam exibidos corretamente.

#### Acceptance Criteria

1. THE Dashboard SHALL renderizar a tabela de Performance da Equipe com estrutura HTML válida, sem elementos `<td>` aninhados dentro de outros elementos `<td>`
2. THE Dashboard SHALL exibir a barra de progresso de taxa de conclusão dentro de uma única célula `<td>` por linha, sem duplicação de containers `<div class="progress">`
3. WHEN a tabela de Performance da Equipe é renderizada, THE Dashboard SHALL produzir HTML que passe na validação W3C sem erros de aninhamento de elementos de tabela

---

### Requirement 5: Aumento do limite de truncamento de títulos no Grid

**User Story:** Como um desenvolvedor visualizando o Grid_Semanal, eu quero que os títulos das atividades exibam mais caracteres, para que eu consiga identificar a atividade sem precisar passar o mouse para ver o tooltip.

#### Acceptance Criteria

1. THE Grid_Semanal SHALL exibir títulos de Atividades com truncamento CSS via `text-overflow: ellipsis` e `-webkit-line-clamp`, em vez de truncamento server-side fixo em 10 caracteres
2. THE Grid_Semanal SHALL remover a lógica Razor de truncamento `atividade.Nome.Substring(0, 10)` e delegar o truncamento visual exclusivamente ao CSS
3. WHILE a largura da célula do Grid_Semanal permite exibição de texto, THE Grid_Semanal SHALL exibir o máximo de caracteres possível do título da Atividade, limitado a 2 linhas de texto

---

### Requirement 6: Correção de encoding quebrado no CSS

**User Story:** Como um desenvolvedor mantendo o código do ProGestao, eu quero que os arquivos CSS usem encoding UTF-8 correto, para que comentários em português sejam legíveis e não apareçam caracteres corrompidos.

#### Acceptance Criteria

1. THE ProGestao SHALL salvar o arquivo `site.css` com encoding UTF-8 válido, substituindo todos os caracteres corrompidos (ex: `Vers�o`, `Descri��o`, `IMPORTA��ES`) pelos equivalentes em português correto
2. THE ProGestao SHALL incluir a declaração `@charset "UTF-8";` no início de cada arquivo CSS que contenha caracteres não-ASCII

---

### Requirement 7: Correção de erro de sintaxe no CSS do Grid

**User Story:** Como um desenvolvedor mantendo o código do ProGestao, eu quero que o CSS do Grid não contenha erros de sintaxe, para que os estilos sejam aplicados corretamente pelo navegador.

#### Acceptance Criteria

1. THE Grid_Semanal SHALL corrigir a regra CSS `.activity-title span` no arquivo `grid.css`, substituindo `display: blockactivity-item white-space: nowrap;` por propriedades CSS válidas separadas (`display: block; white-space: nowrap;`)
2. WHEN o arquivo `grid.css` é processado pelo navegador, THE Grid_Semanal SHALL produzir zero erros de parsing CSS no console do navegador

---

### Requirement 8: Consolidação de CSS duplicado e hierarquia de z-index

**User Story:** Como um desenvolvedor mantendo o código do ProGestao, eu quero que os estilos CSS estejam organizados sem duplicação e com uma hierarquia de z-index consistente, para que a manutenção seja mais simples e previsível.

#### Acceptance Criteria

1. THE ProGestao SHALL definir todas as variáveis de CSS_Z_Index em um único local (`:root` no `site.css`), removendo declarações duplicadas de `--z-dropdown`, `--z-navbar`, `--z-sticky` e demais variáveis de z-index presentes em `dropdown.css`
2. THE ProGestao SHALL remover declarações CSS duplicadas para `.activity-cell`, `.activity-item` e `.activity-title` no arquivo `grid.css`, mantendo apenas uma definição consolidada de cada seletor
3. THE ProGestao SHALL reduzir o uso de `!important` nos arquivos CSS a no máximo as declarações estritamente necessárias para sobrescrever estilos de bibliotecas externas (Bootstrap)
4. THE ProGestao SHALL remover o arquivo `dropdown.css` dedicado, migrando os estilos necessários para `site.css` (estilos globais de dropdown) e `grid.css` (estilos específicos do dropdown do Grid)

---

### Requirement 9: Cadastro de Tipos de Ausência

**User Story:** Como um gestor de equipe, eu quero cadastrar tipos de ausência (Férias, Folga, Afastamento, Day-off, Licença Médica), para que as ausências dos membros da equipe possam ser classificadas corretamente.

#### Acceptance Criteria

1. THE ProGestaoContext SHALL incluir uma entidade `TipoAusencia` com propriedades Id, Nome (obrigatório, máximo 50 caracteres), Cor (código hexadecimal, máximo 7 caracteres), Descricao (opcional, máximo 200 caracteres) e Ativo (booleano, padrão true)
2. THE ProGestao SHALL fornecer uma página Razor de listagem de TipoAusencia exibindo todos os registros ativos com opções de criar, editar e desativar
3. THE ProGestao SHALL fornecer uma página Razor de criação/edição de TipoAusencia com validação de campos obrigatórios
4. WHEN um TipoAusencia é desativado, THE ProGestao SHALL manter o registro no banco de dados com `Ativo = false`, sem excluir fisicamente
5. THE ProGestao SHALL incluir registros seed de TipoAusencia para: Férias (cor #4CAF50), Folga (cor #2196F3), Afastamento (cor #FF9800), Day-off (cor #9C27B0) e Licença Médica (cor #F44336)

---

### Requirement 10: Cadastro de Ausências vinculadas a Usuário

**User Story:** Como um gestor de equipe, eu quero registrar ausências dos membros da equipe com data de início, data de fim e tipo, para que o Grid_Semanal e o Diagrama_Gantt reflitam a disponibilidade real de cada pessoa.

#### Acceptance Criteria

1. THE ProGestaoContext SHALL incluir uma entidade `Ausencia` com propriedades Id, UsuarioId (FK obrigatória para Usuario), TipoAusenciaId (FK obrigatória para TipoAusencia), DataInicio (obrigatória), DataFim (obrigatória), Observacao (opcional, máximo 500 caracteres), DataCriacao e Ativo (booleano, padrão true)
2. WHEN uma Ausencia é criada, THE ProGestao SHALL validar que DataInicio é menor ou igual a DataFim
3. WHEN uma Ausencia é criada, THE ProGestao SHALL validar que não existe sobreposição de datas com outra Ausencia ativa do mesmo Usuario
4. THE ProGestao SHALL fornecer uma página Razor de listagem de Ausencias com filtros por Usuario, TipoAusencia e período
5. THE ProGestao SHALL fornecer uma página Razor de criação/edição de Ausencia com seleção de Usuario, TipoAusencia e datas via date picker
6. IF uma Ausencia é excluída logicamente (Ativo = false), THEN THE ProGestao SHALL manter o registro no banco de dados para histórico

---

### Requirement 11: Integração de Ausências no Grid Semanal

**User Story:** Como um gestor de equipe, eu quero visualizar as ausências dos membros da equipe no Grid_Semanal, para que eu tenha uma visão completa da disponibilidade e das atividades de cada pessoa.

#### Acceptance Criteria

1. WHEN o Grid_Semanal é carregado, THE GridService SHALL consultar as Ausencias ativas dos Usuarios no período exibido, além das Atividades
2. WHILE um Usuario possui uma Ausencia em um dia específico, THE Grid_Semanal SHALL exibir um indicador visual na célula correspondente com a cor do TipoAusencia e o nome do tipo (ex: "Férias", "Folga")
3. WHILE um Usuario possui uma Ausencia que cobre o dia inteiro, THE Grid_Semanal SHALL aplicar um fundo visual diferenciado na célula, distinguindo visualmente de dias com atividades normais
4. THE Grid_Semanal SHALL exibir ausências e atividades simultaneamente na mesma célula quando ambas existirem para o mesmo dia e usuário

---

### Requirement 12: Diagrama de Gantt com barras horizontais

**User Story:** Como um gestor de equipe, eu quero visualizar as atividades em um diagrama de Gantt com barras horizontais representando a duração de cada atividade, para que eu tenha uma visão clara de prazos, sobreposições e carga de trabalho.

#### Acceptance Criteria

1. THE ProGestao SHALL fornecer uma nova página Razor `Pages/Gantt/Index.cshtml` que exiba um Diagrama_Gantt com eixo X representando dias e eixo Y representando Usuarios
2. WHEN o Diagrama_Gantt é carregado, THE Diagrama_Gantt SHALL renderizar cada Atividade como uma barra horizontal que se estende da DataInicio até a DataFimPrevista (ou DataFimReal se existir), posicionada na linha do Usuario responsável
3. THE Diagrama_Gantt SHALL colorir cada barra de Atividade com a cor do StatusAtividade correspondente
4. WHEN o Usuário passa o mouse sobre uma barra de Atividade no Diagrama_Gantt, THE Diagrama_Gantt SHALL exibir um tooltip com nome da Atividade, Projeto, Status, datas e prioridade
5. WHEN o Usuário clica em uma barra de Atividade no Diagrama_Gantt, THE Diagrama_Gantt SHALL navegar para a página de detalhes da Atividade (`/Atividades/Details/{id}`)
6. THE Diagrama_Gantt SHALL exibir Ausencias como barras horizontais diferenciadas (padrão listrado ou hachurado) na linha do Usuario correspondente, com a cor do TipoAusencia
7. THE Diagrama_Gantt SHALL destacar visualmente o dia atual com uma linha vertical ou coluna com fundo diferenciado
8. IF uma Atividade está atrasada (DataFimPrevista anterior à data atual e sem DataFimReal), THEN THE Diagrama_Gantt SHALL exibir a barra com indicador visual de atraso (borda vermelha ou padrão visual distinto)

---

### Requirement 13: Navegação temporal no Diagrama de Gantt

**User Story:** Como um gestor de equipe, eu quero navegar entre períodos no Diagrama de Gantt (semana, mês), para que eu consiga visualizar atividades em diferentes horizontes de tempo.

#### Acceptance Criteria

1. THE Diagrama_Gantt SHALL fornecer botões de navegação "Anterior" e "Próximo" para avançar ou retroceder o período exibido
2. THE Diagrama_Gantt SHALL fornecer um seletor de visão com opções "Semanal" (7 dias) e "Mensal" (30 dias)
3. WHEN o Usuário seleciona a visão "Semanal", THE Diagrama_Gantt SHALL exibir colunas representando cada dia da semana selecionada
4. WHEN o Usuário seleciona a visão "Mensal", THE Diagrama_Gantt SHALL exibir colunas representando cada dia do mês selecionado, com agrupamento visual por semana
5. THE Diagrama_Gantt SHALL fornecer um botão "Hoje" que centraliza a visualização no período que contém a data atual
6. THE Diagrama_Gantt SHALL fornecer filtros por Equipe e por Projeto, consistentes com os filtros existentes no Grid_Semanal

---

### Requirement 14: Visão mensal no Grid Semanal

**User Story:** Como um gestor de equipe, eu quero alternar entre visão semanal e mensal no Grid existente, para que eu tenha flexibilidade na visualização das atividades sem precisar ir ao Diagrama de Gantt.

#### Acceptance Criteria

1. THE Grid_Semanal SHALL fornecer uma opção de visão "Mensal" no Dropdown_Periodo que exiba 30 dias de atividades
2. WHEN a visão "Mensal" é selecionada, THE Grid_Semanal SHALL exibir colunas para cada dia do mês, com cabeçalhos compactos mostrando apenas o número do dia
3. WHILE a visão "Mensal" está ativa, THE Grid_Semanal SHALL reduzir a largura das colunas de dia e exibir indicadores compactos de atividades (apenas cor do status e contagem) em vez dos cards detalhados da visão semanal
4. WHEN o Usuário clica em uma célula compacta na visão mensal, THE Grid_Semanal SHALL exibir um popover ou modal com a lista detalhada de atividades daquele dia e usuário

---

### Requirement 15: Adição do Diagrama de Gantt ao menu de navegação

**User Story:** Como um usuário do ProGestao, eu quero acessar o Diagrama de Gantt pelo menu de navegação, para que eu encontre a funcionalidade facilmente.

#### Acceptance Criteria

1. THE ProGestao SHALL adicionar um item "Gantt" no menu dropdown "Atividades" do navbar, com ícone `fa-chart-gantt` e link para `/Gantt`
2. THE ProGestao SHALL posicionar o item "Gantt" após o item "Grid Semanal" no menu dropdown

---

### Requirement 16: Depreciar projeto legado Pro.Gestao.MVC

**User Story:** Como um desenvolvedor mantendo o ProGestao, eu quero que o projeto legado Pro.Gestao.MVC seja marcado como depreciado, para que novos desenvolvimentos sejam direcionados exclusivamente ao projeto ProGestao (Razor Pages).

#### Acceptance Criteria

1. THE ProGestao SHALL adicionar um arquivo `DEPRECATED.md` na raiz do projeto `Pro.Gestao.MVC` documentando que o projeto está depreciado e que o projeto ativo é `src/ProGestao`
2. THE ProGestao SHALL remover a duplicação de registros de Dependency Injection no `Program.cs` do projeto legado, se existir
3. IF o projeto legado `Pro.Gestao.MVC` contém repositórios com `NotImplementedException`, THEN THE ProGestao SHALL documentar no `DEPRECATED.md` a lista de repositórios não implementados como justificativa da depreciação
