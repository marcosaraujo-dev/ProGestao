using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Pages.Grid;
using System.Globalization;

namespace ProGestao.Services
{
    /// <summary>
    /// Interface para serviço do Grid
    /// Define contrato seguindo Interface Segregation Principle
    /// </summary>
    public interface IGridService
    {
        /// <summary>
        /// Obtém dados do grid baseado nos filtros
        /// </summary>
        Task<GridDataViewModel> GetGridDataAsync(GridFilterViewModel filter);

        /// <summary>
        /// Obtém atividades para um período específico
        /// </summary>
        Task<IEnumerable<AtividadeGridViewModel>> GetAtividadesPorPeriodoAsync(
            DateTime dataInicio, DateTime dataFim, int? equipeId = null);

        /// <summary>
        /// Obtém usuários ativos da equipe
        /// </summary>
        Task<IEnumerable<UsuarioGridViewModel>> GetUsuariosAtivosAsync(int? equipeId = null);

        /// <summary>
        /// Calcula métricas do grid
        /// </summary>
        Task<GridMetricsViewModel> GetGridMetricsAsync(GridFilterViewModel filter);
    }
}

namespace ProGestao.Services
{
    /// <summary>
    /// Implementação do serviço do Grid
    /// Implementa Repository Pattern e SOLID principles
    /// </summary>
    public class GridService : IGridService
    {
        #region Fields and Dependencies

        private readonly ProGestaoContext _context;
        private readonly ILogger<GridService> _logger;
        private readonly CultureInfo _culture;

        #endregion

        #region Constructor

        public GridService(
            ProGestaoContext context,
            ILogger<GridService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _culture = new CultureInfo("pt-BR");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Obtém dados completos do grid
        /// </summary>
        public async Task<GridDataViewModel> GetGridDataAsync(GridFilterViewModel filter)
        {
            try
            {
                _logger.LogInformation("Carregando dados do grid para período {DataInicio} - {DataFim}",
                    filter.DataInicio, filter.DataFim);

                // Valida filtros
                ValidateFilter(filter);

                // Carrega dados em paralelo
                var usuariosTask = GetUsuariosAtivosAsync(filter.EquipeId);
                var atividadesTask = GetAtividadesPorPeriodoAsync(
                    filter.DataInicio, filter.DataFim, filter.EquipeId);
                var diasTask = Task.FromResult(GetDiasSemana(filter.SemanaReferencia));

                await Task.WhenAll(usuariosTask, atividadesTask, diasTask);

                var usuarios = await usuariosTask;
                var atividades = await atividadesTask;
                var dias = await diasTask;

                // Mapeia atividades para usuários
                var usuariosComAtividades = MapearAtividadesParaUsuarios(usuarios, atividades);

                var result = new GridDataViewModel
                {
                    Usuarios = usuariosComAtividades.ToList(),
                    Dias = dias.ToList()
                };

                _logger.LogInformation("Dados do grid carregados: {QtdUsuarios} usuários, {QtdAtividades} atividades",
                    result.Usuarios.Count, atividades.Count());

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados do grid");
                throw;
            }
        }

        /// <summary>
        /// Obtém atividades para período
        /// </summary>
        public async Task<IEnumerable<AtividadeGridViewModel>> GetAtividadesPorPeriodoAsync(
            DateTime dataInicio, DateTime dataFim, int? equipeId = null)
        {
            try
            {
                var query = _context.Atividades
                    .Include(a => a.Usuario)
                    .Include(a => a.Status)
                    .Include(a => a.TipoAtividade)
                    .Include(a => a.Projeto)
                    .Where(a => a.DataInicio <= dataFim &&
                               (a.DataFimReal ?? a.DataFimPrevista) >= dataInicio);

                // Filtro por equipe
                if (equipeId.HasValue)
                {
                    query = query.Where(a => a.Usuario.EquipeId == equipeId.Value);
                }

                var atividades = await query
                    .OrderBy(a => a.DataInicio)
                    .ThenBy(a => a.Prioridade)
                    .Select(a => new AtividadeGridViewModel
                    {
                        Id = a.Id,
                        Nome = a.Nome,
                        Descricao = a.Descricao,
                        Data = a.DataInicio,
                        Prioridade = a.Prioridade,
                        EstaAtrasada = a.DataFimReal == null &&
                                      a.DataFimPrevista < DateTime.Today &&
                                      a.StatusId != GetStatusConcluida(),
                        Status = new StatusAtividadeViewModel
                        {
                            Id = a.Status.Id,
                            Nome = a.Status.Nome,
                            Cor = a.Status.Cor
                        }
                    })
                    .ToListAsync();

                // Expande atividades por dia (para atividades multi-dia)
                var atividadesExpandidas = ExpandirAtividadesPorDia(atividades, dataInicio, dataFim);

                return atividadesExpandidas;
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
                    .Include(u => u.Equipe)
                    .Where(u => u.Ativo);

                if (equipeId.HasValue)
                {
                    query = query.Where(u => u.EquipeId == equipeId.Value);
                }

                var usuarios = await query
                    .OrderBy(u => u.Nome)
                    .Select(u => new UsuarioGridViewModel
                    {
                        Id = u.Id,
                        Nome = u.Nome,
                        Cargo = u.Cargo,
                        Iniciais = GetInitials(u.Nome)
                    })
                    .ToListAsync();

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

                var metrics = new GridMetricsViewModel
                {
                    TotalAtividades = atividades.Count(),
                    AtividadesAtrasadas = atividades.Count(a => a.EstaAtrasada),
                    AtividadesPorStatus = atividades
                        .GroupBy(a => a.Status.Nome)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    AtividadesPorPrioridade = atividades
                        .GroupBy(a => GetPrioridadeName(a.Prioridade))
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
        /// Valida filtros de entrada
        /// </summary>
        private static void ValidateFilter(GridFilterViewModel filter)
        {
            if (filter.DataInicio > filter.DataFim)
                throw new ArgumentException("Data início deve ser anterior à data fim");

            if ((filter.DataFim - filter.DataInicio).TotalDays > 365)
                throw new ArgumentException("Período não pode exceder 365 dias");
        }

        /// <summary>
        /// Obtém dias da semana
        /// </summary>
        private IEnumerable<DiaGridViewModel> GetDiasSemana(DateTime semanaReferencia)
        {
            var inicioSemana = semanaReferencia.StartOfWeek();

            for (int i = 0; i < 7; i++)
            {
                var data = inicioSemana.AddDays(i);
                yield return new DiaGridViewModel
                {
                    Data = data,
                    DiaSemana = _culture.DateTimeFormat.GetAbbreviatedDayName(data.DayOfWeek).ToUpper(),
                    Dia = data.Day,
                    Mes = _culture.DateTimeFormat.GetAbbreviatedMonthName(data.Month)
                };
            }
        }

        /// <summary>
        /// Mapeia atividades para usuários
        /// </summary>
        private static IEnumerable<UsuarioGridViewModel> MapearAtividadesParaUsuarios(
            IEnumerable<UsuarioGridViewModel> usuarios,
            IEnumerable<AtividadeGridViewModel> atividades)
        {
            var atividadesPorUsuario = atividades
                .ToLookup(a => GetUsuarioIdFromAtividade(a));

            foreach (var usuario in usuarios)
            {
                usuario.AtividadesPorDia = atividadesPorUsuario[usuario.Id].ToList();
                yield return usuario;
            }
        }

        /// <summary>
        /// Expande atividades multi-dia para cada dia individual
        /// </summary>
        private static IEnumerable<AtividadeGridViewModel> ExpandirAtividadesPorDia(
            IEnumerable<AtividadeGridViewModel> atividades,
            DateTime dataInicio,
            DateTime dataFim)
        {
            foreach (var atividade in atividades)
            {
                // Para atividades de um dia apenas
                if (atividade.Data.Date >= dataInicio.Date && atividade.Data.Date <= dataFim.Date)
                {
                    yield return atividade;
                }

                // Para atividades multi-dia (expandir para cada dia)
                // TODO: Implementar lógica para atividades que span múltiplos dias
                // Esta seria uma feature adicional baseada em DataInicio e DataFimPrevista
            }
        }

        /// <summary>
        /// Obtém iniciais do nome
        /// </summary>
        private static string GetInitials(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return "??";

            var words = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 1)
                return words[0].Substring(0, Math.Min(2, words[0].Length)).ToUpper();

            return $"{words[0][0]}{words[words.Length - 1][0]}".ToUpper();
        }

        /// <summary>
        /// Obtém nome da prioridade
        /// </summary>
        private static string GetPrioridadeName(int prioridade)
        {
            return prioridade switch
            {
                1 => "Baixa",
                2 => "Normal",
                3 => "Alta",
                4 => "Crítica",
                _ => "Não definida"
            };
        }

        /// <summary>
        /// Obtém ID do status "Concluída"
        /// TODO: Implementar cache ou configuração para evitar hardcoding
        /// </summary>
        private int GetStatusConcluida()
        {
            // Esta seria uma implementação mais robusta em um cenário real
            // Por exemplo, usando um enum ou configuração
            return 3; // Assumindo que 3 é o ID do status "Concluída"
        }

        /// <summary>
        /// Obtém ID do usuário de uma atividade
        /// TODO: Adicionar propriedade UsuarioId em AtividadeGridViewModel
        /// </summary>
        private static int GetUsuarioIdFromAtividade(AtividadeGridViewModel atividade)
        {
            // Esta seria uma implementação mais robusta
            // Por enquanto, retorna 0 - precisa ser ajustado com a propriedade correta
            return 0;
        }

        #endregion
    }

    #region Additional ViewModels

    /// <summary>
    /// ViewModel para métricas do grid
    /// </summary>
    public class GridMetricsViewModel
    {
        public int TotalAtividades { get; set; }
        public int AtividadesAtrasadas { get; set; }
        public Dictionary<string, int> AtividadesPorStatus { get; set; } = new();
        public Dictionary<string, int> AtividadesPorPrioridade { get; set; } = new();
        public double TaxaConclusao => TotalAtividades > 0
            ? (TotalAtividades - AtividadesAtrasadas) / (double)TotalAtividades * 100
            : 0;
    }

    #endregion

   
}
