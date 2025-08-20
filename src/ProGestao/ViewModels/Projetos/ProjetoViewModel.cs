using System.ComponentModel.DataAnnotations;

namespace ProGestao.ViewModels.Projetos
{
    public class ProjetoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(150)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "Data de início é obrigatória")]
        [DataType(DataType.Date)]
        public DateTime DataInicio { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DataFimPrevista { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DataFimReal { get; set; }

        [Required(ErrorMessage = "Status é obrigatório")]
        public int StatusId { get; set; }

        public int? ResponsavelId { get; set; }

        [Url(ErrorMessage = "URL inválida")]
        [StringLength(500)]
        public string? LinkProjeto { get; set; }

        [StringLength(1000)]
        public string? Observacoes { get; set; }

        // Propriedades para exibição
        public string? StatusNome { get; set; }
        public string? StatusCor { get; set; }
        public string? ResponsavelNome { get; set; }

        // Métricas do projeto
        public int QtdAtividades { get; set; }
        public int QtdAtividadesConcluidas { get; set; }
        public decimal PercentualConclusao { get; set; }
        
        
        public decimal ProgressoPercentual => QtdAtividades > 0 ?
            (decimal)QtdAtividadesConcluidas / QtdAtividades * 100 : 0;

        public bool EstaAtrasado => DataFimPrevista.HasValue &&
                                   DataFimPrevista < DateTime.Today &&
                                   StatusNome != "Concluído";

        public string StatusClasse => StatusNome?.ToLowerInvariant() switch
        {
            "planejamento" => "warning",
            "em andamento" => "info",
            "concluído" => "success",
            "cancelado" => "danger",
            _ => "secondary"
        };

        public DateTime DataCriacao { get; set; }
    }
}
