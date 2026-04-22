using ProGestao.ViewModels.Gantt;

namespace ProGestao.Services.Interfaces
{
    /// <summary>
    /// Service responsável por carregar e formatar dados para o diagrama de Gantt
    /// </summary>
    public interface IGanttService
    {
        /// <summary>
        /// Carrega dados formatados para o diagrama de Gantt,
        /// calculando barras horizontais com offset, duração, cor e detecção de atraso
        /// </summary>
        /// <param name="filter">Filtros de período, equipe, projeto e visão</param>
        /// <returns>Dados completos do Gantt com usuários, barras e dias</returns>
        Task<GanttDataViewModel> GetGanttDataAsync(GanttFilterViewModel filter);
    }
}
