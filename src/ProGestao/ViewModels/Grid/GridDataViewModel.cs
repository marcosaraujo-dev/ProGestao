using ProGestao.Services;
using ProGestao.ViewModels.Usuarios;

namespace ProGestao.ViewModels.Grid
{
    /// <summary>
    /// ViewModel para dados do grid
    /// </summary>
    public class GridDataViewModel
    {
        public List<UsuarioGridViewModel> Usuarios { get; set; } = new();
        public List<DiaGridViewModel> Dias { get; set; } = new();
    }
}
