using ProGestao.ViewModels.TipoAusencia;

namespace ProGestao.Services.Interfaces
{
    /// <summary>
    /// Interface para operações de leitura de tipos de ausência
    /// Implementa Interface Segregation Principle
    /// </summary>
    public interface ITipoAusenciaQueryService
    {
        Task<IList<TipoAusenciaViewModel>> GetAllAsync();
        Task<TipoAusenciaViewModel?> GetByIdAsync(int id);
        Task<IList<TipoAusenciaViewModel>> GetAtivosAsync();
    }
}
