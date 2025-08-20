using ProGestao.ViewModels.Atividade;
using ProGestao.ViewModels.Projetos;
using ProGestao.ViewModels.Usuarios;

namespace ProGestao.ViewModels.Dashboard
{
    /// <summary>
    /// ViewModel para métricas do dashboard
    /// </summary>
    public class DashboardMetricsViewModel
    {
        public int TotalAtividades { get; set; }
        public int AtividadesPendentes { get; set; }
        public int AtividadesEmAndamento { get; set; }
        public int AtividadesConcluidas { get; set; }
        public int AtividadesAtrasadas { get; set; }

        public int TotalProjetos { get; set; }
        public int ProjetosAtivos { get; set; }
        public int ProjetosConcluidos { get; set; }

        public int TotalUsuarios { get; set; }
        public int UsuariosAtivos { get; set; }

        public decimal TaxaConclusaoGeral { get; set; }
        public decimal MediaHorasPorAtividade { get; set; }

        // Listas para gráficos
        public List<AtividadesPorStatusViewModel> AtividadesPorStatus { get; set; } = new();
        public List<AtividadesPorPrioridadeViewModel> AtividadesPorPrioridade { get; set; } = new();
        public List<UsuarioPerformanceViewModel> TopUsuarios { get; set; } = new();
        public List<ProjetoDashboardViewModel> ProjetosRecentes { get; set; } = new();
    }
}
