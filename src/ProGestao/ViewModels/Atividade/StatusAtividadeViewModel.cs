namespace ProGestao.ViewModels.Atividade
{
    /// <summary>
    /// ViewModel para status de atividades
    /// </summary>
    public class StatusAtividadeViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cor { get; set; } = string.Empty;
        public int Ordem { get; set; }

    }
}
