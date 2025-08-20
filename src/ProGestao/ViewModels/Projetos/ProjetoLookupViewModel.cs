namespace ProGestao.ViewModels.Projetos
{
    /// <summary>
    /// ViewModel simplificado para dropdowns
    /// </summary>
    public class ProjetoLookupViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? StatusNome { get; set; }
    }
}
