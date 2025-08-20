using System.ComponentModel.DataAnnotations;

namespace ProGestao.ViewModels.Atividade
{
    /// <summary>
    /// ViewModel principal para atividades
    /// </summary>
    public class AtividadeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Descrição deve ter no máximo 1000 caracteres")]
        public string? Descricao { get; set; }

        public int? ProjetoId { get; set; }

        [Required(ErrorMessage = "Tipo de atividade é obrigatório")]
        public int TipoAtividadeId { get; set; }

        [Required(ErrorMessage = "Status é obrigatório")]
        public int StatusId { get; set; }

        [Required(ErrorMessage = "Usuário responsável é obrigatório")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "Data de início é obrigatória")]
        [DataType(DataType.DateTime)]
        public DateTime DataInicio { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DataFimPrevista { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DataFimReal { get; set; }

        [Range(0, 9999.99, ErrorMessage = "Horas estimadas deve ser entre 0 e 9999.99")]
        public decimal? HorasEstimadas { get; set; }

        [Range(0, 9999.99, ErrorMessage = "Horas reais deve ser entre 0 e 9999.99")]
        public decimal? HorasReais { get; set; }

        [Range(1, 4, ErrorMessage = "Prioridade deve ser entre 1 e 4")]
        public int Prioridade { get; set; } = 3;

        [StringLength(1000, ErrorMessage = "Observações devem ter no máximo 1000 caracteres")]
        public string? Observacoes { get; set; }

        // Propriedades para exibição (somente leitura)
        public string? ProjetoNome { get; set; }
        public string? TipoAtividadeNome { get; set; }
        public string? StatusNome { get; set; }
        public string? StatusCor { get; set; }
        public string? UsuarioNome { get; set; }

        // Propriedades estendidas para detalhes
        public string? UsuarioEquipe { get; set; }
        public int? UsuarioEquipeId { get; set; }
        public string? ProjetoStatus { get; set; }
        public string? ProjetoStatusCor { get; set; }
        public string? ProjetoResponsavel { get; set; }

        // Propriedades de auditoria
        public DateTime DataCriacao { get; set; }
        public DateTime DataAtualizacao { get; set; }

        // Propriedades computadas
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

        public bool TemProjeto => ProjetoId.HasValue;
        public string ProjetoNomeOuSemProjeto => !string.IsNullOrEmpty(ProjetoNome) ? ProjetoNome : "Sem projeto";
        public string ClasseProjeto => TemProjeto ? "text-primary" : "text-muted";

        public bool EstaAtrasada => DataFimPrevista.HasValue &&
                                   DataFimPrevista < DateTime.Today &&
                                   StatusNome != "Concluída";

        public int DiasParaVencimento => DataFimPrevista.HasValue
            ? (int)(DataFimPrevista.Value.Date - DateTime.Today).TotalDays
            : int.MaxValue;

        public decimal PercentualConclusao => HorasEstimadas.HasValue && HorasEstimadas > 0 && HorasReais.HasValue
            ? Math.Min(100, (HorasReais.Value / HorasEstimadas.Value) * 100)
            : 0;

        public int? DuracaoEmDias => DataFimPrevista.HasValue
            ? (DataFimPrevista.Value - DataInicio).Days + 1
            : (int?)null;

        public int? DiasRestantes => DataFimPrevista.HasValue && StatusNome != "Concluída"
            ? Math.Max(0, (DataFimPrevista.Value - DateTime.Today).Days)
            : (int?)null;
    }
}
