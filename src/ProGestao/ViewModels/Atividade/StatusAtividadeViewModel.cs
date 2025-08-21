using System.ComponentModel.DataAnnotations;

namespace ProGestao.ViewModels.Atividade
{
    /// <summary>
    /// ViewModel para status de atividades
    /// </summary>
    public class StatusAtividadeViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Nome do status é obrigatório")]
        [StringLength(50, ErrorMessage = "Nome deve ter no máximo 50 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cor é obrigatória")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Cor deve estar no formato hexadecimal (#RRGGBB)")]
        public string Cor { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Descrição deve ter no máximo 200 caracteres")]
        public string? Descricao { get; set; }

        [Range(0, 999, ErrorMessage = "Ordem deve estar entre 0 e 999")]
        public int Ordem { get; set; }

        public bool Ativo { get; set; } = true;

    }
}
