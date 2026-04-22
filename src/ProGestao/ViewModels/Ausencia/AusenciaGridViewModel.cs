namespace ProGestao.ViewModels.Ausencia
{
    /// <summary>
    /// ViewModel compacto para exibição de ausências no grid
    /// </summary>
    public class AusenciaGridViewModel
    {
        public int Id { get; set; }
        public string TipoNome { get; set; } = string.Empty;
        public string TipoCor { get; set; } = "#007bff";
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
    }
}
