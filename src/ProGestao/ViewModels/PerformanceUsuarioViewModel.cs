namespace ProGestao.ViewModels
{
    public class PerformanceUsuarioViewModel
    {
        public int UsuarioId { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public int TotalAtividades { get; set; }
        public int AtividadesConcluidas { get; set; }
        public int AtividadesAndamento { get; set; }
        public int AtividadesAtrasadas { get; set; }
        public decimal TaxaConclusao { get; set; }

        public string TaxaConclusaoFormatada => TaxaConclusao.ToString("P1");

        public string ClassePerformance
        {
            get
            {
                if (TaxaConclusao >= 0.8m) return "success";
                if (TaxaConclusao >= 0.6m) return "warning";
                return "danger";
            }
        }
    }
}
