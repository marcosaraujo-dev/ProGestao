using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProGestao.Services;
using ProGestao.ViewModels;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.ViewModels.Grid;
using ProGestao.ViewModels.Usuarios;

namespace ProGestao.Pages.Grid
{
    /// <summary>
    /// Page Model aprimorado para Grid com períodos dinâmicos
    /// </summary>
    public class IndexModel : PageModel
    {
        #region Fields and Dependencies

        private readonly IGridService _gridService;
        private readonly ProGestaoContext _context;
        private readonly ILogger<IndexModel> _logger;

        #endregion

        #region Properties Principais

        // Dados do Grid
        public List<UsuarioGridViewModel> UsuariosGrid { get; set; } = new();
        public List<DiaGridViewModel> DiasSemana { get; set; } = new(); // Nome mantido para compatibilidade

        // Métricas
        public int TotalAtividades { get; set; }
        public int QtdAtividadesAtrasadas { get; set; }
        public bool TemAtividadesAtrasadas => QtdAtividadesAtrasadas > 0;

        // Navigation baseada no período atual
        public DateTime PeriodoInicio { get; set; }
        public DateTime PeriodoFim { get; set; }
        public int QtdDiasPeriodo { get; set; }

        // Navegação (baseada no período)
        public DateTime PeriodoAnterior => PeriodoInicio.AddDays(-QtdDiasPeriodo);
        public DateTime PeriodoProximo => PeriodoInicio.AddDays(QtdDiasPeriodo);

        // Aliases para compatibilidade com view
        public DateTime SemanaAtual => PeriodoInicio;
        public DateTime SemanaAnterior => PeriodoAnterior;
        public DateTime ProximaSemana => PeriodoProximo;

        public string TituloSemana => GetTituloPeriodo();

        #endregion

        #region Properties da View

        // Filtros
        public List<Equipe> Equipes { get; set; } = new();
        public List<StatusAtividade> StatusAtividades { get; set; } = new();

        // Períodos dinâmicos
        public List<PeriodoViewModel> PeriodosDisponiveis { get; set; } = new();
        public string PeriodoSelecionado { get; set; } = "Esta Semana";
        public string PeriodoAtual { get; set; } = "semana";

        // Auto-refresh
        public bool AutoRefresh { get; set; } = false;
        public int AutoRefreshInterval { get; set; } = 300000;

        // Debug
        public bool IsDebugMode => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        #endregion

        #region Bind Properties

        [BindProperty(SupportsGet = true)]
        public DateTime? Semana { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? EquipeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FiltroEquipeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Periodo { get; set; } // ✅ NOVO: Parâmetro de período

        #endregion

        #region Constructor

        public IndexModel(
            IGridService gridService,
            ProGestaoContext context,
            ILogger<IndexModel> logger)
        {
            _gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Page Methods

        /// <summary>
        /// Handler principal 
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Carregando Grid com período: {Periodo}", Periodo ?? "semana");

                //  Calcula período baseado no parâmetro
                CalcularPeriodo();

                var equipeIdFiltro = EquipeId ?? FiltroEquipeId;

                await LoadStaticDataAsync();
                await LoadGridDataAsync(equipeIdFiltro);

                _logger.LogInformation("Grid carregado: {QtdUsuarios} usuários, {QtdAtividades} atividades, {QtdDias} dias",
                    UsuariosGrid.Count, TotalAtividades, DiasSemana.Count);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar Grid");
                TempData["ErrorMessage"] = "Erro ao carregar dados do grid. Tente novamente.";
                return Page();
            }
        }

        /// <summary>
        /// Handler para período customizado
        /// </summary>
        public async Task<IActionResult> OnPostCustomPeriodAsync(DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                if (dataInicio > dataFim)
                {
                    TempData["ErrorMessage"] = "Data inicial não pode ser maior que data final.";
                    return RedirectToPage();
                }

                var diffDays = (dataFim - dataInicio).TotalDays;
                if (diffDays > 30)
                {
                    TempData["ErrorMessage"] = "Período não pode ser maior que 30 dias.";
                    return RedirectToPage();
                }

                return RedirectToPage(new
                {
                    semana = dataInicio,
                    periodo = "custom",
                    fim = dataFim.ToString("yyyy-MM-dd")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar período customizado");
                TempData["ErrorMessage"] = "Erro ao processar período customizado.";
                return RedirectToPage();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///  Calcula período baseado nos parâmetros
        /// </summary>
        private void CalcularPeriodo()
        {
            var hoje = DateTime.Today;
            var periodoTipo = Periodo ?? "semana";

            switch (periodoTipo.ToLower())
            {
                case "semana":
                    PeriodoInicio = Semana ?? GetStartOfWeek(hoje);
                    QtdDiasPeriodo = 7;
                    PeriodoFim = PeriodoInicio.AddDays(6);
                    PeriodoAtual = "semana";
                    PeriodoSelecionado = "Esta Semana";
                    break;

                case "15dias":
                    PeriodoInicio = Semana ?? hoje.AddDays(-14);
                    QtdDiasPeriodo = 15;
                    PeriodoFim = PeriodoInicio.AddDays(14);
                    PeriodoAtual = "15dias";
                    PeriodoSelecionado = "Últimos 15 dias";
                    break;

                case "mes":
                    var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
                    PeriodoInicio = Semana ?? inicioMes;
                    var fimMes = inicioMes.AddMonths(1).AddDays(-1);
                    QtdDiasPeriodo = (fimMes - inicioMes).Days + 1;
                    PeriodoFim = fimMes;
                    PeriodoAtual = "mes";
                    PeriodoSelecionado = "Este Mês";
                    break;

                case "30dias":
                    PeriodoInicio = Semana ?? hoje.AddDays(-29);
                    QtdDiasPeriodo = 30;
                    PeriodoFim = PeriodoInicio.AddDays(29);
                    PeriodoAtual = "30dias";
                    PeriodoSelecionado = "Últimos 30 dias";
                    break;

                case "custom":
                    PeriodoInicio = Semana ?? hoje;
                    // Para custom, tentar pegar parâmetro 'fim' da query string
                    if (Request.Query.TryGetValue("fim", out var fimStr) &&
                        DateTime.TryParse(fimStr, out var fimCustom))
                    {
                        PeriodoFim = fimCustom;
                        QtdDiasPeriodo = (PeriodoFim - PeriodoInicio).Days + 1;
                    }
                    else
                    {
                        QtdDiasPeriodo = 7;
                        PeriodoFim = PeriodoInicio.AddDays(6);
                    }
                    PeriodoAtual = "custom";
                    PeriodoSelecionado = "Período Customizado";
                    break;

                default:
                    goto case "semana";
            }

            // Validação de limite máximo
            if (QtdDiasPeriodo > 30)
            {
                QtdDiasPeriodo = 30;
                PeriodoFim = PeriodoInicio.AddDays(29);
            }
        }

        /// <summary>
        /// Título dinâmico baseado no período
        /// </summary>
        private string GetTituloPeriodo()
        {
            if (QtdDiasPeriodo <= 7)
            {
                return $"Semana de {PeriodoInicio:dd/MM} a {PeriodoFim:dd/MM/yyyy}";
            }
            else
            {
                return $"Período de {PeriodoInicio:dd/MM} a {PeriodoFim:dd/MM/yyyy} ({QtdDiasPeriodo} dias)";
            }
        }

        /// <summary>
        /// Carrega dados estáticos
        /// </summary>
        private async Task LoadStaticDataAsync()
        {
            try
            {
                Equipes = await _context.Equipes
                    .AsNoTracking()
                    .Where(e => e.Ativo)
                    .OrderBy(e => e.Nome)
                    .ToListAsync();

                StatusAtividades = await _context.StatusAtividades
                    .AsNoTracking()
                    .OrderBy(s => s.Ordem)
                    .ToListAsync();

                ConfigurarPeriodos();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados estáticos");
                throw;
            }
        }

        /// <summary>
        /// Carrega dados do grid com período dinâmico
        /// </summary>
        private async Task LoadGridDataAsync(int? equipeIdFiltro)
        {
            try
            {
                var filter = new GridFilterViewModel
                {
                    DataInicio = PeriodoInicio,
                    DataFim = PeriodoFim,
                    SemanaReferencia = PeriodoInicio, // Para compatibilidade
                    EquipeId = equipeIdFiltro
                };

                var gridData = await _gridService.GetGridDataAsync(filter);
                UsuariosGrid = gridData.Usuarios;
                DiasSemana = gridData.Dias; // Nome mantido para compatibilidade

                var metrics = await _gridService.GetGridMetricsAsync(filter);
                TotalAtividades = metrics.TotalAtividades;
                QtdAtividadesAtrasadas = metrics.AtividadesAtrasadas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados do grid");
                throw;
            }
        }

        /// <summary>
        /// Configura períodos disponíveis
        /// </summary>
        private void ConfigurarPeriodos()
        {
            PeriodosDisponiveis = new List<PeriodoViewModel>
            {
                new() { Nome = "Esta Semana", Valor = "semana", Icone = "fas fa-calendar-week", Descricao = "7 dias" },
                new() { Nome = "Últimos 15 dias", Valor = "15dias", Icone = "fas fa-calendar-alt", Descricao = "15 dias" },
                new() { Nome = "Este Mês", Valor = "mes", Icone = "fas fa-calendar", Descricao = "Mês atual" },
                new() { Nome = "Últimos 30 dias", Valor = "30dias", Icone = "fas fa-calendar-plus", Descricao = "30 dias" }
            };
        }

        /// <summary>
        /// Obtém início da semana
        /// </summary>
        private static DateTime GetStartOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Sunday)) % 7;
            return date.AddDays(-diff).Date;
        }

        #endregion
    }
}