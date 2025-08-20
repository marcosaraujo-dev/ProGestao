namespace ProGestao.ViewModels.Grid
{
    /// <summary>
    /// ViewModel para dias do grid
    /// </summary>
    public class DiaGridViewModel
    {
        public DateTime Data { get; set; }
        public string DiaSemana { get; set; } = string.Empty;
        public string Dia { get; set; } = string.Empty;
        public string Mes { get; set; } = string.Empty;
    }
}
