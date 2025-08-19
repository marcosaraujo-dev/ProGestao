// ===== ViewModels/ProjetoResumoViewModel.cs =====
using System.ComponentModel.DataAnnotations;

namespace ProGestao.ViewModels
{
    public class ProjetoResumoViewModel
    {
        public int ProjetoId { get; set; }

        [Required]
        [StringLength(150)]
        public string NomeProjeto { get; set; } = string.Empty;

        public string? StatusProjeto { get; set; }

        public string? ResponsavelNome { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DataInicio { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DataFimPrevista { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DataFimReal { get; set; }

        // Contadores de Atividades
        public int TotalAtividades { get; set; }
        public int AtividadesPendentes { get; set; }
        public int AtividadesAndamento { get; set; }
        public int AtividadesConcluidas { get; set; }
        public int AtividadesAtrasadas { get; set; }

        // Métricas de Tempo
        [Range(0, 99999.99)]
        public decimal? HorasEstimadas { get; set; }

        [Range(0, 99999.99)]
        public decimal? HorasReais { get; set; }

        [Range(0, 100)]
        public decimal ProgressoPercentual { get; set; }

        // ===== PROPRIEDADES DERIVADAS =====

        /// <summary>
        /// Atividades que não estão concluídas (Pendentes + Em Andamento + Pausadas)
        /// </summary>
        public int AtividadesRestantes => TotalAtividades - AtividadesConcluidas;

        /// <summary>
        /// Indica se o projeto está atrasado (passou da data prevista e não foi concluído)
        /// </summary>
        public bool ProjetoAtrasado => DataFimPrevista.HasValue &&
                                       DataFimPrevista.Value.Date < DateTime.Today &&
                                       !DataFimReal.HasValue &&
                                       StatusProjeto != "Concluído";

        /// <summary>
        /// Indica se o projeto foi concluído
        /// </summary>
        public bool ProjetoConcluido => DataFimReal.HasValue || StatusProjeto == "Concluído";

        /// <summary>
        /// Duração total do projeto (data fim real ou prevista menos data início)
        /// </summary>
        public int? DuracaoEmDias
        {
            get
            {
                var dataFim = DataFimReal ?? DataFimPrevista;
                return dataFim?.Subtract(DataInicio).Days;
            }
        }

        /// <summary>
        /// Dias restantes até o prazo (se aplicável)
        /// </summary>
        public int? DiasRestantes
        {
            get
            {
                if (!DataFimPrevista.HasValue || ProjetoConcluido)
                    return null;

                var dias = DataFimPrevista.Value.Subtract(DateTime.Today).Days;
                return dias >= 0 ? dias : 0;
            }
        }

        /// <summary>
        /// Eficiência baseada em horas (horas estimadas vs reais)
        /// </summary>
        public decimal? EficienciaHoras
        {
            get
            {
                if (!HorasEstimadas.HasValue || !HorasReais.HasValue || HorasReais.Value == 0)
                    return null;

                return Math.Round((HorasEstimadas.Value / HorasReais.Value) * 100, 2);
            }
        }

        /// <summary>
        /// Média de horas por atividade concluída
        /// </summary>
        public decimal? MediaHorasPorAtividade
        {
            get
            {
                if (!HorasReais.HasValue || AtividadesConcluidas == 0)
                    return null;

                return Math.Round(HorasReais.Value / AtividadesConcluidas, 2);
            }
        }

        // ===== PROPRIEDADES PARA EXIBIÇÃO =====

        /// <summary>
        /// Cor do status do projeto para badges/indicadores
        /// </summary>
        public string StatusCor => StatusProjeto?.ToLower() switch
        {
            "planejamento" => "#17a2b8",
            "em andamento" => "#28a745",
            "em teste" => "#ffc107",
            "concluído" => "#007bff",
            "cancelado" => "#dc3545",
            "pausado" => "#6c757d",
            _ => "#6c757d"
        };

        /// <summary>
        /// Classe CSS para o progresso baseado na porcentagem
        /// </summary>
        public string ProgressoClasse => ProgressoPercentual switch
        {
            >= 100 => "success",
            >= 75 => "info",
            >= 50 => "warning",
            >= 25 => "warning",
            _ => "danger"
        };

        /// <summary>
        /// Ícone FontAwesome baseado no status
        /// </summary>
        public string StatusIcone => StatusProjeto?.ToLower() switch
        {
            "planejamento" => "fas fa-clipboard-list",
            "em andamento" => "fas fa-play-circle",
            "em teste" => "fas fa-vial",
            "concluído" => "fas fa-check-circle",
            "cancelado" => "fas fa-times-circle",
            "pausado" => "fas fa-pause-circle",
            _ => "fas fa-project-diagram"
        };

        /// <summary>
        /// Indicador de prioridade baseado em atrasos e atividades pendentes
        /// </summary>
        public string PrioridadeIndicador
        {
            get
            {
                if (ProjetoAtrasado && AtividadesAtrasadas > 0)
                    return "CRÍTICA";
                if (AtividadesAtrasadas > 0)
                    return "ALTA";
                if (DiasRestantes.HasValue && DiasRestantes <= 7)
                    return "MÉDIA";
                return "NORMAL";
            }
        }

        /// <summary>
        /// Cor da prioridade para indicadores visuais
        /// </summary>
        public string PrioridadeCor => PrioridadeIndicador switch
        {
            "CRÍTICA" => "danger",
            "ALTA" => "warning",
            "MÉDIA" => "info",
            _ => "success"
        };

        // ===== MÉTODOS AUXILIARES =====

        /// <summary>
        /// Retorna uma descrição textual do status do progresso
        /// </summary>
        public string DescricaoProgresso
        {
            get
            {
                if (ProjetoConcluido)
                    return "Projeto concluído";

                if (ProgressoPercentual == 0)
                    return "Não iniciado";

                if (ProgressoPercentual < 25)
                    return "Início";

                if (ProgressoPercentual < 50)
                    return "Em desenvolvimento";

                if (ProgressoPercentual < 75)
                    return "Progredindo bem";

                if (ProgressoPercentual < 100)
                    return "Quase finalizado";

                return "Finalizado";
            }
        }

        /// <summary>
        /// Formata as datas para exibição
        /// </summary>
        public string DataInicioFormatada => DataInicio.ToString("dd/MM/yyyy");

        public string? DataFimPrevistaFormatada => DataFimPrevista?.ToString("dd/MM/yyyy");

        public string? DataFimRealFormatada => DataFimReal?.ToString("dd/MM/yyyy");

        /// <summary>
        /// Resumo executivo do projeto em uma linha
        /// </summary>
        public string ResumoExecutivo
        {
            get
            {
                var resumo = $"{AtividadesConcluidas}/{TotalAtividades} atividades concluídas";

                if (HorasReais.HasValue)
                    resumo += $", {HorasReais:F1}h realizadas";

                if (DiasRestantes.HasValue)
                    resumo += $", {DiasRestantes} dias restantes";
                else if (ProjetoAtrasado)
                    resumo += ", projeto atrasado";

                return resumo;
            }
        }

        /// <summary>
        /// Valida se o projeto tem dados consistentes
        /// </summary>
        public bool IsValido()
        {
            var erros = new List<string>();

            if (string.IsNullOrEmpty(NomeProjeto))
                erros.Add("Nome do projeto é obrigatório");

            if (DataInicio == default)
                erros.Add("Data de início é obrigatória");

            if (DataFimPrevista.HasValue && DataFimPrevista < DataInicio)
                erros.Add("Data fim prevista não pode ser anterior à data de início");

            if (DataFimReal.HasValue && DataFimReal < DataInicio)
                erros.Add("Data fim real não pode ser anterior à data de início");

            if (TotalAtividades < 0)
                erros.Add("Total de atividades não pode ser negativo");

            if (AtividadesConcluidas > TotalAtividades)
                erros.Add("Atividades concluídas não podem exceder o total");

            if (ProgressoPercentual < 0 || ProgressoPercentual > 100)
                erros.Add("Progresso deve estar entre 0% e 100%");

            return !erros.Any();
        }

        /// <summary>
        /// Converte para objeto anônimo para APIs JSON
        /// </summary>
        public object ToApiResponse()
        {
            return new
            {
                id = ProjetoId,
                nome = NomeProjeto,
                status = StatusProjeto,
                responsavel = ResponsavelNome,
                dataInicio = DataInicioFormatada,
                dataFimPrevista = DataFimPrevistaFormatada,
                dataFimReal = DataFimRealFormatada,
                atividades = new
                {
                    total = TotalAtividades,
                    pendentes = AtividadesPendentes,
                    andamento = AtividadesAndamento,
                    concluidas = AtividadesConcluidas,
                    atrasadas = AtividadesAtrasadas,
                    restantes = AtividadesRestantes
                },
                progresso = new
                {
                    percentual = ProgressoPercentual,
                    descricao = DescricaoProgresso,
                    classe = ProgressoClasse
                },
                horas = new
                {
                    estimadas = HorasEstimadas,
                    reais = HorasReais,
                    eficiencia = EficienciaHoras,
                    mediaPorAtividade = MediaHorasPorAtividade
                },
                prazos = new
                {
                    duracaoEmDias = DuracaoEmDias,
                    diasRestantes = DiasRestantes,
                    atrasado = ProjetoAtrasado,
                    concluido = ProjetoConcluido
                },
                prioridade = new
                {
                    indicador = PrioridadeIndicador,
                    cor = PrioridadeCor
                },
                resumo = ResumoExecutivo
            };
        }
    }
}