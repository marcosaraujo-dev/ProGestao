using System.ComponentModel.DataAnnotations;

namespace ProGestao.Models
{
    public class Ausencia
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public int TipoAusenciaId { get; set; }
        public TipoAusencia TipoAusencia { get; set; } = null!;

        [Required]
        public DateTime DataInicio { get; set; }

        [Required]
        public DateTime DataFim { get; set; }

        [StringLength(500)]
        public string? Observacao { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public bool Ativo { get; set; } = true;
    }
}
