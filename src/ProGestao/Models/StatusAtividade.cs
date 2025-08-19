
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProGestao.Models
{

    [Table("StatusAtividade")]
    public class StatusAtividade
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nome { get; set; }

        [StringLength(7)]
        public string Cor { get; set; }

        public int Ordem { get; set; }

        public ICollection<Atividade> Atividades { get; set; }
    }
}
