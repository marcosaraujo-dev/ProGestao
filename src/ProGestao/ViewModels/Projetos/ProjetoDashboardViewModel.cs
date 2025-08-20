namespace ProGestao.ViewModels.Projetos
{
    /// <summary>
    /// ViewModel para dashboard de projetos
    /// </summary>
    public class ProjetoDashboardViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? StatusNome { get; set; }
        public string? StatusCor { get; set; }
        public decimal PercentualConclusao { get; set; }
        public int QtdAtividades { get; set; }
        public DateTime? DataFimPrevista { get; set; }
        public string? ResponsavelNome { get; set; }
    }
}
