using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.ViewModels.Atividade;
using ProGestao.ViewModels.Grid;
using ProGestao.ViewModels.Usuarios;
using System.Globalization;

namespace ProGestao.Services
{
    /// <summary>
    /// Interface para serviço do Grid
    /// </summary>
    public interface IGridService
    {
        Task<GridDataViewModel> GetGridDataAsync(GridFilterViewModel filter);
        Task<IEnumerable<AtividadeGridViewModel>> GetAtividadesPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, int? equipeId = null);
        Task<IEnumerable<UsuarioGridViewModel>> GetUsuariosAtivosAsync(int? equipeId = null);
        Task<GridMetricsViewModel> GetGridMetricsAsync(GridFilterViewModel filter);
    }

    /// <summary>
    /// Implementação aprimorada do GridService
    /// </summary>
    public class GridService : IGridService
    {
        #region Fields and Dependencies

        private readonly ProGestaoContext _context;
        private readonly ILogger<GridService> _logger;
        private readonly CultureInfo _culture;

        #endregion

        #region Constructor

        public GridService(ProGestaoContext context, ILogger<GridService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _culture = new CultureInfo("pt-BR");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Obtém dados completos do grid
        /// Suporte a períodos dinâmicos
        /// </summary>
        public async Task<GridDataViewModel> GetGridDataAsync(GridFilterViewModel filter)
        {
            try
            {
                _logger.LogInformation("Carregando dados do grid para período {DataInicio} - {DataFim}",
                    filter.DataInicio, filter.DataFim);

                ValidateFilter(filter);

                // 1. Carrega usuários
                var usuarios = await GetUsuariosAtivosAsync(filter.EquipeId);

                // 2. Carrega atividades
                var atividades = await GetAtividadesPorPeriodoAsync(
                    filter.DataInicio, filter.DataFim, filter.EquipeId);

                // 3.  Gera dias baseado no período (não fixo em 7 dias)
                var dias = GetDiasPeriodo(filter.DataInicio, filter.DataFim);

                // 4.  Mapeia atividades para usuários com expansão multi-dia
                var usuariosComAtividades = MapearAtividadesParaUsuariosComExpansao(usuarios, atividades, filter.DataInicio, filter.DataFim);

                var result = new GridDataViewModel
                {
                    Usuarios = usuariosComAtividades.ToList(),
                    Dias = dias.ToList()
                };

                _logger.LogInformation("Dados do grid carregados: {QtdUsuarios} usuários, {QtdAtividades} atividades, {QtdDias} dias",
                    result.Usuarios.Count, atividades.Count(), result.Dias.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados do grid");
                throw;
            }
        }

        /// <summary>
        ///  Carrega atividades com informações completas (projeto, etc.)
        /// </summary>
        public async Task<IEnumerable<AtividadeGridViewModel>> GetAtividadesPorPeriodoAsync(
            DateTime dataInicio, DateTime dataFim, int? equipeId = null)
        {
            try
            {
                var query = _context.Atividades
                    .AsNoTracking()
                    .Include(a => a.Usuario)
                    .Include(a => a.Status)
                    .Include(a => a.TipoAtividade)
                    .Include(a => a.Projeto) // ✅ IMPORTANTE: Incluir projeto
                    .Where(a => a.DataInicio <= dataFim &&
                               (a.DataFimReal ?? a.DataFimPrevista) >= dataInicio);

                if (equipeId.HasValue)
                {
                    query = query.Where(a => a.Usuario != null && a.Usuario.EquipeId == equipeId.Value);
                }

                var atividadesList = await query
                    .OrderBy(a => a.DataInicio)
                    .ThenBy(a => a.Prioridade)
                    .ToListAsync();

                // Mapeia com TODAS as informações necessárias
                var atividades = atividadesList.Select(a => new AtividadeGridViewModel
                {
                    Id = a.Id,
                    Nome = a.Nome ?? string.Empty,
                    Descricao = a.Descricao ?? string.Empty,
                    ProjetoNome = a.Projeto?.Nome, 
                    TipoAtividadeNome = a.TipoAtividade?.Nome ?? string.Empty,
                    StatusNome = a.Status?.Nome ?? string.Empty,
                    StatusCor = a.Status?.Cor ?? "#6c757d",
                    DataInicio = a.DataInicio,
                    DataFimPrevista = a.DataFimPrevista,
                    DataFimReal = a.DataFimReal,
                    Prioridade = a.Prioridade,
                    UsuarioId = a.UsuarioId
                }).ToList();

                return atividades;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar atividades do período");
                throw;
            }
        }

        /// <summary>
        /// Obtém usuários ativos
        /// </summary>
        public async Task<IEnumerable<UsuarioGridViewModel>> GetUsuariosAtivosAsync(int? equipeId = null)
        {
            try
            {
                var query = _context.Usuarios
                    .AsNoTracking()
                    .Include(u => u.Equipe)
                    .Where(u => u.Ativo);

                if (equipeId.HasValue)
                {
                    query = query.Where(u => u.EquipeId == equipeId.Value);
                }

                var usuariosList = await query
                    .OrderBy(u => u.Nome)
                    .ToListAsync();

                var usuarios = usuariosList.Select(u => new UsuarioGridViewModel
                {
                    Id = u.Id,
                    Nome = u.Nome ?? string.Empty,
                    Cargo = u.Cargo ?? string.Empty,
                    EquipeNome = u.Equipe?.Nome ?? string.Empty,
                    Iniciais = GetInitials(u.Nome ?? string.Empty),
                    Atividades = new List<AtividadeGridViewModel>(),
                    AtividadesPorDia = new Dictionary<DateTime, List<AtividadeGridViewModel>>()
                }).ToList();

                return usuarios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar usuários ativos");
                throw;
            }
        }

        /// <summary>
        /// Calcula métricas do grid
        /// </summary>
        public async Task<GridMetricsViewModel> GetGridMetricsAsync(GridFilterViewModel filter)
        {
            try
            {
                var atividades = await GetAtividadesPorPeriodoAsync(
                    filter.DataInicio, filter.DataFim, filter.EquipeId);

                var atividadesList = atividades.ToList();

                var metrics = new GridMetricsViewModel
                {
                    TotalAtividades = atividadesList.Count,
                    AtividadesAtrasadas = atividadesList.Count(a => a.EstaAtrasada),
                    AtividadesPorStatus = atividadesList
                        .GroupBy(a => a.StatusNome)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    AtividadesPorPrioridade = atividadesList
                        .GroupBy(a => a.PrioridadeTexto)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular métricas do grid");
                throw;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///  Gera dias do período (não fixo em 7 dias)
        /// </summary>
        private List<DiaGridViewModel> GetDiasPeriodo(DateTime dataInicio, DateTime dataFim)
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
                    Mes = _culture.DateTimeFormat.GetMonthName(dataAtual.Month).Substring(0, 3)
                });

                dataAtual = dataAtual.AddDays(1);
            }

            return dias;
        }

        /// <summary>
        ///  Mapeia atividades com expansão multi-dia
        /// </summary>
        private static IEnumerable<UsuarioGridViewModel> MapearAtividadesParaUsuariosComExpansao(
            IEnumerable<UsuarioGridViewModel> usuarios,
            IEnumerable<AtividadeGridViewModel> atividades,
            DateTime dataInicio,
            DateTime dataFim)
        {
            var atividadesPorUsuario = atividades
                .GroupBy(a => a.UsuarioId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var usuario in usuarios)
            {
                if (atividadesPorUsuario.TryGetValue(usuario.Id, out var atividadesUsuario))
                {
                    // Lista simples para compatibilidade
                    usuario.Atividades = atividadesUsuario;

                    //  Expande atividades para todos os dias do período
                    usuario.AtividadesPorDia = ExpandirAtividadesParaTodosPeriodo(
                        atividadesUsuario, dataInicio, dataFim);
                }
            }

            return usuarios;
        }

        /// <summary>
        /// Expande atividades para todos os dias do período
        /// </summary>
        private static Dictionary<DateTime, List<AtividadeGridViewModel>> ExpandirAtividadesParaTodosPeriodo(
            List<AtividadeGridViewModel> atividades,
            DateTime dataInicio,
            DateTime dataFim)
        {
            var atividadesPorDia = new Dictionary<DateTime, List<AtividadeGridViewModel>>();

            foreach (var atividade in atividades)
            {
                // Determina período efetivo da atividade
                var inicioAtividade = atividade.DataInicio.Date;
                var fimAtividade = (atividade.DataFimReal ?? atividade.DataFimPrevista ?? atividade.DataInicio).Date;

                // Garante que está dentro do período do grid
                var inicioEfetivo = inicioAtividade < dataInicio ? dataInicio : inicioAtividade;
                var fimEfetivo = fimAtividade > dataFim ? dataFim : fimAtividade;

                // Adiciona atividade em TODOS os dias do período
                var dataAtual = inicioEfetivo;
                while (dataAtual <= fimEfetivo)
                {
                    if (!atividadesPorDia.ContainsKey(dataAtual))
                    {
                        atividadesPorDia[dataAtual] = new List<AtividadeGridViewModel>();
                    }

                    // Clona a atividade para cada dia (evita referência compartilhada)
                    var atividadeClone = new AtividadeGridViewModel
                    {
                        Id = atividade.Id,
                        Nome = atividade.Nome,
                        Descricao = atividade.Descricao,
                        ProjetoNome = atividade.ProjetoNome,
                        TipoAtividadeNome = atividade.TipoAtividadeNome,
                        StatusNome = atividade.StatusNome,
                        StatusCor = atividade.StatusCor,
                        DataInicio = atividade.DataInicio,
                        DataFimPrevista = atividade.DataFimPrevista,
                        DataFimReal = atividade.DataFimReal,
                        Prioridade = atividade.Prioridade,
                        UsuarioId = atividade.UsuarioId
                    };

                    atividadesPorDia[dataAtual].Add(atividadeClone);
                    dataAtual = dataAtual.AddDays(1);
                }
            }

            return atividadesPorDia;
        }

        /// <summary>
        /// Valida filtros de entrada
        /// Limite máximo de 30 dias
        /// </summary>
        private static void ValidateFilter(GridFilterViewModel filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            if (filter.DataInicio > filter.DataFim)
                throw new ArgumentException("Data inicial não pode ser maior que data final");

            //  Máximo 30 dias 
            var diffDays = (filter.DataFim - filter.DataInicio).TotalDays;
            if (diffDays > 30)
                throw new ArgumentException("Período não pode ser maior que 30 dias");
        }

        /// <summary>
        /// Gera iniciais do nome
        /// </summary>
        private static string GetInitials(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return "??";

            var words = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 1)
                return words[0].Substring(0, Math.Min(2, words[0].Length)).ToUpper();

            return $"{words[0][0]}{words[^1][0]}".ToUpper();
        }

        #endregion
    }
}