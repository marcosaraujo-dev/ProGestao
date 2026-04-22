using ProGestao.ViewModels.Atividade;
using ProGestao.ViewModels.Ausencia;
using ProGestao.ViewModels.Grid;
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

        /// <summary>
        /// Lista de dias com atividades do usuário
        /// </summary>
        public List<DiaUsuarioViewModel> Dias { get; set; } = new();

       

        // Lista de atividades para compatibilidade
        public List<AtividadeGridViewModel> Atividades { get; set; } = new();

        // Propriedade existente mantida
        public Dictionary<DateTime, List<AtividadeGridViewModel>> AtividadesPorDia { get; set; } =
            new Dictionary<DateTime, List<AtividadeGridViewModel>>();

        /// <summary>
        /// Ausências do usuário agrupadas por dia para exibição no grid
        /// </summary>
        public Dictionary<DateTime, List<AusenciaGridViewModel>> AusenciasPorDia { get; set; } =
            new Dictionary<DateTime, List<AusenciaGridViewModel>>();

        // Propriedades calculadas
        public int TotalAtividades => Dias.Sum(d => d.Atividades.Count);


        public int AtividadesPendentes => AtividadesPorDia.Values.SelectMany(x => x)
            .Count(a => a.StatusClasse == "pendente");

        public int AtividadesEmAndamento => AtividadesPorDia.Values.SelectMany(x => x)
            .Count(a => a.StatusClasse == "andamento");

        public int AtividadesConcluidas => AtividadesPorDia.Values.SelectMany(x => x)
            .Count(a => a.StatusClasse == "concluida");

        public int AtividadesAtrasadas => AtividadesPorDia.Values
           .SelectMany(x => x)
           .Count(a => a.EstaAtrasada);

        public double PercentualConclusao => TotalAtividades > 0
            ? Math.Round((double)AtividadesConcluidas / TotalAtividades * 100, 1)
            : 0;
        public bool TemAtividades => TotalAtividades > 0;
        public bool TemAtividadesAtrasadas => AtividadesAtrasadas > 0;

        public string CorIndicadorPerformance => PercentualConclusao switch
        {
            >= 80 => "success",
            >= 60 => "warning",
            _ => "danger"
        };

        public List<AtividadeGridViewModel> ObterAtividadesDoDia(DateTime data)
        {
            return AtividadesPorDia.TryGetValue(data.Date, out var atividades)
                ? atividades
                : new List<AtividadeGridViewModel>();
        }

        public int ContarAtividadesDoDia(DateTime data)
        {
            return ObterAtividadesDoDia(data).Count;
        }

        public bool TemAtividadesNoDia(DateTime data)
        {
            return ContarAtividadesDoDia(data) > 0;
        }

        public List<AusenciaGridViewModel> ObterAusenciasDoDia(DateTime data)
        {
            return AusenciasPorDia.TryGetValue(data.Date, out var ausencias)
                ? ausencias
                : new List<AusenciaGridViewModel>();
        }

        public bool TemAusenciasNoDia(DateTime data)
        {
            return ObterAusenciasDoDia(data).Any();
        }
    }
}
