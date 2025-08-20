namespace ProGestao.ViewModels.Projetos
{
    /// <summary>
    /// ViewModel para métricas do projeto
    /// </summary>
    public class ProjetoMetricasViewModel
    {
        public int TotalAtividades { get; set; }
        public int AtividadesConcluidas { get; set; }
        public int AtividadesAndamento { get; set; }
        public int AtividadesPendentes => TotalAtividades - AtividadesConcluidas - AtividadesAndamento;
        public decimal PercentualConclusao { get; set; }

        // Métricas de prazo
        public int? DiasRestantes { get; set; }
        public int? DiasAtraso { get; set; }
        public bool EstaAtrasado { get; set; }
        public bool EstaNoUltimaSemana { get; set; }
        public bool EstaNoPrazo { get; set; }

        // Progresso temporal
        public decimal ProgressoUltimaSemana { get; set; }

        // Métricas calculadas
        public string StatusPrazo
        {
            get
            {
                if (EstaAtrasado) return "Atrasado";
                if (EstaNoUltimaSemana) return "Urgente";
                if (EstaNoPrazo) return "No Prazo";
                return "Sem Prazo";
            }
        }
    }
}
