using System.ComponentModel.DataAnnotations;

namespace ProGestao.Models
{
    public class TipoAtividade
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nome { get; set; }

        [StringLength(7)]
        public string Cor { get; set; } = "#007bff";

        [StringLength(200)]
        public string? Descricao { get; set; }
        public bool Ativo { get; set; } = true;

        public ICollection<Atividade> Atividades { get; set; } = new List<Atividade>();
    }
}
