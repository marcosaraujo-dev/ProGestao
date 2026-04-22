using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Models;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Ausencia;

namespace ProGestao.Services.Ausencias
{
    /// <summary>
    /// Service para operações de leitura de ausências
    /// </summary>
    public class AusenciaQueryService : IAusenciaQueryService
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<AusenciaQueryService> _logger;

        public AusenciaQueryService(
            ProGestaoContext context,
            ILogger<AusenciaQueryService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IList<AusenciaViewModel>> GetByFiltroAsync(
            int? usuarioId = null,
            int? tipoAusenciaId = null,
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            bool? ativo = null)
        {
            try
            {
                _logger.LogInformation("Buscando ausências por filtro");

                var query = _context.Ausencias
                    .Include(a => a.Usuario)
                    .Include(a => a.TipoAusencia)
                    .AsNoTracking()
                    .AsQueryable();

                if (usuarioId.HasValue)
                    query = query.Where(a => a.UsuarioId == usuarioId.Value);

                if (tipoAusenciaId.HasValue)
                    query = query.Where(a => a.TipoAusenciaId == tipoAusenciaId.Value);

                if (dataInicio.HasValue)
                    query = query.Where(a => a.DataFim >= dataInicio.Value);

                if (dataFim.HasValue)
                    query = query.Where(a => a.DataInicio <= dataFim.Value);

                if (ativo.HasValue)
                    query = query.Where(a => a.Ativo == ativo.Value);

                var ausencias = await query
                    .OrderByDescending(a => a.DataInicio)
                    .ToListAsync();

                var result = ausencias.Select(MapToViewModel).ToList();

                _logger.LogInformation("Encontradas {Count} ausências", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar ausências por filtro");
                throw;
            }
        }

        public async Task<AusenciaViewModel?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando ausência por ID: {AusenciaId}", id);

                var ausencia = await _context.Ausencias
                    .Include(a => a.Usuario)
                    .Include(a => a.TipoAusencia)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (ausencia == null)
                {
                    _logger.LogWarning("Ausência não encontrada: {AusenciaId}", id);
                    return null;
                }

                return MapToViewModel(ausencia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar ausência por ID: {AusenciaId}", id);
                throw;
            }
        }

        public async Task<IList<AusenciaViewModel>> GetPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                _logger.LogInformation("Buscando ausências por período: {DataInicio} a {DataFim}",
                    dataInicio, dataFim);

                // Ausência [A1, A2] intersecta período [P1, P2] quando A1 <= P2 AND A2 >= P1
                var ausencias = await _context.Ausencias
                    .Include(a => a.Usuario)
                    .Include(a => a.TipoAusencia)
                    .Where(a => a.Ativo && a.DataInicio <= dataFim && a.DataFim >= dataInicio)
                    .OrderBy(a => a.DataInicio)
                    .AsNoTracking()
                    .ToListAsync();

                var result = ausencias.Select(MapToViewModel).ToList();

                _logger.LogInformation("Encontradas {Count} ausências no período", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar ausências por período");
                throw;
            }
        }

        public async Task<IList<AusenciaViewModel>> GetPorUsuarioAsync(int usuarioId)
        {
            try
            {
                _logger.LogInformation("Buscando ausências do usuário: {UsuarioId}", usuarioId);

                var ausencias = await _context.Ausencias
                    .Include(a => a.Usuario)
                    .Include(a => a.TipoAusencia)
                    .Where(a => a.UsuarioId == usuarioId)
                    .OrderByDescending(a => a.DataInicio)
                    .AsNoTracking()
                    .ToListAsync();

                var result = ausencias.Select(MapToViewModel).ToList();

                _logger.LogInformation("Encontradas {Count} ausências do usuário {UsuarioId}",
                    result.Count, usuarioId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar ausências do usuário: {UsuarioId}", usuarioId);
                throw;
            }
        }

        private static AusenciaViewModel MapToViewModel(Ausencia ausencia)
        {
            return new AusenciaViewModel
            {
                Id = ausencia.Id,
                UsuarioId = ausencia.UsuarioId,
                UsuarioNome = ausencia.Usuario?.Nome ?? string.Empty,
                TipoAusenciaId = ausencia.TipoAusenciaId,
                TipoAusenciaNome = ausencia.TipoAusencia?.Nome ?? string.Empty,
                TipoAusenciaCor = ausencia.TipoAusencia?.Cor ?? "#007bff",
                DataInicio = ausencia.DataInicio,
                DataFim = ausencia.DataFim,
                Observacao = ausencia.Observacao,
                DataCriacao = ausencia.DataCriacao,
                Ativo = ausencia.Ativo
            };
        }
    }
}
