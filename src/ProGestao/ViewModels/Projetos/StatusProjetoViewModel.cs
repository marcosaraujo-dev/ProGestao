namespace ProGestao.ViewModels.Projetos
{
    /// <summary>
    /// ViewModel para status de projetos
    /// </summary>
    public class StatusProjetoViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cor { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public string? Descricao { get; set; }
    }
}
