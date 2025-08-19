using System.ComponentModel.DataAnnotations;

namespace ProGestao.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        public string Cargo { get; set; }

        public int EquipeId { get; set; }
        public Equipe Equipe { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DataAdmissao { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public bool Ativo { get; set; } = true;

        public ICollection<Atividade> Atividades { get; set; } = new List<Atividade>();
        public ICollection<Projeto> ProjetosResponsavel { get; set; } = new List<Projeto>();
    }
}
