using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Atividade;

namespace ProGestao.Services.Atividades
{
    /// <summary>
    /// Service para operações de leitura de atividades
    /// </summary>
    public class AtividadeQueryService : IAtividadeQueryService
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<AtividadeQueryService> _logger;
        private readonly IAtividadeMapper _mapper; 

        public AtividadeQueryService(
            ProGestaoContext context,
            ILogger<AtividadeQueryService> logger,
            IAtividadeMapper mapper) 
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IList<AtividadeViewModel>> GetAtividadesByFiltroAsync(
            int? projetoId = null, int? usuarioId = null, int? statusId = null)
        {
            try
            {
                _logger.LogInformation("Buscando atividades com filtros: ProjetoId={ProjetoId}, UsuarioId={UsuarioId}, StatusId={StatusId}",
                    projetoId, usuarioId, statusId);

                var query = BuildBaseQuery();

                if (projetoId.HasValue)
                    query = query.Where(a => a.ProjetoId == projetoId.Value);

                if (usuarioId.HasValue)
                    query = query.Where(a => a.UsuarioId == usuarioId.Value);

                if (statusId.HasValue)
                    query = query.Where(a => a.StatusId == statusId.Value);

                var atividades = await query
                    .OrderByDescending(a => a.DataCriacao)
                    .AsNoTracking()
                    .ToListAsync();

                var result = _mapper.MapToViewModelList(atividades);

                _logger.LogInformation("Encontradas {Count} atividades", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar atividades com filtros");
                throw;
            }
        }

        public async Task<AtividadeViewModel?> GetAtividadeByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando atividade por ID: {AtividadeId}", id);

                var atividade = await BuildBaseQuery()
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (atividade == null)
                {
                    _logger.LogWarning("Atividade não encontrada: {AtividadeId}", id);
                    return null;
                }

                return _mapper.MapToViewModel(atividade);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar atividade por ID: {AtividadeId}", id);
                throw;
            }
        }

        public async Task<AtividadeViewModel?> GetAtividadeComDetalhesAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando atividade com detalhes por ID: {AtividadeId}", id);

                var atividade = await _context.Atividades
                    .Include(a => a.Projeto)
                        .ThenInclude(p => p.Status)
                    .Include(a => a.Projeto)
                        .ThenInclude(p => p.Responsavel)
                    .Include(a => a.Usuario)
                        .ThenInclude(u => u.Equipe)
                    .Include(a => a.TipoAtividade)
                    .Include(a => a.Status)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (atividade == null)
                {
                    _logger.LogWarning("Atividade não encontrada: {AtividadeId}", id);
                    return null;
                }

                return _mapper.MapToViewModelDetalhado(atividade); // ← Método específico
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar atividade com detalhes: {AtividadeId}", id);
                throw;
            }
        }

        public async Task<bool> ExisteAtividadeAsync(int id)
        {
            try
            {
                return await _context.Atividades
                    .AsNoTracking()
                    .AnyAsync(a => a.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar existência da atividade: {AtividadeId}", id);
                throw;
            }
        }

        public async Task<IList<AtividadeViewModel>> GetAtividadesAtivasAsync()
        {
            try
            {
                var atividades = await BuildBaseQuery()
                    .Where(a => a.Status.Nome != "Cancelada" && a.Status.Nome != "Concluída")
                    .OrderBy(a => a.DataInicio)
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.MapToViewModelList(atividades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar atividades ativas");
                throw;
            }
        }

        public async Task<IList<AtividadeViewModel>> GetAtividadesPorUsuarioAsync(int usuarioId)
        {
            try
            {
                var atividades = await BuildBaseQuery()
                    .Where(a => a.UsuarioId == usuarioId)
                    .OrderByDescending(a => a.DataCriacao)
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.MapToViewModelList(atividades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar atividades por usuário: {UsuarioId}", usuarioId);
                throw;
            }
        }

        public async Task<IList<AtividadeViewModel>> GetAtividadesPorProjetoAsync(int projetoId)
        {
            try
            {
                var atividades = await BuildBaseQuery()
                    .Where(a => a.ProjetoId == projetoId)
                    .OrderBy(a => a.DataInicio)
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.MapToViewModelList(atividades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar atividades por projeto: {ProjetoId}", projetoId);
                throw;
            }
        }

        private IQueryable<Atividade> BuildBaseQuery()
        {
            return _context.Atividades
                .Include(a => a.Projeto)
                .Include(a => a.Usuario)
                .Include(a => a.TipoAtividade)
                .Include(a => a.Status);
        }
    }

}
