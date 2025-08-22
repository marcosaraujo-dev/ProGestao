using System.ComponentModel.DataAnnotations;

namespace ProGestao.ViewModels.Atividade
{
    /// <summary>
    /// ViewModel específico para atividades no Grid
    /// Otimizado para performance e responsividade
    /// </summary>
    public class AtividadeGridViewModel
    {
        #region Properties Básicas

        public int Id { get; set; }

        [Required]
        [Display(Name = "Nome")]
        [MaxLength(200)]
        public string Nome { get; set; } = string.Empty;

        [Display(Name = "Descrição")]
        [MaxLength(1000)]
        public string Descricao { get; set; } = string.Empty;

        [Display(Name = "Projeto")]
        public string? ProjetoNome { get; set; }

        [Display(Name = "Tipo")]
        public string TipoAtividadeNome { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public string StatusNome { get; set; } = string.Empty;

        [Display(Name = "Cor do Status")]
        public string StatusCor { get; set; } = "#6c757d"; // Cor padrão

        #endregion

        #region Properties de Data

        [Required]
        [Display(Name = "Data Início")]
        public DateTime DataInicio { get; set; }

        [Display(Name = "Data Fim Prevista")]
        public DateTime? DataFimPrevista { get; set; }

        [Display(Name = "Data Fim Real")]
        public DateTime? DataFimReal { get; set; }

        #endregion

        #region Properties de Controle

        [Required]
        [Display(Name = "Prioridade")]
        [Range(1, 4)]
        public int Prioridade { get; set; } = 2; // Normal como padrão

        [Required]
        public int UsuarioId { get; set; }

        [Display(Name = "Usuário")]
        public string UsuarioNome { get; set; } = string.Empty;

        public int? ProjetoId { get; set; }

        public int? TipoAtividadeId { get; set; }

        public int StatusId { get; set; }

        #endregion

        #region Properties Calculadas - Compatibilidade

        /// <summary>
        /// Data para compatibilidade com versões anteriores
        /// </summary>
        public DateTime Data => DataInicio;
        public DateTime DataFinal => DataFimReal ?? DataFimPrevista ?? DataInicio;

        public bool TemProjeto => !string.IsNullOrEmpty(ProjetoNome);

        /// <summary>
        /// Status como objeto para compatibilidade
        /// </summary>
        public StatusAtividadeViewModel Status => new()
        {
            Nome = StatusNome,
            Cor = StatusCor
        };

        #endregion

        #region Properties de Estado

        /// <summary>
        /// Verifica se a atividade está atrasada
        /// </summary>
        public bool EstaAtrasada => DataFimReal == null &&
                                   DataFimPrevista.HasValue &&
                                   DataFimPrevista.Value.Date < DateTime.Today &&
                                   !StatusNome.Equals("Concluída", StringComparison.OrdinalIgnoreCase) &&
                                   !StatusNome.Equals("Cancelada", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Verifica se a atividade está em andamento
        /// </summary>
        public bool EstaEmAndamento => StatusNome.Equals("Em Andamento", StringComparison.OrdinalIgnoreCase) ||
                                      StatusNome.Equals("Andamento", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Verifica se a atividade está concluída
        /// </summary>
        public bool EstaConcluida => StatusNome.Equals("Concluída", StringComparison.OrdinalIgnoreCase) ||
                                    StatusNome.Equals("Finalizada", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Verifica se a atividade está pendente
        /// </summary>
        public bool EstaPendente => StatusNome.Equals("Pendente", StringComparison.OrdinalIgnoreCase) ||
                                   StatusNome.Equals("A Fazer", StringComparison.OrdinalIgnoreCase) ||
                                   StatusNome.Equals("Novo", StringComparison.OrdinalIgnoreCase);

        #endregion

        #region Properties para CSS Classes

        /// <summary>
        /// Classe CSS baseada no status
        /// </summary>
        public string StatusClasse => StatusNome.ToLower().Replace(" ", "").Replace("ã", "a").Replace("ç", "c") switch
        {
            "pendente" or "afazer" or "novo" => "pendente",
            "emandamento" or "andamento" => "andamento",
            "pausada" or "pausado" => "pausada",
            "concluida" or "concluída" or "finalizada" => "concluida",
            "cancelada" or "cancelado" => "cancelada",
            _ => "pendente"
        };

        /// <summary>
        /// Texto da prioridade
        /// </summary>
        public string PrioridadeTexto => Prioridade switch
        {
            1 => "Baixa",
            2 => "Normal",
            3 => "Alta",
            4 => "Crítica",
            _ => "Normal"
        };

        /// <summary>
        /// Cor Bootstrap da prioridade
        /// </summary>
        public string PrioridadeCor => Prioridade switch
        {
            1 => "success",
            2 => "info",
            3 => "warning",
            4 => "danger",
            _ => "info"
        };


        /// <summary>
        /// Classe CSS da prioridade
        /// </summary>
        public string PrioridadeClasse => Prioridade switch
        {
            1 => "baixa",
            2 => "normal",
            3 => "alta",
            4 => "critica",
            _ => "normal"
        };

        /// <summary>
        /// Ícone da prioridade
        /// </summary>
        public string IconePrioridade => Prioridade switch
        {
            1 => "fas fa-arrow-down text-success",
            2 => "fas fa-minus text-info",
            3 => "fas fa-arrow-up text-warning",
            4 => "fas fa-exclamation-triangle text-danger",
            _ => "fas fa-minus text-info"
        };

        #endregion

        #region Properties de Duração

        /// <summary>
        /// Duração prevista em dias
        /// </summary>
        public int DuracaoPrevistaDias => DataFimPrevista.HasValue
            ? Math.Max(1, (DataFimPrevista.Value.Date - DataInicio.Date).Days + 1)
            : 1;

        /// <summary>
        /// Duração real em dias (se concluída)
        /// </summary>
        public int? DuracaoRealDias => DataFimReal.HasValue
            ? Math.Max(1, (DataFimReal.Value.Date - DataInicio.Date).Days + 1)
            : null;

        /// <summary>
        /// Dias restantes (se em andamento)
        /// </summary>
        public int? DiasRestantes => !EstaConcluida && DataFimPrevista.HasValue
            ? Math.Max(0, (DataFimPrevista.Value.Date - DateTime.Today).Days)
            : null;

        #endregion

        #region Properties de Formatação

        /// <summary>
        /// Texto formatado do período
        /// </summary>
        public string PeriodoFormatado
        {
            get
            {
                if (DataFimPrevista.HasValue)
                {
                    if (DataInicio.Date == DataFimPrevista.Value.Date)
                        return DataInicio.ToString("dd/MM");

                    return $"{DataInicio:dd/MM} - {DataFimPrevista:dd/MM}";
                }

                return DataInicio.ToString("dd/MM");
            }
        }

        /// <summary>
        /// Tooltip com informações completas
        /// </summary>
        public string TooltipCompleto
        {
            get
            {
                var tooltip = $"{Nome}\n";
                tooltip += $"Projeto: {ProjetoNome ?? "Sem projeto"}\n";
                tooltip += $"Status: {StatusNome}\n";
                tooltip += $"Prioridade: {PrioridadeTexto}\n";
                tooltip += $"Período: {PeriodoFormatado}";

                if (EstaAtrasada)
                    tooltip += "\n⚠️ ATRASADA";

                if (!string.IsNullOrWhiteSpace(Descricao))
                    tooltip += $"\n\n{Descricao}";

                return tooltip;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Construtor padrão
        /// </summary>
        public AtividadeGridViewModel()
        {
        }

        /// <summary>
        /// Construtor com parâmetros básicos
        /// </summary>
        public AtividadeGridViewModel(int id, string nome, int usuarioId, DateTime dataInicio)
        {
            Id = id;
            Nome = nome;
            UsuarioId = usuarioId;
            DataInicio = dataInicio;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cria uma cópia da atividade para outro dia
        /// Usado na expansão multi-dia do grid
        /// </summary>
        /// <returns>Nova instância clonada</returns>
        public AtividadeGridViewModel Clone()
        {
            return new AtividadeGridViewModel
            {
                Id = this.Id,
                Nome = this.Nome,
                Descricao = this.Descricao,
                ProjetoNome = this.ProjetoNome,
                ProjetoId = this.ProjetoId,
                TipoAtividadeNome = this.TipoAtividadeNome,
                TipoAtividadeId = this.TipoAtividadeId,
                StatusNome = this.StatusNome,
                StatusCor = this.StatusCor,
                StatusId = this.StatusId,
                DataInicio = this.DataInicio,
                DataFimPrevista = this.DataFimPrevista,
                DataFimReal = this.DataFimReal,
                Prioridade = this.Prioridade,
                UsuarioId = this.UsuarioId,
                UsuarioNome = this.UsuarioNome
            };
        }

        /// <summary>
        /// Valida se a atividade está corretamente preenchida
        /// </summary>
        /// <returns>True se válida</returns>
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Nome))
                return false;

            if (UsuarioId <= 0)
                return false;

            if (DataInicio == default)
                return false;

            if (Prioridade < 1 || Prioridade > 4)
                return false;

            return true;
        }

        /// <summary>
        /// Override ToString para debug
        /// </summary>
        public override string ToString()
        {
            return $"{Nome} ({StatusNome}) - {DataInicio:dd/MM}";
        }

        #endregion

        #region functions

        private string GerarTooltip()
        {
            var tooltip = $"📝 {Nome}";

            if (TemProjeto)
                tooltip += $"\n📁 Projeto: {ProjetoNome}";

            tooltip += $"\n📊 Status: {StatusNome}";
            tooltip += $"\n⚡ Prioridade: {PrioridadeTexto}";

            if (DataFimPrevista.HasValue)
                tooltip += $"\n📅 Previsão: {DataFimPrevista:dd/MM/yyyy}";

            if (EstaAtrasada)
                tooltip += "\n⚠️ ATRASADA";

            if (!string.IsNullOrEmpty(Descricao) && Descricao.Length > 0)
                tooltip += $"\n💭 {Descricao.Substring(0, Math.Min(100, Descricao.Length))}";

            return tooltip;
        }
        #endregion
    }


}