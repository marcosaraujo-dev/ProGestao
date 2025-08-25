using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.Services;
using ProGestao.ViewModels.Atividade;

namespace ProGestao.Pages.Timeline
{
    /// <summary>
    /// Page Model para Timeline de Atividades seguindo princípios SOLID e Clean Code
    /// Aplica Single Responsibility: responsável apenas pela coordenação entre View e Services
    /// </summary>
    public class IndexModel : PageModel
    {
        #region Dependencies

        private readonly ITimelineService _timelineService;
        private readonly ProGestaoContext _context;
        private readonly ILogger<IndexModel> _logger;

        #endregion

        #region Constructor - Dependency Injection

        public IndexModel(
            ITimelineService timelineService,
            ProGestaoContext context,
            ILogger<IndexModel> logger)
        {
            _timelineService = timelineService ?? throw new ArgumentNullException(nameof(timelineService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Bind Properties - Filtros de entrada

        [BindProperty(SupportsGet = true)]
        public int PeriodoDias { get; set; } = 7;

        [BindProperty(SupportsGet = true)]
        public List<int> UsuarioIds { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public List<int> ProjetoIds { get; set; } = new();

        #endregion

        #region View Properties - Dados para renderização

        public List<TimelineAtividadeViewModel> AtividadesTimeline { get; set; } = new();
        public List<Usuario> Usuarios { get; set; } = new();
        public List<Projeto> Projetos { get; set; } = new();

        // Para compatibilidade com view existente
        public List<int> UsuariosSelecionados => UsuarioIds;
        public List<int> ProjetosSelecionados => ProjetoIds;

        // Métricas da timeline
        public int TotalEventos => AtividadesTimeline.Count;
        public DateTime DataMaisRecente => AtividadesTimeline.Any()
            ? AtividadesTimeline.Max(a => a.DataReferencia)
            : DateTime.Now;
        public DateTime DataMaisAntiga => AtividadesTimeline.Any()
            ? AtividadesTimeline.Min(a => a.DataReferencia)
            : DateTime.Now;

        #endregion

        #region Page Handlers

        /// <summary>
        /// Handler principal da página
        /// Aplica Open-Closed: extensível para novos filtros sem modificar código existente
        /// Implementa tratamento robusto de erros e retry logic
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Carregando Timeline - Período: {PeriodoDias} dias, Usuários: {UsuarioIds}, Projetos: {ProjetoIds}",
                    PeriodoDias, string.Join(",", UsuarioIds), string.Join(",", ProjetoIds));

                // Retry logic para problemas temporários
                await ExecuteWithRetryAsync(async () =>
                {
                    await LoadFilterDataAsync();
                    await LoadTimelineDataAsync();
                });

                LogTimelineMetrics();

                return Page();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("second operation"))
            {
                _logger.LogError(ex, "Erro de concorrência no DbContext - Timeline");
                TempData["ErrorMessage"] = "Erro temporário no sistema. Tente novamente em alguns segundos.";

                // Dados fallback para não quebrar a UI
                InitializeFallbackData();
                return Page();
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                _logger.LogError(ex, "Erro de conectividade com banco de dados - Timeline");
                TempData["ErrorMessage"] = "Problemas de conectividade. Verifique sua conexão e tente novamente.";

                InitializeFallbackData();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro geral ao carregar Timeline");
                TempData["ErrorMessage"] = "Erro ao carregar timeline. Tente novamente.";

                InitializeFallbackData();
                return Page();
            }
        }

        #endregion

        #region Private Methods - Separation of Concerns

        /// <summary>
        /// Carrega dados estáticos para filtros
        /// Single Responsibility: apenas carregamento de dados de filtro
        /// Execução sequencial para evitar problemas de concorrência no DbContext
        /// </summary>
        private async Task LoadFilterDataAsync()
        {
            try
            {
                // Execução sequencial para evitar concorrência no DbContext
                Usuarios = await LoadUsuariosAsync();
                Projetos = await LoadProjetosAsync();

                _logger.LogDebug("Dados de filtro carregados: {QtdUsuarios} usuários, {QtdProjetos} projetos",
                    Usuarios.Count, Projetos.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados para filtros");

                // Fallback - listas vazias para não quebrar a UI
                Usuarios = new List<Usuario>();
                Projetos = new List<Projeto>();

                throw; // Re-throw para o handler principal tratar
            }
        }

        /// <summary>
        /// Carrega usuários ativos para filtros
        /// Testável: método isolado e focado
        /// </summary>
        private async Task<List<Usuario>> LoadUsuariosAsync()
        {
            return await _context.Usuarios
                .AsNoTracking()
                .Where(u => u.Ativo)
                .OrderBy(u => u.Nome)
                .ToListAsync();
        }

        /// <summary>
        /// Carrega projetos não cancelados para filtros
        /// Testável: método isolado e focado
        /// </summary>
        private async Task<List<Projeto>> LoadProjetosAsync()
        {
            return await _context.Projetos
                .AsNoTracking()
                .Include(p => p.Status)
                .Where(p => p.Status.Nome != "Cancelado")
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }

        /// <summary>
        /// Carrega dados da timeline usando o serviço
        /// Dependency Inversion: delega para abstração (ITimelineService)
        /// </summary>
        private async Task LoadTimelineDataAsync()
        {
            var dataFim = DateTime.Now.Date.AddDays(1).AddMilliseconds(-1);
            var dataInicio = dataFim.AddDays(-PeriodoDias);

            var usuariosFiltro = UsuarioIds.Any() ? UsuarioIds : null;
            var projetosFiltro = ProjetoIds.Any() ? ProjetoIds : null;

            AtividadesTimeline = (await _timelineService.GetTimelineAtividadesAsync(
                dataInicio, dataFim, usuariosFiltro, projetosFiltro)).ToList();
        }

        /// <summary>
        /// Log de métricas para monitoramento
        /// Observabilidade: facilita debugging e monitoramento
        /// </summary>
        private void LogTimelineMetrics()
        {
            _logger.LogInformation("Timeline carregada: {TotalEventos} eventos, " +
                                 "Período: {DataInicio:yyyy-MM-dd} a {DataFim:yyyy-MM-dd}, " +
                                 "Usuários filtrados: {QtdUsuarios}, Projetos filtrados: {QtdProjetos}",
                TotalEventos, DataMaisAntiga, DataMaisRecente,
                UsuarioIds.Count, ProjetoIds.Count);
        }

        /// <summary>
        /// Executa operação com retry logic para problemas temporários de conectividade
        /// Resilience Pattern: implementa retry com backoff exponencial
        /// </summary>
        private async Task ExecuteWithRetryAsync(Func<Task> operation, int maxRetries = 3)
        {
            var delay = TimeSpan.FromMilliseconds(500);

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await operation();
                    return; // Sucesso
                }
                catch (Microsoft.Data.SqlClient.SqlException) when (attempt < maxRetries)
                {
                    _logger.LogWarning("Tentativa {Attempt}/{MaxRetries} falhou. Tentando novamente em {Delay}ms",
                        attempt, maxRetries, delay.TotalMilliseconds);

                    await Task.Delay(delay);
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); // Backoff exponencial
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("second operation") && attempt < maxRetries)
                {
                    _logger.LogWarning("Erro de concorrência na tentativa {Attempt}/{MaxRetries}. Aguardando...",
                        attempt, maxRetries);

                    await Task.Delay(delay);
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 1.5);
                }
            }
        }

        /// <summary>
        /// Inicializa dados fallback quando há erro na carga
        /// Graceful Degradation: aplicação continua funcionando mesmo com erros
        /// </summary>
        private void InitializeFallbackData()
        {
            Usuarios = new List<Usuario>();
            Projetos = new List<Projeto>();
            AtividadesTimeline = new List<TimelineAtividadeViewModel>();

            _logger.LogInformation("Dados fallback inicializados para Timeline");
        }

        #endregion
    }
}