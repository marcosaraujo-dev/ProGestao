
using ProGestao.Common;
using ProGestao.ViewModels.Atividade;

namespace ProGestao.Services.Interfaces
{
    /// <summary>
    /// Interface para validação de atividades
    /// Single Responsibility Principle
    /// </summary>
    public interface IAtividadeValidationService
    {
        Task<ValidationResult> ValidateCreateAsync(AtividadeViewModel atividade);
        Task<ValidationResult> ValidateUpdateAsync(AtividadeViewModel atividade);
        Task<ValidationResult> ValidateDeleteAsync(int atividadeId);
        Task<ValidationResult> ValidateStatusChangeAsync(int atividadeId, int novoStatusId);
    }
}
