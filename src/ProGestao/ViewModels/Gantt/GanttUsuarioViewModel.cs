namespace ProGestao.ViewModels.Gantt
{
    /// <summary>
    /// ViewModel para um usuário no diagrama de Gantt (linha do eixo Y)
    /// </summary>
    public class GanttUsuarioViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Iniciais { get; set; } = string.Empty;

        /// <summary>
        /// Barras de atividades e ausências do usuário no período
        /// </summary>
        public List<GanttBarraViewModel> Barras { get; set; } = new();
    }
}
