using Microsoft.EntityFrameworkCore;
using ProGestao.Data;
using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.TipoAusencia;

namespace ProGestao.Services.TiposAusencia
{
    /// <summary>
    /// Service para operações de leitura de tipos de ausência
    /// </summary>
    public class TipoAusenciaQueryService : ITipoAusenciaQueryService
    {
        private readonly ProGestaoContext _context;
        private readonly ILogger<TipoAusenciaQueryService> _logger;

        public TipoAusenciaQueryService(
            ProGestaoContext context,
            ILogger<TipoAusenciaQueryService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IList<TipoAusenciaViewModel>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Buscando todos os tipos de ausência");

                var tipos = await _context.TiposAusencia
                    .OrderBy(t => t.Nome)
                    .AsNoTracking()
                    .ToListAsync();

                var result = tipos.Select(MapToViewModel).ToList();

                _logger.LogInformation("Encontrados {Count} tipos de ausência", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar tipos de ausência");
                throw;
            }
        }

        public async Task<TipoAusenciaViewModel?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Buscando tipo de ausência por ID: {TipoAusenciaId}", id);

                var tipo = await _context.TiposAusencia
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (tipo == null)
                {
                    _logger.LogWarning("Tipo de ausência não encontrado: {TipoAusenciaId}", id);
                    return null;
                }

                return MapToViewModel(tipo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar tipo de ausência por ID: {TipoAusenciaId}", id);
                throw;
            }
        }

        public async Task<IList<TipoAusenciaViewModel>> GetAtivosAsync()
        {
            try
            {
                _logger.LogInformation("Buscando tipos de ausência ativos");

                var tipos = await _context.TiposAusencia
                    .Where(t => t.Ativo)
                    .OrderBy(t => t.Nome)
                    .AsNoTracking()
                    .ToListAsync();

                var result = tipos.Select(MapToViewModel).ToList();

                _logger.LogInformation("Encontrados {Count} tipos de ausência ativos", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar tipos de ausência ativos");
                throw;
            }
        }

        private static TipoAusenciaViewModel MapToViewModel(Models.TipoAusencia tipo)
        {
            return new TipoAusenciaViewModel
            {
                Id = tipo.Id,
                Nome = tipo.Nome,
                Cor = tipo.Cor,
                Descricao = tipo.Descricao,
                Ativo = tipo.Ativo
            };
        }
    }
}
