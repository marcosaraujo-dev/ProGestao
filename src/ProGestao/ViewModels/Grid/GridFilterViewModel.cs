using System.ComponentModel.DataAnnotations;

namespace ProGestao.ViewModels.Grid
{
    /// <summary>
    /// ViewModel para filtros do grid com validação
    /// </summary>
    public class GridFilterViewModel
    {
        [Required(ErrorMessage = "Data de início é obrigatória")]
        public DateTime DataInicio { get; set; }

        [Required(ErrorMessage = "Data de fim é obrigatória")]
        public DateTime DataFim { get; set; }

        public DateTime? SemanaReferencia { get; set; }
        public int? EquipeId { get; set; }
        public string? TipoPeriodo { get; set; }

        public int QtdDias => (DataFim - DataInicio).Days + 1;
        public bool EhPeriodoValido => DataInicio <= DataFim && QtdDias <= 30;
    }
}
