using System.ComponentModel.DataAnnotations;

namespace ProGestao.Models
{
    public class TipoAusencia
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(7)]
        public string Cor { get; set; } = "#007bff";

        [StringLength(200)]
        public string? Descricao { get; set; }

        public bool Ativo { get; set; } = true;

        public ICollection<Ausencia> Ausencias { get; set; } = new List<Ausencia>();
    }
}
