using System.ComponentModel.DataAnnotations;

namespace ProGestao.ViewModels
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

        public Dictionary<DateTime, List<AtividadeGridViewModel>> AtividadesPorDia { get; set; } =
            new Dictionary<DateTime, List<AtividadeGridViewModel>>();

        // Propriedades calculadas para estatísticas
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
