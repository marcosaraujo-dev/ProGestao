using System.ComponentModel.DataAnnotations;

namespace ProGestao.Models
{
    public class Projeto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Nome { get; set; }

        [StringLength(1000)]
        public string? Descricao { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DataInicio { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DataFimPrevista { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DataFimReal { get; set; }

        public int StatusId { get; set; }
        public StatusProjeto Status { get; set; }

        public int? ResponsavelId { get; set; }
        public Usuario? Responsavel { get; set; }

        [Url]
        [StringLength(500)]
        public string? LinkProjeto { get; set; }

        [StringLength(1000)]
        public string? Observacoes { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public ICollection<Atividade> Atividades { get; set; } = new List<Atividade>();
    }
}
