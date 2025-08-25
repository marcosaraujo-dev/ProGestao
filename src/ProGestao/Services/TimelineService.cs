using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.ViewModels.Atividade;

namespace ProGestao.Services
{
    /// <summary>
    /// Interface para serviço de Timeline
    /// Interface Segregation: métodos específicos para Timeline
    /// </summary>
    public interface ITimelineService
    {
        Task<IList<TimelineAtividadeViewModel>> GetTimelineAtividadesAsync(
            DateTime dataInicio,
            DateTime dataFim,
            List<int>? usuarioIds = null,
            List<int>? projetoIds = null);

        Task<TimelineMetricsViewModel> GetTimelineMetricsAsync(
            DateTime dataInicio,
            DateTime dataFim,
            List<int>? usuarioIds = null,
            List<int>? projetoIds = null);
    }

    /// <summary>
    /// Implementação do serviço de Timeline
    /// Single Responsibility: gerencia apenas lógica de timeline
    /// Open-Closed: extensível para novos tipos de eventos
    /// </summary>
    public class TimelineService : ITimelineService
    {
        #region Dependencies

        private readonly ProGestaoContext _context;
        private readonly ILogger<TimelineService> _logger;

        #endregion

        #region Constructor

        public TimelineService(ProGestaoContext context, ILogger<TimelineService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Obtém eventos da timeline para o período especificado
        /// Testável: lógica isolada e com parâmetros claros
        /// Implementa retry logic para problemas temporários
        /// </summary>
        public async Task<IList<TimelineAtividadeViewModel>> GetTimelineAtividadesAsync(
            DateTime dataInicio,
            DateTime dataFim,
            List<int>? usuarioIds = null,
            List<int>? projetoIds = null)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                _logger.LogDebug("Buscando atividades para timeline: {DataInicio} a {DataFim}",
                    dataInicio, dataFim);

                var atividades = await GetAtividadesFiltradas(dataInicio, dataFim, usuarioIds, projetoIds);
                var eventos = GenerateTimelineEvents(atividades);

                _logger.LogDebug("Timeline gerada: {QtdEventos} eventos de {QtdAtividades} atividades",
                    eventos.Count, atividades.Count);

                return eventos.OrderByDescending(e => e.DataReferencia)
                            .ThenBy(e => e.AtividadeNome)
                            .ToList();
            });
        }

        /// <summary>
        /// Obtém métricas da timeline
        /// Single Responsibility: cálculo de métricas específicas
        /// </summary>
        public async Task<TimelineMetricsViewModel> GetTimelineMetricsAsync(
            DateTime dataInicio,
            DateTime dataFim,
            List<int>? usuarioIds = null,
            List<int>? projetoIds = null)
        {
            var eventos = await GetTimelineAtividadesAsync(dataInicio, dataFim, usuarioIds, projetoIds);

            return new TimelineMetricsViewModel
            {
                TotalEventos = eventos.Count,
                EventosPorTipo = eventos.GroupBy(e => e.TipoEvento)
                                      .ToDictionary(g => g.Key, g => g.Count()),
                AtividadesPorUsuario = eventos.GroupBy(e => e.UsuarioNome)
                                             .ToDictionary(g => g.Key, g => g.Count()),
                AtividadesPorProjeto = eventos.Where(e => !string.IsNullOrEmpty(e.ProjetoNome))
                                             .GroupBy(e => e.ProjetoNome)
                                             .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        #endregion

        #region Private Methods - Business Logic

        /// <summary>
        /// Busca atividades aplicando filtros
        /// DRY: lógica centralizada de filtros
        /// Implementa tratamento de erro e retry
        /// </summary>
        private async Task<List<Atividade>> GetAtividadesFiltradas(
            DateTime dataInicio,
            DateTime dataFim,
            List<int>? usuarioIds,
            List<int>? projetoIds)
        {
            try
            {
                var query = _context.Atividades
                    .AsNoTracking() // Importante: usar AsNoTracking para melhor performance
                    .Include(a => a.Projeto)
                    .Include(a => a.Usuario)
                    .Include(a => a.TipoAtividade)
                    .Include(a => a.Status)
                    .AsQueryable();

                query = ApplyFilters(query, dataInicio, dataFim, usuarioIds, projetoIds);

                // Executar query com timeout maior para problemas de conectividade
                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                return await query.ToListAsync(cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Timeout ao buscar atividades para timeline");
                throw new TimeoutException("A consulta demorou muito para responder. Tente novamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar atividades filtradas");
                throw;
            }
        }

        /// <summary>
        /// Aplica filtros na query de atividades
        /// Single Responsibility: aplicação de filtros específicos
        /// </summary>
        private static IQueryable<Atividade> ApplyFilters(
            IQueryable<Atividade> query,
            DateTime dataInicio,
            DateTime dataFim,
            List<int>? usuarioIds,
            List<int>? projetoIds)
        {
            // Filtro por período - corrigido para os tipos corretos
            query = query.Where(a =>
                (a.DataCriacao >= dataInicio && a.DataCriacao <= dataFim) ||
                (a.DataInicio >= dataInicio && a.DataInicio <= dataFim) ||
                (a.DataFimReal.HasValue && a.DataFimReal >= dataInicio && a.DataFimReal <= dataFim) ||
                (a.DataAtualizacao >= dataInicio && a.DataAtualizacao <= dataFim)
            );

            // Filtro por usuários
            if (usuarioIds?.Any() == true)
            {
                query = query.Where(a => usuarioIds.Contains(a.UsuarioId));
            }

            // Filtro por projetos
            if (projetoIds?.Any() == true)
            {
                query = query.Where(a => a.ProjetoId.HasValue && projetoIds.Contains(a.ProjetoId.Value));
            }

            return query;
        }

        /// <summary>
        /// Gera eventos da timeline a partir das atividades
        /// Open-Closed: facilita adição de novos tipos de evento
        /// </summary>
        private List<TimelineAtividadeViewModel> GenerateTimelineEvents(List<Atividade> atividades)
        {
            var eventos = new List<TimelineAtividadeViewModel>();

            foreach (var atividade in atividades)
            {
                eventos.AddRange(CreateEventsForAtividade(atividade));
            }

            return eventos;
        }

        /// <summary>
        /// Cria eventos específicos para uma atividade
        /// Strategy Pattern: diferentes estratégias de criação de eventos
        /// </summary>
        private IEnumerable<TimelineAtividadeViewModel> CreateEventsForAtividade(Atividade atividade)
        {
            var eventos = new List<TimelineAtividadeViewModel>();

            // Evento de Criação (sempre existe)
            eventos.Add(CreateEventViewModel(atividade, "Criacao", atividade.DataCriacao));

            // Evento de Início (se data de início for diferente da criação)
            // DataInicio é obrigatório mas pode representar o início efetivo da atividade
            if (atividade.DataInicio.Date != atividade.DataCriacao.Date)
            {
                eventos.Add(CreateEventViewModel(atividade, "Inicio", atividade.DataInicio));
            }

            // Evento de Conclusão (se atividade foi concluída)
            if (atividade.DataFimReal.HasValue)
            {
                eventos.Add(CreateEventViewModel(atividade, "Conclusao", atividade.DataFimReal.Value));
            }

            // Evento de Atualização (se diferente das outras datas)
            if (atividade.DataAtualizacao.Date != atividade.DataCriacao.Date &&
                atividade.DataAtualizacao.Date != atividade.DataInicio.Date &&
                (!atividade.DataFimReal.HasValue || atividade.DataAtualizacao.Date != atividade.DataFimReal.Value.Date))
            {
                eventos.Add(CreateEventViewModel(atividade, "Atualizacao", atividade.DataAtualizacao));
            }

            return eventos;
        }

        /// <summary>
        /// Cria ViewModel para um evento específico
        /// Factory Method: criação padronizada de ViewModels
        /// </summary>
        private static TimelineAtividadeViewModel CreateEventViewModel(
            Atividade atividade,
            string tipoEvento,
            DateTime dataReferencia)
        {
            return new TimelineAtividadeViewModel
            {
                Id = atividade.Id,
                AtividadeNome = atividade.Nome ?? string.Empty,
                Descricao = atividade.Descricao,
                ProjetoNome = atividade.Projeto?.Nome ?? "Sem projeto",
                UsuarioNome = atividade.Usuario?.Nome ?? "Usuário não informado",
                TipoAtividadeNome = atividade.TipoAtividade?.Nome ?? string.Empty,
                StatusNome = atividade.Status?.Nome ?? string.Empty,
                StatusCor = atividade.Status?.Cor ?? "secondary",
                DataReferencia = dataReferencia,
                TipoEvento = tipoEvento,
                Prioridade = atividade.Prioridade
            };
        }

        /// <summary>
        /// Executa operação com retry logic para problemas temporários
        /// Resilience Pattern: implementa retry com backoff exponencial no service layer
        /// </summary>
        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 2)
        {
            var delay = TimeSpan.FromMilliseconds(200);
            Exception lastException = null;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (Microsoft.Data.SqlClient.SqlException ex) when (attempt < maxRetries)
                {
                    lastException = ex;
                    _logger.LogWarning("Tentativa {Attempt}/{MaxRetries} falhou no TimelineService. Erro: {Error}",
                        attempt, maxRetries, ex.Message);

                    await Task.Delay(delay);
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 1.5);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("second operation") && attempt < maxRetries)
                {
                    lastException = ex;
                    _logger.LogWarning("Erro de concorrência na tentativa {Attempt}/{MaxRetries} no TimelineService",
                        attempt, maxRetries);

                    await Task.Delay(delay);
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro não recuperável no TimelineService");
                    throw;
                }
            }

            // Se chegou aqui, todas as tentativas falharam
            _logger.LogError(lastException, "Todas as {MaxRetries} tentativas falharam no TimelineService", maxRetries);
            throw lastException ?? new InvalidOperationException("Falha desconhecida após múltiplas tentativas");
        }

        #endregion
    }

    /// <summary>
    /// ViewModel para métricas da timeline
    /// Value Object: encapsula dados de métricas
    /// </summary>
    public class TimelineMetricsViewModel
    {
        public int TotalEventos { get; set; }
        public Dictionary<string, int> EventosPorTipo { get; set; } = new();
        public Dictionary<string, int> AtividadesPorUsuario { get; set; } = new();
        public Dictionary<string, int> AtividadesPorProjeto { get; set; } = new();
    }
}