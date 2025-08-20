namespace ProGestao.ViewModels.Atividade
{
    public class AtividadesPorStatusViewModel
    {
        public string StatusNome { get; set; } = string.Empty;
        public string StatusCor { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal Percentual { get; set; }
    }
}
