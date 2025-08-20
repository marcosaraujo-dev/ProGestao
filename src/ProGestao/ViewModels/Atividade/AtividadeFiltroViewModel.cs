namespace ProGestao.ViewModels.Atividade
{
    /// <summary>
    /// ViewModel para filtros de atividades
    /// </summary>
    public class AtividadeFiltroViewModel
    {
        public int? ProjetoId { get; set; }
        public int? UsuarioId { get; set; }
        public int? StatusId { get; set; }
        public int? TipoAtividadeId { get; set; }
        public int? EquipeId { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int? Prioridade { get; set; }
        public bool? Atrasadas { get; set; }

        // Propriedades para exibição
        public string? ProjetoNome { get; set; }
        public string? UsuarioNome { get; set; }
        public string? StatusNome { get; set; }
        public string? TipoAtividadeNome { get; set; }
        public string? EquipeNome { get; set; }

        public bool TemFiltros => ProjetoId.HasValue ||
                                 UsuarioId.HasValue ||
                                 StatusId.HasValue ||
                                 TipoAtividadeId.HasValue ||
                                 EquipeId.HasValue ||
                                 DataInicio.HasValue ||
                                 DataFim.HasValue ||
                                 Prioridade.HasValue ||
                                 Atrasadas.HasValue;
    }
}
