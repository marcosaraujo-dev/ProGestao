using System.ComponentModel.DataAnnotations;

namespace ProGestao.Models
{
    public class Atividade
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Descricao { get; set; }

        public int? ProjetoId { get; set; }
        public Projeto? Projeto { get; set; }

        public int TipoAtividadeId { get; set; }
        public TipoAtividade TipoAtividade { get; set; }

        public int StatusId { get; set; }
        public StatusAtividade Status { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        [Required]
        public DateTime DataInicio { get; set; }

        public DateTime? DataFimPrevista { get; set; }
        public DateTime? DataFimReal { get; set; }

        [Range(0, 9999.99)]
        public decimal? HorasEstimadas { get; set; }

        [Range(0, 9999.99)]
        public decimal? HorasReais { get; set; }

        [Range(1, 4)]
        public int Prioridade { get; set; } = 3;

        [StringLength(1000)]
        public string? Observacoes { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime DataAtualizacao { get; set; } = DateTime.Now;
    }
}
