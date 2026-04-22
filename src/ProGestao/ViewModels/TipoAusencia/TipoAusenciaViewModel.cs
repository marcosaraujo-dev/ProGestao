using System.ComponentModel.DataAnnotations;

namespace ProGestao.ViewModels.TipoAusencia
{
    /// <summary>
    /// ViewModel para tipos de ausência
    /// </summary>
    public class TipoAusenciaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(50, ErrorMessage = "Nome deve ter no máximo 50 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cor é obrigatória")]
        [StringLength(7, ErrorMessage = "Cor deve ter no máximo 7 caracteres")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Cor deve estar no formato hexadecimal (#XXXXXX)")]
        public string Cor { get; set; } = "#007bff";

        [StringLength(200, ErrorMessage = "Descrição deve ter no máximo 200 caracteres")]
        public string? Descricao { get; set; }

        public bool Ativo { get; set; } = true;
    }
}
