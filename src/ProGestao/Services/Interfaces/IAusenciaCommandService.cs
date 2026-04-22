using ProGestao.Common;
using ProGestao.ViewModels.Ausencia;

namespace ProGestao.Services.Interfaces
{
    /// <summary>
    /// Interface para operações de escrita de ausências
    /// Implementa Interface Segregation Principle
    /// </summary>
    public interface IAusenciaCommandService
    {
        Task<Result<int>> CreateAsync(AusenciaViewModel ausencia);
        Task<Result<bool>> UpdateAsync(AusenciaViewModel ausencia);
        Task<Result<bool>> DesativarAsync(int id);
    }
}
