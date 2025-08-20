namespace ProGestao.ViewModels.Atividade
{
    public class AtividadesPorPrioridadeViewModel
    {
        public string PrioridadeNome { get; set; } = string.Empty;
        public string PrioridadeCor { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal Percentual { get; set; }
    }
}
