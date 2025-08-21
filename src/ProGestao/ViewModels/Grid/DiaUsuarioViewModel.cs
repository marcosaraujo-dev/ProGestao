using ProGestao.ViewModels.Atividade;

namespace ProGestao.ViewModels.Grid
{
    public class DiaUsuarioViewModel
    {
        public DateTime Data { get; set; }
        public string DiaSemana { get; set; } = string.Empty;
        public List<AtividadeGridViewModel> Atividades { get; set; } = new();
    }
}
