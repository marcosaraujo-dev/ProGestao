using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Projetos;
using ProGestao.ViewModels.Atividade;

namespace ProGestao.Pages.Projetos
{
    /// <summary>
    /// PageModel para visualização detalhada de projetos
    /// Implementa visualização read-only com métricas e histórico
    /// </summary>
    public class DetailsModel : PageModel
    {
        #region Dependencies

        private readonly IProjetoQueryService _queryService;
        private readonly ILogger<DetailsModel> _logger;

        #endregion

        #region Constructor

        public DetailsModel(
            IProjetoQueryService queryService,
            ILogger<DetailsModel> logger)
        {
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Properties

        public ProjetoViewModel Projeto { get; set; } = new ProjetoViewModel();
        public List<AtividadeResumoViewModel> AtividadesRecentes { get; set; } = new List<AtividadeResumoViewModel>();

        // Métricas calculadas
        public ProjetoMetricasViewModel Metricas { get; set; } = new ProjetoMetricasViewModel();

        // Flags de controle
        public bool ProjetoEncontrado { get; set; } = false;
        public bool TemPermissaoEdicao { get; set; } = true;
        public bool TemPermissaoExclusao { get; set; } = true;

        #endregion

        #region GET Handler

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                _logger.LogInformation("Carregando detalhes do projeto {ProjetoId}", id);

                var projeto = await _queryService.GetProjetoComDetalhesAsync(id);
                if (projeto == null)
                {
                    _logger.LogWarning("Projeto {ProjetoId} não encontrado", id);
                    ProjetoEncontrado = false;
                    SetErrorMessage("Projeto não encontrado.");
                    return Page(); // Retorna a página com erro, mas não redireciona
                }

                Projeto = projeto;
                ProjetoEncontrado = true;

                // Executar carregamentos paralelos para melhor performance
                await Task.WhenAll(
                    LoadAtividadesRecentesAsync(id),
                    CalculateMetricsAsync(id),
                    CheckPermissionsAsync(id)
                );

                _logger.LogInformation("Detalhes do projeto {ProjetoId} carregados com sucesso", id);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes do projeto {ProjetoId}", id);
                SetErrorMessage("Erro ao carregar projeto. Tente novamente.");
                ProjetoEncontrado = false;
                return Page();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Carrega as atividades mais recentes do projeto
        /// </summary>
        private async Task LoadAtividadesRecentesAsync(int projetoId)
        {
            try
            {
                _logger.LogDebug("Carregando atividades recentes do projeto {ProjetoId}", projetoId);

                // TODO: Implementar método no service para buscar atividades do projeto
                // Por enquanto, usar dados simulados ou disponíveis no projeto carregado
                AtividadesRecentes = new List<AtividadeResumoViewModel>();

                _logger.LogDebug("Carregadas {Count} atividades recentes", AtividadesRecentes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar atividades recentes do projeto {ProjetoId}", projetoId);
                AtividadesRecentes = new List<AtividadeResumoViewModel>();
            }
        }

        /// <summary>
        /// Calcula métricas detalhadas do projeto
        /// </summary>
        private async Task CalculateMetricsAsync(int projetoId)
        {
            try
            {
                _logger.LogDebug("Calculando métricas do projeto {ProjetoId}", projetoId);

                var projetoComAtividades = await _queryService.GetProjetoComAtividadesAsync(projetoId);

                if (projetoComAtividades != null)
                {
                    Metricas = new ProjetoMetricasViewModel
                    {
                        TotalAtividades = projetoComAtividades.QtdAtividades,
                        AtividadesConcluidas = projetoComAtividades.QtdAtividadesConcluidas,
                        AtividadesAndamento = projetoComAtividades.QtdAtividades - projetoComAtividades.QtdAtividadesConcluidas,
                        PercentualConclusao = projetoComAtividades.PercentualConclusao,

                        // Métricas de prazo
                        DiasRestantes = CalculateDaysRemaining(projetoComAtividades.DataFimPrevista),
                        DiasAtraso = CalculateDaysOverdue(projetoComAtividades.DataFimPrevista, projetoComAtividades.StatusNome),

                        // Status
                        EstaAtrasado = projetoComAtividades.EstaAtrasado,
                        EstaNoUltimaSemana = IsInLastWeek(projetoComAtividades.DataFimPrevista),
                        EstaNoPrazo = !projetoComAtividades.EstaAtrasado && projetoComAtividades.DataFimPrevista.HasValue
                    };

                    // Calcular progresso semanal (simulado por enquanto)
                    Metricas.ProgressoUltimaSemana = CalculateWeeklyProgress();
                }

                _logger.LogDebug("Métricas calculadas para projeto {ProjetoId}", projetoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular métricas do projeto {ProjetoId}", projetoId);
                Metricas = new ProjetoMetricasViewModel(); // Valores padrão
            }
        }

        /// <summary>
        /// Verifica permissões do usuário para este projeto
        /// </summary>
        private async Task CheckPermissionsAsync(int projetoId)
        {
            try
            {
                // TODO: Implementar verificação real de permissões baseada no usuário logado
                // Por enquanto, usar regras baseadas no status do projeto

                TemPermissaoEdicao = Projeto.StatusNome != "Concluído" && Projeto.StatusNome != "Cancelado";
                TemPermissaoExclusao = Projeto.StatusNome != "Em Andamento" || Projeto.QtdAtividades == 0;

                _logger.LogDebug("Permissões verificadas para projeto {ProjetoId}: Edição={Edicao}, Exclusão={Exclusao}",
                    projetoId, TemPermissaoEdicao, TemPermissaoExclusao);

                await Task.CompletedTask; // Para manter a assinatura async
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar permissões para projeto {ProjetoId}", projetoId);

                // Em caso de erro, ser conservador com as permissões
                TemPermissaoEdicao = false;
                TemPermissaoExclusao = false;
            }
        }

        /// <summary>
        /// Calcula dias restantes até o prazo
        /// </summary>
        private static int? CalculateDaysRemaining(DateTime? dataFimPrevista)
        {
            if (!dataFimPrevista.HasValue)
                return null;

            var days = (dataFimPrevista.Value.Date - DateTime.Today).Days;
            return days >= 0 ? days : null;
        }

        /// <summary>
        /// Calcula dias de atraso
        /// </summary>
        private static int? CalculateDaysOverdue(DateTime? dataFimPrevista, string? statusNome)
        {
            if (!dataFimPrevista.HasValue || statusNome == "Concluído")
                return null;

            var days = (DateTime.Today - dataFimPrevista.Value.Date).Days;
            return days > 0 ? days : null;
        }

        /// <summary>
        /// Verifica se está na última semana antes do prazo
        /// </summary>
        private static bool IsInLastWeek(DateTime? dataFimPrevista)
        {
            if (!dataFimPrevista.HasValue)
                return false;

            var daysRemaining = (dataFimPrevista.Value.Date - DateTime.Today).Days;
            return daysRemaining >= 0 && daysRemaining <= 7;
        }

        /// <summary>
        /// Calcula progresso da última semana (simulado)
        /// </summary>
        private static decimal CalculateWeeklyProgress()
        {
            // TODO: Implementar cálculo real baseado no histórico de atividades
            // Por enquanto, retornar valor simulado
            return new Random().Next(0, 20); // 0-20% de progresso semanal
        }

        /// <summary>
        /// Define mensagem de erro
        /// </summary>
        private void SetErrorMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                TempData["ErrorMessage"] = message.Trim();
            }
        }

        #endregion

        #region Public Methods for View

        /// <summary>
        /// Retorna classe CSS para o status do projeto
        /// </summary>
        public string GetStatusClass(string? statusNome)
        {
            return statusNome?.ToLowerInvariant() switch
            {
                "planejamento" => "badge bg-warning text-dark",
                "em andamento" => "badge bg-primary",
                "concluído" => "badge bg-success",
                "cancelado" => "badge bg-danger",
                "pausado" => "badge bg-secondary",
                _ => "badge bg-light text-dark"
            };
        }

        /// <summary>
        /// Retorna ícone para o status do projeto
        /// </summary>
        public string GetStatusIcon(string? statusNome)
        {
            return statusNome?.ToLowerInvariant() switch
            {
                "planejamento" => "fas fa-clipboard-list",
                "em andamento" => "fas fa-play-circle",
                "concluído" => "fas fa-check-circle",
                "cancelado" => "fas fa-times-circle",
                "pausado" => "fas fa-pause-circle",
                _ => "fas fa-circle"
            };
        }

        /// <summary>
        /// Retorna classe CSS para a barra de progresso
        /// </summary>
        public string GetProgressBarClass()
        {
            if (Metricas.EstaAtrasado)
                return "bg-danger";
            if (Metricas.EstaNoUltimaSemana)
                return "bg-warning";
            return "bg-success";
        }

        /// <summary>
        /// Retorna texto descritivo do status do prazo
        /// </summary>
        public string GetDeadlineStatusText()
        {
            if (Metricas.DiasAtraso.HasValue)
                return $"Atrasado há {Metricas.DiasAtraso.Value} dia(s)";

            if (Metricas.DiasRestantes.HasValue)
            {
                if (Metricas.DiasRestantes.Value == 0)
                    return "Vence hoje";
                if (Metricas.DiasRestantes.Value == 1)
                    return "Vence amanhã";
                return $"{Metricas.DiasRestantes.Value} dia(s) restante(s)";
            }

            return "Sem prazo definido";
        }

        #endregion
    }
}