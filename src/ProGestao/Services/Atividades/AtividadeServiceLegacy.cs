using ProGestao.Services.Interfaces;
using ProGestao.ViewModels.Atividade;

namespace ProGestao.Services.Atividades
{
    /// <summary>
    /// Service Legacy para compatibilidade durante a transição
    /// </summary>
    public class AtividadeServiceLegacy : IAtividadeService
    {
        private readonly IAtividadeQueryService _queryService;
        private readonly IAtividadeCommandService _commandService;
        private readonly ILogger<AtividadeServiceLegacy> _logger;

        public AtividadeServiceLegacy(
            IAtividadeQueryService queryService,
            IAtividadeCommandService commandService,
            ILogger<AtividadeServiceLegacy> logger)
        {
            _queryService = queryService;
            _commandService = commandService;
            _logger = logger;
        }

        public async Task<IList<AtividadeViewModel>> GetAtividadesByFiltroAsync(int? projetoId = null, int? usuarioId = null, int? statusId = null)
        {
            return await _queryService.GetAtividadesByFiltroAsync(projetoId, usuarioId, statusId);
        }

        public async Task<AtividadeViewModel?> GetAtividadeByIdAsync(int id)
        {
            return await _queryService.GetAtividadeByIdAsync(id);
        }

        public async Task<bool> CreateAtividadeAsync(AtividadeViewModel atividade)
        {
            var result = await _commandService.CreateAtividadeAsync(atividade);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Falha ao criar atividade: {Message}", result.Message);
            }
            return result.IsSuccess;
        }

        public async Task<bool> UpdateAtividadeAsync(AtividadeViewModel atividade)
        {
            var result = await _commandService.UpdateAtividadeAsync(atividade);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Falha ao atualizar atividade: {Message}", result.Message);
            }
            return result.IsSuccess;
        }

        public async Task<bool> DeleteAtividadeAsync(int id)
        {
            var result = await _commandService.DeleteAtividadeAsync(id);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Falha ao excluir atividade: {Message}", result.Message);
            }
            return result.IsSuccess;
        }
    }
}
