using ProGestao.Models;
using ProGestao.ViewModels.Usuarios;

namespace ProGestao.Services.Interfaces
{
    /// <summary>
    /// Interface específica para mapeamento de Usuários
    /// </summary>
    public interface IUsuarioMapper : IBaseMapper<Usuario, UsuarioViewModel>
    {
        /// <summary>
        /// Mapeamento incluindo informações da equipe
        /// </summary>
        UsuarioViewModel MapToViewModelComEquipe(Usuario usuario);

        /// <summary>
        /// Mapeamento para dropdown simplificado
        /// </summary>
        UsuarioLookupViewModel MapToLookupViewModel(Usuario usuario);

        /// <summary>
        /// Mapeamento para performance (dashboard)
        /// </summary>
        UsuarioPerformanceViewModel MapToPerformanceViewModel(Usuario usuario, IList<Atividade> atividades);
    }
}
