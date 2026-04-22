using ProGestao.ViewModels.Grid;

namespace ProGestao.ViewModels.Gantt
{
    /// <summary>
    /// ViewModel principal com todos os dados do diagrama de Gantt
    /// </summary>
    public class GanttDataViewModel
    {
        public List<GanttUsuarioViewModel> Usuarios { get; set; } = new();
        public List<DiaGridViewModel> Dias { get; set; } = new();
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        public int TotalUsuarios => Usuarios.Count;
        public int TotalDias => Dias.Count;
    }
}
