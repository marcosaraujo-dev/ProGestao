namespace ProGestao.ViewModels.Atividade
{
    public class TimelineAtividadeViewModel
    {
        public int Id { get; set; }
        public string AtividadeNome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string ProjetoNome { get; set; } = string.Empty;
        public string UsuarioNome { get; set; } = string.Empty;
        public string TipoAtividadeNome { get; set; } = string.Empty;
        public string StatusNome { get; set; } = string.Empty;
        public string StatusCor { get; set; } = string.Empty;
        public DateTime DataReferencia { get; set; }
        public string TipoEvento { get; set; } = string.Empty; // Criacao, Inicio, Conclusao, Atualizacao
        public int Prioridade { get; set; }

        public string TipoEventoTexto => TipoEvento switch
        {
            "Criacao" => "Criada",
            "Inicio" => "Iniciada",
            "Conclusao" => "Concluída",
            "Atualizacao" => "Atualizada",
            _ => "Evento"
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
