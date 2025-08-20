using ProGestao.ViewModels.Atividade;
using System.ComponentModel.DataAnnotations;

namespace ProGestao.ViewModels.Usuarios
{
    public class UsuarioGridViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [Display(Name = "Cargo")]
        public string Cargo { get; set; } = string.Empty;

        [Display(Name = "Equipe")]
        public string EquipeNome { get; set; } = string.Empty;

        // Iniciais para compatibilidade com Grid
        public string Iniciais { get; set; } = string.Empty;

        // Lista de atividades para compatibilidade
        public List<AtividadeGridViewModel> Atividades { get; set; } = new();

        // Propriedade existente mantida
        public Dictionary<DateTime, List<AtividadeGridViewModel>> AtividadesPorDia { get; set; } =
            new Dictionary<DateTime, List<AtividadeGridViewModel>>();

        // Propriedades calculadas existentes mantidas
        public int TotalAtividades => AtividadesPorDia.Values.SelectMany(x => x).Count();

        public int AtividadesPendentes => AtividadesPorDia.Values.SelectMany(x => x)
            .Count(a => a.StatusClasse == "pendente");

        public int AtividadesEmAndamento => AtividadesPorDia.Values.SelectMany(x => x)
            .Count(a => a.StatusClasse == "andamento");

        public int AtividadesConcluidas => AtividadesPorDia.Values.SelectMany(x => x)
            .Count(a => a.StatusClasse == "concluida");

        public double PercentualConclusao => TotalAtividades > 0
            ? Math.Round((double)AtividadesConcluidas / TotalAtividades * 100, 1)
            : 0;
    }
}
