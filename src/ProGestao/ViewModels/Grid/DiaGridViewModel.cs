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

        public bool EhHoje => Data.Date == DateTime.Today;
        public bool EhFimDeSemana => Data.DayOfWeek == DayOfWeek.Saturday || Data.DayOfWeek == DayOfWeek.Sunday;
        public string DiaAbreviado => DiaSemana.Length > 3 ? DiaSemana.Substring(0, 3) : DiaSemana;
        public string CssClass => $"day-column {(EhHoje ? "today" : "")} {(EhFimDeSemana ? "weekend" : "")}".Trim();
    }
}
