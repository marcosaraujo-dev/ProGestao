using System.ComponentModel.DataAnnotations;

namespace ProGestao.ViewModels
{
    /// <summary>
    /// ViewModel para períodos disponíveis no dropdown do Grid
    /// Representa as opções de período que o usuário pode selecionar
    /// </summary>
    public class PeriodoViewModel
    {
        /// <summary>
        /// Nome exibido no dropdown (ex: "Esta Semana", "Últimos 15 dias")
        /// </summary>
        [Required]
        [Display(Name = "Nome do Período")]
        [MaxLength(50)]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Valor interno usado para identificar o período (ex: "semana", "15dias", "mes")
        /// </summary>
        [Required]
        [Display(Name = "Valor")]
        [MaxLength(20)]
        public string Valor { get; set; } = string.Empty;

        /// <summary>
        /// Ícone Font Awesome para exibir no dropdown (ex: "fas fa-calendar-week")
        /// </summary>
        [Display(Name = "Ícone")]
        [MaxLength(50)]
        public string Icone { get; set; } = string.Empty;

        /// <summary>
        /// Descrição adicional opcional (ex: "7 dias", "15 dias")
        /// </summary>
        [Display(Name = "Descrição")]
        [MaxLength(100)]
        public string? Descricao { get; set; }

        /// <summary>
        /// Controle do período Ativo (ex: true, false)
        /// </summary>
        [Display(Name = "Ativo")]
        public bool Ativo { get; set; }


        /// <summary>
        /// Indica se este período está atualmente selecionado
        /// Propriedade calculada, não persistida
        /// </summary>
        public bool IsSelected { get; set; } = false;

        /// <summary>
        /// Quantidade de dias do período (calculado)
        /// Propriedade auxiliar para validações
        /// </summary>
        public int QtdDias { get; set; }

        /// <summary>
        /// Construtor padrão
        /// </summary>
        public PeriodoViewModel()
        {
        }

        /// <summary>
        /// Construtor com parâmetros principais
        /// </summary>
        /// <param name="nome">Nome exibido</param>
        /// <param name="valor">Valor interno</param>
        /// <param name="icone">Ícone Font Awesome</param>
        /// <param name="descricao">Descrição opcional</param>
        public PeriodoViewModel(string nome, string valor, string icone = "", string? descricao = null)
        {
            Nome = nome;
            Valor = valor;
            Icone = icone;
            Descricao = descricao;
        }

        /// <summary>
        /// Cria instância para período de semana
        /// </summary>
        /// <returns>PeriodoViewModel configurado para semana</returns>
        public static PeriodoViewModel Semana()
        {
            return new PeriodoViewModel
            {
                Nome = "Esta Semana",
                Valor = "semana",
                Icone = "fas fa-calendar-week",
                Descricao = "7 dias",
                QtdDias = 7
            };
        }

        /// <summary>
        /// Cria instância para período de 15 dias
        /// </summary>
        /// <returns>PeriodoViewModel configurado para 15 dias</returns>
        public static PeriodoViewModel QuinzeDias()
        {
            return new PeriodoViewModel
            {
                Nome = "Últimos 15 dias",
                Valor = "15dias",
                Icone = "fas fa-calendar-alt",
                Descricao = "15 dias",
                QtdDias = 15
            };
        }

        /// <summary>
        /// Cria instância para período de mês atual
        /// </summary>
        /// <returns>PeriodoViewModel configurado para mês</returns>
        public static PeriodoViewModel MesAtual()
        {
            var diasDoMes = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

            return new PeriodoViewModel
            {
                Nome = "Este Mês",
                Valor = "mes",
                Icone = "fas fa-calendar",
                Descricao = "Mês atual",
                QtdDias = diasDoMes
            };
        }

        /// <summary>
        /// Cria instância para período de 30 dias
        /// </summary>
        /// <returns>PeriodoViewModel configurado para 30 dias</returns>
        public static PeriodoViewModel TrintaDias()
        {
            return new PeriodoViewModel
            {
                Nome = "Últimos 30 dias",
                Valor = "30dias",
                Icone = "fas fa-calendar-plus",
                Descricao = "30 dias",
                QtdDias = 30
            };
        }

        /// <summary>
        /// Cria instância para período customizado
        /// </summary>
        /// <returns>PeriodoViewModel configurado para período customizado</returns>
        public static PeriodoViewModel Custom()
        {
            return new PeriodoViewModel
            {
                Nome = "Período Customizado",
                Valor = "custom",
                Icone = "fas fa-cog",
                Descricao = "Definido pelo usuário",
                QtdDias = 0 // Será calculado dinamicamente
            };
        }

        /// <summary>
        /// Cria instância para visão mensal compacta
        /// </summary>
        /// <returns>PeriodoViewModel configurado para visão mensal</returns>
        public static PeriodoViewModel Mensal()
        {
            var diasDoMes = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

            return new PeriodoViewModel
            {
                Nome = "Mensal",
                Valor = "mensal",
                Icone = "fas fa-calendar-days",
                Descricao = "Visão compacta do mês",
                QtdDias = diasDoMes
            };
        }

        /// <summary>
        /// Retorna todos os períodos disponíveis
        /// </summary>
        /// <returns>Lista de períodos padrão</returns>
        public static List<PeriodoViewModel> GetPeriodosDisponiveis()
        {
            return new List<PeriodoViewModel>
            {
                Semana(),
                QuinzeDias(),
                MesAtual(),
                TrintaDias(),
                Mensal()
            };
        }

        /// <summary>
        /// Valida se o período é válido
        /// </summary>
        /// <returns>True se válido, False caso contrário</returns>
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Valor))
                return false;

            if (QtdDias > 30 || QtdDias < 1)
                return false;

            return true;
        }

        /// <summary>
        /// Override ToString para facilitar debug
        /// </summary>
        /// <returns>Representação string do período</returns>
        public override string ToString()
        {
            return $"{Nome} ({Valor}) - {QtdDias} dias";
        }

        /// <summary>
        /// Override Equals para comparação
        /// </summary>
        /// <param name="obj">Objeto para comparar</param>
        /// <returns>True se são iguais</returns>
        public override bool Equals(object? obj)
        {
            if (obj is PeriodoViewModel other)
            {
                return Valor.Equals(other.Valor, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        /// <summary>
        /// Override GetHashCode
        /// </summary>
        /// <returns>Hash code baseado no Valor</returns>
        public override int GetHashCode()
        {
            return Valor.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Extensões para trabalhar com PeriodoViewModel
    /// </summary>
    public static class PeriodoViewModelExtensions
    {
        /// <summary>
        /// Converte valor string para PeriodoViewModel
        /// </summary>
        /// <param name="valor">Valor do período (ex: "semana", "15dias")</param>
        /// <returns>PeriodoViewModel correspondente ou null</returns>
        public static PeriodoViewModel? FromValor(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            return valor.ToLower() switch
            {
                "semana" => PeriodoViewModel.Semana(),
                "15dias" => PeriodoViewModel.QuinzeDias(),
                "mes" => PeriodoViewModel.MesAtual(),
                "30dias" => PeriodoViewModel.TrintaDias(),
                "mensal" => PeriodoViewModel.Mensal(),
                "custom" => PeriodoViewModel.Custom(),
                _ => null
            };
        }

        /// <summary>
        /// Calcula quantidade de dias para um período
        /// </summary>
        /// <param name="periodo">Período para calcular</param>
        /// <param name="dataReferencia">Data de referência para cálculo</param>
        /// <returns>Quantidade de dias</returns>
        public static int CalcularQtdDias(this PeriodoViewModel periodo, DateTime? dataReferencia = null)
        {
            var referencia = dataReferencia ?? DateTime.Today;

            return periodo.Valor.ToLower() switch
            {
                "semana" => 7,
                "15dias" => 15,
                "mes" => DateTime.DaysInMonth(referencia.Year, referencia.Month),
                "mensal" => DateTime.DaysInMonth(referencia.Year, referencia.Month),
                "30dias" => 30,
                "custom" => periodo.QtdDias,
                _ => 7
            };
        }

        /// <summary>
        /// Calcula data de início para um período
        /// </summary>
        /// <param name="periodo">Período para calcular</param>
        /// <param name="dataReferencia">Data de referência</param>
        /// <returns>Data de início do período</returns>
        public static DateTime CalcularDataInicio(this PeriodoViewModel periodo, DateTime? dataReferencia = null)
        {
            var referencia = dataReferencia ?? DateTime.Today;

            return periodo.Valor.ToLower() switch
            {
                "semana" => GetStartOfWeek(referencia),
                "15dias" => referencia.AddDays(-14),
                "mes" => new DateTime(referencia.Year, referencia.Month, 1),
                "mensal" => new DateTime(referencia.Year, referencia.Month, 1),
                "30dias" => referencia.AddDays(-29),
                "custom" => referencia,
                _ => GetStartOfWeek(referencia)
            };
        }

        /// <summary>
        /// Obtém início da semana (domingo)
        /// </summary>
        /// <param name="date">Data de referência</param>
        /// <returns>Data do início da semana</returns>
        private static DateTime GetStartOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Sunday)) % 7;
            return date.AddDays(-diff).Date;
        }
    }
}