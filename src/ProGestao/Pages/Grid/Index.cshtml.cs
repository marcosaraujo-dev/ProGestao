using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.Services;
using ProGestao.ViewModels;
using ProGestao.ViewModels.Grid;
using ProGestao.ViewModels.Usuarios;

namespace ProGestao.Pages.Grid
{
    public class IndexModel : PageModel
    {
        private readonly IGridService _gridService;
        private readonly ProGestaoContext _context;
        private readonly ILogger<IndexModel> _logger;

        [BindProperty(SupportsGet = true)]
        public DateTime? Semana { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? EquipeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Periodo { get; set; } = "semana";

        public List<UsuarioGridViewModel> UsuariosGrid { get; private set; } = new();
        public List<DiaGridViewModel> DiasSemana { get; private set; } = new();
        public int TotalAtividades { get; private set; }
        public int QtdAtividadesAtrasadas { get; private set; }
        public DateTime PeriodoInicio { get; private set; }
        public DateTime PeriodoFim { get; private set; }
        public List<Equipe> Equipes { get; private set; } = new();
        public List<PeriodoViewModel> PeriodosDisponiveis { get; private set; } = new();

        public bool TemAtividadesAtrasadas => QtdAtividadesAtrasadas > 0;
        public bool EhVisaoMensal => Periodo?.ToLower() == "mensal";
        public string TituloSemana => ObterTituloPeriodo();
        public DateTime SemanaAnterior => CalcularPeriodoAnterior();
        public DateTime ProximaSemana => CalcularProximoPeriodo();

        public IndexModel(IGridService gridService, ProGestaoContext context, ILogger<IndexModel> logger)
        {
            _gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("=== INICIANDO CARREGAMENTO DO GRID ===");
                _logger.LogInformation("Parâmetros recebidos - Período: {Periodo}, Semana: {Semana}, EquipeId: {EquipeId}",
                    Periodo, Semana, EquipeId);

                ConfigurarPeriodo();

                _logger.LogInformation("Período configurado - Início: {PeriodoInicio}, Fim: {PeriodoFim}",
                    PeriodoInicio, PeriodoFim);

                await CarregarDadosEstaticos();

                _logger.LogInformation("Equipes carregadas: {QtdEquipes}", Equipes.Count);

                await CarregarDadosGrid();

                _logger.LogInformation("=== GRID CARREGADO COM SUCESSO ===");
                _logger.LogInformation("Usuários: {QtdUsuarios}, Atividades Totais: {TotalAtividades}, Dias: {QtdDias}",
                    UsuariosGrid.Count, TotalAtividades, DiasSemana.Count);

                // Debug detalhado de cada usuário
                foreach (var usuario in UsuariosGrid)
                {
                    _logger.LogInformation("Usuário: {NomeUsuario} - Atividades: {QtdAtividades} - Dias com atividades: {DiaComAtividades}",
                        usuario.Nome,
                        usuario.TotalAtividades,
                        usuario.AtividadesPorDia.Count(kvp => kvp.Value.Any()));

                    // Log detalhado das atividades por dia
                    foreach (var dia in usuario.AtividadesPorDia.Where(kvp => kvp.Value.Any()))
                    {
                        _logger.LogInformation("  -> {Data}: {QtdAtividades} atividades",
                            dia.Key.ToString("dd/MM/yyyy"), dia.Value.Count);
                    }
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERRO CRÍTICO ao carregar Grid");
                TempData["ErrorMessage"] = "Erro ao carregar dados do grid. Verifique os logs para mais detalhes.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostPeriodoCustomizadoAsync(DateTime dataInicio, DateTime dataFim, int? equipeId)
        {
            try
            {
                _logger.LogInformation("Aplicando período customizado: {DataInicio} a {DataFim}, EquipeId: {EquipeId}",
                    dataInicio, dataFim, equipeId);

                if (dataInicio > dataFim)
                {
                    TempData["ErrorMessage"] = "Data inicial não pode ser maior que data final.";
                    return RedirectToPage(new { periodo = Periodo, equipeId });
                }

                var diffDays = (dataFim - dataInicio).TotalDays;
                if (diffDays > 30)
                {
                    TempData["ErrorMessage"] = "Período não pode ser maior que 30 dias.";
                    return RedirectToPage(new { periodo = Periodo, equipeId });
                }

                return RedirectToPage(new
                {
                    periodo = "customizado",
                    semana = dataInicio,
                    equipeId,
                    dataFim = dataFim.ToString("yyyy-MM-dd")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao aplicar período customizado");
                TempData["ErrorMessage"] = "Erro ao aplicar período customizado.";
                return RedirectToPage();
            }
        }

        private void ConfigurarPeriodo()
        {
            var hoje = DateTime.Today;
            var dataBase = Semana ?? hoje;

            _logger.LogInformation("Configurando período - Tipo: {Periodo}, DataBase: {DataBase}", Periodo, dataBase);

            (PeriodoInicio, PeriodoFim) = Periodo?.ToLower() switch
            {
                "7dias" or "semana" => ObterPeriodoSemana(dataBase),
                "15dias" => (hoje.AddDays(-14), hoje),
                "30dias" => (hoje.AddDays(-29), hoje),
                "mes" => ObterPeriodoMesAtual(hoje),
                "mensal" => ObterPeriodoMensal(dataBase),
                "customizado" => ObterPeriodoCustomizado(),
                _ => ObterPeriodoSemana(dataBase)
            };

            ConfigurarPeriodosDisponiveis();

            _logger.LogInformation("Período final configurado - Início: {Inicio}, Fim: {Fim}, QtdDias: {QtdDias}",
                PeriodoInicio, PeriodoFim, (PeriodoFim - PeriodoInicio).Days + 1);
        }

        private (DateTime inicio, DateTime fim) ObterPeriodoSemana(DateTime dataBase)
        {
            var inicioDaSemana = dataBase.AddDays(-(int)dataBase.DayOfWeek);
            var resultado = (inicioDaSemana, inicioDaSemana.AddDays(6));

            _logger.LogInformation("Período semana calculado - Base: {DataBase}, Início: {Inicio}, Fim: {Fim}",
                dataBase, resultado.Item1, resultado.Item2);

            return resultado;
        }

        private (DateTime inicio, DateTime fim) ObterPeriodoMesAtual(DateTime data)
        {
            var primeiroDiaDoMes = new DateTime(data.Year, data.Month, 1);
            var ultimoDiaDoMes = primeiroDiaDoMes.AddMonths(1).AddDays(-1);
            return (primeiroDiaDoMes, ultimoDiaDoMes);
        }

        private (DateTime inicio, DateTime fim) ObterPeriodoMensal(DateTime dataBase)
        {
            var primeiroDiaDoMes = new DateTime(dataBase.Year, dataBase.Month, 1);
            var ultimoDiaDoMes = primeiroDiaDoMes.AddMonths(1).AddDays(-1);
            return (primeiroDiaDoMes, ultimoDiaDoMes);
        }

        private (DateTime inicio, DateTime fim) ObterPeriodoCustomizado()
        {
            var dataInicio = Semana ?? DateTime.Today;
            
            if (Request.Query.ContainsKey("dataFim") &&
                DateTime.TryParse(Request.Query["dataFim"], out var dataFim))
            {
                _logger.LogInformation("Período customizado - Início: {DataInicio}, Fim: {DataFim}", dataInicio, dataFim);
                return (dataInicio, dataFim);
            }
            
            _logger.LogWarning("Parâmetro dataFim não encontrado para período customizado, usando período de semana");
            return ObterPeriodoSemana(dataInicio);
        }

        private void ConfigurarPeriodosDisponiveis()
        {
            PeriodosDisponiveis = new List<PeriodoViewModel>
            {
                new() { Nome = "Esta Semana", Valor = "semana", Icone = "fas fa-calendar-week", Descricao = "7 dias" },
                new() { Nome = "Últimos 15 dias", Valor = "15dias", Icone = "fas fa-calendar-alt", Descricao = "15 dias" },
                new() { Nome = "Este Mês", Valor = "mes", Icone = "fas fa-calendar", Descricao = "Mês atual" },
                new() { Nome = "Últimos 30 dias", Valor = "30dias", Icone = "fas fa-calendar-plus", Descricao = "30 dias" },
                new() { Nome = "Mensal", Valor = "mensal", Icone = "fas fa-calendar-days", Descricao = "Visão compacta do mês" }
            };
        }

        private async Task CarregarDadosEstaticos()
        {
            _logger.LogInformation("Carregando dados estáticos...");

            Equipes = await _context.Equipes
                .AsNoTracking()
                .Where(e => e.Ativo)
                .OrderBy(e => e.Nome)
                .ToListAsync();

            _logger.LogInformation("Equipes carregadas: {Equipes}",
                string.Join(", ", Equipes.Select(e => $"{e.Id}-{e.Nome}")));
        }

        private async Task CarregarDadosGrid()
        {
            _logger.LogInformation("=== INICIANDO CARREGAMENTO DOS DADOS DO GRID ===");

            var filtro = new GridFilterViewModel
            {
                DataInicio = PeriodoInicio,
                DataFim = PeriodoFim,
                EquipeId = EquipeId
            };

            _logger.LogInformation("Filtro criado - DataInicio: {DataInicio}, DataFim: {DataFim}, EquipeId: {EquipeId}",
                filtro.DataInicio, filtro.DataFim, filtro.EquipeId);

            try
            {
                var gridData = await _gridService.GetGridDataAsync(filtro);

                _logger.LogInformation("GridService retornou - Usuários: {QtdUsuarios}, Dias: {QtdDias}",
                    gridData.Usuarios.Count, gridData.Dias.Count);

                UsuariosGrid = gridData.Usuarios;
                DiasSemana = gridData.Dias;

                // Verifica se os usuários têm atividades mapeadas
                var usuariosComAtividades = UsuariosGrid.Count(u => u.TotalAtividades > 0);
                _logger.LogInformation("Usuários com atividades: {UsuariosComAtividades} de {TotalUsuarios}",
                    usuariosComAtividades, UsuariosGrid.Count);

                var metricas = await _gridService.GetGridMetricsAsync(filtro);
                TotalAtividades = metricas.TotalAtividades;
                QtdAtividadesAtrasadas = metricas.AtividadesAtrasadas;

                _logger.LogInformation("Métricas calculadas - Total: {TotalAtividades}, Atrasadas: {QtdAtrasadas}",
                    TotalAtividades, QtdAtividadesAtrasadas);

                _logger.LogInformation("=== DADOS DO GRID CARREGADOS COM SUCESSO ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao carregar dados do grid através do GridService");
                throw;
            }
        }

        private string ObterTituloPeriodo()
        {
            var qtdDias = (PeriodoFim - PeriodoInicio).Days + 1;

            return Periodo?.ToLower() switch
            {
                "semana" or "7dias" => $"Semana de {PeriodoInicio:dd/MM} a {PeriodoFim:dd/MM/yyyy}",
                "15dias" => $"Últimos 15 dias ({PeriodoInicio:dd/MM} a {PeriodoFim:dd/MM/yyyy})",
                "30dias" => $"Últimos 30 dias ({PeriodoInicio:dd/MM} a {PeriodoFim:dd/MM/yyyy})",
                "mes" => $"Mês de {PeriodoInicio:MMMM/yyyy}",
                "mensal" => $"Visão Mensal — {PeriodoInicio:MMMM/yyyy}",
                "customizado" => $"Período customizado ({PeriodoInicio:dd/MM} a {PeriodoFim:dd/MM/yyyy})",
                _ => $"Período de {qtdDias} dias ({PeriodoInicio:dd/MM} a {PeriodoFim:dd/MM/yyyy})"
            };
        }

        private DateTime CalcularPeriodoAnterior()
        {
            var qtdDias = (PeriodoFim - PeriodoInicio).Days + 1;
            return PeriodoInicio.AddDays(-qtdDias);
        }

        private DateTime CalcularProximoPeriodo()
        {
            var qtdDias = (PeriodoFim - PeriodoInicio).Days + 1;
            return PeriodoInicio.AddDays(qtdDias);
        }
    }
}