using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;

namespace ProGestao.Services
{
    public interface IStatusService
    {
        Task<IList<StatusProjeto>> GetStatusProjetosAsync();
        Task<IList<StatusAtividade>> GetStatusAtividadesAsync();
        Task<IList<TipoAtividade>> GetTiposAtividadeAsync();
        Task<StatusProjeto?> GetStatusProjetoByIdAsync(int id);
        Task<StatusAtividade?> GetStatusAtividadeByIdAsync(int id);
        Task<TipoAtividade?> GetTipoAtividadeByIdAsync(int id);
    }

    public class StatusService : IStatusService
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<StatusService> _logger;

        public StatusService(ProGestaoContext context, ILogger<StatusService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IList<StatusProjeto>> GetStatusProjetosAsync()
        {
            try
            {
                return await _context.StatusProjetos
                    .OrderBy(s => s.Ordem)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar status de projetos");
                return new List<StatusProjeto>();
            }
        }

        public async Task<IList<StatusAtividade>> GetStatusAtividadesAsync()
        {
            try
            {
                return await _context.StatusAtividades
                    .OrderBy(s => s.Ordem)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar status de atividades");
                return new List<StatusAtividade>();
            }
        }

        public async Task<IList<TipoAtividade>> GetTiposAtividadeAsync()
        {
            try
            {
                return await _context.TiposAtividade
                    .OrderBy(t => t.Nome)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar tipos de atividade");
                return new List<TipoAtividade>();
            }
        }

        public async Task<StatusProjeto?> GetStatusProjetoByIdAsync(int id)
        {
            try
            {
                return await _context.StatusProjetos.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar status de projeto por ID: {Id}", id);
                return null;
            }
        }

        public async Task<StatusAtividade?> GetStatusAtividadeByIdAsync(int id)
        {
            try
            {
                return await _context.StatusAtividades.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar status de atividade por ID: {Id}", id);
                return null;
            }
        }

        public async Task<TipoAtividade?> GetTipoAtividadeByIdAsync(int id)
        {
            try
            {
                return await _context.TiposAtividade.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar tipo de atividade por ID: {Id}", id);
                return null;
            }
        }
    }
}