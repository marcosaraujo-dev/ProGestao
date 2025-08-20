using ProGestao.Common;
using ProGestao.ViewModels.Projetos;

namespace ProGestao.Services.Interfaces
{
    /// <summary>
    /// Interface para validação de projetos
    /// Single Responsibility Principle
    /// </summary>
    public interface IProjetoValidationService
    {
        Task<ValidationResult> ValidateCreateAsync(ProjetoViewModel projeto);
        Task<ValidationResult> ValidateUpdateAsync(ProjetoViewModel projeto);
        Task<ValidationResult> ValidateDeleteAsync(int projetoId);
        Task<ValidationResult> ValidateStatusChangeAsync(int projetoId, int novoStatusId);
        Task<ValidationResult> ValidateResponsavelChangeAsync(int projetoId, int? novoResponsavelId);
    }
}
