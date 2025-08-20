using ProGestao.ViewModels.Projetos;
using ProGestao.Models;

namespace ProGestao.Services.Interfaces
{
    /// <summary>
    /// Interface específica para mapeamento de Projetos
    /// </summary>
    public interface IProjetoMapper : IBaseMapper<Projeto, ProjetoViewModel>
    {
        /// <summary>
        /// Mapeamento incluindo atividades do projeto
        /// </summary>
        ProjetoViewModel MapToViewModelComAtividades(Projeto projeto);

        /// <summary>
        /// Mapeamento simplificado para dropdowns
        /// </summary>
        ProjetoLookupViewModel MapToLookupViewModel(Projeto projeto);

        /// <summary>
        /// Mapeamento para dashboard com métricas
        /// </summary>
        ProjetoDashboardViewModel MapToDashboardViewModel(Projeto projeto);
    }
}
