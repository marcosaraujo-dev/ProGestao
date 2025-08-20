namespace ProGestao.ViewModels
{
    /// <summary>
    /// ViewModel para filtros do grid
    /// </summary>
    public class GridFilterViewModel
    {
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public DateTime SemanaReferencia { get; set; }
        public int? EquipeId { get; set; }
    }
}
