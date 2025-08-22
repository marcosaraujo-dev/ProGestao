using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.ViewModels.Atividade;
using ProGestao.ViewModels.Grid;
using ProGestao.ViewModels.Usuarios;
using System.Globalization;

namespace ProGestao.Services
{
    public interface IGridService
    {
        Task<GridDataViewModel> GetGridDataAsync(GridFilterViewModel filter);
        Task<IEnumerable<AtividadeGridViewModel>> GetAtividadesPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, int? equipeId = null);
        Task<IEnumerable<UsuarioGridViewModel>> GetUsuariosAtivosAsync(int? equipeId = null);
        Task<GridMetricsViewModel> GetGridMetricsAsync(GridFilterViewModel filter);
    }

    public class GridService : IGridService
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<GridService> _logger;
        private readonly CultureInfo _culture;

        public GridService(ProGestaoContext context, ILogger<GridService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _culture = new CultureInfo("pt-BR");
        }

        public async Task<GridDataViewModel> GetGridDataAsync(GridFilterViewModel filter)
        {
            try
            {
                _logger.LogInformation("=== INICIANDO GetGridDataAsync ===");
                _logger.LogInformation("Parâmetros - DataInicio: {DataInicio}, DataFim: {DataFim}, EquipeId: {EquipeId}",
                    filter.DataInicio, filter.DataFim, filter.EquipeId);

                ValidarFiltro(filter);

                // 1. Carrega usuários
                _logger.LogInformation("Carregando usuários...");
                var usuarios = await GetUsuariosAtivosAsync(filter.EquipeId);
                var usuariosList = usuarios.ToList();
                _logger.LogInformation("Usuários carregados: {QtdUsuarios}", usuariosList.Count);

                // 2. Carrega atividades
                _logger.LogInformation("Carregando atividades...");
                var atividades = await GetAtividadesPorPeriodoAsync(filter.DataInicio, filter.DataFim, filter.EquipeId);
                var atividadesList = atividades.ToList();
                _logger.LogInformation("Atividades carregadas: {QtdAtividades}", atividadesList.Count);

                // Debug das atividades carregadas
                foreach (var atividade in atividadesList.Take(5)) // Só as primeiras 5 para não encher o log
                {
                    _logger.LogInformation("Atividade: {Nome}, Usuário: {UsuarioId}, Data: {DataInicio} a {DataFim}",
                        atividade.Nome, atividade.UsuarioId, atividade.DataInicio,
                        atividade.DataFimReal ?? atividade.DataFimPrevista ?? atividade.DataInicio);
                }

                // 3. Gera dias baseado no período
                _logger.LogInformation("Gerando dias do período...");
                var dias = GerarDiasPeriodo(filter.DataInicio, filter.DataFim);
                _logger.LogInformation("Dias gerados: {QtdDias}", dias.Count);

                // 4. Mapeia atividades para usuários
                _logger.LogInformation("Mapeando atividades para usuários...");
                var usuariosComAtividades = MapearAtividadesParaUsuarios(usuariosList, atividadesList, filter.DataInicio, filter.DataFim);
                var usuariosFinais = usuariosComAtividades.ToList();

                // Debug do mapeamento
                foreach (var usuario in usuariosFinais)
                {
                    var diasComAtividades = usuario.AtividadesPorDia.Count(kvp => kvp.Value.Any());
                    _logger.LogInformation("Usuário {Nome}: {TotalAtividades} atividades em {DiaComAtividades} dias",
                        usuario.Nome, usuario.TotalAtividades, diasComAtividades);
                }

                var resultado = new GridDataViewModel
                {
                    Usuarios = usuariosFinais,
                    Dias = dias
                };

                _logger.LogInformation("=== GetGridDataAsync CONCLUÍDO COM SUCESSO ===");
                _logger.LogInformation("Resultado final - Usuários: {QtdUsuarios}, Dias: {QtdDias}, Atividades mapeadas: {AtividadesMapeadas}",
                    resultado.Usuarios.Count, resultado.Dias.Count,
                    resultado.Usuarios.Sum(u => u.TotalAtividades));

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERRO CRÍTICO em GetGridDataAsync");
                throw;
            }
        }

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
                    .Include(a => a.Projeto)
                    .Where(a => a.DataInicio <= dataFim &&
                               (a.DataFimReal ?? a.DataFimPrevista ?? a.DataInicio) >= dataInicio);

                if (equipeId.HasValue)
                {
                    query = query.Where(a => a.Usuario != null && a.Usuario.EquipeId == equipeId.Value);
                }

                var atividadesList = await query
                    .OrderBy(a => a.DataInicio)
                    .ThenBy(a => a.Prioridade)
                    .ToListAsync();

                return atividadesList.Select(a => new AtividadeGridViewModel
                {
                    Id = a.Id,
                    Nome = a.Nome ?? string.Empty,
                    Descricao = a.Descricao ?? string.Empty,
                    ProjetoNome = a.Projeto?.Nome ?? string.Empty,
                    TipoAtividadeNome = a.TipoAtividade?.Nome ?? string.Empty,
                    StatusNome = a.Status?.Nome ?? string.Empty,
                    StatusCor = a.Status?.Cor ?? "#6c757d",
                    DataInicio = a.DataInicio,
                    DataFimPrevista = a.DataFimPrevista,
                    DataFimReal = a.DataFimReal,
                    Prioridade = a.Prioridade,
                    UsuarioId = a.UsuarioId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar atividades do período");
                throw;
            }
        }

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

                return usuariosList.Select(u => new UsuarioGridViewModel
                {
                    Id = u.Id,
                    Nome = u.Nome ?? string.Empty,
                    Cargo = u.Cargo ?? string.Empty,
                    EquipeNome = u.Equipe?.Nome ?? string.Empty,
                    Iniciais = GerarIniciais(u.Nome ?? string.Empty),
                    Atividades = new List<AtividadeGridViewModel>(),
                    AtividadesPorDia = new Dictionary<DateTime, List<AtividadeGridViewModel>>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar usuários ativos");
                throw;
            }
        }

        public async Task<GridMetricsViewModel> GetGridMetricsAsync(GridFilterViewModel filter)
        {
            try
            {
                var atividades = await GetAtividadesPorPeriodoAsync(filter.DataInicio, filter.DataFim, filter.EquipeId);
                var atividadesList = atividades.ToList();

                return new GridMetricsViewModel
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular métricas do grid");
                throw;
            }
        }

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
                    Mes = _culture.DateTimeFormat.GetMonthName(dataAtual.Month).Substring(0, 3)
                });

                dataAtual = dataAtual.AddDays(1);
            }

            return dias;
        }

        private IEnumerable<UsuarioGridViewModel> MapearAtividadesParaUsuarios(
            IEnumerable<UsuarioGridViewModel> usuarios,
            IEnumerable<AtividadeGridViewModel> atividades,
            DateTime dataInicio,
            DateTime dataFim)
        {
            _logger.LogInformation("=== INICIANDO MAPEAMENTO DE ATIVIDADES ===");

            var atividadesPorUsuario = atividades
                .GroupBy(a => a.UsuarioId)
                .ToDictionary(g => g.Key, g => g.ToList());

            _logger.LogInformation("Atividades agrupadas por usuário: {QtdUsuarios} usuários com atividades",
                atividadesPorUsuario.Count);

            foreach (var usuario in usuarios)
            {
                _logger.LogInformation("Processando usuário: {NomeUsuario} (ID: {UsuarioId})",
                    usuario.Nome, usuario.Id);

                // SEMPRE inicializa o dicionário vazio primeiro
                usuario.AtividadesPorDia = InicializarDicionarioVazio(dataInicio, dataFim);
                _logger.LogInformation("Dicionário inicializado com {QtdDias} dias para usuário {Nome}",
                    usuario.AtividadesPorDia.Count, usuario.Nome);

                if (atividadesPorUsuario.TryGetValue(usuario.Id, out var atividadesUsuario))
                {
                    _logger.LogInformation("Usuário {Nome} tem {QtdAtividades} atividades",
                        usuario.Nome, atividadesUsuario.Count);

                    usuario.Atividades = atividadesUsuario;

                    // Mapeia as atividades para os dias correspondentes
                    foreach (var atividade in atividadesUsuario)
                    {
                        var inicioAtividade = atividade.DataInicio.Date;
                        var fimAtividade = (atividade.DataFimReal ?? atividade.DataFimPrevista ?? atividade.DataInicio).Date;

                        _logger.LogInformation("Mapeando atividade '{NomeAtividade}' de {Inicio} a {Fim}",
                            atividade.Nome, inicioAtividade, fimAtividade);

                        // Garante que está dentro do período do grid
                        var inicioEfetivo = inicioAtividade < dataInicio ? dataInicio : inicioAtividade;
                        var fimEfetivo = fimAtividade > dataFim ? dataFim : fimAtividade;

                        _logger.LogInformation("Período efetivo da atividade: {InicioEfetivo} a {FimEfetivo}",
                            inicioEfetivo, fimEfetivo);

                        // Adiciona atividade em TODOS os dias do período
                        var dataAtual = inicioEfetivo.Date;
                        int diasAdicionados = 0;

                        while (dataAtual <= fimEfetivo.Date)
                        {
                            if (usuario.AtividadesPorDia.ContainsKey(dataAtual))
                            {
                                usuario.AtividadesPorDia[dataAtual].Add(ClonarAtividade(atividade));
                                diasAdicionados++;
                            }
                            else
                            {
                                _logger.LogWarning("Data {Data} não encontrada no dicionário do usuário {Nome}",
                                    dataAtual, usuario.Nome);
                            }
                            dataAtual = dataAtual.AddDays(1);
                        }

                        _logger.LogInformation("Atividade '{Nome}' adicionada em {QtdDias} dias",
                            atividade.Nome, diasAdicionados);
                    }

                    _logger.LogInformation("Usuário {Nome} final: {TotalAtividades} atividades, {DiaComAtividades} dias com atividades",
                        usuario.Nome, usuario.TotalAtividades,
                        usuario.AtividadesPorDia.Count(kvp => kvp.Value.Any()));
                }
                else
                {
                    _logger.LogInformation("Usuário {Nome} não possui atividades no período", usuario.Nome);
                }
            }

            _logger.LogInformation("=== MAPEAMENTO DE ATIVIDADES CONCLUÍDO ===");
            return usuarios;
        }

        private Dictionary<DateTime, List<AtividadeGridViewModel>> InicializarDicionarioVazio(DateTime dataInicio, DateTime dataFim)
        {
            var dicionario = new Dictionary<DateTime, List<AtividadeGridViewModel>>();
            var dataAtual = dataInicio.Date;

            while (dataAtual <= dataFim.Date)
            {
                dicionario[dataAtual] = new List<AtividadeGridViewModel>();
                dataAtual = dataAtual.AddDays(1);
            }

            return dicionario;
        }

        private AtividadeGridViewModel ClonarAtividade(AtividadeGridViewModel original)
        {
            return new AtividadeGridViewModel
            {
                Id = original.Id,
                Nome = original.Nome,
                Descricao = original.Descricao,
                ProjetoNome = original.ProjetoNome,
                TipoAtividadeNome = original.TipoAtividadeNome,
                StatusNome = original.StatusNome,
                StatusCor = original.StatusCor,
                DataInicio = original.DataInicio,
                DataFimPrevista = original.DataFimPrevista,
                DataFimReal = original.DataFimReal,
                Prioridade = original.Prioridade,
                UsuarioId = original.UsuarioId
            };
        }

        private static void ValidarFiltro(GridFilterViewModel filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            if (filter.DataInicio > filter.DataFim)
                throw new ArgumentException("Data inicial não pode ser maior que data final");

            var diffDays = (filter.DataFim - filter.DataInicio).TotalDays;
            if (diffDays > 30)
                throw new ArgumentException("Período não pode ser maior que 30 dias");
        }

        private static string GerarIniciais(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return "??";

            var palavras = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (palavras.Length == 1)
                return palavras[0].Substring(0, Math.Min(2, palavras[0].Length)).ToUpper();

            return $"{palavras[0][0]}{palavras[^1][0]}".ToUpper();
        }
    }
}