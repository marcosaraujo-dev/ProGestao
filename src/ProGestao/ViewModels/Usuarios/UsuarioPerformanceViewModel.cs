namespace ProGestao.ViewModels.Usuarios
{
    /// <summary>
    /// ViewModel para performance de usuários
    /// </summary>
    public class UsuarioPerformanceViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? EquipeNome { get; set; }
        public int TotalAtividades { get; set; }
        public int AtividadesConcluidas { get; set; }
        public int AtividadesEmAndamento { get; set; }
        public int AtividadesAtrasadas { get; set; }
        public decimal TaxaConclusao { get; set; }

        public string PerformanceClasse => TaxaConclusao switch
        {
            >= 80 => "success",
            >= 60 => "warning",
            _ => "danger"
        };

        public string PerformanceTexto => TaxaConclusao switch
        {
            >= 90 => "Excelente",
            >= 80 => "Boa",
            >= 60 => "Regular",
            _ => "Baixa"
        };
    }
}
