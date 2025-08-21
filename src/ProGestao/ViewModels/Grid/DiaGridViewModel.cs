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
        public int DiaNumero { get; set; }
        public string Mes { get; set; } = string.Empty;

        /// <summary>
        /// Verifica se é fim de semana
        /// </summary>
        public bool IsFimDeSemana => Data.DayOfWeek == DayOfWeek.Saturday || Data.DayOfWeek == DayOfWeek.Sunday;

        /// <summary>
        /// Verifica se é hoje
        /// </summary>
        public bool IsHoje => Data.Date == DateTime.Today;
    }
}
