using ProGestao.ViewModels.Ausencia;

namespace ProGestao.Services.Interfaces
{
    /// <summary>
    /// Interface para operações de leitura de ausências
    /// Implementa Interface Segregation Principle
    /// </summary>
    public interface IAusenciaQueryService
    {
        Task<IList<AusenciaViewModel>> GetByFiltroAsync(int? usuarioId = null, int? tipoAusenciaId = null, DateTime? dataInicio = null, DateTime? dataFim = null, bool? ativo = null);
        Task<AusenciaViewModel?> GetByIdAsync(int id);
        Task<IList<AusenciaViewModel>> GetPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<IList<AusenciaViewModel>> GetPorUsuarioAsync(int usuarioId);
    }
}
