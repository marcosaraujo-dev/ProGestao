namespace ProGestao.ViewModels.Projetos
{
    /// <summary>
    /// ViewModel específico para resumo de projetos
    /// </summary>
    public class ProjetoResumoViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? StatusNome { get; set; }
        public string? StatusCor { get; set; }
        public string? ResponsavelNome { get; set; }

        // Métricas
        public int TotalAtividades { get; set; }
        public int AtividadesConcluidas { get; set; }
        public int AtividadesEmAndamento { get; set; }
        public int AtividadesPendentes { get; set; }
        public int AtividadesAtrasadas { get; set; }

        // Cálculos
        public decimal PercentualConclusao { get; set; }
        public decimal PercentualAndamento { get; set; }

        // Datas
        public DateTime DataInicio { get; set; }
        public DateTime? DataFimPrevista { get; set; }
        public DateTime? DataFimReal { get; set; }

        // Status computados
        public bool EstaAtrasado { get; set; }
        public bool EstaConcluido { get; set; }
        public bool EstaEmprejeto { get; set; }
        public int DiasRestantes { get; set; }

        // Performance
        public decimal MediaHorasPorAtividade { get; set; }
        public decimal TotalHorasEstimadas { get; set; }
        public decimal TotalHorasReais { get; set; }
    }
}
