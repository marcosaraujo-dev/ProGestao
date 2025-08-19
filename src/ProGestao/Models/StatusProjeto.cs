using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProGestao.Models
{
    [Table("StatusProjeto")]
    public class StatusProjeto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nome { get; set; }

        [StringLength(7)]
        public string Cor { get; set; } = "#007bff";

        public int Ordem { get; set; } = 0;

        public ICollection<Projeto> Projetos { get; set; } = new List<Projeto>();
    }
}
