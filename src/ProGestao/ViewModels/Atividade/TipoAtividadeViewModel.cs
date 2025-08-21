using System.ComponentModel.DataAnnotations;

namespace ProGestao.ViewModels.Atividade
{
    public class TipoAtividadeViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Nome do tipo é obrigatório")]
        [StringLength(50, ErrorMessage = "Nome deve ter no máximo 50 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Cor deve estar no formato hexadecimal (#RRGGBB)")]
        public string? Cor { get; set; }

        [StringLength(200, ErrorMessage = "Descrição deve ter no máximo 200 caracteres")]
        public string? Descricao { get; set; }

        public bool Ativo { get; set; } = true;
    }
}
