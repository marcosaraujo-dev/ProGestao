namespace ProGestao.ViewModels.Gantt
{
    /// <summary>
    /// ViewModel para uma barra horizontal no diagrama de Gantt
    /// Representa uma atividade ou ausência posicionada no eixo temporal
    /// </summary>
    public class GanttBarraViewModel
    {
        public int Id { get; set; }

        /// <summary>
        /// Tipo da barra: "atividade" ou "ausencia"
        /// </summary>
        public string Tipo { get; set; } = "atividade";

        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Cor da barra (hex). Para atividades, cor do status. Para ausências, cor do tipo.
        /// </summary>
        public string Cor { get; set; } = "#6c757d";

        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        /// <summary>
        /// Coluna de início (0-based) relativa ao período visível
        /// </summary>
        public int DiaInicioOffset { get; set; }

        /// <summary>
        /// Largura em colunas (número de dias visíveis)
        /// </summary>
        public int DuracaoDias { get; set; }

        /// <summary>
        /// Indica se a atividade está atrasada (DataFimPrevista &lt; hoje e sem DataFimReal)
        /// </summary>
        public bool EstaAtrasada { get; set; }

        /// <summary>
        /// Link para a página de detalhes
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Conteúdo HTML do tooltip
        /// </summary>
        public string TooltipHtml { get; set; } = string.Empty;
    }
}
