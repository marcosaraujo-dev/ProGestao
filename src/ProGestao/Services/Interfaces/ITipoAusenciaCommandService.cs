using ProGestao.Common;
using ProGestao.ViewModels.TipoAusencia;

namespace ProGestao.Services.Interfaces
{
    /// <summary>
    /// Interface para operações de escrita de tipos de ausência
    /// Implementa Interface Segregation Principle
    /// </summary>
    public interface ITipoAusenciaCommandService
    {
        Task<Result<int>> CreateAsync(TipoAusenciaViewModel tipoAusencia);
        Task<Result<bool>> UpdateAsync(TipoAusenciaViewModel tipoAusencia);
        Task<Result<bool>> DesativarAsync(int id);
    }
}
