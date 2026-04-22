using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Gantt;
using ProGestao.ViewModels.Grid;
using System.Globalization;

namespace ProGestao.Services.Gantt
{
    /// <summary>
    /// Service que calcula barras horizontais do Gantt a partir de atividades e ausências.
    /// Reutiliza IGridService para consultas de dados (sem duplicar queries).
    /// </summary>
    public class GanttService : IGanttService
    {
        private readonly IGridService _gridService;
        private readonly ILogger<GanttService> _logger;
        private readonly CultureInfo _culture;

        public GanttService(IGridService gridService, ILogger<GanttService> logger)
        {
            _gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _culture = new CultureInfo("pt-BR");
        }

        public async Task<GanttDataViewModel> GetGanttDataAsync(GanttFilterViewModel filter)
        {
            try
            {
                _logger.LogInformation(
                    "GetGanttDataAsync — Período: {DataInicio} a {DataFim}, Equipe: {EquipeId}, Projeto: {ProjetoId}, Visão: {Visao}",
                    filter.DataInicio, filter.DataFim, filter.EquipeId, filter.ProjetoId, filter.Visao);

                // 1. Carrega dados via GridService (reutiliza queries existentes)
                var atividades = (await _gridService.GetAtividadesPorPeriodoAsync(
                    filter.DataInicio, filter.DataFim, filter.EquipeId)).ToList();

                var ausencias = (await _gridService.GetAusenciasPorPeriodoAsync(
                    filter.DataInicio, filter.DataFim, filter.EquipeId)).ToList();

                var usuarios = (await _gridService.GetUsuariosAtivosAsync(filter.EquipeId)).ToList();

                _logger.LogInformation(
                    "Dados carregados — Atividades: {Atividades}, Ausências: {Ausencias}, Usuários: {Usuarios}",
                    atividades.Count, ausencias.Count, usuarios.Count);

                // 2. Filtra por projeto se especificado
                if (filter.ProjetoId.HasValue)
                {
                    atividades = atividades
                        .Where(a => a.ProjetoId == filter.ProjetoId.Value)
                        .ToList();
                }

                // 3. Gera dias do período
                var dias = GerarDiasPeriodo(filter.DataInicio, filter.DataFim);

                // 4. Agrupa atividades e ausências por usuário
                var atividadesPorUsuario = atividades
                    .GroupBy(a => a.UsuarioId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var ausenciasPorUsuario = ausencias
                    .GroupBy(a => a.UsuarioId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // 5. Monta ViewModels de usuários com barras calculadas
                var ganttUsuarios = new List<GanttUsuarioViewModel>();

                foreach (var usuario in usuarios)
                {
                    var barras = new List<GanttBarraViewModel>();

                    // Barras de atividades
                    if (atividadesPorUsuario.TryGetValue(usuario.Id, out var atividadesUsuario))
                    {
                        foreach (var atividade in atividadesUsuario)
                        {
                            var barra = CriarBarraAtividade(atividade, filter.DataInicio, filter.DataFim);
                            if (barra != null)
                                barras.Add(barra);
                        }
                    }

                    // Barras de ausências
                    if (ausenciasPorUsuario.TryGetValue(usuario.Id, out var ausenciasUsuario))
                    {
                        foreach (var ausencia in ausenciasUsuario)
                        {
                            var barra = CriarBarraAusencia(ausencia, filter.DataInicio, filter.DataFim);
                            if (barra != null)
                                barras.Add(barra);
                        }
                    }

                    ganttUsuarios.Add(new GanttUsuarioViewModel
                    {
                        Id = usuario.Id,
                        Nome = usuario.Nome,
                        Iniciais = usuario.Iniciais,
                        Barras = barras
                    });
                }

                return new GanttDataViewModel
                {
                    Usuarios = ganttUsuarios,
                    Dias = dias,
                    DataInicio = filter.DataInicio,
                    DataFim = filter.DataFim
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados do Gantt");
                throw;
            }
        }

        /// <summary>
        /// Cria uma barra de atividade com offset e duração clamped ao período visível
        /// </summary>
        private static GanttBarraViewModel? CriarBarraAtividade(
            ViewModels.Atividade.AtividadeGridViewModel atividade,
            DateTime periodoInicio,
            DateTime periodoFim)
        {
            var dataInicio = atividade.DataInicio.Date;
            var dataFim = (atividade.DataFimReal ?? atividade.DataFimPrevista ?? atividade.DataInicio).Date;

            // Clamp ao período visível
            var inicioVisivel = dataInicio < periodoInicio.Date ? periodoInicio.Date : dataInicio;
            var fimVisivel = dataFim > periodoFim.Date ? periodoFim.Date : dataFim;

            // Se a barra não intersecta o período, ignora
            if (inicioVisivel > fimVisivel)
                return null;

            var offset = (inicioVisivel - periodoInicio.Date).Days;
            var duracao = (fimVisivel - inicioVisivel).Days + 1;

            // Detecção de atraso: DataFimPrevista < hoje e sem DataFimReal
            var estaAtrasada = atividade.DataFimPrevista.HasValue
                && atividade.DataFimPrevista.Value.Date < DateTime.Today
                && atividade.DataFimReal == null;

            var tooltipHtml = GerarTooltipAtividade(atividade);

            return new GanttBarraViewModel
            {
                Id = atividade.Id,
                Tipo = "atividade",
                Nome = atividade.Nome,
                Cor = atividade.StatusCor,
                DataInicio = dataInicio,
                DataFim = dataFim,
                DiaInicioOffset = offset,
                DuracaoDias = duracao,
                EstaAtrasada = estaAtrasada,
                Url = $"/Atividades/Details/{atividade.Id}",
                TooltipHtml = tooltipHtml
            };
        }

        /// <summary>
        /// Cria uma barra de ausência com offset e duração clamped ao período visível
        /// </summary>
        private static GanttBarraViewModel? CriarBarraAusencia(
            ViewModels.Ausencia.AusenciaGridViewModel ausencia,
            DateTime periodoInicio,
            DateTime periodoFim)
        {
            var dataInicio = ausencia.DataInicio.Date;
            var dataFim = ausencia.DataFim.Date;

            // Clamp ao período visível
            var inicioVisivel = dataInicio < periodoInicio.Date ? periodoInicio.Date : dataInicio;
            var fimVisivel = dataFim > periodoFim.Date ? periodoFim.Date : dataFim;

            if (inicioVisivel > fimVisivel)
                return null;

            var offset = (inicioVisivel - periodoInicio.Date).Days;
            var duracao = (fimVisivel - inicioVisivel).Days + 1;

            var tooltipHtml = $"<strong>{ausencia.TipoNome}</strong><br/>" +
                              $"{dataInicio:dd/MM/yyyy} — {dataFim:dd/MM/yyyy}";

            return new GanttBarraViewModel
            {
                Id = ausencia.Id,
                Tipo = "ausencia",
                Nome = ausencia.TipoNome,
                Cor = ausencia.TipoCor,
                DataInicio = dataInicio,
                DataFim = dataFim,
                DiaInicioOffset = offset,
                DuracaoDias = duracao,
                EstaAtrasada = false,
                Url = $"/Ausencias/Edit/{ausencia.Id}",
                TooltipHtml = tooltipHtml
            };
        }

        /// <summary>
        /// Gera tooltip HTML para uma atividade
        /// </summary>
        private static string GerarTooltipAtividade(ViewModels.Atividade.AtividadeGridViewModel atividade)
        {
            var html = $"<strong>{System.Net.WebUtility.HtmlEncode(atividade.Nome)}</strong><br/>";

            if (!string.IsNullOrEmpty(atividade.ProjetoNome))
                html += $"Projeto: {System.Net.WebUtility.HtmlEncode(atividade.ProjetoNome)}<br/>";

            html += $"Status: {System.Net.WebUtility.HtmlEncode(atividade.StatusNome)}<br/>";
            html += $"Prioridade: {System.Net.WebUtility.HtmlEncode(atividade.PrioridadeTexto)}<br/>";
            html += $"Período: {atividade.DataInicio:dd/MM/yyyy}";

            if (atividade.DataFimPrevista.HasValue)
                html += $" — {atividade.DataFimPrevista:dd/MM/yyyy}";

            if (atividade.DataFimReal == null
                && atividade.DataFimPrevista.HasValue
                && atividade.DataFimPrevista.Value.Date < DateTime.Today)
            {
                html += "<br/><span style='color:#dc3545;font-weight:bold;'>⚠ ATRASADA</span>";
            }

            return html;
        }

        /// <summary>
        /// Gera lista de dias do período (reutiliza mesma lógica do GridService)
        /// </summary>
        private List<DiaGridViewModel> GerarDiasPeriodo(DateTime dataInicio, DateTime dataFim)
        {
            var dias = new List<DiaGridViewModel>();
            var dataAtual = dataInicio.Date;

            while (dataAtual <= dataFim.Date)
            {
                dias.Add(new DiaGridViewModel
                {
                    Data = dataAtual,
                    DiaSemana = _culture.DateTimeFormat.GetDayName(dataAtual.DayOfWeek),
                    Dia = dataAtual.Day.ToString("00"),
                    DiaNumero = dataAtual.Day,
                    Mes = _culture.DateTimeFormat.GetMonthName(dataAtual.Month).Substring(0, 3)
                });

                dataAtual = dataAtual.AddDays(1);
            }

            return dias;
        }
    }
}
