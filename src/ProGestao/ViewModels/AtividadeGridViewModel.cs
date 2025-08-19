namespace ProGestao.ViewModels
{
    public class AtividadeGridViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string ProjetoNome { get; set; } = string.Empty;
        public string TipoAtividadeNome { get; set; } = string.Empty;
        public string StatusNome { get; set; } = string.Empty;
        public string StatusCor { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime? DataFimPrevista { get; set; }
        public DateTime? DataFimReal { get; set; }
        public int Prioridade { get; set; }

        public string StatusClasse => StatusNome.ToLower() switch
        {
            "pendente" => "pendente",
            "em andamento" => "andamento",
            "pausada" => "pausada",
            "concluída" => "concluida",
            "cancelada" => "cancelada",
            _ => "pendente"
        };

        public string PrioridadeTexto => Prioridade switch
        {
            1 => "Baixa",
            2 => "Normal",
            3 => "Alta",
            4 => "Crítica",
            _ => "Normal"
        };

        public string PrioridadeCor => Prioridade switch
        {
            1 => "success",
            2 => "info",
            3 => "warning",
            4 => "danger",
            _ => "info"
        };
    }
}
