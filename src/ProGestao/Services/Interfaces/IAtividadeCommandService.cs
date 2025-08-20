
using ProGestao.Common;
using ProGestao.ViewModels.Atividade;

namespace ProGestao.Services.Interfaces
{
    /// <summary>
    /// Interface para operações de escrita de atividades
    /// Implementa Interface Segregation Principle
    /// </summary>
    public interface IAtividadeCommandService
    {
        Task<Result<int>> CreateAtividadeAsync(AtividadeViewModel atividade);
        Task<Result<bool>> UpdateAtividadeAsync(AtividadeViewModel atividade);
        Task<Result<bool>> DeleteAtividadeAsync(int id);
        Task<Result<bool>> UpdateStatusAsync(int atividadeId, int novoStatusId);
        Task<Result<bool>> FinalizarAtividadeAsync(int atividadeId, DateTime? dataFim = null);
    }
}
