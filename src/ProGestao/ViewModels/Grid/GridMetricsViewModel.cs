namespace ProGestao.ViewModels.Grid
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
        public double PercentualAtrasadas => TotalAtividades > 0
       ? Math.Round((double)AtividadesAtrasadas / TotalAtividades * 100, 1)
       : 0;

        public bool TemAtividadesAtrasadas => AtividadesAtrasadas > 0;
    }
}
