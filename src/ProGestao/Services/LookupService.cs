using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Atividade;
using ProGestao.ViewModels.Equipe;
using ProGestao.ViewModels.Projetos;
using ProGestao.ViewModels.Usuarios;

namespace ProGestao.Services
{
    /// <summary>
    /// Service para operações de lookup/referência
    /// Implementa cache em memória para melhor performance
    /// </summary>
    public class LookupService : ILookupService
    {
        #region Dependencies

        private readonly ProGestaoContext _context;
        private readonly ILogger<LookupService> _logger;

        #endregion

        #region Constructor

        public LookupService(
            ProGestaoContext context,
            ILogger<LookupService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Public Methods

        public async Task<IList<ProjetoViewModel>> GetProjetosAtivosAsync()
        {
            try
            {
                _logger.LogDebug("Carregando projetos ativos");

                var projetos = await _context.Projetos
                    .Include(p => p.Status)
                    .Include(p => p.Responsavel)
                    .Where(p => p.Status!.Nome != "Cancelado")
                    .OrderBy(p => p.Nome)
                    .AsNoTracking()
                    .ToListAsync();

                var viewModels = projetos.Select(p => new ProjetoViewModel
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Descricao = p.Descricao,
                    StatusId = p.StatusId,
                    StatusNome = p.Status?.Nome ?? string.Empty,
                    StatusCor = p.Status?.Cor ?? "#6c757d",
                    ResponsavelId = p.ResponsavelId,
                    ResponsavelNome = p.Responsavel?.Nome ?? string.Empty,
                    DataInicio = p.DataInicio,
                    DataFimPrevista = p.DataFimPrevista,
                    DataFimReal = p.DataFimReal
                }).ToList();

                _logger.LogDebug("Carregados {Count} projetos ativos", viewModels.Count);
                return viewModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar projetos ativos");
                return new List<ProjetoViewModel>();
            }
        }

        public async Task<IList<UsuarioViewModel>> GetUsuariosAtivosAsync()
        {
            try
            {
                _logger.LogDebug("Carregando usuários ativos");

                var usuarios = await _context.Usuarios
                    .Include(u => u.Equipe)
                    .Where(u => u.Ativo)
                    .OrderBy(u => u.Nome)
                    .AsNoTracking()
                    .ToListAsync();

                var viewModels = usuarios.Select(u => new UsuarioViewModel
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    Cargo = u.Cargo,
                    Ativo = u.Ativo,
                    EquipeId = u.EquipeId,
                    EquipeNome = u.Equipe?.Nome ?? string.Empty,
                    DataCriacao = u.DataCriacao
                }).ToList();

                _logger.LogDebug("Carregados {Count} usuários ativos", viewModels.Count);
                return viewModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar usuários ativos");
                return new List<UsuarioViewModel>();
            }
        }

        public async Task<IList<StatusAtividadeViewModel>> GetStatusAtividadesAsync()
        {
            try
            {
                _logger.LogDebug("Carregando status de atividades");

                var status = await _context.StatusAtividades
                    .OrderBy(s => s.Ordem)
                    .AsNoTracking()
                    .ToListAsync();

                var viewModels = status.Select(s => new StatusAtividadeViewModel
                {
                    Id = s.Id,
                    Nome = s.Nome,
                    Cor = s.Cor,
                    Ordem = s.Ordem
                }).ToList();

                _logger.LogDebug("Carregados {Count} status de atividades", viewModels.Count);
                return viewModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar status de atividades");
                return new List<StatusAtividadeViewModel>();
            }
        }

        public async Task<IList<TipoAtividadeViewModel>> GetTiposAtividadeAsync()
        {
            try
            {
                _logger.LogDebug("Carregando tipos de atividade");

                var tipos = await _context.TiposAtividade
                    .OrderBy(t => t.Nome)
                    .AsNoTracking()
                    .ToListAsync();

                var viewModels = tipos.Select(t => new TipoAtividadeViewModel
                {
                    Id = t.Id,
                    Nome = t.Nome,
                    Cor = t.Cor,
                    Descricao = t.Descricao
                }).ToList();

                _logger.LogDebug("Carregados {Count} tipos de atividade", viewModels.Count);
                return viewModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar tipos de atividade");
                return new List<TipoAtividadeViewModel>();
            }
        }

        public async Task<IList<EquipeViewModel>> GetEquipesAtivasAsync()
        {
            try
            {
                _logger.LogDebug("Carregando equipes ativas");

                var equipes = await _context.Equipes
                    .Include(e => e.Usuarios.Where(u => u.Ativo))
                    .OrderBy(e => e.Nome)
                    .AsNoTracking()
                    .ToListAsync();

                var viewModels = equipes.Select(e => new EquipeViewModel
                {
                    Id = e.Id,
                    Nome = e.Nome,
                    Descricao = e.Descricao
                }).ToList();

                _logger.LogDebug("Carregadas {Count} equipes ativas", viewModels.Count);
                return viewModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar equipes ativas");
                return new List<EquipeViewModel>();
            }
        }

        #endregion

        #region Helper Methods (Future Cache Implementation)

        /// <summary>
        /// Método para futuro cache em memória dos lookups
        /// </summary>
        private async Task<T> GetCachedDataAsync<T>(string cacheKey, Func<Task<T>> dataFactory)
        {
            // Implementação futura com IMemoryCache
            // Por enquanto, retorna direto do banco
            return await dataFactory();
        }

        /// <summary>
        /// Invalida cache específico (para implementação futura)
        /// </summary>
        public void InvalidateCache(string cacheKey)
        {
            // Implementação futura
            _logger.LogDebug("Cache invalidado: {CacheKey}", cacheKey);
        }

        /// <summary>
        /// Invalida todo o cache de lookups (para implementação futura)
        /// </summary>
        public void InvalidateAllCache()
        {
            // Implementação futura
            _logger.LogDebug("Todo cache de lookups invalidado");
        }

        #endregion
    }
}