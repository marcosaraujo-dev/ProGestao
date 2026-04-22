using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Gantt;

namespace ProGestao.Pages.Gantt
{
    public class IndexModel : PageModel
    {
        private readonly IGanttService _ganttService;
        private readonly ProGestaoContext _context;
        private readonly ILogger<IndexModel> _logger;

        // ==============================
        // BIND PROPERTIES (query string)
        // ==============================

        [BindProperty(SupportsGet = true)]
        public string Visao { get; set; } = "semanal";

        [BindProperty(SupportsGet = true)]
        public int? EquipeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ProjetoId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DataInicio { get; set; }

        // ==============================
        // VIEW PROPERTIES
        // ==============================

        public GanttDataViewModel GanttData { get; private set; } = new();
        public List<Equipe> Equipes { get; private set; } = new();
        public List<Projeto> Projetos { get; private set; } = new();
        public DateTime PeriodoInicio { get; private set; }
        public DateTime PeriodoFim { get; private set; }

        // ==============================
        // COMPUTED PROPERTIES
        // ==============================

        public string TituloPeriodo => ObterTituloPeriodo();
        public DateTime PeriodoAnterior => CalcularPeriodoAnterior();
        public DateTime ProximoPeriodo => CalcularProximoPeriodo();
        public DateTime PeriodoHoje => CalcularPeriodoHoje();
        public int IndiceHoje => CalcularIndiceHoje();

        public IndexModel(
            IGanttService ganttService,
            ProGestaoContext context,
            ILogger<IndexModel> logger)
        {
            _ganttService = ganttService ?? throw new ArgumentNullException(nameof(ganttService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation(
                    "Gantt — Visão: {Visao}, EquipeId: {EquipeId}, ProjetoId: {ProjetoId}, DataInicio: {DataInicio}",
                    Visao, EquipeId, ProjetoId, DataInicio);

                ConfigurarPeriodo();
                await CarregarFiltros();
                await CarregarDadosGantt();

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar Diagrama de Gantt");
                TempData["ErrorMessage"] = "Erro ao carregar dados do Gantt. Verifique os logs para mais detalhes.";
                return Page();
            }
        }

        // ==============================
        // CONFIGURAÇÃO DE PERÍODO
        // ==============================

        private void ConfigurarPeriodo()
        {
            var hoje = DateTime.Today;
            var dataBase = DataInicio ?? hoje;

            if (Visao?.ToLower() == "mensal")
            {
                // Mensal: 1º ao último dia do mês
                PeriodoInicio = new DateTime(dataBase.Year, dataBase.Month, 1);
                PeriodoFim = PeriodoInicio.AddMonths(1).AddDays(-1);
            }
            else
            {
                // Semanal: segunda a domingo da semana
                Visao = "semanal";
                var diaSemana = (int)dataBase.DayOfWeek;
                // DayOfWeek: Sunday=0, Monday=1 ... Saturday=6
                // Queremos começar na segunda: se domingo (0), volta 6 dias
                var offsetSegunda = diaSemana == 0 ? -6 : -(diaSemana - 1);
                PeriodoInicio = dataBase.AddDays(offsetSegunda);
                PeriodoFim = PeriodoInicio.AddDays(6);
            }

            _logger.LogInformation("Período configurado — {Inicio} a {Fim} ({Visao})",
                PeriodoInicio, PeriodoFim, Visao);
        }

        // ==============================
        // CARREGAMENTO DE DADOS
        // ==============================

        private async Task CarregarFiltros()
        {
            Equipes = await _context.Equipes
                .AsNoTracking()
                .Where(e => e.Ativo)
                .OrderBy(e => e.Nome)
                .ToListAsync();

            Projetos = await _context.Projetos
                .AsNoTracking()
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }

        private async Task CarregarDadosGantt()
        {
            var filtro = new GanttFilterViewModel
            {
                DataInicio = PeriodoInicio,
                DataFim = PeriodoFim,
                EquipeId = EquipeId,
                ProjetoId = ProjetoId,
                Visao = Visao ?? "semanal"
            };

            GanttData = await _ganttService.GetGanttDataAsync(filtro);

            _logger.LogInformation("Gantt carregado — Usuários: {Usuarios}, Dias: {Dias}",
                GanttData.TotalUsuarios, GanttData.TotalDias);
        }

        // ==============================
        // NAVEGAÇÃO TEMPORAL
        // ==============================

        private DateTime CalcularPeriodoAnterior()
        {
            return Visao?.ToLower() == "mensal"
                ? PeriodoInicio.AddMonths(-1)
                : PeriodoInicio.AddDays(-7);
        }

        private DateTime CalcularProximoPeriodo()
        {
            return Visao?.ToLower() == "mensal"
                ? PeriodoInicio.AddMonths(1)
                : PeriodoInicio.AddDays(7);
        }

        private DateTime CalcularPeriodoHoje()
        {
            var hoje = DateTime.Today;
            if (Visao?.ToLower() == "mensal")
            {
                return new DateTime(hoje.Year, hoje.Month, 1);
            }
            else
            {
                var diaSemana = (int)hoje.DayOfWeek;
                var offsetSegunda = diaSemana == 0 ? -6 : -(diaSemana - 1);
                return hoje.AddDays(offsetSegunda);
            }
        }

        /// <summary>
        /// Índice (0-based) do dia atual dentro do período visível. -1 se hoje não está no período.
        /// </summary>
        private int CalcularIndiceHoje()
        {
            var hoje = DateTime.Today;
            if (hoje >= PeriodoInicio && hoje <= PeriodoFim)
                return (hoje - PeriodoInicio).Days;
            return -1;
        }

        // ==============================
        // TÍTULO DO PERÍODO
        // ==============================

        private string ObterTituloPeriodo()
        {
            if (Visao?.ToLower() == "mensal")
            {
                return PeriodoInicio.ToString("MMMM yyyy", new System.Globalization.CultureInfo("pt-BR"));
            }

            return $"{PeriodoInicio:dd/MM} — {PeriodoFim:dd/MM/yyyy}";
        }
    }
}
