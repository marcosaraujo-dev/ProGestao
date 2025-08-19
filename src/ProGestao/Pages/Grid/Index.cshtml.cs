using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProGestao.Data;
using ProGestao.Services;
using System.ComponentModel.DataAnnotations;

namespace ProGestao.Pages.Grid
{
    /// <summary>
    /// PageModel para o Grid Semanal
    /// Implementa Repository Pattern via EF Context e Cache Strategy
    /// </summary>
    public class IndexModel : PageModel
    {
        #region Fields and Dependencies

        private readonly ProGestaoContext _context;
        private readonly ILogger<IndexModel> _logger;
        private readonly IMemoryCache _cache;
        private readonly IGridService _gridService;
        private readonly IConfiguration _configuration;

        // Cache keys
        private const string CACHE_KEY_EQUIPES = "grid_equipes";
        private const string CACHE_KEY_STATUS = "grid_status_atividades";
        private const string CACHE_KEY_PERIODOS = "grid_periodos_disponiveis";

        #endregion

        #region Constructor

        public IndexModel(
            ProGestaoContext context,
            ILogger<IndexModel> logger,
            IMemoryCache cache,
            IGridService gridService,
            IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        #endregion

        #region Properties - Binding e ViewModels

        [BindProperty(SupportsGet = true)]
        [Display(Name = "Semana")]
        public DateTime? Semana { get; set; }

        [BindProperty(SupportsGet = true)]
        [Display(Name = "Período")]
        public string? Periodo { get; set; }

        [BindProperty(SupportsGet = true)]
        [Display(Name = "Equipe")]
        public int? FiltroEquipeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool AutoRefresh { get; set; }

        // ViewModels
        public IList<UsuarioGridViewModel> UsuariosGrid { get; set; } = new List<UsuarioGridViewModel>();
        public IList<DiaGridViewModel> DiasSemana { get; set; } = new List<DiaGridViewModel>();
        public IList<EquipeViewModel> Equipes { get; set; } = new List<EquipeViewModel>();
        public IList<StatusAtividadeViewModel> StatusAtividades { get; set; } = new List<StatusAtividadeViewModel>();
        public IList<PeriodoViewModel> PeriodosDisponiveis { get; set; } = new List<PeriodoViewModel>();

        // Computed Properties
        public DateTime SemanaAtual => Semana ?? DateTime.Today.StartOfWeek();
        public DateTime SemanaAnterior => SemanaAtual.AddDays(-7);
        public DateTime ProximaSemana => SemanaAtual.AddDays(7);
        public string TituloSemana => $"Semana de {SemanaAtual:dd/MM} a {SemanaAtual.AddDays(6):dd/MM/yyyy}";
        public string PeriodoSelecionado => GetPeriodoNome(Periodo);
        public string PeriodoAtual => Periodo ?? "7dias";
        public int TotalAtividades => UsuariosGrid.SelectMany(u => u.AtividadesPorDia).Count();
        public bool TemAtividadesAtrasadas => QtdAtividadesAtrasadas > 0;
        public int QtdAtividadesAtrasadas => UsuariosGrid
            .SelectMany(u => u.AtividadesPorDia)
            .Count(a => a.EstaAtrasada);

        // Configuration Properties
        public bool IsDebugMode => _configuration.GetValue<bool>("Debug:Enabled", false);
        public int AutoRefreshInterval => _configuration.GetValue<int>("Grid:AutoRefreshInterval", 300000); // 5 min

        #endregion

        #region Page Handlers

        /// <summary>
        /// Handler principal para GET
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Carregando Grid para semana {Semana}, período {Periodo}",
                    SemanaAtual, PeriodoAtual);

                // Validação de entrada
                if (!ValidateInputs())
                {
                    return BadRequest("Parâmetros inválidos");
                }

                // Carrega dados em paralelo
                await LoadDataAsync();

                _logger.LogInformation("Grid carregado com sucesso. {TotalUsuarios} usuários, {TotalAtividades} atividades",
                    UsuariosGrid.Count, TotalAtividades);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar Grid");
                TempData["Error"] = "Erro ao carregar o grid. Tente novamente.";
                return RedirectToPage("/Error");
            }
        }

        /// <summary>
        /// Handler para período customizado
        /// </summary>
        public async Task<IActionResult> OnPostCustomPeriodAsync(
            [Required] DateTime dataInicio,
            [Required] DateTime dataFim)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Datas inválidas";
                    return RedirectToPage();
                }

                if (dataFim <= dataInicio)
                {
                    TempData["Error"] = "Data fim deve ser posterior à data início";
                    return RedirectToPage();
                }

                if ((dataFim - dataInicio).TotalDays > 90)
                {
                    TempData["Error"] = "Período não pode exceder 90 dias";
                    return RedirectToPage();
                }

                // Redireciona com novo período
                var customPeriod = $"custom_{dataInicio:yyyyMMdd}_{dataFim:yyyyMMdd}";
                return RedirectToPage(new
                {
                    semana = dataInicio.StartOfWeek(),
                    periodo = customPeriod,
                    filtroEquipeId = FiltroEquipeId,
                    autoRefresh = AutoRefresh
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao aplicar período customizado");
                TempData["Error"] = "Erro ao aplicar período customizado";
                return RedirectToPage();
            }
        }

        #endregion

        #region Private Methods - Data Loading

        /// <summary>
        /// Carrega todos os dados necessários
        /// </summary>
        private async Task LoadDataAsync()
        {
            // Carrega dados base em paralelo
            var tasks = new[]
            {
                LoadEquipesAsync(),
                LoadStatusAtividadesAsync(),
                LoadPeriodosDisponiveisAsync()
            };

            await Task.WhenAll(tasks);

            // Carrega dados do grid
            await LoadGridDataAsync();
        }

        /// <summary>
        /// Carrega dados do grid de usuários e atividades
        /// </summary>
        private async Task LoadGridDataAsync()
        {
            var (dataInicio, dataFim) = GetPeriodoDatas(PeriodoAtual);

            // Cache key baseado nos filtros
            var cacheKey = $"grid_data_{SemanaAtual:yyyyMMdd}_{PeriodoAtual}_{FiltroEquipeId}";

            if (!_cache.TryGetValue(cacheKey, out GridDataViewModel? gridData))
            {
                gridData = await _gridService.GetGridDataAsync(new GridFilterViewModel
                {
                    DataInicio = dataInicio,
                    DataFim = dataFim,
                    SemanaReferencia = SemanaAtual,
                    EquipeId = FiltroEquipeId
                });

                // Cache por 5 minutos
                _cache.Set(cacheKey, gridData, TimeSpan.FromMinutes(5));
            }

            UsuariosGrid = gridData!.Usuarios;
            DiasSemana = gridData.Dias;
        }

        /// <summary>
        /// Carrega equipes com cache
        /// </summary>
        private async Task LoadEquipesAsync()
        {
            if (!_cache.TryGetValue(CACHE_KEY_EQUIPES, out IList<EquipeViewModel>? equipes))
            {
                equipes = await _context.Equipes
                    .Where(e => e.Usuarios.Any(u => u.Ativo))
                    .OrderBy(e => e.Nome)
                    .Select(e => new EquipeViewModel
                    {
                        Id = e.Id,
                        Nome = e.Nome,
                        QtdUsuarios = e.Usuarios.Count(u => u.Ativo)
                    })
                    .ToListAsync();

                _cache.Set(CACHE_KEY_EQUIPES, equipes, TimeSpan.FromMinutes(30));
            }

            Equipes = equipes!;
        }

        /// <summary>
        /// Carrega status de atividades com cache
        /// </summary>
        private async Task LoadStatusAtividadesAsync()
        {
            if (!_cache.TryGetValue(CACHE_KEY_STATUS, out IList<StatusAtividadeViewModel>? status))
            {
                status = await _context.StatusAtividades
                    .OrderBy(s => s.Ordem)
                    .Select(s => new StatusAtividadeViewModel
                    {
                        Id = s.Id,
                        Nome = s.Nome,
                        Cor = s.Cor
                    })
                    .ToListAsync();

                _cache.Set(CACHE_KEY_STATUS, status, TimeSpan.FromHours(1));
            }

            StatusAtividades = status!;
        }

        /// <summary>
        /// Carrega períodos disponíveis
        /// </summary>
        private async Task LoadPeriodosDisponiveisAsync()
        {
            if (!_cache.TryGetValue(CACHE_KEY_PERIODOS, out IList<PeriodoViewModel>? periodos))
            {
                periodos = new List<PeriodoViewModel>
                {
                    new() { Valor = "7dias", Nome = "Últimos 7 dias",
                           Icone = "fas fa-calendar-day", Descricao = "Uma semana" },
                    new() { Valor = "15dias", Nome = "Últimos 15 dias",
                           Icone = "fas fa-calendar-week", Descricao = "Duas semanas" },
                    new() { Valor = "30dias", Nome = "Último mês",
                           Icone = "fas fa-calendar-alt", Descricao = "Um mês completo" },
                    new() { Valor = "90dias", Nome = "Últimos 3 meses",
                           Icone = "fas fa-calendar", Descricao = "Trimestre" }
                };

                _cache.Set(CACHE_KEY_PERIODOS, periodos, TimeSpan.FromHours(24));
            }

            PeriodosDisponiveis = periodos!;
        }

        #endregion

        #region Private Methods - Utilities

        /// <summary>
        /// Valida parâmetros de entrada
        /// </summary>
        private bool ValidateInputs()
        {
            // Valida semana
            if (Semana.HasValue && (Semana.Value < DateTime.Today.AddYears(-1) ||
                                   Semana.Value > DateTime.Today.AddMonths(6)))
            {
                ModelState.AddModelError(nameof(Semana), "Semana fora do intervalo válido");
                return false;
            }

            // Valida período
            var periodosValidos = new[] { "7dias", "15dias", "30dias", "90dias" };
            if (!string.IsNullOrEmpty(Periodo) &&
                !periodosValidos.Contains(Periodo) &&
                !Periodo.StartsWith("custom_"))
            {
                ModelState.AddModelError(nameof(Periodo), "Período inválido");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Obtém datas de início e fim baseado no período
        /// </summary>
        private (DateTime inicio, DateTime fim) GetPeriodoDatas(string periodo)
        {
            var inicio = SemanaAtual;
            var fim = SemanaAtual.AddDays(6);

            switch (periodo)
            {
                case "7dias":
                    // Já definido acima
                    break;
                case "15dias":
                    inicio = SemanaAtual.AddDays(-7);
                    fim = SemanaAtual.AddDays(6);
                    break;
                case "30dias":
                    inicio = SemanaAtual.AddDays(-21);
                    fim = SemanaAtual.AddDays(6);
                    break;
                case "90dias":
                    inicio = SemanaAtual.AddDays(-84);
                    fim = SemanaAtual.AddDays(6);
                    break;
                default:
                    if (periodo.StartsWith("custom_"))
                    {
                        var parts = periodo.Split('_');
                        if (parts.Length == 3 &&
                            DateTime.TryParseExact(parts[1], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var customInicio) &&
                            DateTime.TryParseExact(parts[2], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var customFim))
                        {
                            inicio = customInicio;
                            fim = customFim;
                        }
                    }
                    break;
            }

            return (inicio, fim);
        }

        /// <summary>
        /// Obtém nome do período
        /// </summary>
        private string GetPeriodoNome(string? periodo)
        {
            if (string.IsNullOrEmpty(periodo))
                return "Últimos 7 dias";

            var periodoObj = PeriodosDisponiveis.FirstOrDefault(p => p.Valor == periodo);
            if (periodoObj != null)
                return periodoObj.Nome;

            if (periodo.StartsWith("custom_"))
            {
                var (inicio, fim) = GetPeriodoDatas(periodo);
                return $"{inicio:dd/MM} - {fim:dd/MM/yyyy}";
            }

            return "Período customizado";
        }

        #endregion
    }

    #region Extension Methods

    /// <summary>
    /// Extensões para DateTime
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Obtém o início da semana (segunda-feira)
        /// </summary>
        public static DateTime StartOfWeek(this DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Obtém o fim da semana (domingo)
        /// </summary>
        public static DateTime EndOfWeek(this DateTime date)
        {
            return date.StartOfWeek().AddDays(6);
        }
    }

    #endregion
}

#region ViewModels

/// <summary>
/// ViewModel para dados do grid
/// </summary>
public class GridDataViewModel
{
    public IList<UsuarioGridViewModel> Usuarios { get; set; } = new List<UsuarioGridViewModel>();
    public IList<DiaGridViewModel> Dias { get; set; } = new List<DiaGridViewModel>();
}

/// <summary>
/// ViewModel para filtros do grid
/// </summary>
public class GridFilterViewModel
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public DateTime SemanaReferencia { get; set; }
    public int? EquipeId { get; set; }
}

/// <summary>
/// ViewModel para usuários no grid
/// </summary>
public class UsuarioGridViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string Iniciais { get; set; } = string.Empty;
    public IList<AtividadeGridViewModel> AtividadesPorDia { get; set; } = new List<AtividadeGridViewModel>();
}

/// <summary>
/// ViewModel para atividades no grid
/// </summary>
public class AtividadeGridViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public int Prioridade { get; set; }
    public bool EstaAtrasada { get; set; }
    public StatusAtividadeViewModel Status { get; set; } = new();
}

/// <summary>
/// ViewModel para dias da semana
/// </summary>
public class DiaGridViewModel
{
    public DateTime Data { get; set; }
    public string DiaSemana { get; set; } = string.Empty;
    public int Dia { get; set; }
    public string Mes { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel para períodos disponíveis
/// </summary>
public class PeriodoViewModel
{
    public string Valor { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Icone { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}

/// <summary>
/// ViewModel para equipes
/// </summary>
public class EquipeViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int QtdUsuarios { get; set; }
}

/// <summary>
/// ViewModel para status de atividades
/// </summary>
public class StatusAtividadeViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
}

#endregion