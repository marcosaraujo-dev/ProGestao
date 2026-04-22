using ProGestao.Common;
using ProGestao.ViewModels.Ausencia;

namespace ProGestao.Services.Interfaces
{
    /// <summary>
    /// Interface para validação de ausências
    /// Implementa Interface Segregation Principle
    /// </summary>
    public interface IAusenciaValidationService
    {
        Task<ValidationResult> ValidateCreateAsync(AusenciaViewModel ausencia);
        Task<ValidationResult> ValidateUpdateAsync(AusenciaViewModel ausencia);
        Task<ValidationResult> ValidateSobreposicaoAsync(int usuarioId, DateTime dataInicio, DateTime dataFim, int? ausenciaIdExcluir = null);
    }
}
