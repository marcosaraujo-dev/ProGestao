namespace ProGestao.ViewModels
{
    /// <summary>
    /// ViewModel para métricas do grid
    /// </summary>
    public class GridMetricsViewModel
    {
        public int TotalAtividades { get; set; }
        public int AtividadesAtrasadas { get; set; }
        public Dictionary<string, int> AtividadesPorStatus { get; set; } = new();
        public Dictionary<string, int> AtividadesPorPrioridade { get; set; } = new();

        public double TaxaConclusao => TotalAtividades > 0
            ? (TotalAtividades - AtividadesAtrasadas) / (double)TotalAtividades * 100
            : 0;
    }
}
