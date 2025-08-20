using ProGestao.ViewModels.Atividade;
using ProGestao.ViewModels.Equipe;
using ProGestao.ViewModels.Projetos;
using ProGestao.ViewModels.Usuarios;

namespace ProGestao.Services.Interfaces
{
    /// <summary>
    /// Interface para operações de lookup/referência
    /// </summary>
    public interface ILookupService
    {
        Task<IList<ProjetoViewModel>> GetProjetosAtivosAsync();
        Task<IList<UsuarioViewModel>> GetUsuariosAtivosAsync();
        Task<IList<StatusAtividadeViewModel>> GetStatusAtividadesAsync();
        Task<IList<TipoAtividadeViewModel>> GetTiposAtividadeAsync();
        Task<IList<EquipeViewModel>> GetEquipesAtivasAsync();
    }
}
