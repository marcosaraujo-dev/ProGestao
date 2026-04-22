using System.ComponentModel.DataAnnotations;

namespace ProGestao.ViewModels.Gantt
{
    /// <summary>
    /// ViewModel para filtros do diagrama de Gantt
    /// </summary>
    public class GanttFilterViewModel
    {
        [Required(ErrorMessage = "Data de início é obrigatória")]
        public DateTime DataInicio { get; set; }

        [Required(ErrorMessage = "Data de fim é obrigatória")]
        public DateTime DataFim { get; set; }

        public int? EquipeId { get; set; }

        public int? ProjetoId { get; set; }

        /// <summary>
        /// Tipo de visão: "semanal" ou "mensal"
        /// </summary>
        public string Visao { get; set; } = "semanal";

        public int QtdDias => (DataFim - DataInicio).Days + 1;
        public bool EhPeriodoValido => DataInicio <= DataFim;
    }
}
