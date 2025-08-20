using ProGestao.ViewModels;
using ProGestao.ViewModels.Projetos;
using ProGestao.ViewModels.Usuarios;

namespace ProGestao.Services.Interfaces
{
    /// <summary>
    /// Interface para operações específicas de lookup de projetos
    /// </summary>
    public interface IProjetoLookupService
    {
        Task<IList<ProjetoLookupViewModel>> GetProjetosForDropdownAsync();
        Task<IList<StatusProjetoViewModel>> GetStatusProjetosAsync();
        Task<IList<UsuarioLookupViewModel>> GetResponsaveisAsync();
        Task<ProjetoResumoViewModel?> GetResumoProjetoAsync(int projetoId);
    }
}
