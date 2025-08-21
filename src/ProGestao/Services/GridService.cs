using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.ViewModels.Atividade;
using ProGestao.ViewModels.Grid;
using ProGestao.ViewModels.Usuarios;

namespace ProGestao.Services
{
    public interface IGridService
    {
        Task<GridDataViewModel> GetGridDataAsync(GridFilterViewModel filter);
        Task<GridMetricsViewModel> GetGridMetricsAsync(GridFilterViewModel filter);
        Task<List<EquipeGridViewModel>> GetEquipesForFilterAsync();
    }

    /// <summary>
    /// Serviço responsável pela lógica de negócio do Grid Semanal
    /// VERSÃO CORRIGIDA - Sem problemas de casting do Entity Framework
    /// </summary>
    public class GridService : IGridService
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<GridService> _logger;

        public GridService(ProGestaoContext context, ILogger<GridService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GridDataViewModel> GetGridDataAsync(GridFilterViewModel filter)
        {
            ValidateFilter(filter);

            _logger.LogInformation("Carregando grid: {DataInicio} até {DataFim}, Equipe: {EquipeId}",
                filter.DataInicio, filter.DataFim, filter.EquipeId);

            var dias = GenerateGridDays(filter.DataInicio, filter.DataFim);
            var usuarios = await GetUsuariosForGridAsync(filter.EquipeId);
            var atividades = await GetAtividadesGridAsync(filter);

            var atividadesPorDia = OrganizeActivitiesByDay(atividades, filter.DataInicio, filter.DataFim);

            foreach (var usuario in usuarios)
            {
                usuario.Dias = dias.Select(dia => new DiaUsuarioViewModel
                {
                    Data = dia.Data,
                    DiaSemana = dia.DiaSemana,
                    Atividades = atividadesPorDia.ContainsKey(dia.Data)
                        ? atividadesPorDia[dia.Data]
                            .Where(a => a.UsuarioId == usuario.Id)
                            .ToList()
                        : new List<AtividadeGridViewModel>()
                }).ToList();
            }

            return new GridDataViewModel
            {
                Usuarios = usuarios,
                Dias = dias
            };
        }

        public async Task<GridMetricsViewModel> GetGridMetricsAsync(GridFilterViewModel filter)
        {
            ValidateFilter(filter);

            // ✅ CORRIGIDO: Declara explicitamente como IQueryable<Atividade>
            IQueryable<Atividade> atividadesQuery = _context.Atividades.AsNoTracking();

            // Aplica filtro de equipe se especificado
            if (filter.EquipeId.HasValue)
            {
                atividadesQuery = atividadesQuery.Where(a => a.Usuario.EquipeId == filter.EquipeId.Value);
            }

            // Aplica filtro de período
            var atividadesNoPeriodo = atividadesQuery.Where(a =>
                (a.DataInicio <= filter.DataFim && a.DataFimPrevista >= filter.DataInicio) ||
                (a.DataInicio >= filter.DataInicio && a.DataInicio <= filter.DataFim));

            var totalAtividades = await atividadesNoPeriodo.CountAsync();
            var atividadesAtrasadas = await atividadesNoPeriodo
                .Where(a => a.DataFimPrevista < DateTime.Now && a.DataFimReal == null)
                .CountAsync();

            return new GridMetricsViewModel
            {
                TotalAtividades = totalAtividades,
                AtividadesAtrasadas = atividadesAtrasadas
            };
        }

        public async Task<List<EquipeGridViewModel>> GetEquipesForFilterAsync()
        {
            return await _context.Equipes
                .AsNoTracking()
                .Where(e => e.Ativo)
                .Select(e => new EquipeGridViewModel
                {
                    Id = e.Id,
                    Nome = e.Nome,
                    QtdUsuarios = e.Usuarios.Count(u => u.Ativo)
                })
                .OrderBy(e => e.Nome)
                .ToListAsync();
        }

        #region Private Methods

        /// <summary>
        /// Obtém usuários para exibição no grid
        /// </summary>
        private async Task<List<UsuarioGridViewModel>> GetUsuariosForGridAsync(int? equipeId)
        {
            // ✅ CORRIGIDO: Declara explicitamente como IQueryable<Usuario>
            IQueryable<Usuario> usuariosQuery = _context.Usuarios
                .AsNoTracking()
                .Where(u => u.Ativo);

            if (equipeId.HasValue)
            {
                usuariosQuery = usuariosQuery.Where(u => u.EquipeId == equipeId.Value);
            }

            return await usuariosQuery
                .Include(u => u.Equipe)
                .Select(u => new UsuarioGridViewModel
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Cargo = u.Cargo,
                    EquipeNome = u.Equipe.Nome,
                    Iniciais = GetInitials(u.Nome),
                    Dias = new List<DiaUsuarioViewModel>() // Será populado posteriormente
                })
                .OrderBy(u => u.Nome)
                .ToListAsync();
        }

        /// <summary>
        /// Obtém atividades do grid para o período especificado
        /// </summary>
        private async Task<List<AtividadeGridViewModel>> GetAtividadesGridAsync(GridFilterViewModel filter)
        {
            // ✅ CORRIGIDO: Declara explicitamente como IQueryable<Atividade>
            IQueryable<Atividade> atividadesQuery = _context.Atividades
                .AsNoTracking()
                .Include(a => a.Projeto)
                .Include(a => a.TipoAtividade)
                .Include(a => a.Status)
                .Include(a => a.Usuario)
                    .ThenInclude(u => u.Equipe);

            // Aplica filtro de equipe se especificado
            if (filter.EquipeId.HasValue)
            {
                atividadesQuery = atividadesQuery.Where(a => a.Usuario.EquipeId == filter.EquipeId.Value);
            }

            // Aplica filtro de período
            var atividadesFiltradas = atividadesQuery.Where(a =>
                (a.DataInicio <= filter.DataFim && a.DataFimPrevista >= filter.DataInicio) ||
                (a.DataInicio >= filter.DataInicio && a.DataInicio <= filter.DataFim));

            return await atividadesFiltradas
                .Select(a => new AtividadeGridViewModel
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    Descricao = a.Descricao ?? string.Empty,
                    ProjetoNome = a.Projeto != null ? a.Projeto.Nome : "Sem Projeto",
                    TipoAtividadeNome = a.TipoAtividade.Nome,
                    StatusNome = a.Status.Nome,
                    StatusCor = a.Status.Cor,
                    DataInicio = a.DataInicio,
                    DataFimPrevista = a.DataFimPrevista,
                    DataFimReal = a.DataFimReal,
                    Prioridade = a.Prioridade, // ✅ Sem conversão - ambos são int
                    UsuarioId = a.UsuarioId
                })
                .ToListAsync();
        }

        /// <summary>
        /// Gera lista de dias para o grid baseado no período
        /// </summary>
        private static List<DiaGridViewModel> GenerateGridDays(DateTime dataInicio, DateTime dataFim)
        {
            var dias = new List<DiaGridViewModel>();
            var dataAtual = dataInicio;

            while (dataAtual <= dataFim)
            {
                dias.Add(new DiaGridViewModel
                {
                    Data = dataAtual,
                    DiaSemana = GetDayOfWeekInPortuguese(dataAtual.DayOfWeek),
                    DiaNumero = dataAtual.Day
                });
                dataAtual = dataAtual.AddDays(1);
            }

            return dias;
        }

        /// <summary>
        /// Converte DayOfWeek para português
        /// </summary>
        private static string GetDayOfWeekInPortuguese(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => "domingo",
                DayOfWeek.Monday => "segunda-feira",
                DayOfWeek.Tuesday => "terça-feira",
                DayOfWeek.Wednesday => "quarta-feira",
                DayOfWeek.Thursday => "quinta-feira",
                DayOfWeek.Friday => "sexta-feira",
                DayOfWeek.Saturday => "sábado",
                _ => "indefinido"
            };
        }

        /// <summary>
        /// Organiza atividades por dia, duplicando para cada dia do período da atividade
        /// </summary>
        private static Dictionary<DateTime, List<AtividadeGridViewModel>> OrganizeActivitiesByDay(
            List<AtividadeGridViewModel> atividades, DateTime dataInicio, DateTime dataFim)
        {
            var atividadesPorDia = new Dictionary<DateTime, List<AtividadeGridViewModel>>();

            foreach (var atividade in atividades)
            {
                var inicioAtividade = atividade.DataInicio.Date;
                var fimAtividade = (atividade.DataFimReal ?? atividade.DataFimPrevista ?? atividade.DataInicio).Date;

                var inicioEfetivo = inicioAtividade < dataInicio ? dataInicio : inicioAtividade;
                var fimEfetivo = fimAtividade > dataFim ? dataFim : fimAtividade;

                var dataAtual = inicioEfetivo;
                while (dataAtual <= fimEfetivo)
                {
                    if (!atividadesPorDia.ContainsKey(dataAtual))
                    {
                        atividadesPorDia[dataAtual] = new List<AtividadeGridViewModel>();
                    }

                    // Clona a atividade para cada dia
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
        /// </summary>
        private static void ValidateFilter(GridFilterViewModel filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            if (filter.DataInicio > filter.DataFim)
                throw new ArgumentException("Data inicial não pode ser maior que data final");

            var diffDays = (filter.DataFim - filter.DataInicio).TotalDays;
            if (diffDays > 30)
                throw new ArgumentException("Período não pode ser maior que 30 dias");
        }

        /// <summary>
        /// Gera iniciais do nome do usuário
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