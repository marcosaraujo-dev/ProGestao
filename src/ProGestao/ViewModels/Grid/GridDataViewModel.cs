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

        public GridMetricsViewModel? Metricas { get; set; }

        public int TotalUsuarios => Usuarios.Count;
        public int TotalDias => Dias.Count;
        public DateTime? PrimeiroDia => Dias.FirstOrDefault()?.Data;
        public DateTime? UltimoDia => Dias.LastOrDefault()?.Data;
    }
}
