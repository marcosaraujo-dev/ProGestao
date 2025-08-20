
using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.ViewModels;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Projetos;

namespace ProGestao.Services.Implementation
{
    /// <summary>
    /// Service para operações de leitura de projetos
    /// Implementa Single Responsibility Principle
    /// </summary>
    public class ProjetoQueryService : IProjetoQueryService
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<ProjetoQueryService> _logger;
        private readonly IProjetoMapper _mapper;

        public ProjetoQueryService(
            ProGestaoContext context,
            ILogger<ProjetoQueryService> logger,
            IProjetoMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IList<ProjetoViewModel>> GetProjetosAsync()
        {
            try
            {
                _logger.LogInformation("Buscando todos os projetos");

                var projetos = await BuildBaseQuery()
                    .OrderByDescending(p => p.DataCriacao)
                    .AsNoTracking()
                    .ToListAsync();

                var result = _mapper.MapToViewModelList(projetos);

                _logger.LogInformation("Encontrados {Count} projetos", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar projetos");
                throw;
            }
        }

        public async Task<IList<ProjetoViewModel>> GetProjetosByFiltroAsync(
            string? filtroStatus = null, int? responsavelId = null, string? filtroNome = null)
        {
            try
            {
                _logger.LogInformation("Buscando projetos com filtros: Status={Status}, ResponsavelId={ResponsavelId}, Nome={Nome}",
                    filtroStatus, responsavelId, filtroNome);

                var query = BuildBaseQuery();

                if (!string.IsNullOrWhiteSpace(filtroStatus))
                    query = query.Where(p => p.Status.Nome.Contains(filtroStatus));

                if (responsavelId.HasValue)
                    query = query.Where(p => p.ResponsavelId == responsavelId.Value);

                if (!string.IsNullOrWhiteSpace(filtroNome))
                    query = query.Where(p => p.Nome.Contains(filtroNome));

                var projetos = await query
                    .OrderByDescending(p => p.DataCriacao)
                    .AsNoTracking()
                    .ToListAsync();

                var result = _mapper.MapToViewModelList(projetos);

                _logger.LogInformation("Encontrados {Count} projetos com filtros", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar projetos com filtros");
                throw;
            }
        }

        public async Task<ProjetoViewModel?> GetProjetoByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando projeto por ID: {ProjetoId}", id);

                var projeto = await BuildBaseQuery()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (projeto == null)
                {
                    _logger.LogWarning("Projeto não encontrado: {ProjetoId}", id);
                    return null;
                }

                return _mapper.MapToViewModel(projeto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar projeto por ID: {ProjetoId}", id);
                throw;
            }
        }

        public async Task<ProjetoViewModel?> GetProjetoComDetalhesAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando projeto com detalhes por ID: {ProjetoId}", id);

                var projeto = await _context.Projetos
                    .Include(p => p.Status)
                    .Include(p => p.Responsavel)
                        .ThenInclude(r => r.Equipe)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (projeto == null)
                {
                    _logger.LogWarning("Projeto não encontrado: {ProjetoId}", id);
                    return null;
                }

                return _mapper.MapToViewModel(projeto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar projeto com detalhes: {ProjetoId}", id);
                throw;
            }
        }

        public async Task<ProjetoViewModel?> GetProjetoComAtividadesAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando projeto com atividades por ID: {ProjetoId}", id);

                var projeto = await _context.Projetos
                    .Include(p => p.Status)
                    .Include(p => p.Responsavel)
                    .Include(p => p.Atividades)
                        .ThenInclude(a => a.Status)
                    .Include(p => p.Atividades)
                        .ThenInclude(a => a.Usuario)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (projeto == null)
                {
                    _logger.LogWarning("Projeto não encontrado: {ProjetoId}", id);
                    return null;
                }

                return _mapper.MapToViewModelComAtividades(projeto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar projeto com atividades: {ProjetoId}", id);
                throw;
            }
        }

        public async Task<bool> ExisteProjetoAsync(int id)
        {
            try
            {
                return await _context.Projetos
                    .AsNoTracking()
                    .AnyAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar existência do projeto: {ProjetoId}", id);
                throw;
            }
        }

        public async Task<IList<ProjetoViewModel>> GetProjetosAtivosAsync()
        {
            try
            {
                var projetos = await BuildBaseQuery()
                    .Where(p => p.Status.Nome != "Cancelado" && p.Status.Nome != "Concluído")
                    .OrderBy(p => p.DataInicio)
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.MapToViewModelList(projetos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar projetos ativos");
                throw;
            }
        }

        public async Task<IList<ProjetoViewModel>> GetProjetosPorResponsavelAsync(int responsavelId)
        {
            try
            {
                var projetos = await BuildBaseQuery()
                    .Where(p => p.ResponsavelId == responsavelId)
                    .OrderByDescending(p => p.DataCriacao)
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.MapToViewModelList(projetos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar projetos por responsável: {ResponsavelId}", responsavelId);
                throw;
            }
        }

        public async Task<IList<ProjetoViewModel>> GetProjetosVencendoAsync(int diasAntecedencia = 7)
        {
            try
            {
                var dataLimite = DateTime.Today.AddDays(diasAntecedencia);

                var projetos = await BuildBaseQuery()
                    .Where(p => p.DataFimPrevista.HasValue &&
                                p.DataFimPrevista <= dataLimite &&
                                p.Status.Nome != "Concluído" &&
                                p.Status.Nome != "Cancelado")
                    .OrderBy(p => p.DataFimPrevista)
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.MapToViewModelList(projetos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar projetos vencendo em {Dias} dias", diasAntecedencia);
                throw;
            }
        }

        public async Task<IList<ProjetoViewModel>> GetProjetosAtrasadosAsync()
        {
            try
            {
                var hoje = DateTime.Today;

                var projetos = await BuildBaseQuery()
                    .Where(p => p.DataFimPrevista.HasValue &&
                                p.DataFimPrevista < hoje &&
                                p.Status.Nome != "Concluído" &&
                                p.Status.Nome != "Cancelado")
                    .OrderBy(p => p.DataFimPrevista)
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.MapToViewModelList(projetos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar projetos atrasados");
                throw;
            }
        }

        public async Task<decimal> GetProgressoProjetoAsync(int projetoId)
        {
            try
            {
                var totalAtividades = await _context.Atividades
                    .Where(a => a.ProjetoId == projetoId)
                    .CountAsync();

                if (totalAtividades == 0)
                    return 0;

                var atividadesConcluidas = await _context.Atividades
                    .Include(a => a.Status)
                    .Where(a => a.ProjetoId == projetoId && a.Status.Nome == "Concluída")
                    .CountAsync();

                return Math.Round((decimal)atividadesConcluidas / totalAtividades * 100, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular progresso do projeto: {ProjetoId}", projetoId);
                throw;
            }
        }

        public async Task<int> GetQuantidadeAtividadesAsync(int projetoId)
        {
            try
            {
                return await _context.Atividades
                    .Where(a => a.ProjetoId == projetoId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao contar atividades do projeto: {ProjetoId}", projetoId);
                throw;
            }
        }

        private IQueryable<Projeto> BuildBaseQuery()
        {
            return _context.Projetos
                .Include(p => p.Status)
                .Include(p => p.Responsavel);
        }
    }
}

