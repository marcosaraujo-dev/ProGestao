using System.ComponentModel.DataAnnotations;

namespace ProGestao.ViewModels.Ausencia
{
    /// <summary>
    /// ViewModel para ausências (CRUD completo)
    /// </summary>
    public class AusenciaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Usuário é obrigatório")]
        public int UsuarioId { get; set; }

        public string UsuarioNome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tipo de ausência é obrigatório")]
        public int TipoAusenciaId { get; set; }

        public string TipoAusenciaNome { get; set; } = string.Empty;

        public string TipoAusenciaCor { get; set; } = "#007bff";

        [Required(ErrorMessage = "Data de início é obrigatória")]
        [DataType(DataType.Date)]
        public DateTime DataInicio { get; set; }

        [Required(ErrorMessage = "Data de fim é obrigatória")]
        [DataType(DataType.Date)]
        public DateTime DataFim { get; set; }

        [StringLength(500, ErrorMessage = "Observação deve ter no máximo 500 caracteres")]
        public string? Observacao { get; set; }

        public DateTime DataCriacao { get; set; }

        public bool Ativo { get; set; } = true;
    }
}
