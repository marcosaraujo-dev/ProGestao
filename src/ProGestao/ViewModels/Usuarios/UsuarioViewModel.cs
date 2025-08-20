using System.ComponentModel.DataAnnotations;

namespace ProGestao.ViewModels.Usuarios
{
    public class UsuarioViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cargo é obrigatório")]
        [StringLength(50)]
        public string Cargo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Equipe é obrigatória")]
        public int EquipeId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DataAdmissao { get; set; }

        public bool Ativo { get; set; } = true;

        // Propriedades para exibição
        public string? EquipeNome { get; set; }
        public int TotalAtividades { get; set; }
        public int AtividadesAndamento { get; set; }
        public int AtividadesConcluidas { get; set; }
        public DateTime DataCriacao { get; set; }

        public string StatusTexto => Ativo ? "Ativo" : "Inativo";
        public string StatusClasse => Ativo ? "success" : "secondary";
        public string InicialNome => string.IsNullOrEmpty(Nome) ? "?" : Nome.Substring(0, 1).ToUpperInvariant();
    }
}
