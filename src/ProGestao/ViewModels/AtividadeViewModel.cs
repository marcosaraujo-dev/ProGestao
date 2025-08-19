using System.ComponentModel.DataAnnotations;

namespace ProGestao.ViewModels
{
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

       
        public string ClasseProjeto => TemProjeto ? "com-projeto" : "sem-projeto";

        public string StatusClasse => StatusNome?.ToLower() switch
        {
            "pendente" => "pendente",
            "em andamento" => "andamento",
            "pausada" => "pausada",
            "concluída" or "concluida" => "concluida",
            "cancelada" => "cancelada",
            _ => "pendente"
        };

        // Propriedades calculadas para relatórios
        public decimal? VariacaoHoras => HorasEstimadas.HasValue && HorasReais.HasValue
            ? HorasReais.Value - HorasEstimadas.Value
            : null;

        public decimal? VariacaoPercentual => HorasEstimadas.HasValue && HorasReais.HasValue && HorasEstimadas.Value > 0
            ? ((HorasReais.Value - HorasEstimadas.Value) / HorasEstimadas.Value) * 100
            : null;

        public bool EstaAtrasada => DataFimPrevista.HasValue &&
                                   !DataFimReal.HasValue &&
                                   DataFimPrevista.Value < DateTime.Now;

        public bool EstaConcluida => !string.IsNullOrEmpty(StatusNome) &&
                                    (StatusNome.Equals("Concluída", StringComparison.OrdinalIgnoreCase) ||
                                     StatusNome.Equals("Concluida", StringComparison.OrdinalIgnoreCase));

        public int DiasRestantes => DataFimPrevista.HasValue
            ? (DataFimPrevista.Value.Date - DateTime.Today).Days
            : 0;

        public string SituacaoPrazo
        {
            get
            {
                if (EstaConcluida) return "Concluída";
                if (EstaAtrasada) return "Atrasada";
                if (DiasRestantes <= 1) return "Vence hoje/amanhã";
                if (DiasRestantes <= 3) return "Vence em breve";
                return "No prazo";
            }
        }

        public string CorSituacaoPrazo => SituacaoPrazo switch
        {
            "Concluída" => "success",
            "Atrasada" => "danger",
            "Vence hoje/amanhã" => "warning",
            "Vence em breve" => "info",
            _ => "secondary"
        };

        // Método para validação customizada atualizado
        public bool IsValid(out List<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Nome))
                errors.Add("Nome é obrigatório");

            // ALTERAÇÃO: ProjetoId não é mais obrigatório
            // if (ProjetoId <= 0) errors.Add("Projeto deve ser selecionado");

            if (UsuarioId <= 0)
                errors.Add("Usuário responsável deve ser selecionado");

            if (TipoAtividadeId <= 0)
                errors.Add("Tipo de atividade deve ser selecionado");

            if (StatusId <= 0)
                errors.Add("Status deve ser selecionado");

            if (DataInicio == default)
                errors.Add("Data de início é obrigatória");

            if (DataFimPrevista.HasValue && DataFimPrevista.Value < DataInicio)
                errors.Add("Data fim prevista não pode ser anterior à data de início");

            if (DataFimReal.HasValue && DataFimReal.Value < DataInicio)
                errors.Add("Data de conclusão não pode ser anterior à data de início");

            if (HorasEstimadas.HasValue && HorasEstimadas.Value < 0)
                errors.Add("Horas estimadas não podem ser negativas");

            if (HorasReais.HasValue && HorasReais.Value < 0)
                errors.Add("Horas reais não podem ser negativas");

            if (Prioridade < 1 || Prioridade > 4)
                errors.Add("Prioridade deve estar entre 1 e 4");

            return !errors.Any();
        }
    }
}
