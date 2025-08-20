namespace ProGestao.ViewModels.Usuarios
{
    /// <summary>
    /// ViewModel simplificado para dropdowns
    /// </summary>
    public class UsuarioLookupViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Cargo { get; set; }
        public string? EquipeNome { get; set; }
    }
}
