using ProGestao.Services;

namespace ProGestao.ViewModels
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
