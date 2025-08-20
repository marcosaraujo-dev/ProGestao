namespace ProGestao.ViewModels.Projetos
{
    /// <summary>
    /// ViewModel para filtros de projetos
    /// </summary>
    public class ProjetoFiltroViewModel
    {
        public string? Status { get; set; }
        public int? ResponsavelId { get; set; }
        public string? Nome { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public bool? SomenteAtrasados { get; set; }
        public bool? SomenteAtivos { get; set; }

        // Propriedades para exibição
        public string? StatusNome { get; set; }
        public string? ResponsavelNome { get; set; }

        public bool TemFiltros => !string.IsNullOrWhiteSpace(Status) ||
                                 ResponsavelId.HasValue ||
                                 !string.IsNullOrWhiteSpace(Nome) ||
                                 DataInicio.HasValue ||
                                 DataFim.HasValue ||
                                 SomenteAtrasados.HasValue ||
                                 SomenteAtivos.HasValue;
    }
}
